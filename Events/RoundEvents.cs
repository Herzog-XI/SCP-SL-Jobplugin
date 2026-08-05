using Exiled.API.Features;
using FacilityJobs.Managers;
using FacilityJobs.SerpentsHand;
using MEC;
using Server = Exiled.Events.Handlers.Server;

namespace FacilityJobs.Events
{
    internal sealed class RoundEvents
    {
        private CoroutineHandle assignmentCoroutine;

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

            CancelRoundWork();
        }

        private void OnWaitingForPlayers()
        {
            CancelRoundWork();
            SerpentsHandManager.Reset();
            RoundState.Reset();
        }

        private void OnRoundStarted()
        {
            RoundState.Initialize();
            SerpentsHandManager.Reset();

            if (Plugin.Instance.Config.Debug)
                Log.Debug($"[FacilityJobs] Round initialized. Starting SCPs: {RoundState.StartingScpCount}.");

            CancelAssignment();
            assignmentCoroutine = Timing.CallDelayed(1.5f, JobManager.AssignRoundStartJobs);
        }

        private void OnRestartingRound()
        {
            CancelRoundWork();
            SerpentsHandManager.Reset();
            RoundState.Reset();
        }

        private void CancelRoundWork()
        {
            CancelAssignment();
            Plugin.Instance?.SerpentsHandSpawnManager?.Cancel();
        }

        private void CancelAssignment()
        {
            if (assignmentCoroutine.IsRunning)
                Timing.KillCoroutines(assignmentCoroutine);

            assignmentCoroutine = default;
        }
    }
}
