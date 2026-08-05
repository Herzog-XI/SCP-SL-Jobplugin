using Exiled.API.Features;
using FacilityJobs.Managers;
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

            CancelAssignment();
        }

        private void OnWaitingForPlayers()
        {
            CancelAssignment();
            RoundState.Reset();
        }

        private void OnRoundStarted()
        {
            RoundState.Initialize();

            if (Plugin.Instance.Config.Debug)
                Log.Debug($"[FacilityJobs] Round initialized. Starting SCPs: {RoundState.StartingScpCount}.");

            CancelAssignment();
            assignmentCoroutine = Timing.CallDelayed(1.5f, JobManager.AssignRoundStartJobs);
        }

        private void OnRestartingRound()
        {
            CancelAssignment();
            RoundState.Reset();
        }

        private void CancelAssignment()
        {
            if (assignmentCoroutine.IsRunning)
                Timing.KillCoroutines(assignmentCoroutine);

            assignmentCoroutine = default;
        }
    }
}
