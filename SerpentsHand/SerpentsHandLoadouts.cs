using Exiled.API.Enums;
using Exiled.API.Features;
using InventorySystem.Items;

namespace FacilityJobs.SerpentsHand
{
    internal static class SerpentsHandLoadouts
    {
        private const ushort BroadcastDuration = 15;
        private const string Motto = "<i>Der Wille der Schlange steht über der Foundation. Hilf den SCPs beim Sieg.</i>";

        public static void Apply(Player player, SerpentsHandRole role)
        {
            if (player == null)
                return;

            player.ClearInventory();
            player.AddItem(ItemType.KeycardChaosInsurgency);

            switch (role)
            {
                case SerpentsHandRole.Warden:
                    ApplyWarden(player);
                    break;

                case SerpentsHandRole.Wraith:
                    ApplyWraith(player);
                    break;

                case SerpentsHandRole.Seeker:
                    ApplySeeker(player);
                    break;

                case SerpentsHandRole.Infiltrator:
                    ApplyInfiltrator(player);
                    break;
            }
        }

        private static void ApplyWarden(Player player)
        {
            player.AddItem(ItemType.GunLogicer);
            player.AddItem(ItemType.ArmorHeavy);
            player.AddItem(ItemType.SCP500);
            player.AddItem(ItemType.Painkillers);
            player.AddAmmo(AmmoType.Nato762, 100);
            ShowRole(player, "WARDEN",
                "Du bist die Speerspitze der Serpent's Hand. Führe deine Verbündeten an, sichere wichtige Bereiche und unterstütze die SCPs im Kampf gegen die Foundation.");
        }

        private static void ApplyWraith(Player player)
        {
            player.AddItem(ItemType.GunRevolver);
            player.AddItem(ItemType.ArmorCombat);
            player.AddItem(ItemType.SCP2176);
            player.AddItem(ItemType.GrenadeFlash);
            player.AddItem(ItemType.Painkillers);
            player.AddItem(ItemType.Adrenaline);
            player.AddAmmo(AmmoType.Ammo44Cal, 18);
            ShowRole(player, "WRAITH",
                "Du bewegst dich lautlos durch die Anlage und nutzt Spezialausrüstung, um den Feind zu überraschen. Unterstütze die SCPs dort, wo sie dich am meisten brauchen.");
        }

        private static void ApplySeeker(Player player)
        {
            player.AddItem(ItemType.GunShotgun);
            player.AddItem(ItemType.ArmorLight);
            player.AddItem(ItemType.SCP207);
            player.AddItem(ItemType.Medkit);
            player.AddItem(ItemType.Painkillers);
            player.AddAmmo(AmmoType.Ammo12Gauge, 16);
            ShowRole(player, "SEEKER",
                "Du jagst die Feinde der Serpent's Hand und schaltest Bedrohungen für die SCPs aus. Nutze deine Geschwindigkeit und Ausrüstung, um den Verlauf der Runde zu beeinflussen.");
        }

        private static void ApplyInfiltrator(Player player)
        {
            player.AddItem(ItemType.GunA7);
            player.AddItem(ItemType.ArmorCombat);
            player.AddItem(ItemType.SCP1853);
            player.AddItem(ItemType.GrenadeHE);
            player.AddItem(ItemType.Medkit);
            player.AddAmmo(AmmoType.Nato762, 60);
            ShowRole(player, "INFILTRATOR",
                "Du bist ein vielseitiger Kämpfer der Serpent's Hand. Nutze deine Ausrüstung, um Schwachstellen auszunutzen und die SCPs bei ihrem Vormarsch zu unterstützen.");
        }

        private static void ShowRole(Player player, string roleName, string description)
        {
            player.CustomInfo = $"Serpent's Hand — {roleName}";
            player.Broadcast(
                BroadcastDuration,
                $"<color=#4FA45B><b>SERPENT'S HAND — {roleName}</b></color>\n" +
                $"<color=#FFFFFF>{description}</color>\n" +
                $"<color=#8BCF8B>{Motto}</color>");
        }
    }
}
