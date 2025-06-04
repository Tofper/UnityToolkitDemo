using Scripts.Data;
using Scripts.UI.Components;
using Scripts.Utilities;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace Scripts.UI.GameScreens
{
    /// <summary>
    /// Manages the Daily Rewards UI screen, connecting it to ViewModel.
    /// </summary>
    public class DailyRewardsScreen : MonoBehaviour
    {
        [Header("UI Toolkit References")]
        /// <summary>
        /// The UIDocument component that contains the Daily Rewards UI hierarchy.
        /// </summary>
        [SerializeField] public UIDocument uiDocument;

        // Cached UI Toolkit elements
        private VisualElement _root;
        private CurrencyDisplayControl _currencyDisplay;
        private VisualElement _cardsRow;
        private Label _rerollInfoText;
        private RewardsRerollButton _rerollButton;

        // ViewModel reference
        private DailyRewardsViewModel _viewModel;

        /// <summary>
        /// Called when the script instance is being loaded.
        /// Initializes UI elements and registers event handlers.
        /// </summary>
        void OnEnable()
        {
            // Attach to UXML if not already assigned
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
                if (uiDocument == null)
                {
                    Debug.LogError($"{nameof(DailyRewardsScreen)}: UIDocument is not assigned and not found on GameObject.");
                    enabled = false; // Disable script if UIDocument is missing
                    return;
                }
            }
            _root = uiDocument.rootVisualElement;
            if (_root == null)
            {
                Debug.LogError($"{nameof(DailyRewardsScreen)}: Root VisualElement is null. Check UIDocument setup.");
                enabled = false;
                return;
            }

            // Query UI elements
            _currencyDisplay = _root.Q<CurrencyDisplayControl>(UISelectors.DailyRewardsScreen.CURRENCY_DISPLAY);
            if (_currencyDisplay == null)
            {
                Debug.LogWarning($"{nameof(DailyRewardsScreen)}: CurrencyDisplay element not found.");
            }

            _cardsRow = _root.Q<VisualElement>(UISelectors.DailyRewardsScreen.CARDS_ROW);
            if (_cardsRow == null)
            {
                Debug.LogWarning($"{nameof(DailyRewardsScreen)}: CardsRow element not found.");
            }

            _rerollInfoText = _root.Q<Label>(UISelectors.DailyRewardsScreen.REROLL_INFO_TEXT);
            if (_rerollInfoText == null)
            {
                Debug.LogWarning($"{nameof(DailyRewardsScreen)}: RerollInfoText element not found.");
            }

            _rerollButton = _root.Q<RewardsRerollButton>(UISelectors.DailyRewardsScreen.REROLL_BUTTON);
            if (_rerollButton == null)
            {
                Debug.LogWarning($"{nameof(DailyRewardsScreen)}: RerollButton element not found.");
            }

            // Update UI initially from ViewModel if already set
            if (_viewModel != null)
            {
                UpdateUIFromViewModel();
            }

            if (_rerollButton != null)
            {
                _rerollButton.OnRerollClickedEvent += OnRerollClickedHandler;
            }
            else
            {
                Debug.LogWarning($"{nameof(DailyRewardsScreen)}: RerollButton is null. Cannot subscribe to click event.");
            }

            _rerollInfoText.SetBinding("text", new MultiDependencyBinding
            {
                dependencyPaths = new[] { "MaxRerolls", "RerollCount" }
            }.SetConverter<int, int, string>((MaxRerolls, RerollCount) =>
            {
                int remainingRerolls = MaxRerolls - RerollCount;
                var stringDatabase = LocalizationSettings.StringDatabase;
                var remainingText = remainingRerolls > 0
                    ? stringDatabase.GetLocalizedString(LocalizationKeys.Tables.MAIN, LocalizationKeys.UI.DailyRewards.REROLLS_COUNT_REMAIN, new object[] { remainingRerolls })
                    : stringDatabase.GetLocalizedString(LocalizationKeys.Tables.MAIN, LocalizationKeys.UI.DailyRewards.REROLLS_COUNT_EMPTY);
                return stringDatabase.GetLocalizedString(LocalizationKeys.Tables.MAIN, LocalizationKeys.UI.DailyRewards.REROLLS_COUNT_USED,
                    new object[] { RerollCount, MaxRerolls, remainingText });
            }));
            _rerollInfoText.dataSource = _viewModel;
            SetupCurrencyDisplayBinding();
        }

        /// <summary>
        /// Sets the ViewModel for the Daily Rewards screen.
        /// Subscribes to the ViewModel's state changed event.
        /// </summary>
        /// <param name="vm">The DailyRewardsViewModel instance.</param>
        public void SetViewModel(DailyRewardsViewModel vm)
        {
            // Unsubscribe from previous ViewModel if exists
            if (_viewModel != null)
            {
                _viewModel.OnStateChangedEvent -= OnViewModelStateChangedHandler;
            }
            _viewModel = vm;
            // Subscribe to new ViewModel if it's not null
            if (_viewModel != null)
            {
                _viewModel.OnStateChangedEvent += OnViewModelStateChangedHandler;
            }
            else
            {
                Debug.LogWarning($"{nameof(DailyRewardsScreen)}: SetViewModel called with null ViewModel.");
            }
        }

        private void SetupCurrencyDisplayBinding()
        {
            if (_currencyDisplay != null && _viewModel != null)
            {
                _currencyDisplay.dataSource = _viewModel;
                _currencyDisplay.SetBinding("coins", new AnimateBinding
                {
                    dataSourcePath = new PropertyPath(nameof(DailyRewardsViewModel.CurrentCoins)),
                    bindingMode = BindingMode.ToTarget
                });
                _currencyDisplay.SetBinding("gems", new AnimateBinding
                {
                    dataSourcePath = new PropertyPath(nameof(DailyRewardsViewModel.CurrentGems)),
                    bindingMode = BindingMode.ToTarget
                });
            }
            else
            {
                if (_currencyDisplay == null)
                {
                    Debug.LogWarning($"{nameof(DailyRewardsScreen)}: Cannot setup currency display binding, CurrencyDisplayControl element is null.");
                }

                if (_viewModel == null)
                {
                    Debug.LogWarning($"{nameof(DailyRewardsScreen)}: Cannot setup currency display binding, ViewModel is null.");
                }
            }
        }

        /// <summary>
        /// Called when the behaviour becomes disabled or inactive.
        /// Unregisters event handlers to prevent memory leaks.
        /// </summary>
        void OnDisable()
        {
            // Unsubscribe from ViewModel events
            if (_viewModel != null)
            {
                _viewModel.OnStateChangedEvent -= OnViewModelStateChangedHandler;
            }
            // Unsubscribe from RerollButton event
            if (_rerollButton != null)
            {
                _rerollButton.OnRerollClickedEvent -= OnRerollClickedHandler;
            }

            // Clear bindings
            if (_rerollInfoText != null)
            {
                _rerollInfoText.ClearBinding("text");
            }
            if (_currencyDisplay != null)
            {
                _currencyDisplay.ClearBinding("coins");
                _currencyDisplay.ClearBinding("gems");
            }

            // Clear cards
            if (_cardsRow != null)
            {
                foreach (var child in _cardsRow.Children())
                {
                    if (child is DailyRewardCardControl cardControl)
                    {
                        cardControl.OnClaimEvent -= OnClaimRewardHandler;
                    }
                }
                _cardsRow.Clear();
            }
        }

        /// <summary>
        /// Handles the ViewModel's state changed event.
        /// Triggers a UI update.
        /// </summary>
        private void OnViewModelStateChangedHandler() => UpdateUIFromViewModel();

        /// <summary>
        /// Updates all UI elements based on the current ViewModel data.
        /// </summary>
        private void UpdateUIFromViewModel()
        {
            if (_viewModel == null)
            {
                Debug.LogWarning($"{nameof(DailyRewardsScreen)}: UpdateUIFromViewModel called with null ViewModel.");
                return;
            }
            _rerollButton.SetEnabled(_viewModel.RerollCount < _viewModel.MaxRerolls);
            PopulateCards();
        }

        /// <summary>
        /// Populates or updates the daily reward cards based on the ViewModel's data.
        /// Subscribes to the OnClaim event for each card.
        /// Logs warnings if the cards row or ViewModel is missing.
        /// </summary>
        private void PopulateCards()
        {
            if (_cardsRow == null)
            {
                Debug.LogWarning($"{nameof(DailyRewardsScreen)}: Cannot populate cards, CardsRow element is null.");
                return;
            }
            if (_viewModel == null)
            {
                Debug.LogWarning($"{nameof(DailyRewardsScreen)}: Cannot populate cards, ViewModel is null.");
                _cardsRow.Clear(); // Clear cards if ViewModel is null
                return;
            }

            var cards = _viewModel.Cards;

            // Ensure we have enough card controls or remove excess
            while (_cardsRow.childCount < cards.Count)
            {
                DailyRewardCardControl newCardControl = new DailyRewardCardControl();
                _cardsRow.Add(newCardControl);
                newCardControl.OnClaimEvent += OnClaimRewardHandler; // Subscribe to claim event
            }
            while (_cardsRow.childCount > cards.Count)
            {
                int lastIndex = _cardsRow.childCount - 1;
                DailyRewardCardControl cardControlToRemove = _cardsRow[lastIndex] as DailyRewardCardControl;
                if (cardControlToRemove != null)
                {
                    cardControlToRemove.OnClaimEvent -= OnClaimRewardHandler; // Unsubscribe
                }
                _cardsRow.RemoveAt(lastIndex);
            }

            // Update existing card controls with new data
            for (int i = 0; i < cards.Count; i++)
            {
                DailyRewardCardControl cardControl = _cardsRow[i] as DailyRewardCardControl;
                if (cardControl != null)
                {
                    var cardData = cards[i];
                    cardControl.SetData(cardData);
                }
                else
                {
                    Debug.LogError($"{nameof(DailyRewardsScreen)}: Child element at index {i} is not a DailyRewardCardControl.");
                }
            }
        }

        /// <summary>
        /// Handles the RerollButton click event.
        /// Triggers the reroll action in the ViewModel if allowed.
        /// Logs a warning if the ViewModel is missing or no rerolls are left.
        /// </summary>
        private void OnRerollClickedHandler()
        {
            if (_viewModel == null)
            {
                Debug.LogWarning($"{nameof(DailyRewardsScreen)}: Cannot reroll, ViewModel is null.");
                return;
            }
            if (_viewModel.RerollCount < _viewModel.MaxRerolls)
            {
                _viewModel.Reroll();
                // UI update is handled by ViewModel's OnStateChanged event
            }
            else
            {
                Debug.LogWarning($"{nameof(DailyRewardsScreen)}: Reroll button clicked, but no rerolls left.");
            }
        }

        /// <summary>
        /// Handles the OnClaim event from a DailyRewardCardControl.
        /// Triggers the claim action in the ViewModel.
        /// </summary>
        /// <param name="day">The day number of the claimed reward.</param>
        private void OnClaimRewardHandler(int day) => _viewModel.ClaimReward(day);
    }
}
