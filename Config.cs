using System.ComponentModel;
using Exiled.API.Interfaces;

namespace FacilityJobs
{
    public sealed class Config : IConfig
    {
        [Description("Whether the plugin is enabled.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Whether debug messages are written to the server console.")]
        public bool Debug { get; set; } = false;

        [Description("Chance in percent for a Zone Manager to spawn.")]
        public int ZoneManagerChance { get; set; } = 75;

        [Description("Chance in percent for a Chaos Insurgency Agent to spawn.")]
        public int CiAgentChance { get; set; } = 25;

        [Description("Chance in percent for the Serpent's Hand wave to be enabled for the round.")]
        public int SerpentsHandChance { get; set; } = 25;

        [Description("Player count required to spawn a second Hausmeister.")]
        public int PlayersForSecondHausmeister { get; set; } = 20;

        [Description("Minimum delay in seconds after the second normal respawn wave before Serpent's Hand spawns.")]
        public int SerpentsHandMinimumDelay { get; set; } = 60;

        [Description("Maximum delay in seconds after the second normal respawn wave before Serpent's Hand spawns.")]
        public int SerpentsHandMaximumDelay { get; set; } = 180;
    }
}
