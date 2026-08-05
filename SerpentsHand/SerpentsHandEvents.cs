using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp096;
using Exiled.Events.EventArgs.Server;
using Player = Exiled.Events.Handlers.Player;
using Scp096 = Exiled.Events.Handlers.Scp096;
using Server = Exiled.Events.Handlers.Server;

namespace FacilityJobs.SerpentsHand
{
    internal sealed class SerpentsHandEvents
    {
        public void Register()
        {
            Player.Left += OnLeft;
            Server.RespawningTeam += OnRespawningTeam;
            Scp096.AddingTarget += OnAddingScp096Target;
        }

        public void Unregister()
        {
            Player.Left -= OnLeft;
            Server.RespawningTeam -= OnRespawningTeam;
            Scp096.AddingTarget -= OnAddingScp096Target;
        }

        private static void OnLeft(LeftEventArgs ev)
        {
            SerpentsHandManager.Remove(ev.Player);
        }

        private static void OnAddingScp096Target(AddingTargetEventArgs ev)
        {
            if (ev.Target != null && SerpentsHandManager.IsMember(ev.Target))
                ev.IsAllowed = false;
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
