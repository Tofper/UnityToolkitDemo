namespace Scripts.Data
{
    /// <summary>
    /// Defines the types of rewards that can be granted.
    /// </summary>
    public enum ERewardType
    {
        Coins,
        Gems,
        Tokens,
        XP
    }

    /// <summary>
    /// Represents the data for a reward that can be granted.
    /// </summary>
    public class RewardData
    {
        /// <summary>The type of reward associated with this data.</summary>
        public ERewardType rewardType;
        /// <summary>The amount of the reward associated with this data.</summary>
        public int rewardAmount;

        public RewardData(ERewardType type, int amount)
        {
            rewardType = type;
            rewardAmount = amount;
        }
    }
}