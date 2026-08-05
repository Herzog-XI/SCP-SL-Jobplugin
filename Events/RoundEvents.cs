using Exiled.API.Features;
using Server = Exiled.Events.Handlers.Server;

namespace FacilityJobs.Events
{
    internal sealed class RoundEvents
    {
        public void Register()
        {
            Server.WaitingForPlayers += OnWaitingForPlayers;
            Server.RoundStarted += OnRoundStarted;
            Server.RestartingRound += OnRestartingRound;
        }

        public void Unregister()
        {
            Server.WaitingForPlayers -= OnWaitingForPlayers;
            Server.RoundStarted -= OnRoundStarted;
            Server.RestartingRound -= OnRestartingRound;
        }

        private static void OnWaitingForPlayers()
        {
            RoundState.Reset();
        }

        private static void OnRoundStarted()
        {
            RoundState.Initialize();

            if (Plugin.Instance.Config.Debug)
                Log.Debug($"[FacilityJobs] Round initialized. Starting SCPs: {RoundState.StartingScpCount}.");
        }

        private static void OnRestartingRound()
        {
            RoundState.Reset();
        }
    }
}
