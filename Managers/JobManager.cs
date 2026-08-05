using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using FacilityJobs.Roles;
using PlayerRoles;
using UnityEngine;

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
            Room room = GetRandomRoom(ZoneType.HeavyContainment);
            if (room == null)
                return;

            RoleRegistry.ZoneManager.AddRole(player);
            player.Position = GetSafeRoomPosition(room);
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
            RoleRegistry.CiAgent.AddRole(player);
            RoundState.CiAgentUserId = player.UserId;
            Debug($"CI Agent assigned to {player.Nickname}.");
        }

        private static void AssignHausmeister()
        {
            if (RoleRegistry.Hausmeister == null)
                return;

            List<Player> classD = Player.List.Where(p => p != null && p.IsConnected && p.Role.Type == RoleTypeId.ClassD).ToList();
            int desiredCount = Player.List.Count() >= Math.Max(1, Plugin.Instance.Config.PlayersForSecondHausmeister) ? 2 : 1;

            for (int index = 0; index < Math.Min(desiredCount, classD.Count); index++)
            {
                Player player = TakeRandom(classD);
                classD.Remove(player);
                Room room = GetRandomRoom(ZoneType.LightContainment);
                if (room == null)
                    return;

                RoleRegistry.Hausmeister.AddRole(player);
                player.Position = GetSafeRoomPosition(room);
                Debug($"Hausmeister assigned to {player.Nickname}.");
            }
        }

        private static List<Player> GetUnassignedScientists() => Player.List
            .Where(p => p != null && p.IsConnected && p.Role.Type == RoleTypeId.Scientist && string.IsNullOrEmpty(p.CustomInfo))
            .ToList();

        private static Room GetRandomRoom(ZoneType zone)
        {
            List<Room> rooms = Room.List.Where(room => room != null && room.Zone == zone).ToList();
            return rooms.Count == 0 ? null : TakeRandom(rooms);
        }

        private static Vector3 GetSafeRoomPosition(Room room) => room.Position + (Vector3.up * 1.2f);
        private static T TakeRandom<T>(IReadOnlyList<T> values) => values[UnityEngine.Random.Range(0, values.Count)];

        private static void Debug(string message)
        {
            if (Plugin.Instance.Config.Debug)
                Log.Debug($"[FacilityJobs] {message}");
        }
    }
}
