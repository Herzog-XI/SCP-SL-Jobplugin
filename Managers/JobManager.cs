using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using InventorySystem.Items;
using PlayerRoles;
using UnityEngine;

namespace FacilityJobs.Managers
{
    internal static class JobManager
    {
        private const ushort RoleBroadcastDuration = 12;

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
            if (!RoundState.ZoneManagerSelected)
                return;

            List<Player> scientists = GetUnassignedScientists();
            if (scientists.Count == 0)
            {
                Debug("Zone Manager was selected, but no Scientist was available.");
                return;
            }

            Player player = TakeRandom(scientists);
            Room room = GetRandomRoom(ZoneType.HeavyContainment);

            if (room == null)
            {
                Log.Warn("[FacilityJobs] Zone Manager could not spawn because no Heavy Containment room was found.");
                return;
            }

            player.ClearInventory();
            player.AddItem(ItemType.KeycardZoneManager);
            player.AddItem(ItemType.Flashlight);
            player.Position = GetSafeRoomPosition(room);
            player.CustomInfo = "Zonenmanager";
            player.Broadcast(
                RoleBroadcastDuration,
                "<color=#5B8CFF><b>ZONENMANAGER</b></color>\n" +
                "<color=#FFFFFF>Du bist für den Betrieb der Heavy Containment Zone verantwortlich. " +
                "Nutze deine Zugriffsrechte, unterstütze die Foundation und finde einen Weg aus der Anlage.</color>");

            Debug($"Zone Manager assigned to {player.Nickname} in {room.Type}.");
        }

        private static void AssignCiAgent()
        {
            if (!RoundState.CiAgentSelected)
                return;

            List<Player> scientists = GetUnassignedScientists();
            if (scientists.Count == 0)
            {
                Debug("CI Agent was selected, but no unassigned Scientist was available.");
                return;
            }

            Player player = TakeRandom(scientists);
            player.ClearInventory();
            player.AddItem(ItemType.KeycardScientist);
            RoundState.CiAgentUserId = player.UserId;

            player.Broadcast(
                RoleBroadcastDuration,
                "<color=#4FA45B><b>CHAOS INSURGENCY AGENT</b></color>\n" +
                "<color=#FFFFFF>Du bist als Wissenschaftler in die Foundation eingeschleust. " +
                "Bewahre deine Tarnung und entscheide selbst, wann du dich zu erkennen gibst. " +
                "Gelingt dir die Flucht, kehrst du zur Chaos Insurgency zurück.</color>");

            Debug($"CI Agent assigned to {player.Nickname}.");
        }

        private static void AssignHausmeister()
        {
            List<Player> classD = Player.List
                .Where(player => player != null && player.IsConnected && player.Role.Type == RoleTypeId.ClassD)
                .ToList();

            if (classD.Count == 0)
            {
                Debug("No D-Class players were available for the Hausmeister job.");
                return;
            }

            int configuredThreshold = Math.Max(1, Plugin.Instance.Config.PlayersForSecondHausmeister);
            int desiredCount = Player.List.Count() >= configuredThreshold ? 2 : 1;
            int count = Math.Min(desiredCount, classD.Count);

            for (int index = 0; index < count; index++)
            {
                Player player = TakeRandom(classD);
                classD.Remove(player);

                Room room = GetRandomRoom(ZoneType.LightContainment);
                if (room == null)
                {
                    Log.Warn("[FacilityJobs] Hausmeister could not spawn because no Light Containment room was found.");
                    return;
                }

                player.ClearInventory();
                player.AddItem(ItemType.KeycardJanitor);
                player.Position = GetSafeRoomPosition(room);
                player.CustomInfo = "Hausmeister";
                player.Broadcast(
                    RoleBroadcastDuration,
                    "<color=#D6B35A><b>HAUSMEISTER</b></color>\n" +
                    "<color=#FFFFFF>Du bist für die Reinigung und Instandhaltung der Light Containment Zone verantwortlich. " +
                    "Der Ausbruch hat dich während deiner Arbeit überrascht. Nutze deine Zugriffsrechte und versuche zu entkommen.</color>");

                Debug($"Hausmeister assigned to {player.Nickname} in {room.Type}.");
            }
        }

        private static List<Player> GetUnassignedScientists()
        {
            return Player.List
                .Where(player => player != null &&
                                 player.IsConnected &&
                                 player.Role.Type == RoleTypeId.Scientist &&
                                 string.IsNullOrEmpty(player.CustomInfo))
                .ToList();
        }

        private static Room GetRandomRoom(ZoneType zone)
        {
            List<Room> rooms = Room.List
                .Where(room => room != null && room.Zone == zone)
                .ToList();

            return rooms.Count == 0 ? null : TakeRandom(rooms);
        }

        private static Vector3 GetSafeRoomPosition(Room room)
        {
            return room.Position + (Vector3.up * 1.2f);
        }

        private static T TakeRandom<T>(IReadOnlyList<T> values)
        {
            return values[UnityEngine.Random.Range(0, values.Count)];
        }

        private static void Debug(string message)
        {
            if (Plugin.Instance.Config.Debug)
                Log.Debug($"[FacilityJobs] {message}");
        }
    }
}
