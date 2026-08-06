using System;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using Exiled.Permissions.Extensions;
using FacilityJobs.Managers;
using FacilityJobs.Roles;
using RemoteAdmin;
using UnityEngine;

namespace FacilityJobs.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public sealed class JobCommand : ParentCommand
    {
        public JobCommand() => LoadGeneratedCommands();

        public override string Command { get; } = "job";
        public override string[] Aliases { get; } = { "jobs" };
        public override string Description { get; } = "FacilityJobs role management.";

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new JobListCommand());
            RegisterCommand(new JobSetCommand());
            RegisterCommand(new JobCoordsCommand());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "Usage: job list | job set <player> <job> | job coords";
            return false;
        }
    }

    internal sealed class JobListCommand : ICommand
    {
        public string Command { get; } = "list";
        public string[] Aliases { get; } = { "ls" };
        public string Description { get; } = "Lists all FacilityJobs roles.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("customroles.give"))
            {
                response = "Permission denied (customroles.give).";
                return false;
            }

            response = string.Join("\n", CustomRole.Registered
                .Where(r => r.Id >= 5101 && r.Id <= 5114)
                .OrderBy(r => r.Id)
                .Select(r => $"{r.Id} - {r.Name}"));
            return true;
        }
    }

    internal sealed class JobSetCommand : ICommand
    {
        public string Command { get; } = "set";
        public string[] Aliases { get; } = { "give" };
        public string Description { get; } = "Sets a FacilityJobs role with its correct spawn, inventory and class text.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("customroles.give"))
            {
                response = "Permission denied (customroles.give).";
                return false;
            }

            if (arguments.Count < 2)
            {
                response = "Usage: job set <player> <job name/id>";
                return false;
            }

            Player player = Player.Get(arguments.At(0));
            if (player == null)
            {
                response = $"Player '{arguments.At(0)}' not found.";
                return false;
            }

            string roleName = string.Join(" ", arguments.Skip(1));
            if (!RoleRegistry.TryGet(roleName, out CustomRole role))
            {
                response = $"Job '{roleName}' not found. Use 'job list'.";
                return false;
            }

            if (!JobAssignmentManager.Assign(player, role, out string error))
            {
                response = $"Could not set {role.Name}: {error}";
                return false;
            }

            response = $"{role.Name} set for {player.Nickname}.";
            return true;
        }
    }

    internal sealed class JobCoordsCommand : ICommand
    {
        public string Command { get; } = "coords";
        public string[] Aliases { get; } = { "position", "pos" };
        public string Description { get; } = "Shows your world and room-local coordinates for spawn setup.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("customroles.give"))
            {
                response = "Permission denied (customroles.give).";
                return false;
            }

            if (!(sender is PlayerCommandSender playerSender))
            {
                response = "This command must be executed from the in-game Remote Admin console.";
                return false;
            }

            Player player = Player.Get(playerSender);
            if (player == null || !player.IsConnected)
            {
                response = "Could not resolve your player.";
                return false;
            }

            Room room = player.CurrentRoom ?? Room.Get(player.Position);
            if (room == null)
            {
                response = $"No room detected. World: {Format(player.Position)}";
                return false;
            }

            Vector3 local = room.LocalPosition(player.Position);
            response =
                $"Room: {room.Type} ({room.Name})\n" +
                $"World: {Format(player.Position)}\n" +
                $"Local: {Format(local)}\n" +
                $"Room rotation Y: {room.Rotation.eulerAngles.y:0.###}\n" +
                $"Player rotation Y: {player.Rotation.y:0.###}";
            return true;
        }

        private static string Format(Vector3 position) =>
            $"X={position.x:0.###}, Y={position.y:0.###}, Z={position.z:0.###}";
    }
}
