using System;
using Scripts.Utilities;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scripts.UI.Components
{
    [UnityEngine.Scripting.Preserve]
    [UxmlElement]
    /// <summary>
    /// A premium button control for UI Toolkit.
    /// </summary>
    public partial class PremiumButtonControl : VisualElement
    {
        // Animation Constants
        private const float DEFAULT_ANIMATION_DURATION = 0.5f; // seconds
        private const string RESOURCE_PATH = "PremiumButton";

        private Button _button;
        private VisualElement _shadow;

        private string _label = LocalizationKeys.UI.Components.PREMIUM_BUTTON_CLAIM;
        /// <summary>
        /// The button label text. Can be set via UXML attribute "label".
        /// </summary>
        [UxmlAttribute("label")]
        public string label
        {
            get => _label;
            set
            {
                _label = value;
                SetText(_label);
            }
        }

        /// <summary>
        /// Animation duration for button effects.
        /// Exposed for flexibility and UXML binding via attribute "animation-duration".
        /// </summary>
        [UxmlAttribute("animation-duration")]
        public float AnimationDuration { get; set; } = DEFAULT_ANIMATION_DURATION;

        /// <summary>
        /// Event triggered when the button is clicked.
        /// External classes can subscribe to this event.
        /// </summary>
        public event Action OnClickedEvent;

        /// <summary>
        /// Defines the possible visual states of the premium button's shadow.
        /// These correspond to USS classes for styling.
        /// </summary>
        private enum EButtonState
        {
            Default,
            Hover,
            Pressed,
            Disabled
        }

        /// <summary>
        /// Constructor for the PremiumButtonControl.
        /// Loads UXML, queries elements, and registers event callbacks.
        /// Logs warnings if resources or required elements are not found.
        /// </summary>
        public PremiumButtonControl()
        {
            // Load the UXML from Resources
            var visualTree = Resources.Load<VisualTreeAsset>(RESOURCE_PATH);
            if (visualTree != null)
            {
                var instance = visualTree.Instantiate();
                hierarchy.Add(instance);

                // Query elements
                _button = this.Q<Button>(UISelectors.PremiumButton.BUTTON);
                _shadow = this.Q<VisualElement>(UISelectors.PremiumButton.SHADOW);

                // Log warnings if elements are missing
                if (_button == null)
                    Debug.LogWarning($"{nameof(PremiumButtonControl)}: Button element with name '{UISelectors.PremiumButton.BUTTON}' not found in UXML. Button will not be interactive.");
                if (_shadow == null)
                    Debug.LogWarning($"{nameof(PremiumButtonControl)}: Shadow element with name '{UISelectors.PremiumButton.SHADOW}' not found in UXML.");

                // Register events if button element is found
                if (_button != null)
                {
                    _button.clicked += OnButtonClicked;
                    _button.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
                    _button.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
                    _button.RegisterCallback<PointerDownEvent>(OnPointerDown);
                    _button.RegisterCallback<PointerUpEvent>(OnPointerUp);
                    // Add DetachFromPanelEvent for cleanup
                    RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
                }

                // Set initial label and shadow state
                SetText(_label);
                SetButtonState(_button != null && _button.enabledSelf ? EButtonState.Default : EButtonState.Disabled); // Set initial state based on button enabled status

            }
            else
            {
                Debug.LogWarning($"{nameof(PremiumButtonControl)}: Could not load VisualTreeAsset at '{UISelectors.PremiumButton.RESOURCE_PATH}'. Ensure it is in a Resources folder.");
            }
        }

        /// <summary>
        /// Unregisters event callbacks and cleans up when the element is detached from the panel.
        /// </summary>
        /// <param name="e">The detach event data.</param>
        private void OnDetachFromPanel(DetachFromPanelEvent e)
        {
            UnregisterCallbacks();
        }

        /// <summary>
        /// Unregisters event callbacks.
        /// </summary>
        private void UnregisterCallbacks()
        {
            if (_button != null)
            {
                _button.clicked -= OnButtonClicked;
                _button.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
                _button.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
                _button.UnregisterCallback<PointerDownEvent>(OnPointerDown);
                _button.UnregisterCallback<PointerUpEvent>(OnPointerUp);
                UnregisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            }
        }

        /// <summary>
        /// Handles the base button click event and invokes the public Clicked action.
        /// </summary>
        private void OnButtonClicked()
        {
            Debug.Log($"{nameof(PremiumButtonControl)}: Base button clicked! Invoking Clicked event.");
            OnClickedEvent?.Invoke(); // Invoke the public event
            // Removed the original redispatching logic as invoking the event is sufficient for external subscribers.
        }

        /// <summary>
        /// Handles the pointer enter event to transition the shadow state.
        /// </summary>
        /// <param name="evt">The pointer enter event data.</param>
        private void OnPointerEnter(PointerEnterEvent evt)
        {
            if (_button != null && _button.enabledSelf) // Check if button element exists and is enabled
            {
                Debug.Log($"{nameof(PremiumButtonControl)}: Pointer Enter. Setting shadow state to Hover.");
                SetButtonState(EButtonState.Hover);
            }
        }

        /// <summary>
        /// Handles the pointer leave event to transition the shadow state.
        /// </summary>
        /// <param name="evt">The pointer leave event data.</param>
        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            if (_button != null && _button.enabledSelf) // Check if button element exists and is enabled
            {
                Debug.Log($"{nameof(PremiumButtonControl)}: Pointer Leave. Setting shadow state to Default.");
                SetButtonState(EButtonState.Default);
            }
        }

        /// <summary>
        /// Handles the pointer down event to transition the shadow state.
        /// </summary>
        /// <param name="evt">The pointer down event data.</param>
        private void OnPointerDown(PointerDownEvent evt)
        {
            if (_button != null && _button.enabledSelf && evt.button == 0) // Check element, enabled state, and left mouse button
            {
                Debug.Log($"{nameof(PremiumButtonControl)}: Pointer Down. Setting shadow state to Pressed.");
                SetButtonState(EButtonState.Pressed);
            }
        }

        /// <summary>
        /// Handles the pointer up event to transition the shadow state.
        /// </summary>
        /// <param name="evt">The pointer up event data.</param>
        private void OnPointerUp(PointerUpEvent evt)
        {
            if (_button != null && _button.enabledSelf && evt.button == 0) // Check element, enabled state, and left mouse button
            {
                Debug.Log($"{nameof(PremiumButtonControl)}: Pointer Up. Setting state to Hover if enabled, as PointerLeave will handle transition to Default if pointer is not over.");
                SetButtonState(EButtonState.Hover); // Transition from Pressed to Hover if enabled
            }
        }

        /// <summary>
        /// Sets the visual state of the button's shadow by applying/removing USS classes.
        /// Logs a warning if the shadow element is not found.
        /// </summary>
        /// <param name="state">The desired shadow state.</param>
        private void SetButtonState(EButtonState state)
        {
            if (_shadow == null)
            {
                Debug.LogWarning($"{nameof(PremiumButtonControl)}: Cannot set shadow state to {state}, shadow element not found.");
                return;
            }
            Debug.Log($"{nameof(PremiumButtonControl)}: Setting shadow state to {state}.");
            _shadow.EnableInClassList(UISelectors.PremiumButton.SHADOW_DEFAULT_CLASS, state == EButtonState.Default);
            _shadow.EnableInClassList(UISelectors.PremiumButton.SHADOW_HOVER_CLASS, state == EButtonState.Hover);
            _shadow.EnableInClassList(UISelectors.PremiumButton.SHADOW_PRESSED_CLASS, state == EButtonState.Pressed);
            _shadow.EnableInClassList(UISelectors.PremiumButton.SHADOW_DISABLED_CLASS, state == EButtonState.Disabled);
        }

        /// <summary>
        /// Updates the shadow state based on the button's current enabled state.
        /// Should be called when the button's enabled state changes.
        /// Logs a warning if the button element is not found.
        /// </summary>
        private void UpdateButtonStateFromEnabled()
        {
            if (_button == null)
            {
                Debug.LogWarning($"{nameof(PremiumButtonControl)}: Cannot update shadow state, button element not found.");
                return;
            }
            Debug.Log($"{nameof(PremiumButtonControl)}: Updating shadow state from enabled state: {_button.enabledSelf}.");
            SetButtonState(_button.enabledSelf ? EButtonState.Default : EButtonState.Disabled);
        }

        /// <summary>
        /// Sets the text label of the internal button element.
        /// Logs a warning if the button element is not found.
        /// </summary>
        /// <param name="text">The text to display on the button.</param>
        private void SetText(string text)
        {
            if (_button != null)
            {
                Debug.Log($"{nameof(PremiumButtonControl)}: Setting button text to '{text}'.");
                _button.text = text;
            }
            else
            {
                Debug.LogWarning($"{nameof(PremiumButtonControl)}: Cannot set button text to '{text}', button element not found.");
            }
        }

        /// <summary>
        /// Enables or disables the premium button.
        /// Updates the shadow state accordingly.
        /// Logs a warning if the button element is not found.
        /// </summary>
        /// <param name="enabled">True to enable, false to disable.</param>
        public void SetPremiumButtonEnabled(bool enabled)
        {
            if (_button != null)
            {
                Debug.Log($"{nameof(PremiumButtonControl)}: Setting enabled state to {enabled}.");
                _button.SetEnabled(enabled);
                UpdateButtonStateFromEnabled(); // Update shadow visual based on new enabled state
            }
            else
            {
                Debug.LogWarning($"{nameof(PremiumButtonControl)}: Cannot set enabled state to {enabled}, button element not found.");
            }
        }
    }
}
