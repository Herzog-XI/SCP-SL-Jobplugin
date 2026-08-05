using System;
using System.Linq;
using Exiled.API.Features;

namespace FacilityJobs
{
    internal static class RoundState
    {
        private static readonly Random Random = new Random();

        public static int StartingScpCount { get; private set; }
        public static bool ZoneManagerSelected { get; private set; }
        public static bool CiAgentSelected { get; private set; }
        public static bool SerpentsHandSelected { get; private set; }
        public static bool SerpentsHandSpawned { get; set; }
        public static int RespawnWaveCount { get; set; }
        public static string CiAgentUserId { get; set; }

        public static bool IsCiAgent(Player player)
        {
            return player != null &&
                   player.IsConnected &&
                   !string.IsNullOrEmpty(CiAgentUserId) &&
                   string.Equals(player.UserId, CiAgentUserId, StringComparison.Ordinal);
        }

        public static void Initialize()
        {
            Config config = Plugin.Instance.Config;

            StartingScpCount = Player.List.Count(player => player != null && player.IsConnected && player.IsScp);
            ZoneManagerSelected = Roll(config.ZoneManagerChance);
            CiAgentSelected = Roll(config.CiAgentChance);
            SerpentsHandSelected = Roll(config.SerpentsHandChance);
            SerpentsHandSpawned = false;
            RespawnWaveCount = 0;
            CiAgentUserId = null;

            if (config.Debug)
            {
                Log.Debug($"[FacilityJobs] Zone Manager selected: {ZoneManagerSelected}.");
                Log.Debug($"[FacilityJobs] CI Agent selected: {CiAgentSelected}.");
                Log.Debug($"[FacilityJobs] Serpent's Hand selected: {SerpentsHandSelected}.");
            }
        }

        public static void Reset()
        {
            StartingScpCount = 0;
            ZoneManagerSelected = false;
            CiAgentSelected = false;
            SerpentsHandSelected = false;
            SerpentsHandSpawned = false;
            RespawnWaveCount = 0;
            CiAgentUserId = null;
        }

        private static bool Roll(int chance)
        {
            int clampedChance = Math.Max(0, Math.Min(100, chance));
            return Random.Next(0, 100) < clampedChance;
        }
    }
}
