using System;
using Exiled.API.Features;
using FacilityJobs.Events;
using FacilityJobs.SerpentsHand;

namespace FacilityJobs
{
    public sealed class Plugin : Plugin<Config>
    {
        public static Plugin Instance { get; private set; }

        public override string Name => "FacilityJobs";
        public override string Author => "Herzog-XI";
        public override Version Version => new Version(0, 4, 0);
        public override Version RequiredExiledVersion => new Version(9, 14, 2);

        internal SerpentsHandSpawnManager SerpentsHandSpawnManager { get; private set; }

        private RoundEvents roundEvents;
        private CiAgentEvents ciAgentEvents;
        private SerpentsHandEvents serpentsHandEvents;

        public override void OnEnabled()
        {
            Instance = this;

            SerpentsHandSpawnManager = new SerpentsHandSpawnManager();
            roundEvents = new RoundEvents();
            ciAgentEvents = new CiAgentEvents();
            serpentsHandEvents = new SerpentsHandEvents();

            roundEvents.Register();
            ciAgentEvents.Register();
            serpentsHandEvents.Register();

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            SerpentsHandSpawnManager?.Cancel();
            SerpentsHandManager.Reset();

            serpentsHandEvents?.Unregister();
            ciAgentEvents?.Unregister();
            roundEvents?.Unregister();

            serpentsHandEvents = null;
            ciAgentEvents = null;
            roundEvents = null;
            SerpentsHandSpawnManager = null;
            Instance = null;

            base.OnDisabled();
        }
    }
}
