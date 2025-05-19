using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Data
{
    /// <summary>
    /// ViewModel for the Daily Rewards screen.
    /// Manages the state and logic for displaying and interacting with daily reward cards.
    /// </summary>
    public class DailyRewardsViewModel
    {
        /// <summary>
        /// Gets the list of daily reward card data.
        /// </summary>
        public List<CardData> Cards { get; private set; }
        /// <summary>
        /// Gets the current number of rerolls used.
        /// </summary>
        public int RerollCount { get; private set; }
        /// <summary>
        /// Gets the maximum number of allowed rerolls.
        /// </summary>
        public int MaxRerolls { get; private set; }
        /// <summary>
        /// Gets the current day for daily rewards.
        /// </summary>
        public int CurrentDay { get; private set; }
        /// <summary>
        /// Event triggered when the state of the daily rewards changes.
        /// </summary>
        public event Action OnStateChangedEvent;

        // Currency Properties to expose to the View
        /// <summary>Gets the current number of coins.</summary>
        public int CurrentCoins { get; private set; }
        /// <summary>Gets the current number of gems.</summary>
        public int CurrentGems { get; private set; }

        private DailyRewardsDataService _dataService;
        private CurrencyService _currencyService;

        /// <summary>
        /// Initializes a new instance of the DailyRewardsViewModel.
        /// </summary>
        /// <param name="dataService">The data service for daily rewards.</param>
        /// <param name="currencyService">The currency service for granting rewards.</param>
        public DailyRewardsViewModel(DailyRewardsDataService dataService, CurrencyService currencyService)
        {
            if (dataService == null)
            {
                Debug.LogError($"{nameof(DailyRewardsViewModel)}: DailyRewardsDataService is null.");
            }
            if (currencyService == null)
            {
                Debug.LogError($"{nameof(DailyRewardsViewModel)}: CurrencyService is null.");
            }

            _dataService = dataService;
            _currencyService = currencyService;

            if (_currencyService != null)
            {
                _currencyService.OnCoinsChanged += HandleCoinsChanged;
                _currencyService.OnGemsChanged += HandleGemsChanged;
                CurrentCoins = _currencyService.Coins;
                CurrentGems = _currencyService.Gems;
                Debug.Log($"{nameof(DailyRewardsViewModel)}: Subscribed to CurrencyService events and initialized currency.");
            }
            else
            {
                Debug.LogWarning($"{nameof(DailyRewardsViewModel)}: CurrencyService is null, cannot subscribe to events or initialize currency.");
            }

            MaxRerolls = 3;
            RerollCount = 0;
            CurrentDay = _dataService.CurrentDay;
            Cards = new List<CardData>();
            List<RewardData> initialRewards = _dataService != null ? _dataService.GetRewardData() : new List<RewardData>();

            // Create cards for each day
            for (int day = 1; day <= initialRewards.Count; day++)
            {
                ECardType initialCardType = ECardType.Locked;
                bool initialClaimedState = false;

                if (day < CurrentDay)
                {
                    initialCardType = ECardType.Free;
                }
                else if (day == CurrentDay)
                {
                    initialCardType = ECardType.Free;
                }

                // Create a new reward data for this day
                var rewardData = initialRewards[day - 1];
                Cards.Add(new CardData(day, initialCardType, rewardData, initialClaimedState));
            }
            Debug.Log($"{nameof(DailyRewardsViewModel)}: Initialized with {Cards.Count} cards.");
        }

        /// <summary>
        /// Claims the reward for the specified day.
        /// </summary>
        /// <param name="day">The day number of the reward to claim.</param>
        public void ClaimReward(int day)
        {
            Debug.Log($"{nameof(DailyRewardsViewModel)}: Attempting to claim reward for day {day}.");
            var card = Cards.Find(c => c.day == day);
            if (card == null)
            {
                Debug.LogWarning($"{nameof(DailyRewardsViewModel)}: ClaimReward called for invalid day: {day}.");
                return;
            }
            if (card.claimed)
            {
                Debug.LogWarning($"{nameof(DailyRewardsViewModel)}: Attempted to claim already claimed reward for day: {day}.");
                return;
            }

            if (_dataService != null)
            {
                _dataService.ClaimReward(day, Cards, _currencyService);
                OnStateChangedEvent.Invoke();
                Debug.Log($"{nameof(DailyRewardsViewModel)}: Reward for day {day} claimed.");
            }
            else
            {
                Debug.LogError($"{nameof(DailyRewardsViewModel)}: Cannot claim reward, DailyRewardsDataService is null.");
            }
        }

        /// <summary>
        /// Rerolls all unclaimed rewards if reroll attempts are available.
        /// </summary>
        public void Reroll()
        {
            Debug.Log($"{nameof(DailyRewardsViewModel)}: Attempting to reroll rewards. Rerolls left: {MaxRerolls - RerollCount}.");
            if (RerollCount < MaxRerolls)
            {
                RerollCount++;
                if (_dataService != null)
                {
                    _dataService.RerollUnclaimedRewards(Cards);
                    OnStateChangedEvent.Invoke();
                    Debug.Log($"{nameof(DailyRewardsViewModel)}: Rewards rerolled. Rerolls used: {RerollCount}.");
                }
                else
                {
                    Debug.LogError($"{nameof(DailyRewardsViewModel)}: Cannot reroll rewards, DailyRewardsDataService is null.");
                }
            }
            else
            {
                Debug.LogWarning($"{nameof(DailyRewardsViewModel)}: Attempted to reroll, but no rerolls left.");
            }
        }

        /// <summary>
        /// Handles the OnCoinsChanged event from the CurrencyService.
        /// Updates the ViewModel's coin property and triggers a state change.
        /// </summary>
        /// <param name="coins">The new coin value.</param>
        private void HandleCoinsChanged(int coins)
        {
            CurrentCoins = coins;
            OnStateChangedEvent.Invoke(); // Notify View that state might have changed
        }

        /// <summary>
        /// Handles the OnGemsChanged event from the CurrencyService.
        /// Updates the ViewModel's gem property and triggers a state change.
        /// </summary>
        /// <param name="gems">The new gem value.</param>
        private void HandleGemsChanged(int gems)
        {
            CurrentGems = gems;
            OnStateChangedEvent.Invoke(); // Notify View that state might have changed
        }
    }
}
