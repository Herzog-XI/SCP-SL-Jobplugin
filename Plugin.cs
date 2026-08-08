using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using FacilityJobs.Events;
using FacilityJobs.Roles;
using FacilityJobs.SerpentsHand;
using HarmonyLib;
using MEC;
using Player = Exiled.Events.Handlers.Player;

namespace FacilityJobs
{
    public sealed class Plugin : Plugin<Config>
    {
        public static Plugin Instance { get; private set; }

        public override string Name => "FacilityJobs";
        public override string Author => "Herzog-XI";
        public override Version Version => new Version(0, 6, 0);
        public override Version RequiredExiledVersion => new Version(9, 14, 2);

        internal SerpentsHandSpawnManager SerpentsHandSpawnManager { get; private set; }

        private RoundEvents roundEvents;
        private CiAgentEvents ciAgentEvents;
        private SerpentsHandEvents serpentsHandEvents;
        private Harmony harmony;
        private JobIntroHints jobIntroHints;

        public override void OnEnabled()
        {
            Instance = this;
            RoleRegistry.Register();

            harmony = new Harmony($"facilityjobs.{DateTime.UtcNow.Ticks}");
            harmony.PatchAll();

            SerpentsHandSpawnManager = new SerpentsHandSpawnManager();
            roundEvents = new RoundEvents();
            ciAgentEvents = new CiAgentEvents();
            serpentsHandEvents = new SerpentsHandEvents();
            jobIntroHints = new JobIntroHints();

            roundEvents.Register();
            ciAgentEvents.Register();
            serpentsHandEvents.Register();
            jobIntroHints.Register();

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            SerpentsHandSpawnManager?.Cancel();
            SerpentsHandManager.Reset();

            jobIntroHints?.Unregister();
            serpentsHandEvents?.Unregister();
            ciAgentEvents?.Unregister();
            roundEvents?.Unregister();

            harmony?.UnpatchAll(harmony.Id);
            harmony = null;
            RoleRegistry.Unregister();

            jobIntroHints = null;
            serpentsHandEvents = null;
            ciAgentEvents = null;
            roundEvents = null;
            SerpentsHandSpawnManager = null;
            Instance = null;

            base.OnDisabled();
        }

        private sealed class JobIntroHints
        {
            private const float IntroYCoordinate = 520f;
            private const float HausmeisterDuration = 7f;
            private const float ZoneManagerDuration = 7f;
            private const float CiAgentDuration = 10f;
            private const float SerpentsHandDuration = 10f;

            private Type hintType;
            private Type playerDisplayType;
            private MethodInfo playerDisplayFactory;
            private MethodInfo addHintMethod;
            private MethodInfo removeHintMethod;
            private bool resolved;
            private bool warningShown;

            public void Register()
            {
                Player.Spawned += OnSpawned;
            }

            public void Unregister()
            {
                Player.Spawned -= OnSpawned;
            }

            private void OnSpawned(SpawnedEventArgs ev)
            {
                if (ev?.Player == null)
                    return;

                Timing.CallDelayed(0.75f, () => ShowForFacilityJob(ev.Player));
            }

            private void ShowForFacilityJob(Exiled.API.Features.Player player)
            {
                if (player == null || !player.IsConnected)
                    return;

                FacilityCustomRole role = CustomRole.Registered
                    .OfType<FacilityCustomRole>()
                    .FirstOrDefault(item => item.Check(player));

                if (role == null)
                    return;

                float duration = role is CiAgentRole
                    ? CiAgentDuration
                    : role is SerpentsHandCustomRole
                        ? SerpentsHandDuration
                        : role is ZoneManagerRole
                            ? ZoneManagerDuration
                            : HausmeisterDuration;

                if (!ResolveHintServiceMeow())
                {
                    if (!warningShown)
                    {
                        warningShown = true;
                        Log.Warn("[FacilityJobs] HintServiceMeow API was not found; job intro cannot be displayed.");
                    }
                    return;
                }

                try
                {
                    object hint = Activator.CreateInstance(hintType);
                    SetProperty(hint, "Id", "facility_jobs_intro");
                    SetProperty(hint, "Text", $"<size=34><b><color={role.IntroTitleColor}>Du bist ein {role.IntroTitle}.</color></b></size>\n<size=24><color=#FFFFFF>{role.IntroBody ?? string.Empty}</color></size>");
                    SetProperty(hint, "YCoordinate", IntroYCoordinate);
                    SetEnumProperty(hint, "Alignment", "Center");
                    SetProperty(hint, "FontSize", 24);

                    object display = GetPlayerDisplay(player);
                    if (display == null)
                        throw new InvalidOperationException("PlayerDisplay could not be resolved.");

                    addHintMethod.Invoke(display, new[] { hint });
                    Debug($"Job intro shown for {player.Nickname}: {role.Name}, {duration:0.#}s.");

                    Timing.CallDelayed(duration, () =>
                    {
                        try
                        {
                            if (player != null && player.IsConnected)
                            {
                                object currentDisplay = GetPlayerDisplay(player);
                                if (currentDisplay != null)
                                    removeHintMethod.Invoke(currentDisplay, new[] { hint });
                            }
                        }
                        catch (Exception exception)
                        {
                            Log.Debug($"[FacilityJobs] Could not remove job intro: {exception.Message}");
                        }
                    });
                }
                catch (Exception exception)
                {
                    Log.Error($"[FacilityJobs] Could not display job intro: {exception}");
                }
            }

            private bool ResolveHintServiceMeow()
            {
                if (resolved)
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

                playerDisplayFactory = playerDisplayType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(method =>
                        (method.Name == "Get" || method.Name == "GetPlayerDisplay") &&
                        method.GetParameters().Length == 1 &&
                        playerDisplayType.IsAssignableFrom(method.ReturnType));

                addHintMethod = playerDisplayType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(method => method.Name == "AddHint" && method.GetParameters().Length == 1);
                removeHintMethod = playerDisplayType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(method => method.Name == "RemoveHint" && method.GetParameters().Length == 1);

                if (playerDisplayFactory == null || addHintMethod == null || removeHintMethod == null)
                    return false;

                resolved = true;
                Debug($"HintServiceMeow resolved through {playerDisplayType.FullName}.");
                return true;
            }

            private object GetPlayerDisplay(Exiled.API.Features.Player player)
            {
                ParameterInfo parameter = playerDisplayFactory.GetParameters()[0];
                object argument = null;

                if (parameter.ParameterType.IsInstanceOfType(player))
                    argument = player;
                else if (parameter.ParameterType.IsInstanceOfType(player.ReferenceHub))
                    argument = player.ReferenceHub;

                if (argument == null)
                    throw new InvalidOperationException($"Cannot convert player to {parameter.ParameterType.FullName}.");

                return playerDisplayFactory.Invoke(null, new[] { argument });
            }

            private static void SetProperty(object target, string propertyName, object value)
            {
                PropertyInfo property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (property == null || !property.CanWrite)
                    return;

                Type targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                object converted = value;
                if (value != null && !targetType.IsInstanceOfType(value))
                    converted = Convert.ChangeType(value, targetType);

                property.SetValue(target, converted);
            }

            private static void SetEnumProperty(object target, string propertyName, string value)
            {
                PropertyInfo property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (property == null || !property.CanWrite || !property.PropertyType.IsEnum)
                    return;

                property.SetValue(target, Enum.Parse(property.PropertyType, value, true));
            }

            private static void Debug(string message)
            {
                if (Plugin.Instance?.Config?.Debug == true)
                    Log.Debug($"[FacilityJobs] {message}");
            }
        }
    }
}
