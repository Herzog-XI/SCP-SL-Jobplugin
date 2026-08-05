using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using Player = Exiled.Events.Handlers.Player;

namespace FacilityJobs.Events
{
    internal sealed class CiAgentEvents
    {
        public void Register()
        {
            Player.Hurting += OnHurting;
            Player.Escaping += OnEscaping;
            Player.Left += OnLeft;
        }

        public void Unregister()
        {
            Player.Hurting -= OnHurting;
            Player.Escaping -= OnEscaping;
            Player.Left -= OnLeft;
        }

        private static void OnHurting(HurtingEventArgs ev)
        {
            if (ev == null || ev.Player == null || ev.Attacker == null)
                return;

            bool victimIsAgent = RoundState.IsCiAgent(ev.Player);
            bool attackerIsAgent = RoundState.IsCiAgent(ev.Attacker);

            if (!victimIsAgent && !attackerIsAgent)
                return;

            bool victimIsChaos = ev.Player.Role.Team == Team.ChaosInsurgency;
            bool attackerIsChaos = ev.Attacker.Role.Team == Team.ChaosInsurgency;

            if ((victimIsAgent && attackerIsChaos) || (attackerIsAgent && victimIsChaos))
                ev.IsAllowed = false;
        }

        private static void OnEscaping(EscapingEventArgs ev)
        {
            if (ev == null || !RoundState.IsCiAgent(ev.Player))
                return;

            ev.NewRole = RoleTypeId.ChaosConscript;
            ev.Player.CustomInfo = string.Empty;
            RoundState.CiAgentUserId = null;

            if (Plugin.Instance.Config.Debug)
                Log.Debug($"[FacilityJobs] CI Agent {ev.Player.Nickname} escaped and became Chaos Conscript.");
        }

        private static void OnLeft(LeftEventArgs ev)
        {
            if (ev?.Player == null || !RoundState.IsCiAgent(ev.Player))
                return;

            RoundState.CiAgentUserId = null;
        }
    }
}
