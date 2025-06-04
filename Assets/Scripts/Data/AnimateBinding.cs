using System;
using System.Collections;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scripts.Data
{
    [UxmlObject]
    public partial class AnimateBinding : DataBinding
    {
        [UxmlAttribute]
        public float AnimationDuration { get; set; } = 0.5f;

        private float _currentValue;
        private IVisualElementScheduledItem _scheduledItem;
        private float _startValue;
        private float _targetValue;
        private float _startTime;
        private bool _isFirstUpdate = true;
        private VisualElement _targetElement;
        private PropertyPath _bindingId;

        public AnimateBinding()
        {
            updateTrigger = BindingUpdateTrigger.OnSourceChanged;
        }

        protected override void OnDeactivated(in BindingActivationContext context)
        {
            base.OnDeactivated(context);
            StopAnimation();
            _targetElement = null;
            _bindingId = default;
            _isFirstUpdate = true;
        }

        private void StopAnimation()
        {
            if (_scheduledItem != null)
            {
                _scheduledItem.Pause();
                _scheduledItem = null;
            }
            _currentValue = 0f;
            _startValue = 0f;
            _targetValue = 0f;
            _startTime = 0f;
        }

        protected override BindingResult UpdateUI<TValue>(in BindingContext context, ref TValue value)
        {
            if (value == null)
            {
                Debug.LogError("AnimateBinding: Value is null");
                return new BindingResult(BindingStatus.Failure, "Value is null");
            }

            // Store context values for the animation
            _targetElement = context.targetElement;
            _bindingId = new PropertyPath(context.bindingId);
            float newValue;
            try
            {
                newValue = Convert.ToSingle(value);
            }
            catch (Exception ex)
            {
                Debug.LogError($"AnimateBinding: Failed to convert value to float: {ex.Message}");
                return new BindingResult(BindingStatus.Failure, $"Failed to convert value to float: {ex.Message}");
            }

            float targetValue = newValue;
            if (Mathf.Approximately(_currentValue, targetValue))
            {
                Debug.Log("AnimateBinding: Values are approximately equal, skipping animation");
                return new BindingResult(BindingStatus.Success);
            }

            // For first update, set value immediately without animation
            if (_isFirstUpdate)
            {
                Debug.Log($"AnimateBinding: First update, setting value {targetValue} immediately");
                _currentValue = targetValue;
                var element = _targetElement;
                if (!ConverterGroups.TrySetValueGlobal(ref element, _bindingId, _currentValue, out var errorCode))
                {
                    return new BindingResult(BindingStatus.Failure, $"Failed to set initial value: {errorCode}");
                }
                _isFirstUpdate = false;
                return new BindingResult(BindingStatus.Success);
            }

            try
            {
                StopAnimation();

                _startValue = _currentValue;
                _targetValue = targetValue;
                _startTime = Time.time;
                _scheduledItem = context.targetElement.schedule.Execute(UpdateAnimation).Every(0);
            }
            catch (Exception ex)
            {
                Debug.LogError($"AnimateBinding: Failed to start animation: {ex.Message}");
                return new BindingResult(BindingStatus.Failure, $"Failed to start animation: {ex.Message}");
            }

            return new BindingResult(BindingStatus.Success);
        }

        protected override BindingResult UpdateSource<TValue>(in BindingContext context, ref TValue value)
        {
            // For this binding, we don't need to handle source updates
            return new BindingResult(BindingStatus.Success);
        }

        private void UpdateAnimation()
        {
            float elapsedTime = Time.time - _startTime;
            float animationProgress = Mathf.Clamp01(elapsedTime / AnimationDuration);
            _currentValue = Mathf.Lerp(_startValue, _targetValue, animationProgress);

            // Set the value back to the UI element
            var element = _targetElement;
            if (!ConverterGroups.TrySetValueGlobal(ref element, _bindingId, _currentValue, out var errorCode))
            {
                Debug.LogError($"AnimateBinding: Failed to update UI during animation: {errorCode}");
                _scheduledItem.Pause();
                return;
            }

            if (animationProgress >= 1f)
            {
                _scheduledItem.Pause();
            }
        }
    }
}
