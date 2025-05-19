using System;
using UnityEngine;

namespace Scripts.Data
{
    /// <summary>
    /// Service responsible for managing in-game currency (Coins and Gems).
    /// </summary>
    public class CurrencyService
    {
        private int _coins;
        private int _gems;

        /// <summary>
        /// Event triggered when the number of coins changes.
        /// </summary>
        public event Action<int> OnCoinsChanged;
        /// <summary>
        /// Event triggered when the number of gems changes.
        /// </summary>
        public event Action<int> OnGemsChanged;

        /// <summary>
        /// Gets or sets the current number of coins.
        /// Invokes <see cref="OnCoinsChanged"/> when the value changes.
        /// </summary>
        public int Coins
        {
            get => _coins;
            set
            {
                if (_coins != value)
                {
                    _coins = value;
                    OnCoinsChanged?.Invoke(_coins);
                }
            }
        }
        /// <summary>
        /// Gets or sets the current number of gems.
        /// Invokes <see cref="OnGemsChanged"/> when the value changes.
        /// </summary>
        public int Gems
        {
            get => _gems;
            set
            {
                if (_gems != value)
                {
                    _gems = value;
                    OnGemsChanged?.Invoke(_gems);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the CurrencyService with optional starting currency amounts.
        /// </summary>
        /// <param name="initialCoins">The initial number of coins.</param>
        /// <param name="initialGems">The initial number of gems.</param>
        public CurrencyService(int initialCoins = 1000, int initialGems = 25)
        {
            _coins = initialCoins;
            _gems = initialGems;
        }

        /// <summary>
        /// Adds the specified amount of coins.
        /// </summary>
        /// <param name="amount">The amount of coins to add. Should be non-negative.</param>
        public void AddCoins(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"{nameof(CurrencyService)}: Attempted to add negative coins amount: {amount}. Adding 0 instead.");
                amount = 0;
            }
            int newCoins = _coins + amount;
            Coins = newCoins;
        }

        /// <summary>
        /// Adds the specified amount of gems.
        /// </summary>
        /// <param name="amount">The amount of gems to add. Should be non-negative.</param>
        public void AddGems(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"{nameof(CurrencyService)}: Attempted to add negative gems amount: {amount}. Adding 0 instead.");
                amount = 0;
            }
            int newGems = _gems + amount;
            Gems = newGems;
        }

        /// <summary>
        /// Subtracts the specified amount of coins, clamping the result at zero.
        /// </summary>
        /// <param name="amount">The amount of coins to subtract. Should be non-negative.</param>
        public void SubtractCoins(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"{nameof(CurrencyService)}: Attempted to subtract negative coins amount: {amount}. Subtracting 0 instead.");
                amount = 0;
            }
            int newCoins = Mathf.Max(0, _coins - amount); // Clamp at zero
            if (newCoins < _coins - amount)
            {
                Debug.LogWarning($"{nameof(CurrencyService)}: Attempted to subtract {amount} coins from {_coins}, but clamped at zero.");
            }
            Coins = newCoins;
        }

        /// <summary>
        /// Subtracts the specified amount of gems, clamping the result at zero.
        /// </summary>
        /// <param name="amount">The amount of gems to subtract. Should be non-negative.</param>
        public void SubtractGems(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"{nameof(CurrencyService)}: Attempted to subtract negative gems amount: {amount}. Subtracting 0 instead.");
                amount = 0;
            }
            int newGems = Mathf.Max(0, _gems - amount); // Clamp at zero
            if (newGems < _gems - amount)
            {
                Debug.LogWarning($"{nameof(CurrencyService)}: Attempted to subtract {amount} gems from {_gems}, but clamped at zero.");
            }
            Gems = newGems;
        }
    }
}