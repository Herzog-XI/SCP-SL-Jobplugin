using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using FacilityJobs.Managers;
using FacilityJobs.Roles;
using MEC;
using PlayerRoles;
using UnityEngine;

namespace FacilityJobs.SerpentsHand
{
    internal sealed class SerpentsHandSpawnManager
    {
        // Tested local room coordinates. Local coordinates remain valid even when the
        // procedural facility rotates or moves the room in another map seed.
        private static readonly Vector3 CollapsedTunnelLocalSpawn = new Vector3(0.005f, 0.96f, 4.961f);
        private static readonly Vector3 ShelterLocalSpawn = new Vector3(-0.406f, 0.96f, 4.969f);

        private CoroutineHandle pendingSpawn;
        public bool HasPendingSpawn => pendingSpawn.IsRunning;

        public void ScheduleAfterSecondWave()
        {
            Cancel();
            if (!RoundState.SerpentsHandSelected || RoundState.SerpentsHandSpawned)
                return;

            int minimum = Math.Max(0, Plugin.Instance.Config.SerpentsHandMinimumDelay);
            int maximum = Math.Max(minimum, Plugin.Instance.Config.SerpentsHandMaximumDelay);
            float delay = UnityEngine.Random.Range(minimum, maximum + 1);
            pendingSpawn = Timing.CallDelayed(delay, TrySpawnWave);
            Debug($"Serpent's Hand spawn scheduled in {delay:0} seconds.");
        }

        public void Cancel()
        {
            if (pendingSpawn.IsRunning)
                Timing.KillCoroutines(pendingSpawn);
            pendingSpawn = default;
        }

        private void TrySpawnWave()
        {
            pendingSpawn = default;
            if (!Round.IsStarted || !RoundState.SerpentsHandSelected || RoundState.SerpentsHandSpawned)
                return;

            if (!Player.List.Any(p => p != null && p.IsConnected && p.IsAlive && p.IsScp))
                return;

            List<Player> spectators = Player.List
                .Where(p => p != null && p.IsConnected && p.Role.Type == RoleTypeId.Spectator)
                .OrderBy(_ => UnityEngine.Random.value)
                .ToList();

            int requestedSize = GetWaveSize();
            int actualSize = Math.Min(requestedSize, spectators.Count);
            if (actualSize <= 0)
                return;

            if (!TryFindSpawn(out Room spawnRoom, out Vector3 localBasePosition))
                return;

            List<SerpentsHandRole> roles = BuildRoleList(actualSize);

            for (int index = 0; index < actualSize; index++)
            {
                Player player = spectators[index];
                SerpentsHandRole shRole = roles[index];
                Exiled.CustomRoles.API.Features.CustomRole customRole = GetCustomRole(shRole);
                Vector3 localPosition = localBasePosition + GetFormationOffset(index);
                Vector3 worldPosition = spawnRoom.WorldPosition(localPosition);

                customRole?.AddRole(player);
                SerpentsHandManager.Register(player, shRole);

                Timing.CallDelayed(0.35f, () =>
                {
                    if (player == null || !player.IsConnected || !SerpentsHandManager.IsMember(player))
                        return;

                    player.Position = worldPosition;
                    if (customRole != null)
                    {
                        JobAssignmentManager.ApplyVisibleJobTag(player, customRole);
                        JobAssignmentManager.ShowIntro(player, customRole);
                    }
                });
            }

            RoundState.SerpentsHandSpawned = true;
            Debug($"Serpent's Hand spawned {actualSize}/{requestedSize} players in {spawnRoom.Type} at local {localBasePosition}: {string.Join(", ", roles)}.");
        }

        private static Exiled.CustomRoles.API.Features.CustomRole GetCustomRole(SerpentsHandRole role)
        {
            switch (role)
            {
                case SerpentsHandRole.Warden: return RoleRegistry.Warden;
                case SerpentsHandRole.Wraith: return RoleRegistry.Wraith;
                case SerpentsHandRole.Seeker: return RoleRegistry.Seeker;
                case SerpentsHandRole.Infiltrator: return RoleRegistry.Infiltrator;
                default: return null;
            }
        }

        public static int GetWaveSize() => RoundState.StartingScpCount <= 0 ? 0 : Math.Min(4, RoundState.StartingScpCount + 1);

        private static List<SerpentsHandRole> BuildRoleList(int count)
        {
            List<SerpentsHandRole> result = new List<SerpentsHandRole> { SerpentsHandRole.Warden };
            List<SerpentsHandRole> remaining = new List<SerpentsHandRole>
            {
                SerpentsHandRole.Wraith,
                SerpentsHandRole.Seeker,
                SerpentsHandRole.Infiltrator,
            };

            while (result.Count < count && remaining.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, remaining.Count);
                result.Add(remaining[index]);
                remaining.RemoveAt(index);
            }

            return result;
        }

        private static bool TryFindSpawn(out Room room, out Vector3 localPosition)
        {
            List<Room> collapsedRooms = Room.List
                .Where(r => r != null && IsCollapsedTunnel(r))
                .ToList();

            List<Room> shelterRooms = Room.List
                .Where(r => r != null && IsShelter(r))
                .ToList();

            bool chooseCollapsed = UnityEngine.Random.Range(0, 2) == 0;
            List<Room> preferred = chooseCollapsed ? collapsedRooms : shelterRooms;
            List<Room> fallback = chooseCollapsed ? shelterRooms : collapsedRooms;

            List<Room> selectedPool = preferred.Count > 0 ? preferred : fallback;
            if (selectedPool.Count == 0)
            {
                room = null;
                localPosition = Vector3.zero;
                return false;
            }

            room = selectedPool[UnityEngine.Random.Range(0, selectedPool.Count)];
            localPosition = IsCollapsedTunnel(room) ? CollapsedTunnelLocalSpawn : ShelterLocalSpawn;
            return true;
        }

        private static bool IsCollapsedTunnel(Room room)
        {
            string type = room.Type.ToString();
            string name = room.Name ?? string.Empty;
            return type.IndexOf("CollapsedTunnel", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("CollapsedTunnel", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsShelter(Room room)
        {
            string type = room.Type.ToString();
            string name = room.Name ?? string.Empty;
            return type.IndexOf("Shelter", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("Shelter", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static Vector3 GetFormationOffset(int index)
        {
            switch (index)
            {
                case 1: return new Vector3(0.55f, 0f, 0f);
                case 2: return new Vector3(-0.55f, 0f, 0f);
                case 3: return new Vector3(0f, 0f, -0.55f);
                default: return Vector3.zero;
            }
        }

        private static void Debug(string message)
        {
            if (Plugin.Instance.Config.Debug)
                Log.Debug($"[FacilityJobs] {message}");
        }
    }
}
