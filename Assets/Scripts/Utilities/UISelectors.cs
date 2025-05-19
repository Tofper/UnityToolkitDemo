namespace Scripts.Utilities
{
    public static class UISelectors
    {
        public static class DailyRewardsScreen
        {
            public const string CURRENCY_DISPLAY = "CurrencyDisplay";
            public const string CARDS_ROW = "CardsRow";
            public const string REROLL_INFO_TEXT = "RerollInfoText";
            public const string REROLL_BUTTON = "RerollButton";
        }

        public static class DailyRewardCard
        {
            // Element Names
            public const string CARD = "card";
            public const string CARD_GLOW = "card-glow";
            public const string CARD_HEADER = "card-header";
            public const string DAY_TEXT = "day-text";
            public const string REWARD_GRADIENT = "reward-gradient";
            public const string REWARD_AMOUNT = "reward-amount";
            public const string ICON_LABEL = "icon-label";
            public const string CLAIM_BUTTON = "claim-button";

            // USS Class Names
            public const string CARD_LOCKED_CLASS = "card--locked";
            public const string CARD_CURRENT_CLASS = "card--current";
            public const string CARD_HEADER_LOCKED_CLASS = "card-header--locked";
            public const string CARD_HEADER_CLAIMED_CLASS = "card-header--claimed";
            public const string DAY_TEXT_LOCKED_CLASS = "card-header__day--locked";
            public const string DAY_TEXT_CLAIMED_CLASS = "card-header__day--claimed";
            public const string REWARD_GRADIENT_LOCKED_CLASS = "reward__gradient--locked";
            public const string REWARD_GRADIENT_GEMS_CLASS = "reward__gradient--gems";
            public const string REWARD_GRADIENT_TOKENS_CLASS = "reward__gradient--tokens";
            public const string REWARD_AMOUNT_LOCKED_CLASS = "reward__amount--locked";
            public const string REWARD_AMOUNT_GEMS_CLASS = "reward__amount--gems";
            public const string REWARD_AMOUNT_TOKENS_CLASS = "reward__amount--tokens";
            public const string REWARD_AMOUNT_COINS_CLASS = "reward__amount--coins";
            public const string CARD_GLOW_GEMS_CLASS = "card__glow--gems";
            public const string CARD_GLOW_TOKENS_CLASS = "card__glow--tokens";
            public const string CARD_REROLL_OUT_CLASS = "card--reroll-out";
            public const string CARD_REROLL_IN_CLASS = "card--reroll-in";
            public const string CARD_REVEALED_CLASS = "card--revealed";
            public const string CARD_SETTLE_CLASS = "card--settle";
            public const string CARD_GLOW_POP_CLASS = "card__glow--pop";
        }

        public static class CurrencyDisplay
        {
            public const string COINS_VALUE_LABEL = "coins-value";
            public const string GEMS_VALUE_LABEL = "gems-value";
        }

        public static class RewardsRerollButton
        {
            // Element Names
            public const string DICE_WRAPPER = "DiceWrapper";
            public const string DICE_ICON = "DiceIcon";
            public const string LABEL = "Label";

            // USS Class Names
            public const string BASE_CLASS = "rewards-reroll";
            public const string PRESSED_CLASS = "pressed";
            public const string HOVERED_CLASS = "hovered";
        }

        public static class PremiumButton
        {
            // Element Names
            public const string BUTTON = "premium-button";
            public const string SHADOW = "premium-button-shadow";
            public const string RESOURCE_PATH = "PremiumButton";

            // USS Class Names
            public const string SHADOW_DEFAULT_CLASS = "shadow--default";
            public const string SHADOW_HOVER_CLASS = "shadow--hover";
            public const string SHADOW_PRESSED_CLASS = "shadow--pressed";
            public const string SHADOW_DISABLED_CLASS = "shadow--disabled";
        }
    }
}