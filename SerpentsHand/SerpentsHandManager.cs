using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;

namespace FacilityJobs.SerpentsHand
{
    internal static class SerpentsHandManager
    {
        private static readonly Dictionary<int, SerpentsHandPlayer> Members = new Dictionary<int, SerpentsHandPlayer>();

        public static IReadOnlyCollection<SerpentsHandPlayer> ActiveMembers => Members.Values.ToList().AsReadOnly();

        public static void Register(Player player, SerpentsHandRole role)
        {
            if (player == null)
                return;

            Members[player.Id] = new SerpentsHandPlayer(player, role);
        }

        public static bool IsMember(Player player)
        {
            return player != null && Members.ContainsKey(player.Id);
        }

        public static bool TryGet(Player player, out SerpentsHandPlayer member)
        {
            member = null;
            return player != null && Members.TryGetValue(player.Id, out member);
        }

        public static SerpentsHandRole GetRole(Player player)
        {
            return TryGet(player, out SerpentsHandPlayer member)
                ? member.Role
                : SerpentsHandRole.None;
        }

        public static void Remove(Player player)
        {
            if (player != null)
                Members.Remove(player.Id);
        }

        public static void RemoveDisconnected()
        {
            foreach (int playerId in Members
                         .Where(pair => !pair.Value.IsConnected)
                         .Select(pair => pair.Key)
                         .ToList())
            {
                Members.Remove(playerId);
            }
        }

        public static void Reset()
        {
            Members.Clear();
        }
    }
}
