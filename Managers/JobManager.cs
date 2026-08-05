using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using FacilityJobs.Roles;
using PlayerRoles;

namespace FacilityJobs.Managers
{
    internal static class JobManager
    {
        public static void AssignRoundStartJobs()
        {
            try
            {
                if (!Round.IsStarted)
                    return;

                JobAssignmentManager.CaptureScientistSpawns();
                AssignZoneManager();
                AssignCiAgent();
                AssignHausmeister();
            }
            catch (Exception exception)
            {
                Log.Error($"[FacilityJobs] Failed to assign round-start jobs: {exception}");
            }
        }

        private static void AssignZoneManager()
        {
            if (!RoundState.ZoneManagerSelected || RoleRegistry.ZoneManager == null)
                return;

            List<Player> scientists = GetUnassignedScientists();
            if (scientists.Count == 0)
                return;

            Player player = TakeRandom(scientists);
            if (!JobAssignmentManager.Assign(player, RoleRegistry.ZoneManager, out string error))
            {
                Log.Warn($"[FacilityJobs] Zonenmanager assignment failed: {error}");
                return;
            }

            Debug($"Zonenmanager assigned to {player.Nickname}.");
        }

        private static void AssignCiAgent()
        {
            if (!RoundState.CiAgentSelected || RoleRegistry.CiAgent == null)
                return;

            List<Player> scientists = GetUnassignedScientists();
            if (scientists.Count == 0)
                return;

            Player player = TakeRandom(scientists);
            if (!JobAssignmentManager.Assign(player, RoleRegistry.CiAgent, out string error))
            {
                Log.Warn($"[FacilityJobs] CI Agent assignment failed: {error}");
                return;
            }

            Debug($"CI Agent assigned to {player.Nickname}.");
        }

        private static void AssignHausmeister()
        {
            if (RoleRegistry.Hausmeister == null)
                return;

            List<Player> classD = Player.List
                .Where(p => p != null &&
                            p.IsConnected &&
                            p.Role.Type == RoleTypeId.ClassD &&
                            !JobAssignmentManager.HasFacilityJob(p))
                .ToList();

            int desiredCount = Player.List.Count() >= Math.Max(1, Plugin.Instance.Config.PlayersForSecondHausmeister) ? 2 : 1;

            for (int index = 0; index < Math.Min(desiredCount, classD.Count); index++)
            {
                Player player = TakeRandom(classD);
                classD.Remove(player);

                if (!JobAssignmentManager.Assign(player, RoleRegistry.Hausmeister, out string error))
                {
                    Log.Warn($"[FacilityJobs] Hausmeister assignment failed: {error}");
                    continue;
                }

                Debug($"Hausmeister assigned to {player.Nickname}.");
            }
        }

        private static List<Player> GetUnassignedScientists() => Player.List
            .Where(p => p != null &&
                        p.IsConnected &&
                        p.Role.Type == RoleTypeId.Scientist &&
                        !JobAssignmentManager.HasFacilityJob(p))
            .ToList();

        private static T TakeRandom<T>(IReadOnlyList<T> values) =>
            values[UnityEngine.Random.Range(0, values.Count)];

        private static void Debug(string message)
        {
            if (Plugin.Instance.Config.Debug)
                Log.Debug($"[FacilityJobs] {message}");
        }
    }
}
