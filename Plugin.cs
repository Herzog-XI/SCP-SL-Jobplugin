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
        public override Version Version => new Version(0, 2, 0);
        public override Version RequiredExiledVersion => new Version(9, 14, 2);

        private RoundEvents roundEvents;
        private CiAgentEvents ciAgentEvents;

        public override void OnEnabled()
        {
            Instance = this;

            roundEvents = new RoundEvents();
            ciAgentEvents = new CiAgentEvents();

            roundEvents.Register();
            ciAgentEvents.Register();

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            ciAgentEvents?.Unregister();
            roundEvents?.Unregister();

            ciAgentEvents = null;
            roundEvents = null;
            Instance = null;

            base.OnDisabled();
        }
    }
}
