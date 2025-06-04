using System;
using Scripts.Utilities;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scripts.UI.Components
{
    /// <summary>
    /// A custom UI Toolkit button for rerolling rewards, with dice emoji and animated states.
    /// - Emoji is used for the dice icon (no SVG/image).
    /// - Dice emoji rotates on press for feedback.
    /// - Can be positioned absolutely by parent or via C# API.
    /// - Exposes a public event for reroll clicks.
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    [UxmlElement]
    public partial class RewardsRerollButton : Button
    {
        // Animation Constants
        private const float DEFAULT_ANIMATION_DURATION = 0.1f;
        private const int TARGET_FPS = 60;
        private const int FRAME_INTERVAL_MS = 1000 / TARGET_FPS; // ~16.67ms per frame
        // Resource Paths
        private const string RESOURCE_PATH = "RewardsRerollButton";

        /// <summary>
        /// The duration of the dice reroll animation in seconds.
        /// Exposed for flexibility and UXML binding.
        /// </summary>
        [UxmlAttribute]
        public float AnimationDuration { get; set; } = DEFAULT_ANIMATION_DURATION;

        private VisualElement _diceWrapper;
        private Label _diceIcon;
        private Label _label;
        private IVisualElementScheduledItem _animationItem;
        private float _animationElapsed = 0f;
        private bool _isAnimating = false;

        private EButtonState _currentState = EButtonState.Normal;

        /// <summary>
        /// Represents the possible visual states of the button.
        /// </summary>
        private enum EButtonState
        {
            Normal,
            Hovered,
            Pressed,
            Animating,
            Disabled
        }

        /// <summary>
        /// Event fired when the reroll button is clicked.
        /// </summary>
        public event Action OnRerollClickedEvent;

        /// <summary>
        /// Constructor for the RewardsRerollButton.
        /// Initializes the button's appearance, loads UXML, queries elements, and registers event callbacks.
        /// </summary>
        public RewardsRerollButton()
        {
            // Clear default button content (label, etc.)
            Clear();

            // Load and clone UXML into this button
            var visualTree = Resources.Load<VisualTreeAsset>(RESOURCE_PATH);
            if (visualTree != null)
            {
                visualTree.CloneTree(this);
            }
            else
            {
                Debug.LogWarning($"{nameof(RewardsRerollButton)}: Could not load VisualTreeAsset at '{RESOURCE_PATH}'.");
            }
            // Query elements from UXML
            InitializeElements();

            // Register events
            RegisterCallbacks();

            // Set initial state
            SetState(EButtonState.Normal);
        }

        /// <summary>
        /// Queries and initializes the UI elements of the button.
        /// Adds warnings if elements are not found.
        /// </summary>
        private void InitializeElements()
        {
            _diceWrapper = this.Q<VisualElement>(UISelectors.RewardsRerollButton.DICE_WRAPPER);
            if (_diceWrapper == null)
            {
                Debug.LogWarning($"{nameof(RewardsRerollButton)}: Element '{UISelectors.RewardsRerollButton.DICE_WRAPPER}' not found.");
            }

            _diceIcon = this.Q<Label>(UISelectors.RewardsRerollButton.DICE_ICON);
            if (_diceIcon == null)
            {
                Debug.LogWarning($"{nameof(RewardsRerollButton)}: Element '{UISelectors.RewardsRerollButton.DICE_ICON}' not found.");
            }

            _label = this.Q<Label>(UISelectors.RewardsRerollButton.LABEL);
            if (_label == null)
            {
                Debug.LogWarning($"{nameof(RewardsRerollButton)}: Element '{UISelectors.RewardsRerollButton.LABEL}' not found.");
            }
        }

        private void SetState(EButtonState newState)
        {
            if (_currentState == newState)
            {
                return;
            }

            Debug.Log($"{nameof(RewardsRerollButton)}: State changing from {_currentState} to {newState}");

            // Remove old state classes
            RemoveFromClassList(UISelectors.RewardsRerollButton.PRESSED_CLASS);
            RemoveFromClassList(UISelectors.RewardsRerollButton.HOVERED_CLASS);

            // Add new state classes if applicable
            if (newState == EButtonState.Pressed)
            {
                AddToClassList(UISelectors.RewardsRerollButton.PRESSED_CLASS);
            }

            if (newState == EButtonState.Hovered)
            {
                AddToClassList(UISelectors.RewardsRerollButton.HOVERED_CLASS);
            }

            // Set picking mode based on state
            pickingMode = newState is EButtonState.Disabled or EButtonState.Animating
                ? PickingMode.Ignore
                : PickingMode.Position;

            _currentState = newState;
        }

        /// <summary>
        /// Sets whether the button is enabled or disabled.
        /// </summary>
        public new void SetEnabled(bool enabled)
        {
            Debug.Log($"{nameof(RewardsRerollButton)}: SetEnabled called with {enabled}, current state: {_currentState}");
            base.SetEnabled(enabled);

            if (enabled && _currentState != EButtonState.Animating)
            {
                SetState(EButtonState.Normal);
            }
            else if (!enabled)
            {
                SetState(EButtonState.Disabled);
            }
        }

        /// <summary>
        /// Registers event callbacks for the button.
        /// </summary>
        private void RegisterCallbacks()
        {
            RegisterCallback<PointerDownEvent>(OnPointerDownRoll);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerOverEvent>(OnPointerOver);
            RegisterCallback<PointerOutEvent>(OnPointerOut);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            clicked += InternalRerollClicked;
        }

        /// <summary>
        /// Unregisters event callbacks and stops animation on detach.
        /// </summary>
        /// <param name="e">The detach event data.</param>
        private void OnDetachFromPanel(DetachFromPanelEvent e)
        {
            StopDiceAnimation();
            UnregisterCallbacks();
        }

        /// <summary>
        /// Unregisters event callbacks.
        /// </summary>
        private void UnregisterCallbacks()
        {
            UnregisterCallback<PointerDownEvent>(OnPointerDownRoll);
            UnregisterCallback<PointerUpEvent>(OnPointerUp);
            UnregisterCallback<PointerOverEvent>(OnPointerOver);
            UnregisterCallback<PointerOutEvent>(OnPointerOut);
            UnregisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            clicked -= InternalRerollClicked;
        }

        /// <summary>
        /// Called when the button is clicked. Override or subscribe to OnRerollClickedEvent.
        /// </summary>
        private void OnRerollClicked()
        {
            OnRerollClickedEvent?.Invoke();
        }

        private void InternalRerollClicked()
        {
            Debug.Log($"{nameof(RewardsRerollButton)}: Reroll clicked, current state: {_currentState}, enabled: {enabledSelf}");
            // Check if currently animating or disabled before allowing click logic
            if (_currentState is EButtonState.Animating or EButtonState.Disabled)
            {
                Debug.Log($"{nameof(RewardsRerollButton)}: Ignoring click - button is {_currentState}");
                return;
            }

            OnRerollClicked();
            StartDiceAnimation();
        }

        /// <summary>
        /// Handles the pointer down event to start the animation and change state.
        /// </summary>
        /// <param name="evt">The pointer down event data.</param>
        private void OnPointerDownRoll(PointerDownEvent evt)
        {
            // Only change to pressed state if not disabled or animating
            if (_currentState is not (EButtonState.Disabled or EButtonState.Animating))
            {
                SetState(EButtonState.Pressed);
            }
        }

        /// <summary>
        /// Handles the pointer up event to change state.
        /// </summary>
        /// <param name="evt">The pointer up event data.</param>
        private void OnPointerUp(PointerUpEvent evt)
        {
            // Don't change state if disabled or animating
            if (_currentState is EButtonState.Disabled or EButtonState.Animating)
            {
                return;
            }

            // Simple state transition: if pointer is over element, go to hovered, otherwise normal
            bool isPointerOver = ContainsPoint(evt.localPosition);
            SetState(isPointerOver ? EButtonState.Hovered : EButtonState.Normal);
        }

        /// <summary>
        /// Handles the pointer over event to change state.
        /// </summary>
        /// <param name="evt">The pointer over event data.</param>
        private void OnPointerOver(PointerOverEvent evt)
        {
            // Only transition to Hovered if in Normal state
            if (_currentState == EButtonState.Normal)
            {
                SetState(EButtonState.Hovered);
            }
        }

        /// <summary>
        /// Handles the pointer out event to change state.
        /// </summary>
        /// <param name="evt">The pointer out event data.</param>
        private void OnPointerOut(PointerOutEvent evt)
        {
            // Only transition to Normal if currently Hovered
            if (_currentState == EButtonState.Hovered)
            {
                SetState(EButtonState.Normal);
            }
        }

        /// <summary>
        /// Starts the dice emoji animation using keyframes.
        /// Changes state to Animating.
        /// </summary>
        private void StartDiceAnimation()
        {
            if (_currentState == EButtonState.Animating)
            {
                return;
            }

            Debug.Log($"{nameof(RewardsRerollButton)}: Starting animation, current state: {_currentState}");
            _isAnimating = true;
            SetState(EButtonState.Animating);
            _animationElapsed = 0f;
            // (normalizedTime, position.xy, rotation.z, scale.xy)
            var frames = new[]
            {
                (0.0f,    new Vector2(0, 0),    0f,   1f),
                (0.08f,   new Vector2(8, -5),  40f,  1.10f),
                (0.16f,   new Vector2(14, -8), 80f,  1.13f),
                (0.24f,   new Vector2(20, -4), 120f, 1.15f),
                (0.32f,   new Vector2(14, 0),  160f, 1.13f),
                (0.48f,   new Vector2(11, -2), 110f, 1.09f),
                (0.64f,   new Vector2(7, 0),   70f,  1.04f),
                (0.80f,   new Vector2(2, -2), 30f,  1.01f),
                (0.88f,   new Vector2(0, 0),  15f,  1.00f),
                (1.0f,    new Vector2(0, 0),  0f,   1f)
            };
            int frameIndex = 0;

            // Ensure any previous animation is stopped
            _animationItem?.Pause();

            Debug.Log($"{nameof(RewardsRerollButton)}: StartDiceAnimation!!");

            _animationItem = schedule.Execute(() =>
            {
                _animationElapsed += Time.deltaTime;
                float normalizedTime = _animationElapsed / AnimationDuration;

                // Find the two frames to interpolate between
                while (frameIndex < frames.Length - 1 && normalizedTime > frames[frameIndex + 1].Item1)
                {
                    frameIndex++;
                }

                var (t0, pos0, rot0, scale0) = frames[frameIndex];
                var (t1, pos1, rot1, scale1) = frameIndex < frames.Length - 1 ? frames[frameIndex + 1] : frames[frameIndex];
                float segmentProgress = (normalizedTime - t0) / (t1 - t0);

                // Interpolate transform properties
                if (_diceWrapper != null)
                {
                    _diceWrapper.style.translate = new Translate(Mathf.Lerp(pos0.x, pos1.x, segmentProgress), Mathf.Lerp(pos0.y, pos1.y, segmentProgress), 0);
                    _diceWrapper.style.rotate = new Rotate(Mathf.Lerp(rot0, rot1, segmentProgress));
                    float currentScale = Mathf.Lerp(scale0, scale1, segmentProgress);
                    _diceWrapper.style.scale = new Scale(new Vector3(currentScale, currentScale, 1));
                }

                // Check for animation completion
                if (normalizedTime >= 1f)
                {
                    StopDiceAnimation();
                    // Transition back to Normal or Hovered state after animation
                    SetState(EButtonState.Normal);
                }
            }).StartingIn(0).Every(FRAME_INTERVAL_MS).Until(() => !_isAnimating);
        }

        /// <summary>
        /// Stops the dice emoji animation.
        /// Changes state back to Normal or Hovered.
        /// </summary>
        private void StopDiceAnimation()
        {
            Debug.Log($"{nameof(RewardsRerollButton)}: Stopping animation, current state: {_currentState}, enabled: {enabledSelf}");
            _isAnimating = false;
            _animationItem?.Pause();
            _animationItem = null;
            _animationElapsed = 0f;

            // Ensure final state is applied (reset transform)
            if (_diceWrapper != null)
            {
                _diceWrapper.style.translate = new Translate(0, 0, 0);
                _diceWrapper.style.rotate = new Rotate(0);
                _diceWrapper.style.scale = new Scale(Vector3.one);
            }
            else
            {
                Debug.LogWarning($"{nameof(RewardsRerollButton)}: Cannot reset dice wrapper transform, element not found.");
            }

            // Return to normal state after animation completes
            if (enabledSelf)
            {
                SetState(EButtonState.Normal);
            }
            else
            {
                SetState(EButtonState.Disabled);
            }
        }
    }
}
