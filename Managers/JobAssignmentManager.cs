using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using FacilityJobs.Roles;
using MEC;
using PlayerRoles;
using UnityEngine;

namespace FacilityJobs.Managers
{
    internal static class JobAssignmentManager
    {
        private static readonly List<Vector3> scientistSpawnPositions = new List<Vector3>();

        public static void CaptureScientistSpawns()
        {
            scientistSpawnPositions.Clear();

            foreach (Player player in Player.List.Where(p => p != null && p.IsConnected && p.Role.Type == RoleTypeId.Scientist))
            {
                if (!scientistSpawnPositions.Any(position => Vector3.Distance(position, player.Position) < 0.5f))
                    scientistSpawnPositions.Add(player.Position);
            }

            Debug($"Captured {scientistSpawnPositions.Count} Scientist spawn position(s).");
        }

        public static bool Assign(Player player, CustomRole role, out string error)
        {
            error = null;
            if (player == null || !player.IsConnected)
            {
                error = "Player is not connected.";
                return false;
            }

            if (role == null)
            {
                error = "Job is not registered.";
                return false;
            }

            Vector3 targetPosition;
            if (!TryGetSpawnPosition(role, out targetPosition))
            {
                error = $"No valid spawn position found for {role.Name}.";
                return false;
            }

            RoleTypeId requiredRole = role.Role;
            bool requiresRoleChange = requiredRole != RoleTypeId.None && player.Role.Type != requiredRole;

            role.AddRole(player);

            float delay = requiresRoleChange ? 0.45f : 0.1f;
            Timing.CallDelayed(delay, () => FinalizeAssignment(player, role, targetPosition));
            return true;
        }

        private static void FinalizeAssignment(Player player, CustomRole role, Vector3 targetPosition)
        {
            if (player == null || !player.IsConnected || role == null || !role.Check(player))
                return;

            player.Position = targetPosition;
            player.CustomInfo = role.CustomInfo ?? string.Empty;

            // The custom-role description is the role/class text. Show it immediately as
            // well, so players receive the correct information at the moment of assignment.
            if (!string.IsNullOrWhiteSpace(role.Description))
                player.ShowHint(role.Description, 12f);

            if (role is CiAgentRole)
                RoundState.CiAgentUserId = player.UserId;

            Debug($"Finalized {role.Name} for {player.Nickname} at {targetPosition}.");
        }

        private static bool TryGetSpawnPosition(CustomRole role, out Vector3 position)
        {
            if (role is ZoneManagerRole)
                return TryGetHeavyCorridor(out position);

            if (role is HausmeisterRole || role is CiAgentRole)
                return TryGetScientistSpawn(out position);

            // SH wave spawns are managed as one formation by SerpentsHandSpawnManager.
            // For manual administration, place a single SH member in a configured SH room.
            if (role is SerpentsHandCustomRole)
                return TryGetSerpentsHandRoom(out position);

            position = Vector3.zero;
            return false;
        }

        private static bool TryGetScientistSpawn(out Vector3 position)
        {
            if (scientistSpawnPositions.Count > 0)
            {
                position = scientistSpawnPositions[UnityEngine.Random.Range(0, scientistSpawnPositions.Count)];
                return true;
            }

            List<Player> scientists = Player.List
                .Where(p => p != null && p.IsConnected && p.Role.Type == RoleTypeId.Scientist)
                .ToList();

            if (scientists.Count > 0)
            {
                position = scientists[UnityEngine.Random.Range(0, scientists.Count)].Position;
                return true;
            }

            List<Room> lightRooms = Room.List
                .Where(room => room != null && room.Zone == ZoneType.LightContainment)
                .ToList();

            if (lightRooms.Count > 0)
            {
                position = GetSafePosition(lightRooms[UnityEngine.Random.Range(0, lightRooms.Count)]);
                return true;
            }

            position = Vector3.zero;
            return false;
        }

        private static bool TryGetHeavyCorridor(out Vector3 position)
        {
            List<Room> corridors = Room.List
                .Where(room => room != null &&
                               room.Zone == ZoneType.HeavyContainment &&
                               IsCorridor(room.Type.ToString()))
                .ToList();

            if (corridors.Count == 0)
                corridors = Room.List.Where(room => room != null && room.Zone == ZoneType.HeavyContainment).ToList();

            if (corridors.Count > 0)
            {
                position = GetSafePosition(corridors[UnityEngine.Random.Range(0, corridors.Count)]);
                return true;
            }

            position = Vector3.zero;
            return false;
        }

        private static bool TryGetSerpentsHandRoom(out Vector3 position)
        {
            List<Room> rooms = Room.List
                .Where(room => room != null &&
                    (room.Type.ToString().IndexOf("Loading", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     room.Type.ToString().IndexOf("Collapsed", StringComparison.OrdinalIgnoreCase) >= 0))
                .ToList();

            if (rooms.Count > 0)
            {
                position = GetSafePosition(rooms[UnityEngine.Random.Range(0, rooms.Count)]);
                return true;
            }

            position = Vector3.zero;
            return false;
        }

        private static bool IsCorridor(string roomName)
        {
            return roomName.IndexOf("Straight", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   roomName.IndexOf("Curve", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   roomName.IndexOf("Corner", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   roomName.IndexOf("Cross", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   roomName.IndexOf("TCross", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   roomName.IndexOf("Hallway", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static Vector3 GetSafePosition(Room room) => room.Position + (Vector3.up * 1.2f);

        private static void Debug(string message)
        {
            if (Plugin.Instance?.Config?.Debug == true)
                Log.Debug($"[FacilityJobs] {message}");
        }
    }
}
