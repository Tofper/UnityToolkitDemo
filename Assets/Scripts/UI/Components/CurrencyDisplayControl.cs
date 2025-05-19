using System;
using System.Collections;
using Scripts.Utilities;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scripts.UI.Components
{
    [UnityEngine.Scripting.Preserve]
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
        private bool _coinsInitialized = false;
        private bool _gemsInitialized = false;
        private int _coins;
        private int _gems;
        private Coroutine _coinsAnimCoroutine;
        private Coroutine _gemsAnimCoroutine;

        /// <summary>
        /// The animation duration for currency value changes.
        /// Exposed for flexibility and UXML binding.
        /// </summary>
        [UxmlAttribute("animation-duration")]
        public float AnimationDuration { get; set; } = DEFAULT_ANIMATION_DURATION;

        /// <summary>
        /// The current number of coins. Animates the displayed value when changed.
        /// </summary>
        public int coins
        {
            get => _coins;
            set
            {
                if (!_coinsInitialized)
                {
                    if (_coinsValueLabel != null)
                    {
                        _coinsValueLabel.text = value.ToString();
                    }
                    else
                    {
                        Debug.LogWarning($"{nameof(CurrencyDisplayControl)}: Initial coin value set ({value}), but _coinsValueLabel is null.");
                    }

                    _coins = value;
                    _coinsInitialized = true;
                    return;
                }
                if (value == _coins)
                {
                    return;
                }
                Debug.Log($"[{nameof(CurrencyDisplayControl)}] coins: Updating from {_coins} to {value}");

                if (Application.isPlaying)
                {
                    if (_coinsAnimCoroutine != null)
                    {
                        UIToolkitCoroutineHelper.StopUIToolkitCoroutine(_coinsAnimCoroutine);
                    }

                    Debug.Log($"[{nameof(CurrencyDisplayControl)}] coins: Starting animation from {_coins} to {value}");
                    if (_coinsValueLabel != null)
                    {
                        _coinsAnimCoroutine = UIToolkitCoroutineHelper.StartUIToolkitCoroutine(
                           AnimateNumber(_coins, value, v =>
                           {
                               if (_coinsValueLabel != null)
                               {
                                   _coinsValueLabel.text = Mathf.FloorToInt(v).ToString();
                               }
                           }, AnimationDuration)
                       );
                    }
                    else
                    {
                        Debug.LogWarning($"{nameof(CurrencyDisplayControl)}: Cannot animate coins, _coinsValueLabel is null.");
                    }
                }
                else
                {
                    if (_coinsValueLabel != null)
                    {
                        _coinsValueLabel.text = value.ToString();
                    }
                    else
                    {
                        Debug.LogWarning($"{nameof(CurrencyDisplayControl)}: Cannot set coins text, _coinsValueLabel is null.");
                    }
                }
                _coins = value;
            }
        }

        /// <summary>
        /// The current number of gems. Animates the displayed value when changed.
        /// </summary>
        public int gems
        {
            get => _gems;
            set
            {
                if (!_gemsInitialized)
                {
                    Debug.Log($"[{nameof(CurrencyDisplayControl)}] gems: Initializing to {value}");
                    if (_gemsValueLabel != null)
                    {
                        _gemsValueLabel.text = value.ToString();
                    }
                    else
                    {
                        Debug.LogWarning($"{nameof(CurrencyDisplayControl)}: Initial gem value set ({value}), but _gemsValueLabel is null.");
                    }

                    _gems = value;
                    _gemsInitialized = true;
                    return;
                }
                if (value == _gems)
                {
                    return;
                }

                if (Application.isPlaying)
                {
                    if (_gemsAnimCoroutine != null)
                    {
                        UIToolkitCoroutineHelper.StopUIToolkitCoroutine(_gemsAnimCoroutine);
                    }

                    if (_gemsValueLabel != null)
                    {
                        _gemsAnimCoroutine = UIToolkitCoroutineHelper.StartUIToolkitCoroutine(
                           AnimateNumber(_gems, value, v =>
                           {
                               if (_gemsValueLabel != null)
                               {
                                   _gemsValueLabel.text = Mathf.FloorToInt(v).ToString();
                               }
                           }, AnimationDuration)
                       );
                    }
                    else
                    {
                        Debug.LogWarning($"{nameof(CurrencyDisplayControl)}: Cannot animate gems, _gemsValueLabel is null.");
                    }
                }
                else
                {
                    if (_gemsValueLabel != null)
                    {
                        _gemsValueLabel.text = value.ToString();
                    }
                    else
                    {
                        Debug.LogWarning($"{nameof(CurrencyDisplayControl)}: Cannot set gems text, _gemsValueLabel is null.");
                    }
                }
                _gems = value;
            }
        }

        /// <summary>
        /// Coroutine to animate a number change over time.
        /// </summary>
        /// <param name="from">Starting value.</param>
        /// <param name="to">Target value.</param>
        /// <param name="onUpdate">Action to call with the current animated value.</param>
        /// <param name="duration">Duration of the animation in seconds.</param>
        /// <returns>IEnumerator for the coroutine.</returns>
        private IEnumerator AnimateNumber(float from, float to, Action<float> onUpdate, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float val = Mathf.Lerp(from, to, t);
                onUpdate(val);
                yield return null;
            }
            onUpdate(to);
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

            _coinsInitialized = false;
            _gemsInitialized = false;
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
            if (_coinsAnimCoroutine != null)
            {
                UIToolkitCoroutineHelper.StopUIToolkitCoroutine(_coinsAnimCoroutine);
            }

            if (_gemsAnimCoroutine != null)
            {
                UIToolkitCoroutineHelper.StopUIToolkitCoroutine(_gemsAnimCoroutine);
            }
        }

        /// <summary>
        /// Unregisters event callbacks.
        /// </summary>
        private void UnregisterCallbacks() => UnregisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
    }
}