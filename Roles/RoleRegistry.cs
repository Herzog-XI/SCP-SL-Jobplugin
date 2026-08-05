using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Exiled.API.Features;
using Exiled.CustomRoles.API;
using Exiled.CustomRoles.API.Features;

namespace FacilityJobs.Roles
{
    internal static class RoleRegistry
    {
        private static readonly List<CustomRole> registeredRoles = new List<CustomRole>();

        public static ZoneManagerRole ZoneManager => Get<ZoneManagerRole>();
        public static HausmeisterRole Hausmeister => Get<HausmeisterRole>();
        public static CiAgentRole CiAgent => Get<CiAgentRole>();
        public static WardenRole Warden => Get<WardenRole>();
        public static WraithRole Wraith => Get<WraithRole>();
        public static SeekerRole Seeker => Get<SeekerRole>();
        public static InfiltratorRole Infiltrator => Get<InfiltratorRole>();

        public static void Register()
        {
            registeredRoles.Clear();
            registeredRoles.AddRange(CustomRole.RegisterRoles(
                skipReflection: true,
                overrideClass: null,
                inheritAttributes: true,
                assembly: Assembly.GetExecutingAssembly()));

            Log.Info($"[FacilityJobs] Registered {registeredRoles.Count} custom roles.");
        }

        public static void Unregister()
        {
            registeredRoles.Unregister();
            registeredRoles.Clear();
        }

        private static T Get<T>() where T : CustomRole
        {
            return registeredRoles.OfType<T>().FirstOrDefault();
        }
    }
}
