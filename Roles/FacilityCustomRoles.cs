using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.CustomRoles.API.Features;
using PlayerRoles;

namespace FacilityJobs.Roles
{
    internal abstract class FacilityCustomRole : CustomRole
    {
        public override int MaxHealth { get; set; } = 100;
        public override string CustomInfo { get; set; }
        public override bool IgnoreSpawnSystem { get; set; } = true;
        public override bool KeepPositionOnSpawn { get; set; } = true;
        public override bool KeepInventoryOnSpawn { get; set; } = false;
        public override bool RemovalKillsPlayer { get; set; } = false;
        public override float SpawnChance { get; set; } = 0f;
        public override string ConsoleMessage { get; set; } = string.Empty;
        public override bool DisplayCustomItemMessages { get; set; } = false;

        public override void AddRole(Player player)
        {
            if (player != null && Role != RoleTypeId.None && player.Role.Type == Role)
            {
                RoleTypeId baseRole = Role;
                try
                {
                    Role = RoleTypeId.None;
                    base.AddRole(player);
                }
                finally
                {
                    Role = baseRole;
                }

                return;
            }

            base.AddRole(player);
        }
    }

    [CustomRole(RoleTypeId.Scientist)]
    internal sealed class ZoneManagerRole : FacilityCustomRole
    {
        public override uint Id { get; set; } = 5101;
        public override string Name { get; set; } = "Zonenmanager";
        public override string Description { get; set; } =
            "<color=#5B8CFF><b>ZONENMANAGER</b></color>\n\n" +
            "Die Heavy Containment Zone war dein Verantwortungsbereich – bis die Anlage außer Kontrolle geriet.\n\n" +
            "<color=#D8E4FF>• Nutze deine erweiterten Zugriffsrechte.\n" +
            "• Unterstütze die Foundation bei der Evakuierung.\n" +
            "• Verlasse die Anlage lebend.</color>";
        public override string CustomInfo { get; set; } = "Zonenmanager";
        public override RoleTypeId Role { get; set; } = RoleTypeId.Scientist;
        public override List<string> Inventory { get; set; } = new List<string> { "KeycardZoneManager", "Flashlight" };
    }

    [CustomRole(RoleTypeId.ClassD)]
    internal sealed class HausmeisterRole : FacilityCustomRole
    {
        public override uint Id { get; set; } = 5102;
        public override string Name { get; set; } = "Hausmeister";
        public override string Description { get; set; } =
            "<color=#D6B35A><b>HAUSMEISTER</b></color>\n\n" +
            "Eigentlich sollte es nur eine gewöhnliche Schicht in der Light Containment Zone werden. Jetzt zählt nur noch dein Überleben.\n\n" +
            "<color=#F1DEAA>• Nutze deine Hausmeisterkarte und deine Ortskenntnis.\n" +
            "• Halte dich aus unnötigen Kämpfen heraus.\n" +
            "• Finde einen Weg aus der Anlage.</color>";
        public override string CustomInfo { get; set; } = "Hausmeister";
        public override RoleTypeId Role { get; set; } = RoleTypeId.ClassD;
        public override List<string> Inventory { get; set; } = new List<string> { "KeycardJanitor" };
    }

    [CustomRole(RoleTypeId.Scientist)]
    internal sealed class CiAgentRole : FacilityCustomRole
    {
        public override uint Id { get; set; } = 5103;
        public override string Name { get; set; } = "Chaos Insurgency Agent";
        public override string Description { get; set; } =
            "<color=#4FA45B><b>CHAOS INSURGENCY AGENT</b></color>\n\n" +
            "Niemand in der Foundation kennt deine wahre Loyalität. Du wurdest als Wissenschaftler eingeschleust und wartest auf den richtigen Moment.\n\n" +
            "<color=#A8DDB0>• Bewahre deine Tarnung.\n" +
            "• Unterstütze die Chaos Insurgency, ohne dich früh zu verraten.\n" +
            "• Entkomme und kehre als Chaos-Soldat zurück.</color>";
        public override string CustomInfo { get; set; } = "Wissenschaftler";
        public override RoleTypeId Role { get; set; } = RoleTypeId.Scientist;
        public override List<string> Inventory { get; set; } = new List<string> { "KeycardScientist" };
    }

    internal abstract class SerpentsHandCustomRole : FacilityCustomRole
    {
        protected const string Motto = "Der Wille der Schlange steht über der Foundation. Hilf den SCPs beim Sieg.";
        public override RoleTypeId Role { get; set; } = RoleTypeId.Tutorial;
        public override bool KeepRoleOnChangingRole { get; set; } = false;
    }

