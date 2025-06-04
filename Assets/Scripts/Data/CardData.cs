using System;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scripts.Data
{
    /// <summary>
    /// Defines the types of daily reward cards.
    /// </summary>
    public enum ECardType
    {
        Free,
        Premium,
        Locked
    }

    /// <summary>
    /// Represents the data associated with a single daily reward card in the UI.
    /// This data is typically used for display and interaction logic.
    /// </summary>
    public class CardData : INotifyBindablePropertyChanged
    {
        /// <summary>The day number this card represents.</summary>

        private int _day;
        [CreateProperty]
        public int day
        {
            get => _day;
            set
            {
                _day = value;
                Notify();
            }
        }

        /// <summary>The type of the card (e.g., Free, Premium, Locked).</summary>
        [CreateProperty]
        public ECardType type;
        /// <summary>The reward data associated with this card.</summary>
        private RewardData _reward;
        [CreateProperty]
        public RewardData reward
        {
            get => _reward;
            set
            {
                _reward = value;
                Notify();
            }
        }
        /// <summary>Indicates whether the reward for this day has been claimed.</summary>
        private bool _claimed = false;

        [CreateProperty]
        public bool claimed
        {
            get => _claimed;
            set
            {
                _claimed = value;
                Notify();
            }
        }

        private bool _isCurrentDay = false;
        [CreateProperty]
        public bool isCurrentDay
        {
            get => _isCurrentDay;
            set
            {
                _isCurrentDay = value;
                Notify();
            }
        }

        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        public CardData(int day, ECardType type, RewardData reward, bool claimed = false)
        {
            this.day = day;
            this.type = type;
            this.reward = reward;
            this.claimed = claimed;
        }

        public long GetViewHashCode()
        {
            return HashCode.Combine(day, type, reward, claimed);
        }

        void Notify([CallerMemberName] string property = "")
        {
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
        }
    }
}
