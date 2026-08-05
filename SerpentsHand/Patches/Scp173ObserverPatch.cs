using Exiled.API.Features;
using HarmonyLib;
using PlayerRoles.PlayableScps.Scp173;

namespace FacilityJobs.SerpentsHand.Patches
{
    [HarmonyPatch(typeof(Scp173ObserversTracker), nameof(Scp173ObserversTracker.UpdateObserver))]
    internal static class Scp173ObserverPatch
    {
        private static bool Prefix(Scp173ObserversTracker __instance, ReferenceHub targetHub, ref int __result)
        {
            Player player = Player.Get(targetHub);
            if (player == null || !SerpentsHandManager.IsMember(player))
                return true;

            __result = __instance.Observers.Remove(targetHub) ? -1 : 0;
            return false;
        }
    }
}
