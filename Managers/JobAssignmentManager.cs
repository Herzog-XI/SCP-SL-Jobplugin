using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using FacilityJobs.Roles;
using MEC;
using PlayerRoles;
using UnityEngine;

namespace FacilityJobs.Managers
{
    internal static class JobAssignmentManager
    {
        private const float HausmeisterIntroDuration = 7f;
        private const float ZoneManagerIntroDuration = 7f;
        private const float CiAgentIntroDuration = 10f;
        private const float SerpentsHandIntroDuration = 10f;
        private const int IntroBodyWrapLength = 90;
        private const float IntroFontSize = 24f;
        private static readonly List<Vector3> scientistSpawnPositions = new List<Vector3>();

        private static bool hsmResolved;
        private static bool hsmWarningShown;
        private static Type hintType;
        private static Type playerDisplayType;
        private static MethodInfo playerDisplayFactoryMethod;
        private static MethodInfo addHintMethod;
        private static MethodInfo removeHintMethod;

        public static void CaptureScientistSpawns()
        {
            scientistSpawnPositions.Clear();

            foreach (Player player in Player.List.Where(p => p != null && p.IsConnected && p.Role.Type == RoleTypeId.Scientist))
            {
                if (!scientistSpawnPositions.Any(position => Vector3.Distance(position, player.Position) < 0.5f))
                    scientistSpawnPositions.Add(player.Position);
            }

            Debug($"Captured {scientistSpawnPositions.Count} Scientist spawn position(s).");
        }

        public static bool HasFacilityJob(Player player)
        {
            return player != null && CustomRole.Registered.Any(role => IsFacilityJob(role) && role.Check(player));
        }

        public static bool Assign(Player player, CustomRole role, out string error)
        {
            error = null;
            if (player == null || !player.IsConnected)
            {
                error = "Player is not connected.";
                return false;
            }

            if (role == null || !IsFacilityJob(role))
            {
                error = "Job is not registered.";
                return false;
            }

            if (HasFacilityJob(player))
            {
                error = "Dieser Spieler besitzt bereits einen Facility-Job.";
                return false;
            }

            if (!HasRequiredBaseRole(player, role, out error))
                return false;

            if (!TryGetSpawnPosition(role, out Vector3 targetPosition))
            {
                error = $"No valid spawn position found for {role.Name}.";
                return false;
            }

            role.AddRole(player);
            Timing.CallDelayed(0.1f, () => FinalizeAssignment(player, role, targetPosition));
            return true;
        }

        private static bool HasRequiredBaseRole(Player player, CustomRole role, out string error)
        {
            error = null;

            if (role is HausmeisterRole && player.Role.Type != RoleTypeId.ClassD)
            {
                error = "Der Hausmeister kann nur einer D-Klasse zugewiesen werden.";
                return false;
            }

            if ((role is ZoneManagerRole || role is CiAgentRole) && player.Role.Type != RoleTypeId.Scientist)
            {
                error = "Dieser Job kann nur einem Wissenschaftler zugewiesen werden.";
                return false;
            }

            if (role is SerpentsHandCustomRole && player.Role.Type != RoleTypeId.Tutorial)
            {
                error = "Eine Serpent's-Hand-Klasse kann manuell nur einem Tutorial zugewiesen werden.";
                return false;
            }

            return true;
        }

        private static bool IsFacilityJob(CustomRole role)
        {
            return role is ZoneManagerRole ||
                   role is HausmeisterRole ||
                   role is CiAgentRole ||
                   role is SerpentsHandCustomRole;
        }

        private static void FinalizeAssignment(Player player, CustomRole role, Vector3 targetPosition)
        {
            if (player == null || !player.IsConnected || role == null)
                return;

            if (role is HausmeisterRole)
                targetPosition += Vector3.up * 0.35f;

            player.Position = targetPosition;

            if (role is SerpentsHandCustomRole)
                ApplySerpentsHandShelterRotation(player, targetPosition);

            ApplyVisibleJobTag(player, role);

            Debug($"Job assignment trigger reached for {role.Name} on {player.Nickname}. Showing intro now.");
            ShowIntro(player, role);

            if (role is CiAgentRole)
                RoundState.CiAgentUserId = player.UserId;

            Debug($"Finalized {role.Name} for {player.Nickname} at {targetPosition}.");
        }

