using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using FacilityJobs.Roles;
using PlayerRoles;
using Player = Exiled.Events.Handlers.Player;

namespace FacilityJobs.Events
{
    internal sealed class CiAgentEvents
    {
        public void Register()
        {
            Player.Escaping += OnEscaping;
            Player.Left += OnLeft;
        }

        public void Unregister()
        {
            Player.Escaping -= OnEscaping;
            Player.Left -= OnLeft;
        }

        private static bool IsCiAgent(Exiled.API.Features.Player player)
        {
            return player != null && RoleRegistry.CiAgent != null && RoleRegistry.CiAgent.Check(player);
        }

        private static void OnEscaping(EscapingEventArgs ev)
        {
            if (ev == null || !IsCiAgent(ev.Player))
                return;

            RoleRegistry.CiAgent.RemoveRole(ev.Player);
            ev.NewRole = RoleTypeId.ChaosConscript;
            ev.Player.CustomInfo = string.Empty;
            RoundState.CiAgentUserId = null;

            if (Plugin.Instance.Config.Debug)
                Log.Debug($"[FacilityJobs] CI Agent {ev.Player.Nickname} escaped and became Chaos Conscript.");
        }

        private static void OnLeft(LeftEventArgs ev)
        {
            if (ev?.Player == null || !IsCiAgent(ev.Player))
                return;

            RoundState.CiAgentUserId = null;
        }
    }
}
