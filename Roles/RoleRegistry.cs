using System;
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

        public static bool TryGet(string value, out CustomRole role)
        {
            role = null;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            string normalized = Normalize(value);
            role = registeredRoles.FirstOrDefault(r =>
                r.Id.ToString() == value ||
                Normalize(r.Name) == normalized ||
                Normalize(GetShortName(r)) == normalized);

            return role != null;
        }

        private static string GetShortName(CustomRole role)
        {
            if (role is ZoneManagerRole) return "zonenmanager";
            if (role is HausmeisterRole) return "hausmeister";
            if (role is CiAgentRole) return "ciagent";
            if (role is WardenRole) return "warden";
            if (role is WraithRole) return "wraith";
            if (role is SeekerRole) return "seeker";
            if (role is InfiltratorRole) return "infiltrator";
            return role.Name;
        }

        private static string Normalize(string value) =>
            new string(value.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());

        private static T Get<T>() where T : CustomRole
        {
            return registeredRoles.OfType<T>().FirstOrDefault();
        }
    }
}
