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
    }

    [CustomRole(RoleTypeId.Scientist)]
    internal sealed class ZoneManagerRole : FacilityCustomRole
    {
        public override uint Id { get; set; } = 5101;
        public override string Name { get; set; } = "Zonenmanager";
        public override string Description { get; set; } = "Du bist für den Betrieb der Heavy Containment Zone verantwortlich. Nutze deine Zugriffsrechte, unterstütze die Foundation und finde einen Weg aus der Anlage.";
        public override string CustomInfo { get; set; } = "Zonenmanager";
        public override RoleTypeId Role { get; set; } = RoleTypeId.Scientist;
        public override List<string> Inventory { get; set; } = new List<string> { "KeycardZoneManager", "Flashlight" };
        public override Broadcast Broadcast { get; set; } = new Broadcast("<color=#5B8CFF><b>ZONENMANAGER</b></color>\nDu bist für den Betrieb der Heavy Containment Zone verantwortlich. Nutze deine Zugriffsrechte, unterstütze die Foundation und finde einen Weg aus der Anlage.", 12);
    }

    [CustomRole(RoleTypeId.ClassD)]
    internal sealed class HausmeisterRole : FacilityCustomRole
    {
        public override uint Id { get; set; } = 5102;
        public override string Name { get; set; } = "Hausmeister";
        public override string Description { get; set; } = "Du bist für die Reinigung und Instandhaltung der Light Containment Zone verantwortlich. Der Ausbruch hat dich während deiner Arbeit überrascht. Nutze deine Zugriffsrechte und versuche zu entkommen.";
        public override string CustomInfo { get; set; } = "Hausmeister";
        public override RoleTypeId Role { get; set; } = RoleTypeId.ClassD;
        public override List<string> Inventory { get; set; } = new List<string> { "KeycardJanitor" };
        public override Broadcast Broadcast { get; set; } = new Broadcast("<color=#D6B35A><b>HAUSMEISTER</b></color>\nDu bist für die Reinigung und Instandhaltung der Light Containment Zone verantwortlich. Der Ausbruch hat dich während deiner Arbeit überrascht. Nutze deine Zugriffsrechte und versuche zu entkommen.", 12);
    }

    [CustomRole(RoleTypeId.Scientist)]
    internal sealed class CiAgentRole : FacilityCustomRole
    {
        public override uint Id { get; set; } = 5103;
        public override string Name { get; set; } = "Chaos Insurgency Agent";
        public override string Description { get; set; } = "Du bist als Wissenschaftler in die Foundation eingeschleust. Bewahre deine Tarnung und entscheide selbst, wann du dich zu erkennen gibst. Gelingt dir die Flucht, kehrst du zur Chaos Insurgency zurück.";
        public override string CustomInfo { get; set; } = string.Empty;
        public override RoleTypeId Role { get; set; } = RoleTypeId.Scientist;
        public override List<string> Inventory { get; set; } = new List<string> { "KeycardScientist" };
        public override Broadcast Broadcast { get; set; } = new Broadcast("<color=#4FA45B><b>CHAOS INSURGENCY AGENT</b></color>\nDu bist als Wissenschaftler in die Foundation eingeschleust. Bewahre deine Tarnung und entscheide selbst, wann du dich zu erkennen gibst. Gelingt dir die Flucht, kehrst du zur Chaos Insurgency zurück.", 15);
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
        public override string Description { get; set; } = "Du bist die Speerspitze der Serpent's Hand. Führe deine Verbündeten an, sichere wichtige Bereiche und unterstütze die SCPs im Kampf gegen die Foundation. " + Motto;
        public override string CustomInfo { get; set; } = "Serpent's Hand — Warden";
        public override List<string> Inventory { get; set; } = new List<string> { "KeycardChaosInsurgency", "GunLogicer", "ArmorHeavy", "SCP500", "Painkillers" };
        public override Dictionary<AmmoType, ushort> Ammo { get; set; } = new Dictionary<AmmoType, ushort> { [AmmoType.Nato762] = 100 };
        public override Broadcast Broadcast { get; set; } = new Broadcast("<color=#4FA45B><b>SERPENT'S HAND — WARDEN</b></color>\nDu bist die Speerspitze der Serpent's Hand. Führe deine Verbündeten an, sichere wichtige Bereiche und unterstütze die SCPs im Kampf gegen die Foundation.\n<color=#8BCF8B><i>" + Motto + "</i></color>", 15);
    }

    [CustomRole(RoleTypeId.Tutorial)]
    internal sealed class WraithRole : SerpentsHandCustomRole
    {
        public override uint Id { get; set; } = 5112;
        public override string Name { get; set; } = "Serpent's Hand Wraith";
        public override string Description { get; set; } = "Du bewegst dich lautlos durch die Anlage und nutzt Spezialausrüstung, um den Feind zu überraschen. Unterstütze die SCPs dort, wo sie dich am meisten brauchen. " + Motto;
        public override string CustomInfo { get; set; } = "Serpent's Hand — Wraith";
        public override List<string> Inventory { get; set; } = new List<string> { "KeycardChaosInsurgency", "GunRevolver", "ArmorCombat", "SCP2176", "GrenadeFlash", "Painkillers", "Adrenaline" };
        public override Dictionary<AmmoType, ushort> Ammo { get; set; } = new Dictionary<AmmoType, ushort> { [AmmoType.Ammo44Cal] = 18 };
        public override Broadcast Broadcast { get; set; } = new Broadcast("<color=#4FA45B><b>SERPENT'S HAND — WRAITH</b></color>\nDu bewegst dich lautlos durch die Anlage und nutzt Spezialausrüstung, um den Feind zu überraschen. Unterstütze die SCPs dort, wo sie dich am meisten brauchen.\n<color=#8BCF8B><i>" + Motto + "</i></color>", 15);
    }

    [CustomRole(RoleTypeId.Tutorial)]
    internal sealed class SeekerRole : SerpentsHandCustomRole
    {
        public override uint Id { get; set; } = 5113;
        public override string Name { get; set; } = "Serpent's Hand Seeker";
        public override string Description { get; set; } = "Du jagst die Feinde der Serpent's Hand und schaltest Bedrohungen für die SCPs aus. Nutze deine Geschwindigkeit und Ausrüstung, um den Verlauf der Runde zu beeinflussen. " + Motto;
        public override string CustomInfo { get; set; } = "Serpent's Hand — Seeker";
        public override List<string> Inventory { get; set; } = new List<string> { "KeycardChaosInsurgency", "GunShotgun", "ArmorLight", "SCP207", "Medkit", "Painkillers" };
        public override Dictionary<AmmoType, ushort> Ammo { get; set; } = new Dictionary<AmmoType, ushort> { [AmmoType.Ammo12Gauge] = 16 };
        public override Broadcast Broadcast { get; set; } = new Broadcast("<color=#4FA45B><b>SERPENT'S HAND — SEEKER</b></color>\nDu jagst die Feinde der Serpent's Hand und schaltest Bedrohungen für die SCPs aus. Nutze deine Geschwindigkeit und Ausrüstung, um den Verlauf der Runde zu beeinflussen.\n<color=#8BCF8B><i>" + Motto + "</i></color>", 15);
    }

    [CustomRole(RoleTypeId.Tutorial)]
    internal sealed class InfiltratorRole : SerpentsHandCustomRole
    {
        public override uint Id { get; set; } = 5114;
        public override string Name { get; set; } = "Serpent's Hand Infiltrator";
        public override string Description { get; set; } = "Du bist ein vielseitiger Kämpfer der Serpent's Hand. Nutze deine Ausrüstung, um Schwachstellen auszunutzen und die SCPs bei ihrem Vormarsch zu unterstützen. " + Motto;
        public override string CustomInfo { get; set; } = "Serpent's Hand — Infiltrator";
        public override List<string> Inventory { get; set; } = new List<string> { "KeycardChaosInsurgency", "GunA7", "ArmorCombat", "SCP1853", "GrenadeHE", "Medkit" };
        public override Dictionary<AmmoType, ushort> Ammo { get; set; } = new Dictionary<AmmoType, ushort> { [AmmoType.Nato762] = 60 };
        public override Broadcast Broadcast { get; set; } = new Broadcast("<color=#4FA45B><b>SERPENT'S HAND — INFILTRATOR</b></color>\nDu bist ein vielseitiger Kämpfer der Serpent's Hand. Nutze deine Ausrüstung, um Schwachstellen auszunutzen und die SCPs bei ihrem Vormarsch zu unterstützen.\n<color=#8BCF8B><i>" + Motto + "</i></color>", 15);
    }
}
