namespace Scripts.Utilities
{
    public static class LocalizationKeys
    {
        // UI Section
        public static class UI
        {
            public static class DailyRewards
            {
                public const string REROLLS_COUNT_USED = "rerolls-count-used";
                public const string REROLLS_COUNT_REMAIN = "rerolls-count-remain";
                public const string REROLLS_COUNT_EMPTY = "rerolls-count-empty";
                public const string DAY_LABEL = "day-label";
                public const string CARD_STATE_LOCKED = "card-state-locked";
                public const string REWARD_TYPE_XP = "reward-type-xp";
                public const string REWARD_TYPE_GEMS = "reward-type-gems";
                public const string REWARD_TYPE_TOKENS = "reward-type-tokens";
                public const string REWARD_TYPE_COINS = "reward-type-coins";
                public const string REWARD_AMOUNT_FORMAT = "reward-amount-format";
                public const string REWARD_AMOUNT_LOCKED = "reward-amount-locked";
                public const string CLAIM_BUTTON_TEXT = "claim-button-text";
                public const string CLAIM_BUTTON_CLAIMED = "claim-button-claimed";
            }

            public static class CurrencyDisplay
            {
                public const string COINS_VALUE_FORMAT = "coins-value-format";
                public const string GEMS_VALUE_FORMAT = "gems-value-format";
            }

            public static class Components
            {
                public const string PREMIUM_BUTTON_CLAIM = "CLAIM"; // This was a hardcoded string in PremiumButtonControl.cs
            }
        }

        public static class Tables
        {
            public const string MAIN = "LocalesTable";
        }
    }
}