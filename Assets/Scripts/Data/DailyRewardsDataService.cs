using System.Collections.Generic;
using Scripts.Data;
using UnityEngine;

/// <summary>
/// Represents the data for a single daily reward.
/// </summary>
public class RewardData
{
    /// <summary>The day number for this reward.</summary>
    public int day;
    /// <summary>The type of reward.</summary>
    public ERewardType rewardType;
    /// <summary>The amount of the reward.</summary>
    public int rewardAmount;

    public RewardData(ERewardType type, int amount)
    {
        rewardType = type;
        rewardAmount = amount;
    }
}

/// <summary>
/// Service responsible for providing and managing daily rewards data.
/// </summary>
namespace Scripts.Data
{
    public class DailyRewardsDataService
    {
        /// <summary>
        /// Gets the current day for daily rewards.
        /// </summary>
        public int CurrentDay { get; private set; } = 2; // Example: today is day 2

        // TODO: Implement data persistence for claimed rewards (e.g., using PlayerPrefs, saving to file, or backend).

        /// <summary>
        /// Retrieves the list of daily reward data.
        /// TODO: Load reward data from a persistent source instead of hardcoding.
        /// </summary>
        /// <returns>A list of RewardData objects.</returns>
        public List<RewardData> GetRewardData()
        {
            return new List<RewardData>
            {
                new RewardData(ERewardType.Coins, 100),
                new RewardData(ERewardType.Gems, 10),
                new RewardData(ERewardType.Tokens, 5),
                new RewardData(ERewardType.XP, 50)
            };
        }

        /// <summary>
        /// Attempts to claim the reward for a specific day.
        /// Grants the reward and marks it as claimed if it exists and hasn't been claimed already.
        /// </summary>
        /// <param name="day">The day number of the reward to claim.</param>
        /// <param name="rewards">The list of all daily rewards data.</param>
        /// <param name="currencyService">The CurrencyService to grant the reward.</param>
        public void ClaimReward(int day, List<CardData> rewards, CurrencyService currencyService)
        {
            if (currencyService == null)
            {
                Debug.LogError($"{nameof(DailyRewardsDataService)}: Cannot claim reward, CurrencyService is null.");
                return;
            }

            var rewardCardData = rewards.Find(r => r.day == day);

            if (rewardCardData == null)
            {
                Debug.LogWarning($"{nameof(DailyRewardsDataService)}: Attempted to claim reward for day {day}, but no matching reward data found.");
                return;
            }

            if (rewardCardData.claimed)
            {
                Debug.LogWarning($"{nameof(DailyRewardsDataService)}: Attempted to claim reward for day {day}, but it has already been claimed.");
                return;
            }

            // Get the reward details from the card's reward data
            var rewardDetails = rewardCardData.reward;

            if (rewardDetails == null)
            {
                Debug.LogError($"{nameof(DailyRewardsDataService)}: Could not find reward data for day {day} during claiming.");
                return;
            }

            // Grant reward to player via currencyService
            switch (rewardDetails.rewardType)
            {
                case ERewardType.Coins:
                    currencyService.AddCoins(rewardDetails.rewardAmount);
                    Debug.Log($"{nameof(DailyRewardsDataService)}: Claimed {rewardDetails.rewardAmount} Coins for day {day}.");
                    break;
                case ERewardType.Gems:
                    currencyService.AddGems(rewardDetails.rewardAmount);
                    Debug.Log($"{nameof(DailyRewardsDataService)}: Claimed {rewardDetails.rewardAmount} Gems for day {day}.");
                    break;
                case ERewardType.Tokens:
                    Debug.LogWarning($"{nameof(DailyRewardsDataService)}: Claimed {rewardDetails.rewardAmount} Tokens for day {day}. AddTokens not implemented in CurrencyService.");
                    break;
                case ERewardType.XP:
                    Debug.LogWarning($"{nameof(DailyRewardsDataService)}: Claimed {rewardDetails.rewardAmount} XP for day {day}. XP handling not implemented.");
                    break;
                default:
                    Debug.LogWarning($"{nameof(DailyRewardsDataService)}: Attempted to claim reward for day {day} with unhandled reward type: {rewardDetails.rewardType}");
                    break;
            }
            rewardCardData.claimed = true;
        }

        /// <summary>
        /// Rerolls the reward type and amount for all unclaimed rewards in the provided list.
        /// </summary>
        /// <param name="rewards">The list of daily rewards data (CardData) to reroll.</param>
        public void RerollUnclaimedRewards(List<CardData> rewards)
        {
            if (rewards == null)
            {
                Debug.LogWarning($"{nameof(DailyRewardsDataService)}: RerollUnclaimedRewards called with null rewards list.");
                return;
            }

            var random = new System.Random();
            var rewardTypes = (ERewardType[])System.Enum.GetValues(typeof(ERewardType));

            if (rewardTypes.Length == 0)
            {
                Debug.LogWarning($"{nameof(DailyRewardsDataService)}: No RewardTypes defined for rerolling.");
                return;
            }

            foreach (var reward in rewards)
            {
                if (reward != null && !reward.claimed)
                {
                    // Create new reward data with randomized values
                    ERewardType newType = (ERewardType)rewardTypes[random.Next(rewardTypes.Length)];
                    int newAmount;

                    // Randomize reward amount based on type
                    switch (newType)
                    {
                        case ERewardType.Coins:
                            newAmount = random.Next(50, 201); // 50-200 coins
                            break;
                        case ERewardType.Gems:
                            newAmount = random.Next(5, 21); // 5-20 gems
                            break;
                        case ERewardType.Tokens:
                            newAmount = random.Next(2, 11); // 2-10 tokens
                            break;
                        case ERewardType.XP:
                            newAmount = random.Next(25, 101); // 25-100 XP
                            break;
                        default:
                            Debug.LogWarning($"{nameof(DailyRewardsDataService)}: Rerolled to unhandled reward type: {newType}");
                            newAmount = 0;
                            break;
                    }

                    // Update the reward data
                    reward.reward = new RewardData(newType, newAmount);
                    Debug.Log($"{nameof(DailyRewardsDataService)}: Rerolled day {reward.day} to {newType} x{newAmount}");
                }
                else if (reward == null)
                {
                    Debug.LogWarning($"{nameof(DailyRewardsDataService)}: Encountered null reward in list during reroll.");
                }
            }
        }
    }
}

