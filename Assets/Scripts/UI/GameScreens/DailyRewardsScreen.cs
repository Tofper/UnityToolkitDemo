using Scripts.Data;
using Scripts.UI.Components;
using Scripts.UI.Controls;
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
        private SimpleListView _cardsListView;
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

            // Create and setup SimpleListView for cards
            _cardsListView = new SimpleListView();
            _cardsListView.layoutDirection = FlexDirection.Row;
            _cardsListView.style.flexGrow = 1;
            _root.Q<VisualElement>(UISelectors.DailyRewardsScreen.CARDS_ROW).Add(_cardsListView);

            // Setup card creation and binding
            _cardsListView.makeItem = () => new DailyRewardCardControl();
            _cardsListView.bindItem = (element, index) =>
            {
                if (element is DailyRewardCardControl cardControl && _viewModel != null)
                {
                    var cardData = _viewModel.Cards[index];
                    cardControl.SetData(cardData);
                    cardControl.OnClaimEvent += OnClaimRewardHandler;
                }
            };
            _cardsListView.unbindItem = (element, index) =>
            {
                if (element is DailyRewardCardControl cardControl)
                {
                    cardControl.OnClaimEvent -= OnClaimRewardHandler;
                }
            };

            // Setup data binding for itemsSource
            _cardsListView.dataSource = _viewModel;
            _cardsListView.SetBinding("itemsSource", new DataBinding() { dataSourcePath = new PropertyPath(nameof(DailyRewardsViewModel.Cards)) });

            _rerollInfoText = _root.Q<Label>(UISelectors.DailyRewardsScreen.REROLL_INFO_TEXT);
            if (_rerollInfoText == null)
            {
                Debug.LogWarning($"{nameof(DailyRewardsScreen)}: RerollInfoText element not found.");
            }

            _rerollButton = _root.Q<RewardsRerollButton>(UISelectors.DailyRewardsScreen.REROLL_BUTTON);
            if (_rerollButton != null)
            {
                _rerollButton.OnRerollClickedEvent += OnRerollClickedHandler;
            }
            else
            {
                Debug.LogWarning($"{nameof(DailyRewardsScreen)}: RerollButton element not found.");
            }

            // Update UI initially from ViewModel if already set
            if (_viewModel != null)
            {
                UpdateUIFromViewModel();
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

            // Dispose SimpleListView
            _cardsListView?.Dispose();
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
