using Scripts.Data;

namespace Scripts.Utilities
{

    /// <summary>
    /// Defines the types of icons used for rewards.
    /// </summary>
    public enum EIconType
    {
        Star,
        Crown,
        Gem,
        Lock
    }

    /// <summary>
    /// Utility class for mapping reward types to icons and retrieving corresponding emoji strings.
    /// </summary>
    public static class RewardIconUtils
    {
        // Emoji Constants
        private const string STAR_EMOJI = "⭐";
        private const string CROWN_EMOJI = "👑";
        private const string GEM_EMOJI = "💎";
        private const string LOCK_EMOJI = "🔒";
        private const string UNKNOWN_EMOJI = "❓";

        /// <summary>
        /// Gets the appropriate icon type for a given reward type.
        /// </summary>
        /// <param name="rewardType">The type of the reward.</param>
        /// <returns>The corresponding IconType.</returns>
        public static EIconType GetIconTypeForReward(ERewardType rewardType)
        {
            switch (rewardType)
            {
                case ERewardType.Coins:
                case ERewardType.XP:
                    return EIconType.Star;
                case ERewardType.Gems:
                    return EIconType.Gem;
                case ERewardType.Tokens:
                    return EIconType.Crown;
                default:
                    UnityEngine.Debug.LogWarning($"{nameof(RewardIconUtils)}: No icon mapping found for reward type: {rewardType}. Returning Star icon.");
                    return EIconType.Star;
            }
        }

        /// <summary>
        /// Gets the emoji string representation for a given icon type.
        /// </summary>
        /// <param name="iconType">The type of the icon.</param>
        /// <returns>The emoji string.</returns>
        public static string GetEmojiForIconType(EIconType iconType)
        {
            switch (iconType)
            {
                case EIconType.Star:
                    return STAR_EMOJI;
                case EIconType.Crown:
                    return CROWN_EMOJI;
                case EIconType.Gem:
                    return GEM_EMOJI;
                case EIconType.Lock:
                    return LOCK_EMOJI;
                default:
                    UnityEngine.Debug.LogWarning($"{nameof(RewardIconUtils)}: No emoji mapping found for icon type: {iconType}. Returning unknown emoji.");
                    return UNKNOWN_EMOJI;
            }
        }
    }
}
