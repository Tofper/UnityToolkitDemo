using Scripts.Utilities;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scripts.UI.Components
{
    [UxmlElement]
    /// <summary>
    /// Control for displaying currency values (coins and gems) with animations.
    /// </summary>
    public partial class CurrencyDisplayControl : VisualElement
    {
        // Resource Paths
        private const string UXML_RESOURCE_PATH = "CurrencyDisplay";
        private const string USS_RESOURCE_PATH = "CurrencyDisplay";

        // Animation Constants
        private const float DEFAULT_ANIMATION_DURATION = 0.5f; // seconds

        private Label _coinsValueLabel;
        private Label _gemsValueLabel;
        private int _coins;
        private int _gems;

        /// <summary>
        /// The animation duration for currency value changes.
        /// Exposed for flexibility and UXML binding.
        /// </summary>
        [UxmlAttribute("animation-duration")]
        public float AnimationDuration { get; set; } = DEFAULT_ANIMATION_DURATION;

        /// <summary>
        /// The current number of coins. Animates the displayed value when changed.
        /// </summary>
        [CreateProperty]
        public int coins
        {
            get => _coins;
            set
            {
                _coins = value;
                _coinsValueLabel.text = value.ToString();
            }
        }

        /// <summary>
        /// The current number of gems. Animates the displayed value when changed.
        /// </summary>
        [CreateProperty]
        public int gems
        {
            get => _gems;
            set
            {
                _gems = value;
                _gemsValueLabel.text = value.ToString();
            }
        }
        /// <summary>
        /// Constructor for the CurrencyDisplayControl.
        /// Loads UXML and USS, initializes elements, and registers callbacks.
        /// Logs warnings if resources are not found.
        /// </summary>
        public CurrencyDisplayControl()
        {
            // Load UXML and USS from Resources
            var visualTree = Resources.Load<VisualTreeAsset>(UXML_RESOURCE_PATH);
            if (visualTree != null)
            {
                var instance = visualTree.Instantiate();
                hierarchy.Add(instance);
            }
            else
            {
                Debug.LogWarning($"{nameof(CurrencyDisplayControl)}: Could not load VisualTreeAsset at '{UXML_RESOURCE_PATH}'. Ensure it is in a Resources folder.");
            }

            var styleSheet = Resources.Load<StyleSheet>(USS_RESOURCE_PATH);
            if (styleSheet != null)
            {
                styleSheets.Add(styleSheet);
            }
            else
            {
                Debug.LogWarning($"{nameof(CurrencyDisplayControl)}: Could not load StyleSheet at '{USS_RESOURCE_PATH}'. Ensure it is in a Resources folder.");
            }

            InitializeElements();
            RegisterCallbacks();
            // Initialize with 0 and let the setter handle the initial display
            coins = 0;
            gems = 0;
        }

        /// <summary>
        /// Queries and initializes the UI elements.
        /// Adds warnings if elements are not found.
        /// </summary>
        private void InitializeElements()
        {
            _coinsValueLabel = this.Q<Label>(UISelectors.CurrencyDisplay.COINS_VALUE_LABEL);
            if (_coinsValueLabel == null)
            {
                Debug.LogWarning($"{nameof(CurrencyDisplayControl)}: Element '{UISelectors.CurrencyDisplay.COINS_VALUE_LABEL}' not found. Currency display may not function correctly.");
            }

            _gemsValueLabel = this.Q<Label>(UISelectors.CurrencyDisplay.GEMS_VALUE_LABEL);
            if (_gemsValueLabel == null)
            {
                Debug.LogWarning($"{nameof(CurrencyDisplayControl)}: Element '{UISelectors.CurrencyDisplay.GEMS_VALUE_LABEL}' not found. Currency display may not function correctly.");
            }
        }

        /// <summary>
        /// Registers event callbacks for the control.
        /// </summary>
        private void RegisterCallbacks() => RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

        /// <summary>
        /// Unregisters event callbacks and stops coroutines when the element is detached.
        /// </summary>
        /// <param name="e">The detach event data.</param>
        private void OnDetachFromPanel(DetachFromPanelEvent e)
        {
            UnregisterCallbacks();
            // Stop any running coroutines to prevent issues after detach
        }

        /// <summary>
        /// Unregisters event callbacks.
        /// </summary>
        private void UnregisterCallbacks() => UnregisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
    }
}