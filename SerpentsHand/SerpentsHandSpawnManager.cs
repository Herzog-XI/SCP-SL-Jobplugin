using System;
using Exiled.API.Features;
using MEC;

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

            bool anyScpAlive = false;
            foreach (Player player in Player.List)
            {
                if (player != null && player.IsConnected && player.IsAlive && player.IsScp)
                {
                    anyScpAlive = true;
                    break;
                }
            }

            if (!anyScpAlive)
            {
                Debug("Serpent's Hand spawn cancelled because no SCP is alive.");
                return;
            }

            // The actual spectator selection, role distribution, spawn location and loadouts
            // are implemented in the next Serpent's Hand step.
            Debug($"Serpent's Hand spawn conditions passed. Planned wave size: {GetWaveSize()}.");
        }

        public static int GetWaveSize()
        {
            if (RoundState.StartingScpCount <= 0)
                return 0;

            return Math.Min(4, RoundState.StartingScpCount + 1);
        }

        private static void Debug(string message)
        {
            if (Plugin.Instance.Config.Debug)
                Log.Debug($"[FacilityJobs] {message}");
        }
    }
}
