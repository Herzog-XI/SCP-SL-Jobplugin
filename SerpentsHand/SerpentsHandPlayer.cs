using Exiled.API.Features;

namespace FacilityJobs.SerpentsHand
{
    internal sealed class SerpentsHandPlayer
    {
        public SerpentsHandPlayer(Player player, SerpentsHandRole role)
        {
            PlayerId = player.Id;
            UserId = player.UserId;
            Role = role;
        }

        public int PlayerId { get; }
        public string UserId { get; }
        public SerpentsHandRole Role { get; set; }

        public Player Player => Player.Get(PlayerId);
        public bool IsConnected => Player != null && Player.IsConnected;
    }
}