    [CustomRole(RoleTypeId.Tutorial)]
    internal sealed class WardenRole : SerpentsHandCustomRole
    {
        public override uint Id { get; set; } = 5111;
        public override string Name { get; set; } = "Serpent's Hand Warden";
        public override string Description { get; set; } = "<color=#4FA45B><b>SERPENT'S HAND — WARDEN</b></color>\n\nDu führst die Vorhut. Wo du vorrückst, soll die Kontrolle der Foundation zusammenbrechen.\n\n<color=#A8DDB0>• Führe deine Verbündeten.\n• Sichere wichtige Bereiche.\n• Unterstütze die SCPs im direkten Kampf.</color>\n\n<color=#79C987><i>" + Motto + "</i></color>";
        public override string CustomInfo { get; set; } = "Serpent's Hand — Warden";
        public override List<string> Inventory { get; set; } = new List<string> { "KeycardChaosInsurgency", "GunLogicer", "ArmorHeavy", "SCP500", "Painkillers" };
        public override Dictionary<AmmoType, ushort> Ammo { get; set; } = new Dictionary<AmmoType, ushort> { [AmmoType.Nato762] = 100 };
    }

    [CustomRole(RoleTypeId.Tutorial)]
    internal sealed class WraithRole : SerpentsHandCustomRole
    {
        public override uint Id { get; set; } = 5112;
        public override string Name { get; set; } = "Serpent's Hand Wraith";
        public override string Description { get; set; } = "<color=#4FA45B><b>SERPENT'S HAND — WRAITH</b></color>\n\nDu bist der Schatten zwischen den Korridoren. Deine Feinde sollen dich erst bemerken, wenn es bereits zu spät ist.\n\n<color=#A8DDB0>• Bewege dich unauffällig.\n• Überrasche isolierte Gegner.\n• Unterstütze die SCPs dort, wo der Feind verwundbar ist.</color>\n\n<color=#79C987><i>" + Motto + "</i></color>";
        public override string CustomInfo { get; set; } = "Serpent's Hand — Wraith";
        public override List<string> Inventory { get; set; } = new List<string> { "KeycardChaosInsurgency", "GunRevolver", "ArmorCombat", "SCP2176", "GrenadeFlash", "Painkillers", "Adrenaline" };
        public override Dictionary<AmmoType, ushort> Ammo { get; set; } = new Dictionary<AmmoType, ushort> { [AmmoType.Ammo44Cal] = 18 };
    }

    [CustomRole(RoleTypeId.Tutorial)]
    internal sealed class SeekerRole : SerpentsHandCustomRole
    {
        public override uint Id { get; set; } = 5113;
        public override string Name { get; set; } = "Serpent's Hand Seeker";
        public override string Description { get; set; } = "<color=#4FA45B><b>SERPENT'S HAND — SEEKER</b></color>\n\nDu bist der Jäger. Niemand, der den SCPs gefährlich werden könnte, soll der Anlage entkommen.\n\n<color=#A8DDB0>• Verfolge fliehende Gegner.\n• Kontrolliere Engpässe und Fluchtwege.\n• Entferne Bedrohungen für die SCPs.</color>\n\n<color=#79C987><i>" + Motto + "</i></color>";
        public override string CustomInfo { get; set; } = "Serpent's Hand — Seeker";
        public override List<string> Inventory { get; set; } = new List<string> { "KeycardChaosInsurgency", "GunShotgun", "ArmorLight", "SCP207", "Medkit", "Painkillers" };
        public override Dictionary<AmmoType, ushort> Ammo { get; set; } = new Dictionary<AmmoType, ushort> { [AmmoType.Ammo12Gauge] = 16 };
    }

    [CustomRole(RoleTypeId.Tutorial)]
    internal sealed class InfiltratorRole : SerpentsHandCustomRole
    {
        public override uint Id { get; set; } = 5114;
        public override string Name { get; set; } = "Serpent's Hand Infiltrator";
        public override string Description { get; set; } = "<color=#4FA45B><b>SERPENT'S HAND — INFILTRATOR</b></color>\n\nDu bist der vielseitige Kämpfer der Einheit. Passe dich der Lage an und brich dort durch, wo die Foundation Schwäche zeigt.\n\n<color=#A8DDB0>• Kämpfe flexibel.\n• Nutze Ausrüstung und Gelände zu deinem Vorteil.\n• Unterstütze die SCPs bei ihrem Vormarsch.</color>\n\n<color=#79C987><i>" + Motto + "</i></color>";
        public override string CustomInfo { get; set; } = "Serpent's Hand — Infiltrator";
        public override List<string> Inventory { get; set; } = new List<string> { "KeycardChaosInsurgency", "GunA7", "ArmorCombat", "SCP1853", "GrenadeHE", "Medkit" };
        public override Dictionary<AmmoType, ushort> Ammo { get; set; } = new Dictionary<AmmoType, ushort> { [AmmoType.Nato762] = 60 };
    }
}
