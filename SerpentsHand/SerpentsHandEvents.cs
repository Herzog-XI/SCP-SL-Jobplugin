using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using Player = Exiled.Events.Handlers.Player;
using Server = Exiled.Events.Handlers.Server;

namespace FacilityJobs.SerpentsHand
{
    internal sealed class SerpentsHandEvents
    {
        public void Register()
        {
            Player.Left += OnLeft;
            Server.RespawningTeam += OnRespawningTeam;
        }

        public void Unregister()
        {
            Player.Left -= OnLeft;
            Server.RespawningTeam -= OnRespawningTeam;
        }

        private static void OnLeft(LeftEventArgs ev)
        {
            SerpentsHandManager.Remove(ev.Player);
        }

        private static void OnRespawningTeam(RespawningTeamEventArgs ev)
        {
            if (!RoundState.SerpentsHandSelected || RoundState.SerpentsHandSpawned)
                return;

            RoundState.RespawnWaveCount++;

            if (Plugin.Instance.Config.Debug)
                Exiled.API.Features.Log.Debug($"[FacilityJobs] Normal respawn wave #{RoundState.RespawnWaveCount} detected.");

            if (RoundState.RespawnWaveCount == 2)
                Plugin.Instance.SerpentsHandSpawnManager.ScheduleAfterSecondWave();
        }
    }
}