        private static void ApplySerpentsHandShelterRotation(Player player, Vector3 targetPosition)
        {
            Room room = Room.Get(targetPosition);
            if (room == null || room.Type.ToString().IndexOf("Shelter", StringComparison.OrdinalIgnoreCase) < 0)
                return;

            float y = room.Rotation.eulerAngles.y + 180f;
            player.Rotation = Quaternion.Euler(0f, y, 0f);
            Debug($"Rotated Serpent's Hand spawn in {room.Type} by 180 degrees toward the shelter door.");
        }

        public static void ApplyVisibleJobTag(Player player, CustomRole role)
        {
            if (player == null || !player.IsConnected || role == null)
                return;

            player.CustomInfo = role.CustomInfo ?? string.Empty;
            player.InfoArea |= PlayerInfoArea.CustomInfo;

            if (role is CiAgentRole)
            {
                Timing.CallDelayed(0.6f, () => ReapplyCiTag(player, role));
                Timing.CallDelayed(1.5f, () => ReapplyCiTag(player, role));
            }
        }

        private static void ReapplyCiTag(Player player, CustomRole role)
        {
            if (!IsStillRole(player, role))
                return;

            player.CustomInfo = role.CustomInfo ?? "Wissenschaftler";
            player.InfoArea |= PlayerInfoArea.CustomInfo;
            Debug($"Reapplied CI disguise tag for {player.Nickname}.");
        }

        private static void ShowIntro(Player player, CustomRole role)
        {
            if (player == null || !player.IsConnected || role is not FacilityCustomRole facilityRole)
                return;

            float duration = role is CiAgentRole
                ? CiAgentIntroDuration
                : role is SerpentsHandCustomRole
                    ? SerpentsHandIntroDuration
                    : ZoneManagerIntroDuration;

            string body = facilityRole.IntroBody ?? string.Empty;
            Config config = Plugin.Instance?.Config ?? new Config();
            bool isTutorial = role is SerpentsHandCustomRole;

            float titleX = isTutorial ? config.TutorialHintXCoordinate : config.JobHintXCoordinate;
            float titleY = isTutorial ? config.TutorialHintTitleYCoordinate : config.JobHintTitleYCoordinate;
            float bodyX = isTutorial ? config.TutorialHintXCoordinate : config.JobHintXCoordinate;
            float bodyStartY = isTutorial ? config.TutorialHintBodyStartYCoordinate : config.JobHintBodyStartYCoordinate;
            float bodyLineSpacing = isTutorial ? config.TutorialHintBodyLineSpacing : config.JobHintBodyLineSpacing;

            try
            {
                if (!ResolveHintServiceMeow())
                {
                    if (!hsmWarningShown)
                    {
                        hsmWarningShown = true;
                        Log.Warn("[FacilityJobs] HintServiceMeow API was not found. Job intro is disabled.");
                    }

                    return;
                }

                object display = GetPlayerDisplay(player);
                if (display == null)
                    throw new InvalidOperationException("HintServiceMeow returned no PlayerDisplay for the player.");

                List<object> introHints = new List<object>();

                object titleHint = Activator.CreateInstance(hintType);
                SetProperty(titleHint, "Id", $"facility_job_intro_title_{player.Id}");
                SetProperty(titleHint, "Text", $"<size={IntroFontSize:0}><b><color=#FFFFFF>Du bist ein </color><color={facilityRole.IntroTitleColor}>{facilityRole.IntroTitle}.</color></b></size>");
                SetProperty(titleHint, "XCoordinate", titleX);
                SetProperty(titleHint, "YCoordinate", titleY);
                SetEnumProperty(titleHint, "YCoordinateAlign", "Bottom");
                SetEnumProperty(titleHint, "Alignment", "Center");
                SetProperty(titleHint, "FontSize", (int)IntroFontSize);
                SetEnumProperty(titleHint, "SyncSpeed", "Fast");
                introHints.Add(titleHint);

                List<string> bodyLines = WrapIntroBody(body, IntroBodyWrapLength);
                for (int index = 0; index < bodyLines.Count; index++)
                {
                    if (string.IsNullOrWhiteSpace(bodyLines[index]))
                        continue;

                    object bodyHint = Activator.CreateInstance(hintType);
                    SetProperty(bodyHint, "Id", $"facility_job_intro_body_{player.Id}_{index}");
                    SetProperty(bodyHint, "Text", $"<size={IntroFontSize:0}><color=#FFFFFF>{bodyLines[index]}</color></size>");
                    SetProperty(bodyHint, "XCoordinate", bodyX);
                    SetProperty(bodyHint, "YCoordinate", bodyStartY + (index * bodyLineSpacing));
                    SetEnumProperty(bodyHint, "YCoordinateAlign", "Bottom");
                    SetEnumProperty(bodyHint, "Alignment", "Center");
                    SetProperty(bodyHint, "FontSize", (int)IntroFontSize);
                    SetEnumProperty(bodyHint, "SyncSpeed", "Fast");
                    introHints.Add(bodyHint);
                }

                Debug($"Calling HintServiceMeow AddHint for {role.Name} ({duration:0.#}s, x={titleX:0.###}, titleY={titleY:0.###}, bodyStartY={bodyStartY:0.###}, spacing={bodyLineSpacing:0.###}, lines={bodyLines.Count}).");

                foreach (object hint in introHints)
                    addHintMethod.Invoke(display, new[] { hint });

                Debug($"Displayed MeowHint intro for {role.Name} to {player.Nickname}.");
                Timing.CallDelayed(duration, () => RemoveIntroHints(player, introHints));
            }
            catch (Exception exception)
            {
                Log.Error($"[FacilityJobs] Failed to show HintServiceMeow intro for {role.Name}: {exception}");
            }
        }

