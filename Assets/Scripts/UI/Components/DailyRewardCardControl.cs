using System;
using Scripts.Data;
using Scripts.Utilities;
using Unity.Properties;
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
        #region Constants
        // Resource Paths
        private const string CARD_UXML_PATH = "DailyRewardCard";
        private const string CARD_USS_PATH = "DailyRewardCard";

        // Animation Constants (in milliseconds)
        private const long REROLL_HALF_DURATION_MS = 250;
        private const long SETTLE_DELAY_MS = 350;
        private const long REVEAL_BASE_DELAY_MS = 1280;
        private const long REVEAL_CARD_DELAY_MS = 100;
        private const long GLOW_POP_DURATION_MS = 250;
        private const string BINDING_CALLBACK = "__bindingCallback";
        private const string BINDING_CALLBACK_REWARD_CHANGE = "__bindingRewardChange";
        #endregion

        #region UI Elements
        private VisualElement _card;
        private VisualElement _cardGlow;
        private VisualElement _cardHeader;
        private Label _dayText;
        private Label _rewardGradient;
        private Label _rewardAmount;
        private Label _iconLabel; // Will display emoji

        private PremiumButtonControl _claimButton;
        #endregion

        #region State
        private LocalizedStringDatabase _stringDatabase = LocalizationSettings.StringDatabase;
        private bool _initialized = false;
        private CardData _cardData;
        #endregion

        #region Animation Schedules
        private IVisualElementScheduledItem _rerollOutSchedule;
        private IVisualElementScheduledItem _rerollInSchedule;
        private IVisualElementScheduledItem _revealSchedule;
        #endregion

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

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            InitializeElements();
            RegisterCallbacks();
            SetupBindings();
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            UnregisterCallbacks();
            StopAllAnimations();
            ClearElementsBindings();
            ClearData();
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
            _claimButton?.RegisterCallback<ClickEvent>(OnClaimButtonClicked);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        /// <summary>
        /// Unregisters event callbacks.
        /// </summary>
        private void UnregisterCallbacks()
        {
            _claimButton?.UnregisterCallback<ClickEvent>(OnClaimButtonClicked);
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
            if (_cardData != null && _cardData.type != ECardType.Locked && !_cardData.claimed)
            {
                OnClaimEvent?.Invoke(_cardData.day);
            }
            else
            {
                Debug.LogWarning($"{nameof(DailyRewardCardControl)}: Claim button clicked but card is locked or already claimed (Locked: {_cardData.type == ECardType.Locked}, Claimed: {_cardData.claimed}). Claim ignored.");
            }
        }

        /// <summary>
        /// Sets the data for the daily reward card and updates its visuals.
        /// Handles initial setup and reroll animations.
        /// Logs the state of the card when data is set.
        /// </summary>
        /// <param name="cardData">The data for the daily reward card.</param>
        public void SetData(CardData cardData)
        {
            dataSource = _cardData = cardData;

            if (!_initialized)
            {
                Debug.Log($"[{nameof(DailyRewardCardControl)}] Initializing card for day {_cardData.day}");
                // Initial reveal animation
                Reveal(_cardData.day); // Pass day for staggered animation
                _initialized = true;
            }
        }

        private void SetupRewardUpdateCallback()
        {
            var rewardUpdateBinding = new MultiDependencyBinding
            {
                dependencyPaths = new[] { "reward.rewardType", "reward.rewardAmount" }
            };
            rewardUpdateBinding.AddCallback<ERewardType, int>((rewardType, rewardAmount) =>
            {
                if (_initialized)
                {
                    PlayRerollAnimation();
                }
            });
            SetBinding(BINDING_CALLBACK_REWARD_CHANGE, rewardUpdateBinding);
        }

        private void SetupBindings()
        {

            SetupDayTextBindings();
            SetupRewardUpdateCallback();
            SetupRewardGradientBindings();
            SetupRewardAmountBindings();
            SetupCardStateBindings();
            SetupClaimButtonBindings();
            SetupIconBindings();
        }

        private void SetupDayTextBindings()
        {
            if (_dayText == null) return;

            var dayTextBinding = new DataBinding
            {
                dataSourcePath = new PropertyPath("day"),
                bindingMode = BindingMode.ToTarget
            };
            dayTextBinding.sourceToUiConverters.AddConverter((ref int day) =>
            {
                return _stringDatabase.GetLocalizedString(LocalizationKeys.Tables.MAIN, LocalizationKeys.UI.DailyRewards.DAY_LABEL, new object[] { day });
            });
            _dayText.SetBinding("text", dayTextBinding);

            var dayTextClassBinding = new MultiDependencyBinding
            {
                dependencyPaths = new[] { "type", "claimed" }
            };
            dayTextClassBinding.AddCallback<ECardType, bool>((type, claimed) =>
            {
                UpdateDayTextClasses(type, claimed);
            });
            _dayText.SetBinding(BINDING_CALLBACK, dayTextClassBinding);
        }

        private void UpdateDayTextClasses(ECardType type, bool claimed)
        {
            _dayText.RemoveFromClassList(UISelectors.DailyRewardCard.DAY_TEXT_LOCKED_CLASS);
            _dayText.RemoveFromClassList(UISelectors.DailyRewardCard.DAY_TEXT_CLAIMED_CLASS);
            if (type == ECardType.Locked)
            {
                _dayText.AddToClassList(UISelectors.DailyRewardCard.DAY_TEXT_LOCKED_CLASS);
            }
            else if (claimed)
            {
                _dayText.AddToClassList(UISelectors.DailyRewardCard.DAY_TEXT_CLAIMED_CLASS);
            }

            _cardHeader.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_HEADER_LOCKED_CLASS);
            _cardHeader.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_HEADER_CLAIMED_CLASS);
            if (type == ECardType.Locked)
            {
                _cardHeader.AddToClassList(UISelectors.DailyRewardCard.CARD_HEADER_LOCKED_CLASS);
            }
            else if (claimed)
            {
                _cardHeader.AddToClassList(UISelectors.DailyRewardCard.CARD_HEADER_CLAIMED_CLASS);
            }
        }

        private void SetupRewardGradientBindings()
        {
            if (_rewardGradient == null) return;

            var gradientTextBinding = new MultiDependencyBinding
            {
                dependencyPaths = new[] { "type", "reward.rewardType" }
            };
            gradientTextBinding.SetConverter<ECardType, ERewardType, string>((type, rewardType) =>
            {
                return GetRewardGradientText(type, rewardType);
            });
            _rewardGradient.SetBinding("text", gradientTextBinding);

            var gradientClassBinding = new MultiDependencyBinding
            {
                dependencyPaths = new[] { "type", "reward.rewardType" }
            };
            gradientClassBinding.AddCallback<ECardType, ERewardType>((type, rewardType) =>
            {
                UpdateRewardGradientClasses(type, rewardType);
                UpdateRewardAmountClasses(type, rewardType);
                UpdateCardGlowClasses(type, rewardType);
            });
            SetBinding(BINDING_CALLBACK, gradientClassBinding);
        }

        private string GetRewardGradientText(ECardType type, ERewardType rewardType)
        {
            if (type == ECardType.Locked)
                return _stringDatabase.GetLocalizedString(LocalizationKeys.Tables.MAIN, LocalizationKeys.UI.DailyRewards.CARD_STATE_LOCKED);

            return rewardType switch
            {
                ERewardType.Gems => _stringDatabase.GetLocalizedString(LocalizationKeys.Tables.MAIN, LocalizationKeys.UI.DailyRewards.REWARD_TYPE_GEMS),
                ERewardType.Tokens => _stringDatabase.GetLocalizedString(LocalizationKeys.Tables.MAIN, LocalizationKeys.UI.DailyRewards.REWARD_TYPE_TOKENS),
                ERewardType.Coins => _stringDatabase.GetLocalizedString(LocalizationKeys.Tables.MAIN, LocalizationKeys.UI.DailyRewards.REWARD_TYPE_COINS),
                ERewardType.XP => _stringDatabase.GetLocalizedString(LocalizationKeys.Tables.MAIN, LocalizationKeys.UI.DailyRewards.REWARD_TYPE_XP),
                _ => string.Empty
            };
        }

        private void UpdateRewardGradientClasses(ECardType type, ERewardType rewardType)
        {
            if (_rewardGradient != null)
            {
                _rewardGradient.RemoveFromClassList(UISelectors.DailyRewardCard.REWARD_GRADIENT_LOCKED_CLASS);
                _rewardGradient.RemoveFromClassList(UISelectors.DailyRewardCard.REWARD_GRADIENT_GEMS_CLASS);
                _rewardGradient.RemoveFromClassList(UISelectors.DailyRewardCard.REWARD_GRADIENT_TOKENS_CLASS);
                if (type == ECardType.Locked)
                {
                    _rewardGradient.AddToClassList(UISelectors.DailyRewardCard.REWARD_GRADIENT_LOCKED_CLASS);
                }
                else
                {
                    switch (rewardType)
                    {
                        case ERewardType.Gems:
                            _rewardGradient.AddToClassList(UISelectors.DailyRewardCard.REWARD_GRADIENT_GEMS_CLASS);
                            break;
                        case ERewardType.Tokens:
                            _rewardGradient.AddToClassList(UISelectors.DailyRewardCard.REWARD_GRADIENT_TOKENS_CLASS);
                            break;
                    }
                }
            }
        }

        private void UpdateRewardAmountClasses(ECardType type, ERewardType rewardType)
        {
            _rewardAmount.RemoveFromClassList(UISelectors.DailyRewardCard.REWARD_AMOUNT_LOCKED_CLASS);
            _rewardAmount.RemoveFromClassList(UISelectors.DailyRewardCard.REWARD_AMOUNT_GEMS_CLASS);
            _rewardAmount.RemoveFromClassList(UISelectors.DailyRewardCard.REWARD_AMOUNT_TOKENS_CLASS);
            _rewardAmount.RemoveFromClassList(UISelectors.DailyRewardCard.REWARD_AMOUNT_COINS_CLASS);
            if (type == ECardType.Locked)
            {
                _rewardAmount.text = _stringDatabase.GetLocalizedString(LocalizationKeys.Tables.MAIN, LocalizationKeys.UI.DailyRewards.REWARD_AMOUNT_LOCKED);
                _rewardAmount.AddToClassList(UISelectors.DailyRewardCard.REWARD_AMOUNT_LOCKED_CLASS);
            }
            else
            {
                switch (rewardType)
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
                }
            }
        }

        private void UpdateCardGlowClasses(ECardType type, ERewardType rewardType)
        {
            if (_cardGlow != null)
            {
                _cardGlow.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_GLOW_GEMS_CLASS);
                _cardGlow.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_GLOW_TOKENS_CLASS);
                if (type != ECardType.Locked)
                {
                    switch (rewardType)
                    {
                        case ERewardType.Gems:
                            _cardGlow.AddToClassList(UISelectors.DailyRewardCard.CARD_GLOW_GEMS_CLASS);
                            break;
                        case ERewardType.Tokens:
                            _cardGlow.AddToClassList(UISelectors.DailyRewardCard.CARD_GLOW_TOKENS_CLASS);
                            break;
                    }
                }
            }
        }

        private void SetupRewardAmountBindings()
        {
            if (_rewardAmount == null) return;

            var amountTextBinding = new MultiDependencyBinding
            {
                dependencyPaths = new[] { "type", "reward.rewardAmount" }
            };
            amountTextBinding.SetConverter<ECardType, int, string>((type, amount) =>
            {
                if (type == ECardType.Locked)
                    return _stringDatabase.GetLocalizedString(LocalizationKeys.Tables.MAIN, LocalizationKeys.UI.DailyRewards.REWARD_AMOUNT_LOCKED);
                return _stringDatabase.GetLocalizedString(LocalizationKeys.Tables.MAIN, LocalizationKeys.UI.DailyRewards.REWARD_AMOUNT_FORMAT, new object[] { amount });
            });
            _rewardAmount.SetBinding("text", amountTextBinding);
        }

        private void SetupCardStateBindings()
        {
            if (_card == null) return;

            var cardClassBinding = new MultiDependencyBinding
            {
                dependencyPaths = new[] { "type", "isCurrentDay" }
            };
            cardClassBinding.AddCallback<ECardType, bool>((type, isCurrentDay) =>
            {
                if (_card != null)
                {
                    _card.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_LOCKED_CLASS);
                    _card.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_CURRENT_CLASS);
                    if (type == ECardType.Locked)
                    {
                        _card.AddToClassList(UISelectors.DailyRewardCard.CARD_LOCKED_CLASS);
                    }
                    else if (isCurrentDay)
                    {
                        _card.AddToClassList(UISelectors.DailyRewardCard.CARD_CURRENT_CLASS);
                    }
                }
            });
            _card.SetBinding(BINDING_CALLBACK, cardClassBinding);
        }

        private void SetupClaimButtonBindings()
        {
            if (_claimButton == null) return;

            var buttonLabelBinding = new DataBinding
            {
                dataSourcePath = new PropertyPath("claimed"),
                bindingMode = BindingMode.ToTarget
            };
            buttonLabelBinding.sourceToUiConverters.AddConverter((ref bool claimed) => claimed
                ? _stringDatabase.GetLocalizedString(LocalizationKeys.Tables.MAIN, LocalizationKeys.UI.DailyRewards.CLAIM_BUTTON_CLAIMED)
                : _stringDatabase.GetLocalizedString(LocalizationKeys.Tables.MAIN, LocalizationKeys.UI.DailyRewards.CLAIM_BUTTON_TEXT));
            _claimButton.SetBinding("label", buttonLabelBinding);

            var buttonEnabledBinding = new MultiDependencyBinding
            {
                dependencyPaths = new[] { "type", "claimed" }
            };
            buttonEnabledBinding.AddCallback<ECardType, bool>((type, claimed) =>
            {
                _claimButton.SetPremiumButtonEnabled(type != ECardType.Locked && !claimed);
            });
            _claimButton.SetBinding(BINDING_CALLBACK, buttonEnabledBinding);
        }

        private void SetupIconBindings()
        {
            if (_iconLabel == null) return;

            var iconBinding = new DataBinding
            {
                dataSourcePath = new PropertyPath("reward.rewardType"),
                bindingMode = BindingMode.ToTarget
            };
            iconBinding.sourceToUiConverters.AddConverter((ref ERewardType rewardType) =>
            {
                var iconType = RewardIconUtils.GetIconTypeForReward(rewardType);
                return RewardIconUtils.GetEmojiForIconType(iconType);
            });
            _iconLabel.SetBinding("text", iconBinding);
        }

        /// <summary>
        /// Plays the reroll animation sequence for the card.
        /// </summary>
        private void PlayRerollAnimation()
        {
            StopAllAnimations();
            StartRerollOutAnimation();
        }

        private void StartRerollOutAnimation()
        {
            _card.AddToClassList(UISelectors.DailyRewardCard.CARD_REROLL_OUT_CLASS);
            _rerollOutSchedule = _card.schedule.Execute(() =>
            {
                _card.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_REROLL_OUT_CLASS);
                StartRerollInAnimation();
            });
            _rerollOutSchedule.ExecuteLater(REROLL_HALF_DURATION_MS);
        }

        private void StartRerollInAnimation()
        {
            _card.AddToClassList(UISelectors.DailyRewardCard.CARD_REROLL_IN_CLASS);
            _rerollInSchedule = _card.schedule.Execute(() =>
            {
                _card.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_REROLL_IN_CLASS);
                StartSettleAnimation();
            });
            _rerollInSchedule.ExecuteLater(REROLL_HALF_DURATION_MS);
        }

        private void StartSettleAnimation()
        {
            _card.AddToClassList(UISelectors.DailyRewardCard.CARD_SETTLE_CLASS);
            var settleSchedule = _card.schedule.Execute(() => _card.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_SETTLE_CLASS));
            settleSchedule.ExecuteLater(SETTLE_DELAY_MS);
        }

        /// <summary>
        /// Plays the reveal animation for the card.
        /// </summary>
        /// <param name="dayNumber">The day number for staggered animation timing.</param>
        public void Reveal(int dayNumber)
        {
            StopAllAnimations();
            long revealDelay = REVEAL_BASE_DELAY_MS + (dayNumber * REVEAL_CARD_DELAY_MS);
            StartRevealAnimation(revealDelay);
        }

        private void StartRevealAnimation(long delay)
        {
            _card.AddToClassList(UISelectors.DailyRewardCard.CARD_REVEALED_CLASS);
            _revealSchedule = _card.schedule.Execute(() =>
            {
                _card.AddToClassList(UISelectors.DailyRewardCard.CARD_SETTLE_CLASS);
                StartGlowAnimation();
            });
            _revealSchedule.ExecuteLater(delay);
        }

        private void StartGlowAnimation()
        {
            if (_cardGlow == null) return;

            var glowSchedule = _card.schedule.Execute(() =>
            {
                _cardGlow.AddToClassList(UISelectors.DailyRewardCard.CARD_GLOW_POP_CLASS);
                var popSchedule = _cardGlow.schedule.Execute(() => _cardGlow.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_GLOW_POP_CLASS));
                popSchedule.ExecuteLater(GLOW_POP_DURATION_MS);
            });
            glowSchedule.ExecuteLater(GLOW_POP_DURATION_MS);
        }

        /// <summary>
        /// Stops all ongoing animations on the card.
        /// </summary>
        private void StopAllAnimations()
        {
            StopScheduledAnimations();
            RemoveAnimationClasses();
        }

        private void StopScheduledAnimations()
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
        }

        private void RemoveAnimationClasses()
        {
            if (_card != null)
            {
                _card.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_REROLL_OUT_CLASS);
                _card.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_REROLL_IN_CLASS);
                _card.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_REVEALED_CLASS);
                _card.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_SETTLE_CLASS);
            }
            _cardGlow?.RemoveFromClassList(UISelectors.DailyRewardCard.CARD_GLOW_POP_CLASS);
        }

        /// <summary>
        /// Clears all bindings and their associated resources.
        /// </summary>
        private void ClearElementsBindings()
        {
            ClearBindings();
            _dayText?.ClearBindings();
            _rewardGradient?.ClearBindings();
            _rewardAmount?.ClearBindings();
            _card?.ClearBindings();
            _claimButton?.ClearBindings();
            _iconLabel?.ClearBindings();
        }

        /// <summary>
        /// Clears all data and resets the control to its initial state.
        /// </summary>
        private void ClearData()
        {
            _initialized = false;
            dataSource = null;
            _cardData = null;
            _stringDatabase = null;
        }
    }
}
