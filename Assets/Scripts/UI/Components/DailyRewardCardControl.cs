using System;
using Scripts.Data;
using Scripts.Utilities;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace Scripts.UI.Components
{
    /// <summary>
    /// Control for displaying a daily reward card.
    /// </summary>
    [UxmlElement]
    public partial class DailyRewardCardControl : VisualElement
    {
        // Resource Paths
        private const string CARD_UXML_PATH = "DailyRewardCard";
        private const string CARD_USS_PATH = "DailyRewardCard";

        // Animation Constants (in milliseconds)
        private const long REROLL_HALF_DURATION_MS = 250;
        private const long SETTLE_DELAY_MS = 350;
        private const long REVEAL_BASE_DELAY_MS = 1280;
        private const long REVEAL_CARD_DELAY_MS = 100;
        private const long GLOW_POP_DURATION_MS = 250;

        private VisualElement _card;
        private VisualElement _cardGlow;
        private VisualElement _cardHeader;
        private Label _dayText;
        private Label _rewardGradient;
        private Label _rewardAmount;
        private Label _iconLabel; // Will display emoji

        private PremiumButtonControl _claimButton;

        private int _day;
        private int _rewardAmountValue;
        private bool _isCurrentDay;
        private bool _isClaimed;
        private bool _initialized = false;
        private ERewardType _rewardType;
        private ECardType _cardType;

        // Scheduled items for animations
        private IVisualElementScheduledItem _rerollOutSchedule;
        private IVisualElementScheduledItem _rerollInSchedule;
        private IVisualElementScheduledItem _revealSchedule;

        /// <summary>
        /// Event triggered when the claim button is clicked.
        /// The integer parameter is the day number of the card.
        /// </summary>
        public event Action<int> OnClaimEvent;

        /// <summary>
        /// Constructor for the DailyRewardCardControl.
        /// Loads UXML and USS, initializes elements and registers callbacks.
        /// Logs warnings if resources are not found.
        /// </summary>
        public DailyRewardCardControl()
        {
            // Load UXML and USS resources from Resources folders
            var visualTree = Resources.Load<VisualTreeAsset>(CARD_UXML_PATH);
            if (visualTree != null)
            {
                visualTree.CloneTree(this);
            }
            else
            {
                Debug.LogWarning($"{nameof(DailyRewardCardControl)}: Could not load VisualTreeAsset at '{CARD_UXML_PATH}'. Ensure it is in a Resources folder.");
            }

            var styleSheet = Resources.Load<StyleSheet>(CARD_USS_PATH);
            if (styleSheet != null)
            {
                styleSheets.Add(styleSheet);
            }
            else
            {
                Debug.LogWarning($"{nameof(DailyRewardCardControl)}: Could not load StyleSheet at '{CARD_USS_PATH}'. Ensure it is in a Resources folder.");
            }

            InitializeElements();
            RegisterCallbacks();
        }

        /// <summary>
        /// Queries and initializes the UI elements of the card.
        /// Adds warnings if elements are not found.
        /// </summary>
        private void InitializeElements()
        {
            _card = this.Q(UISelectors.DailyRewardCard.CARD);
            if (_card == null)
                Debug.LogWarning($"{nameof(DailyRewardCardControl)}: Element '{UISelectors.DailyRewardCard.CARD}' not found.");

            _cardGlow = this.Q(UISelectors.DailyRewardCard.CARD_GLOW);
            if (_cardGlow == null)
                Debug.LogWarning($"{nameof(DailyRewardCardControl)}: Element '{UISelectors.DailyRewardCard.CARD_GLOW}' not found.");

            _cardHeader = this.Q(UISelectors.DailyRewardCard.CARD_HEADER);
            if (_cardHeader == null)
                Debug.LogWarning($"{nameof(DailyRewardCardControl)}: Element '{UISelectors.DailyRewardCard.CARD_HEADER}' not found.");

            _dayText = this.Q<Label>(UISelectors.DailyRewardCard.DAY_TEXT);
            if (_dayText == null)
                Debug.LogWarning($"{nameof(DailyRewardCardControl)}: Element '{UISelectors.DailyRewardCard.DAY_TEXT}' not found.");

            _rewardGradient = this.Q<Label>(UISelectors.DailyRewardCard.REWARD_GRADIENT);
            if (_rewardGradient == null)
                Debug.LogWarning($"{nameof(DailyRewardCardControl)}: Element '{UISelectors.DailyRewardCard.REWARD_GRADIENT}' not found.");

            _rewardAmount = this.Q<Label>(UISelectors.DailyRewardCard.REWARD_AMOUNT);
            if (_rewardAmount == null)
                Debug.LogWarning($"{nameof(DailyRewardCardControl)}: Element '{UISelectors.DailyRewardCard.REWARD_AMOUNT}' not found.");

            _iconLabel = this.Q<Label>(UISelectors.DailyRewardCard.ICON_LABEL);
            if (_iconLabel == null)
                Debug.LogWarning($"{nameof(DailyRewardCardControl)}: Element '{UISelectors.DailyRewardCard.ICON_LABEL}' not found.");

            _claimButton = this.Q<PremiumButtonControl>(UISelectors.DailyRewardCard.CLAIM_BUTTON);
            if (_claimButton == null)
                Debug.LogWarning($"{nameof(DailyRewardCardControl)}: Element '{UISelectors.DailyRewardCard.CLAIM_BUTTON}' not found.");
        }

        /// <summary>
        /// Registers event callbacks for UI elements.
        /// </summary>
        private void RegisterCallbacks()
        {
            if (_claimButton != null)
            {
                _claimButton.RegisterCallback<ClickEvent>(OnClaimButtonClicked);
            }
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        /// <summary>
        /// Unregisters event callbacks and stops ongoing animations when the element is detached from the panel.
        /// </summary>
        /// <param name="e">The detach event data.</param>
        private void OnDetachFromPanel(DetachFromPanelEvent e)
        {
            UnregisterCallbacks();
            StopAllAnimations(); // Ensure scheduled animations are stopped
        }

        /// <summary>
        /// Unregisters event callbacks.
        /// </summary>
        private void UnregisterCallbacks()
        {
            if (_claimButton != null)
            {
                _claimButton.UnregisterCallback<ClickEvent>(OnClaimButtonClicked);
            }
            UnregisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        /// <summary>
        /// Handles the click event for the claim button.
        /// Invokes the OnClaim action if the card is not locked and not claimed.
        /// Logs a warning if the claim is invalid.
        /// </summary>
        /// <param name="evt">The click event data.</param>
        private void OnClaimButtonClicked(ClickEvent evt)
        {
            if (_cardType != ECardType.Locked && !_isClaimed)
            {
                OnClaimEvent?.Invoke(_day);
            }
            else
            {
                Debug.LogWarning($"{nameof(DailyRewardCardControl)}: Claim button clicked for day {_day} but card is locked or already claimed (Locked: {_cardType == ECardType.Locked}, Claimed: {_isClaimed}). Claim ignored.");
            }
        }

        /// <summary>
        /// Sets the data for the daily reward card and updates its visuals.
        /// Handles initial setup and reroll animations.
        /// Logs the state of the card when data is set.
        /// </summary>
        /// <param name="cardData">The data for the daily reward card.</param>
        /// <param name="isCurrentDay">Indicates if this card represents the current day's reward.</param>
        public void SetData(CardData cardData, bool isCurrentDay = false)
        {
            bool rewardUpdated = _rewardType != cardData.reward.rewardType || _rewardAmountValue != cardData.reward.rewardAmount;
            _day = cardData.day;
            _cardType = cardData.type;
            _rewardType = cardData.reward.rewardType;
            _rewardAmountValue = cardData.reward.rewardAmount;
            _isCurrentDay = isCurrentDay;
            _isClaimed = cardData.claimed;
            if (!_initialized)
            {
                Debug.Log($"[{nameof(DailyRewardCardControl)}] Initializing card for day {_day}");
                UpdateVisuals();
                // Initial reveal animation
                Reveal(_day); // Pass day for staggered animation
                _initialized = true;
            }
            else if (rewardUpdated)
            {
                PlayRerollAnimation(); // Animation reads updated data
            }
            else
            {
                UpdateVisuals();
            }
        }

        /// <summary>
        /// Plays the reroll animation sequence for the card.
        /// </summary>
        private void PlayRerollAnimation()
        {
            // Stop any ongoing animations first
            StopAllAnimations();

            // Start reroll out animation
            _card.AddToClassList(UISelectors.DailyRewardCard.CARD_REROLL_OUT_CLASS);
            _rerollOutSchedule = _card.schedule.Execute(() =>
            {
                _card.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_REROLL_OUT_CLASS);
                _card.AddToClassList(UISelectors.DailyRewardCard.CARD_REROLL_IN_CLASS);
                UpdateVisuals(); // Update visuals while card is "flipped"
                _rerollInSchedule = _card.schedule.Execute(() =>
                {
                    _card.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_REROLL_IN_CLASS);
                    _card.AddToClassList(UISelectors.DailyRewardCard.CARD_SETTLE_CLASS);
                    var settleSchedule = _card.schedule.Execute(() => _card.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_SETTLE_CLASS));
                    settleSchedule.ExecuteLater(SETTLE_DELAY_MS);
                });
                _rerollInSchedule.ExecuteLater(REROLL_HALF_DURATION_MS);
            });
            _rerollOutSchedule.ExecuteLater(REROLL_HALF_DURATION_MS);
        }

        /// <summary>
        /// Updates the visual state of the card based on its current data.
        /// </summary>
        private void UpdateVisuals()
        {
            var stringDatabase = LocalizationSettings.StringDatabase;

            // Update day text
            if (_dayText != null)
            {
                _dayText.text = stringDatabase.GetLocalizedString(LocalizationKeys.Tables.MAIN, LocalizationKeys.UI.DailyRewards.DAY_LABEL) + $" {_day}";
                _dayText.RemoveFromClassList(UISelectors.DailyRewardCard.DAY_TEXT_LOCKED_CLASS);
                _dayText.RemoveFromClassList(UISelectors.DailyRewardCard.DAY_TEXT_CLAIMED_CLASS);
                if (_cardType == ECardType.Locked)
                {
                    _dayText.AddToClassList(UISelectors.DailyRewardCard.DAY_TEXT_LOCKED_CLASS);
                }
                else if (_isClaimed)
                {
                    _dayText.AddToClassList(UISelectors.DailyRewardCard.DAY_TEXT_CLAIMED_CLASS);
                }
            }

            // Update card header
            if (_cardHeader != null)
            {
                _cardHeader.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_HEADER_LOCKED_CLASS);
                _cardHeader.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_HEADER_CLAIMED_CLASS);
                if (_cardType == ECardType.Locked)
                {
                    _cardHeader.AddToClassList(UISelectors.DailyRewardCard.CARD_HEADER_LOCKED_CLASS);
                }
                else if (_isClaimed)
                {
                    _cardHeader.AddToClassList(UISelectors.DailyRewardCard.CARD_HEADER_CLAIMED_CLASS);
                }
            }

            // Update reward gradient
            if (_rewardGradient != null)
            {
                _rewardGradient.RemoveFromClassList(UISelectors.DailyRewardCard.REWARD_GRADIENT_LOCKED_CLASS);
                _rewardGradient.RemoveFromClassList(UISelectors.DailyRewardCard.REWARD_GRADIENT_GEMS_CLASS);
                _rewardGradient.RemoveFromClassList(UISelectors.DailyRewardCard.REWARD_GRADIENT_TOKENS_CLASS);
                if (_cardType == ECardType.Locked)
                {
                    _rewardGradient.text = stringDatabase.GetLocalizedString(LocalizationKeys.Tables.MAIN, LocalizationKeys.UI.DailyRewards.CARD_STATE_LOCKED);
                    _rewardGradient.AddToClassList(UISelectors.DailyRewardCard.REWARD_GRADIENT_LOCKED_CLASS);
                }
                else
                {
                    switch (_rewardType)
                    {
                        case ERewardType.Gems:
                            _rewardGradient.text = stringDatabase.GetLocalizedString(LocalizationKeys.Tables.MAIN, LocalizationKeys.UI.DailyRewards.REWARD_TYPE_GEMS);
                            _rewardGradient.AddToClassList(UISelectors.DailyRewardCard.REWARD_GRADIENT_GEMS_CLASS);
                            break;
                        case ERewardType.Tokens:
                            _rewardGradient.text = stringDatabase.GetLocalizedString(LocalizationKeys.Tables.MAIN, LocalizationKeys.UI.DailyRewards.REWARD_TYPE_TOKENS);
                            _rewardGradient.AddToClassList(UISelectors.DailyRewardCard.REWARD_GRADIENT_TOKENS_CLASS);
                            break;
                        case ERewardType.Coins:
                            _rewardGradient.text = stringDatabase.GetLocalizedString(LocalizationKeys.Tables.MAIN, LocalizationKeys.UI.DailyRewards.REWARD_TYPE_COINS);
                            break;
                        case ERewardType.XP:
                            _rewardGradient.text = stringDatabase.GetLocalizedString(LocalizationKeys.Tables.MAIN, LocalizationKeys.UI.DailyRewards.REWARD_TYPE_XP);
                            break;
                        default:
                            break;
                    }
                }
            }

            // Update reward amount
            if (_rewardAmount != null)
            {
                _rewardAmount.text = $"x{_rewardAmountValue}";
                _rewardAmount.RemoveFromClassList(UISelectors.DailyRewardCard.REWARD_AMOUNT_LOCKED_CLASS);
                _rewardAmount.RemoveFromClassList(UISelectors.DailyRewardCard.REWARD_AMOUNT_GEMS_CLASS);
                _rewardAmount.RemoveFromClassList(UISelectors.DailyRewardCard.REWARD_AMOUNT_TOKENS_CLASS);
                _rewardAmount.RemoveFromClassList(UISelectors.DailyRewardCard.REWARD_AMOUNT_COINS_CLASS);
                if (_cardType == ECardType.Locked)
                {
                    _rewardAmount.text = stringDatabase.GetLocalizedString(LocalizationKeys.Tables.MAIN, LocalizationKeys.UI.DailyRewards.REWARD_AMOUNT_LOCKED);
                    _rewardAmount.AddToClassList(UISelectors.DailyRewardCard.REWARD_AMOUNT_LOCKED_CLASS);
                }
                else
                {
                    switch (_rewardType)
                    {
                        case ERewardType.Gems:
                            _rewardAmount.AddToClassList(UISelectors.DailyRewardCard.REWARD_AMOUNT_GEMS_CLASS);
                            break;
                        case ERewardType.Tokens:
                            _rewardAmount.AddToClassList(UISelectors.DailyRewardCard.REWARD_AMOUNT_TOKENS_CLASS);
                            break;
                        case ERewardType.Coins:
                            _rewardAmount.AddToClassList(UISelectors.DailyRewardCard.REWARD_AMOUNT_COINS_CLASS);
                            break;
                        case ERewardType.XP:
                            break;
                        default:
                            break;
                    }
                }
            }

            // Update card glow
            if (_cardGlow != null)
            {
                _cardGlow.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_GLOW_GEMS_CLASS);
                _cardGlow.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_GLOW_TOKENS_CLASS);
                if (_cardType != ECardType.Locked)
                {
                    switch (_rewardType)
                    {
                        case ERewardType.Gems:
                            _cardGlow.AddToClassList(UISelectors.DailyRewardCard.CARD_GLOW_GEMS_CLASS);
                            break;
                        case ERewardType.Tokens:
                            _cardGlow.AddToClassList(UISelectors.DailyRewardCard.CARD_GLOW_TOKENS_CLASS);
                            break;
                        case ERewardType.Coins:
                            break;
                        case ERewardType.XP:
                            break;
                        default:
                            break;
                    }
                }
            }

            // Update card state
            if (_card != null)
            {
                _card.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_LOCKED_CLASS);
                _card.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_CURRENT_CLASS);
                if (_cardType == ECardType.Locked)
                {
                    _card.AddToClassList(UISelectors.DailyRewardCard.CARD_LOCKED_CLASS);
                }
                else if (_isCurrentDay)
                {
                    _card.AddToClassList(UISelectors.DailyRewardCard.CARD_CURRENT_CLASS);
                }
            }

            // Update claim button
            if (_claimButton != null)
            {
                _claimButton.label = _isClaimed
                    ? stringDatabase.GetLocalizedString(LocalizationKeys.Tables.MAIN, LocalizationKeys.UI.DailyRewards.CLAIM_BUTTON_CLAIMED)
                    : stringDatabase.GetLocalizedString(LocalizationKeys.Tables.MAIN, LocalizationKeys.UI.DailyRewards.CLAIM_BUTTON_TEXT);
                _claimButton.SetPremiumButtonEnabled(_cardType != ECardType.Locked && !_isClaimed);
            }

            // Update icon
            UpdateIcon();
        }

        /// <summary>
        /// Plays the reveal animation for the card.
        /// </summary>
        /// <param name="dayNumber">The day number for staggered animation timing.</param>
        public void Reveal(int dayNumber)
        {
            // Stop any ongoing animations first
            StopAllAnimations();

            // Calculate delay based on day number for staggered reveal
            long revealDelay = REVEAL_BASE_DELAY_MS + (dayNumber * REVEAL_CARD_DELAY_MS);

            // Schedule the reveal animation
            _revealSchedule = _card.schedule.Execute(() =>
            {
                _card.AddToClassList(UISelectors.DailyRewardCard.CARD_SETTLE_CLASS);
                var glowSchedule = _card.schedule.Execute(() =>
                {
                    if (_cardGlow != null)
                    {
                        _cardGlow.AddToClassList(UISelectors.DailyRewardCard.CARD_GLOW_POP_CLASS);
                        var popSchedule = _cardGlow.schedule.Execute(() => _cardGlow.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_GLOW_POP_CLASS));
                        popSchedule.ExecuteLater(GLOW_POP_DURATION_MS);
                    }
                });
                glowSchedule.ExecuteLater(GLOW_POP_DURATION_MS);
            });
            _card.AddToClassList(UISelectors.DailyRewardCard.CARD_REVEALED_CLASS);
            _revealSchedule.ExecuteLater(revealDelay);
        }

        /// <summary>
        /// Stops all ongoing animations on the card.
        /// </summary>
        private void StopAllAnimations()
        {
            if (_rerollOutSchedule != null)
            {
                _rerollOutSchedule.Pause();
                _rerollOutSchedule = null;
            }
            if (_rerollInSchedule != null)
            {
                _rerollInSchedule.Pause();
                _rerollInSchedule = null;
            }
            if (_revealSchedule != null)
            {
                _revealSchedule.Pause();
                _revealSchedule = null;
            }

            // Remove animation classes
            if (_card != null)
            {
                _card.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_REROLL_OUT_CLASS);
                _card.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_REROLL_IN_CLASS);
                _card.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_REVEALED_CLASS);
                _card.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_SETTLE_CLASS);
            }
            if (_cardGlow != null)
            {
                _cardGlow.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_GLOW_POP_CLASS);
            }
        }

        /// <summary>
        /// Sets the icon for the card based on the reward type.
        /// </summary>
        private void UpdateIcon()
        {
            if (_iconLabel != null)
            {
                var iconType = RewardIconUtils.GetIconTypeForReward(_rewardType);
                _iconLabel.text = RewardIconUtils.GetEmojiForIconType(iconType);
            }
        }
    }
}
