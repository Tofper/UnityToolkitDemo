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
    public class CardData
    {
        /// <summary>The day number this card represents.</summary>
        public int day;
        /// <summary>The type of the card (e.g., Free, Premium, Locked).</summary>
        public ECardType type;
        /// <summary>The reward data associated with this card.</summary>
        public RewardData reward;
        /// <summary>Indicates whether the reward for this day has been claimed.</summary>
        public bool claimed;

        public CardData(int day, ECardType type, RewardData reward, bool claimed = false)
        {
            this.day = day;
            this.type = type;
            this.reward = reward;
            this.claimed = claimed;
        }
    }
}