        private static List<string> WrapIntroBody(string body, int maxLength)
        {
            List<string> lines = new List<string>();

            foreach (string paragraph in body.Replace("\r", string.Empty).Split('\n'))
            {
                string remaining = paragraph.Trim();
                if (remaining.Length == 0)
                    continue;

                while (remaining.Length > maxLength)
                {
                    int split = remaining.LastIndexOf(' ', maxLength);
                    if (split <= 0)
                        split = maxLength;

                    lines.Add(remaining.Substring(0, split).Trim());
                    remaining = remaining.Substring(split).TrimStart();
                }

                if (remaining.Length > 0)
                    lines.Add(remaining);
            }

            if (lines.Count == 0)
                lines.Add(string.Empty);

            return lines;
        }

        private static void RemoveIntroHints(Player player, List<object> introHints)
        {
            if (player == null || !player.IsConnected)
                return;

            try
            {
                if (!ResolveHintServiceMeow())
                    return;

                object display = GetPlayerDisplay(player);
                if (display == null)
                    return;

                foreach (object hint in introHints)
                {
                    if (hint != null)
                        removeHintMethod.Invoke(display, new[] { hint });
                }
            }
            catch
            {
                // Player may already be disconnected during cleanup.
            }
        }

        private static bool ResolveHintServiceMeow()
        {
            if (hsmResolved)
                return true;

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                hintType = assembly.GetType("HintServiceMeow.Core.Models.Hints.Hint", false);
                if (hintType != null)
                    break;
            }

            foreach (Assembly assembly in assemblies)
            {
                playerDisplayType = assembly.GetType("HintServiceMeow.Core.Utilities.PlayerDisplay", false);
                if (playerDisplayType != null)
                    break;
            }

            if (hintType == null || playerDisplayType == null)
                return false;

            playerDisplayFactoryMethod = FindPlayerDisplayFactory(assemblies);
            if (playerDisplayFactoryMethod == null)
                return false;

            addHintMethod = playerDisplayType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(method => method.Name == "AddHint" && method.GetParameters().Length == 1);
            removeHintMethod = playerDisplayType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(method => method.Name == "RemoveHint" && method.GetParameters().Length == 1);

            if (addHintMethod == null || removeHintMethod == null)
                return false;

            hsmResolved = true;
            Log.Info($"[FacilityJobs] HintServiceMeow HUD API detected through {playerDisplayFactoryMethod.DeclaringType?.FullName}.{playerDisplayFactoryMethod.Name}.");
            return true;
        }

