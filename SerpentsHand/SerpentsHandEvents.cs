using Exiled.Events.EventArgs.Player;
using Player = Exiled.Events.Handlers.Player;

namespace FacilityJobs.SerpentsHand
{
    internal sealed class SerpentsHandEvents
    {
        public void Register()
        {
            Player.Left += OnLeft;
        }

        public void Unregister()
        {
            Player.Left -= OnLeft;
        }

        private static void OnLeft(LeftEventArgs ev)
        {
            SerpentsHandManager.Remove(ev.Player);
        }
    }
}
