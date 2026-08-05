using System;
using Exiled.API.Features;
using FacilityJobs.Events;

namespace FacilityJobs
{
    public sealed class Plugin : Plugin<Config>
    {
        public static Plugin Instance { get; private set; }

        public override string Name => "FacilityJobs";
        public override string Author => "Herzog-XI";
        public override Version Version => new Version(0, 1, 0);
        public override Version RequiredExiledVersion => new Version(9, 14, 2);

        private RoundEvents roundEvents;

        public override void OnEnabled()
        {
            Instance = this;
            roundEvents = new RoundEvents();
            roundEvents.Register();

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            roundEvents?.Unregister();
            roundEvents = null;
            Instance = null;

            base.OnDisabled();
        }
    }
}