        private static MethodInfo FindPlayerDisplayFactory(IEnumerable<Assembly> assemblies)
        {
            MethodInfo directMethod = playerDisplayType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(method =>
                    (method.Name == "Get" || method.Name == "GetPlayerDisplay") &&
                    method.GetParameters().Length == 1);

            if (directMethod != null)
                return directMethod;

            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in GetTypesSafe(assembly))
                {
                    if (type == null)
                        continue;

                    MethodInfo method = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .FirstOrDefault(candidate =>
                            (candidate.Name == "Get" || candidate.Name == "GetPlayerDisplay") &&
                            candidate.ReturnType == playerDisplayType &&
                            candidate.GetParameters().Length == 1);

                    if (method != null)
                        return method;
                }
            }

            return null;
        }

        private static IEnumerable<Type> GetTypesSafe(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                return exception.Types.Where(type => type != null);
            }
            catch
            {
                return Array.Empty<Type>();
            }
        }

        private static object GetPlayerDisplay(Player player)
        {
            if (playerDisplayFactoryMethod == null)
                return null;

            ParameterInfo parameter = playerDisplayFactoryMethod.GetParameters()[0];
            object argument = parameter.ParameterType == typeof(Player)
                ? player
                : player.ReferenceHub;

            return playerDisplayFactoryMethod.Invoke(null, new[] { argument });
        }

        private static void SetProperty(object target, string propertyName, object value)
        {
            PropertyInfo property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property == null || !property.CanWrite)
                return;

            property.SetValue(target, value);
        }

        private static void SetEnumProperty(object target, string propertyName, string value)
        {
            PropertyInfo property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property == null || !property.CanWrite || !property.PropertyType.IsEnum)
                return;

            property.SetValue(target, Enum.Parse(property.PropertyType, value, true));
        }

        private static bool IsStillRole(Player player, CustomRole role)
        {
            return player != null && player.IsConnected && role != null && role.Check(player);
        }

        private static bool TryGetSpawnPosition(CustomRole role, out Vector3 position)
        {
            if (role is ZoneManagerRole)
                return TryGetHeavyCorridor(out position);

            if (role is HausmeisterRole || role is CiAgentRole)
                return TryGetScientistSpawn(out position);

            if (role is SerpentsHandCustomRole)
                return TryGetSerpentsHandRoom(out position);

            position = Vector3.zero;
            return false;
        }

        private static bool TryGetScientistSpawn(out Vector3 position)
        {
            if (scientistSpawnPositions.Count > 0)
            {
                position = scientistSpawnPositions[UnityEngine.Random.Range(0, scientistSpawnPositions.Count)];
                return true;
            }

            List<Player> scientists = Player.List
                .Where(p => p != null && p.IsConnected && p.Role.Type == RoleTypeId.Scientist)
                .ToList();

            if (scientists.Count > 0)
            {
                position = scientists[UnityEngine.Random.Range(0, scientists.Count)].Position;
                return true;
            }

            List<Room> lightRooms = Room.List
                .Where(room => room != null && room.Zone == ZoneType.LightContainment)
                .ToList();

            if (lightRooms.Count > 0)
            {
                position = GetSafePosition(lightRooms[UnityEngine.Random.Range(0, lightRooms.Count)]);
                return true;
            }

            position = Vector3.zero;
            return false;
        }

        private static bool TryGetHeavyCorridor(out Vector3 position)
        {
            List<Room> corridors = Room.List
                .Where(room => room != null &&
                               room.Zone == ZoneType.HeavyContainment &&
                               IsCorridor(room.Type.ToString()))
                .ToList();

            if (corridors.Count == 0)
                corridors = Room.List.Where(room => room != null && room.Zone == ZoneType.HeavyContainment).ToList();

            if (corridors.Count > 0)
            {
                position = GetSafePosition(corridors[UnityEngine.Random.Range(0, corridors.Count)]);
                return true;
            }

            position = Vector3.zero;
            return false;
        }

        private static bool TryGetSerpentsHandRoom(out Vector3 position)
        {
            List<Room> rooms = Room.List
                .Where(room => room != null &&
                    (room.Type.ToString().IndexOf("Loading", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     room.Type.ToString().IndexOf("Collapsed", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     room.Type.ToString().IndexOf("Shelter", StringComparison.OrdinalIgnoreCase) >= 0))
                .ToList();

            if (rooms.Count > 0)
            {
                position = GetSafePosition(rooms[UnityEngine.Random.Range(0, rooms.Count)]);
                return true;
            }

            position = Vector3.zero;
            return false;
        }

        private static bool IsCorridor(string roomName)
        {
            return roomName.IndexOf("Straight", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   roomName.IndexOf("Curve", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   roomName.IndexOf("Corner", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   roomName.IndexOf("Cross", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   roomName.IndexOf("TCross", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   roomName.IndexOf("Hallway", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static Vector3 GetSafePosition(Room room) => room.Position + (Vector3.up * 1.2f);

        private static void Debug(string message)
        {
            if (Plugin.Instance?.Config?.Debug == true)
                Log.Debug($"[FacilityJobs] {message}");
        }
    }
}
