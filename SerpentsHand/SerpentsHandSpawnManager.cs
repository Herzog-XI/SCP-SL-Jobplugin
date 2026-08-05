using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using MEC;
using PlayerRoles;
using UnityEngine;

namespace FacilityJobs.SerpentsHand
{
    internal sealed class SerpentsHandSpawnManager
    {
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

            if (!Player.List.Any(player => player != null && player.IsConnected && player.IsAlive && player.IsScp))
            {
                Debug("Serpent's Hand spawn cancelled because no SCP is alive.");
                return;
            }

            List<Player> spectators = Player.List
                .Where(player => player != null && player.IsConnected && player.Role.Type == RoleTypeId.Spectator)
                .OrderBy(_ => UnityEngine.Random.value)
                .ToList();

            int requestedSize = GetWaveSize();
            int actualSize = Math.Min(requestedSize, spectators.Count);

            if (actualSize <= 0)
            {
                Debug("Serpent's Hand spawn cancelled because no spectators were available.");
                return;
            }

            Room spawnRoom = FindSpawnRoom();
            if (spawnRoom == null)
            {
                Log.Warn("[FacilityJobs] Serpent's Hand could not spawn because neither Loading Dock nor Collapsed Room was found.");
                return;
            }

            List<SerpentsHandRole> roles = BuildRoleList(actualSize);
            Vector3 basePosition = spawnRoom.Position + (Vector3.up * 1.2f);

            for (int index = 0; index < actualSize; index++)
            {
                Player player = spectators[index];
                SerpentsHandRole role = roles[index];
                Vector3 offset = GetFormationOffset(index);

                player.Role.Set(RoleTypeId.Tutorial);
                SerpentsHandManager.Register(player, role);

                Timing.CallDelayed(0.35f, () =>
                {
                    if (player == null || !player.IsConnected || !SerpentsHandManager.IsMember(player))
                        return;

                    player.Position = basePosition + offset;
                    SerpentsHandLoadouts.Apply(player, role);
                });
            }

            RoundState.SerpentsHandSpawned = true;
            Debug($"Serpent's Hand spawned {actualSize}/{requestedSize} players in {spawnRoom.Type}: {string.Join(", ", roles)}.");
        }

        public static int GetWaveSize()
        {
            if (RoundState.StartingScpCount <= 0)
                return 0;

            return Math.Min(4, RoundState.StartingScpCount + 1);
        }

        private static List<SerpentsHandRole> BuildRoleList(int count)
        {
            List<SerpentsHandRole> result = new List<SerpentsHandRole>
            {
                SerpentsHandRole.Warden,
            };

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

        private static Room FindSpawnRoom()
        {
            List<Room> candidates = Room.List
                .Where(room => room != null && IsConfiguredSpawnRoom(room.Type.ToString()))
                .ToList();

            if (candidates.Count == 0)
                return null;

            List<Room> loadingDock = candidates
                .Where(room => room.Type.ToString().IndexOf("Loading", StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
            List<Room> collapsed = candidates
                .Where(room => room.Type.ToString().IndexOf("Collapsed", StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            bool chooseLoadingDock = UnityEngine.Random.Range(0, 2) == 0;
            List<Room> preferred = chooseLoadingDock ? loadingDock : collapsed;
            List<Room> fallback = chooseLoadingDock ? collapsed : loadingDock;

            if (preferred.Count > 0)
                return preferred[UnityEngine.Random.Range(0, preferred.Count)];

            return fallback.Count > 0 ? fallback[UnityEngine.Random.Range(0, fallback.Count)] : null;
        }

        private static bool IsConfiguredSpawnRoom(string roomTypeName)
        {
            return roomTypeName.IndexOf("Loading", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   roomTypeName.IndexOf("Collapsed", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static Vector3 GetFormationOffset(int index)
        {
            switch (index)
            {
                case 1:
                    return new Vector3(1.25f, 0f, 0f);
                case 2:
                    return new Vector3(-1.25f, 0f, 0f);
                case 3:
                    return new Vector3(0f, 0f, 1.25f);
                default:
                    return Vector3.zero;
            }
        }

        private static void Debug(string message)
        {
            if (Plugin.Instance.Config.Debug)
                Log.Debug($"[FacilityJobs] {message}");
        }
    }
}
