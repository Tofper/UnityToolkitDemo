using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scripts.Data
{

    /// <summary>
    /// ViewModel for the Daily Rewards screen.
    /// Manages the state and logic for displaying and interacting with daily reward cards.
    /// </summary>
    public class DailyRewardsViewModel : INotifyBindablePropertyChanged
    {
        private int _MaxRerolls = 3;
        private int _RerollCount = 0;
        private int _CurrentGems = 0;
        private int _CurrentCoins = 0;
        /// <summary>
        /// Gets the list of daily reward card data.
        /// </summary>
        [CreateProperty]
        public List<CardData> Cards { get; } = new List<CardData>();
        /// <summary>
        /// Gets the current number of rerolls used.
        /// </summary>
        [CreateProperty]
        public int RerollCount
        {
            get => _RerollCount;
            private set
            {
                _RerollCount = value;
                Notify();
            }
        }
        /// Gets the maximum number of allowed rerolls.
        /// </summary>
        [CreateProperty]
        public int MaxRerolls
        {
            get => _MaxRerolls;
            private set
            {
                _MaxRerolls = value;
                Notify();
            }
        }
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
        [CreateProperty]
        public int CurrentCoins
        {
            get => _CurrentCoins;
            private set
            {
                _CurrentCoins = value;
                Notify();
            }
        }

        /// <summary>Gets the current number of gems.</summary>
        [CreateProperty]

        public int CurrentGems
        {
            get => _CurrentGems;
            private set
            {
                _CurrentGems = value;
                Notify();
            }
        }

        public int CurrentGemsFrem = 88;

        private DailyRewardsDataService _dataService;
        private CurrencyService _currencyService;

        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

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
            CurrentDay = _dataService?.CurrentDay ?? 0;
            List<RewardData> initialRewards = _dataService != null ? _dataService.GetRewardData() : new List<RewardData>();

            Debug.Log($"{nameof(DailyRewardsViewModel)}: Creating cards for {initialRewards.Count} rewards. Current day: {CurrentDay}");

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
                var cardData = new CardData(day, initialCardType, rewardData, initialClaimedState);
                cardData.isCurrentDay = day == CurrentDay;
                Cards.Add(cardData);
            }
            Debug.Log($"{nameof(DailyRewardsViewModel)}: Initialized with {Cards.Count} cards.");
            Notify(nameof(Cards)); // Notify initial cards creation
        }

        void Notify([CallerMemberName] string property = "")
        {
            Debug.Log($"This is name of {property}: Subscribed invoke.");
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
        }

        /// <summary>
        /// Claims the reward for the specified day.
        /// </summary>
        /// <param name="day">The day number of the reward to claim.</param>
        public void ClaimReward(int day)
        {
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
                Notify(nameof(Cards)); // Notify that Cards collection has changed
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
                    Notify(nameof(Cards)); // Notify that Cards collection has changed
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
