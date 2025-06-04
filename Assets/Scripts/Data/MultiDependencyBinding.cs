using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlObject]
public partial class MultiDependencyBinding : CustomBinding
{
    [UxmlAttribute]
    public string[] dependencyPaths = Array.Empty<string>();

    [UxmlAttribute]
    public string sourcePath = "";

    private Func<object[], object> customConverter = null;
    private readonly Dictionary<string, object> _lastValues = new Dictionary<string, object>();
    private INotifyBindablePropertyChanged _notifySource;
    private Action<object[]> _callback;

    public MultiDependencyBinding()
    {
        updateTrigger = BindingUpdateTrigger.OnSourceChanged;
    }

    public MultiDependencyBinding AddCallback<T1, T2, T3>(Action<T1, T2, T3> callback)
    {
        _callback = args =>
        {
            try
            {
                if (args.Length >= 3)
                {
                    var arg1 = (T1)Convert.ChangeType(args[0], typeof(T1));
                    var arg2 = (T2)Convert.ChangeType(args[1], typeof(T2));
                    var arg3 = (T3)Convert.ChangeType(args[2], typeof(T3));
                    callback(arg1, arg2, arg3);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"MultiDependencyBinding: Error in callback: {ex.Message}");
            }
        };
        return this;
    }

    public MultiDependencyBinding AddCallback<T1, T2>(Action<T1, T2> callback)
    {
        _callback = args =>
        {
            try
            {
                if (args.Length >= 2)
                {
                    var arg1 = (T1)Convert.ChangeType(args[0], typeof(T1));
                    var arg2 = (T2)Convert.ChangeType(args[1], typeof(T2));
                    callback(arg1, arg2);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"MultiDependencyBinding: Error in callback: {ex.Message}");
            }
        };
        return this;
    }

    public MultiDependencyBinding AddCallback<T>(Action<T> callback)
    {
        _callback = args =>
        {
            try
            {
                if (args.Length >= 1)
                {
                    var arg = (T)Convert.ChangeType(args[0], typeof(T));
                    callback(arg);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"MultiDependencyBinding: Error in callback: {ex.Message}");
            }
        };
        return this;
    }

    public MultiDependencyBinding SetConverter<TResult>(Func<object[], TResult> converter)
    {
        customConverter = args => converter(args);
        return this;
    }

    public MultiDependencyBinding SetConverter<T, TResult>(Func<T, TResult> converter)
    {
        customConverter = args => converter((T)(args.FirstOrDefault() ?? default(T)));
        return this;
    }

    public MultiDependencyBinding SetConverter<T1, T2, TResult>(Func<T1, T2, TResult> converter)
    {
        customConverter = args =>
        {
            var arg1 = (T1)(args.ElementAtOrDefault(0) ?? default(T1));
            var arg2 = (T2)(args.ElementAtOrDefault(1) ?? default(T2));
            var result = converter(arg1, arg2);
            return result;
        };
        return this;
    }

    public MultiDependencyBinding SetConverter<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> converter)
    {
        customConverter = args => converter(
            (T1)(args.ElementAtOrDefault(0) ?? default(T1)),
            (T2)(args.ElementAtOrDefault(1) ?? default(T2)),
            (T3)(args.ElementAtOrDefault(2) ?? default(T3)));
        return this;
    }

    public MultiDependencyBinding SetConverter<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> converter)
    {
        customConverter = args => converter(
            (T1)(args.ElementAtOrDefault(0) ?? default(T1)),
            (T2)(args.ElementAtOrDefault(1) ?? default(T2)),
            (T3)(args.ElementAtOrDefault(2) ?? default(T3)),
            (T4)(args.ElementAtOrDefault(3) ?? default(T4)));
        return this;
    }

    protected override void OnDataSourceChanged(in DataSourceContextChanged context)
    {
        // Unregister from old source
        if (_notifySource != null)
        {
            _notifySource.propertyChanged -= OnSourcePropertyChanged;
            _notifySource = null;
        }

        // Register to new source
        if (context.newContext.dataSource is INotifyBindablePropertyChanged newNotifySource)
        {
            _notifySource = newNotifySource;
            _notifySource.propertyChanged += OnSourcePropertyChanged;
        }
    }

    protected override void OnDeactivated(in BindingActivationContext context)
    {
        try
        {
            // Unregister from property changes
            if (_notifySource != null)
            {
                _notifySource.propertyChanged -= OnSourcePropertyChanged;
                _notifySource = null;
            }

            _callback = null;

            // Clear cached values
            if (_lastValues.Count > 0)
            {
                _lastValues.Clear();
            }

            customConverter = null;

            base.OnDeactivated(context);
        }
        catch (Exception ex)
        {
            Debug.LogError($"MultiDependencyBinding: Error during cleanup: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void OnSourcePropertyChanged(object sender, BindablePropertyChangedEventArgs e)
    {
        Debug.Log($"MultiDependencyBinding: Property changed: {e.propertyName}");

        // Check if the changed property is in our watched paths
        var watchedPaths = GetPaths();
        if (watchedPaths.Any(path => path.StartsWith(e.propertyName)))
        {
            Debug.Log($"MultiDependencyBinding: Property change affects our binding, marking dirty");

            // Clear the cached value to force a change detection
            if (_lastValues.ContainsKey(e.propertyName))
            {
                _lastValues.Remove(e.propertyName);
            }

            MarkDirty();
        }
    }

    protected override BindingResult Update(in BindingContext context)
    {
        try
        {
            var paths = GetPaths();
            if (paths.Length == 0)
            {
                Debug.LogWarning("MultiDependencyBinding: No paths to watch");
                return new BindingResult(BindingStatus.Success);
            }

            var values = new List<object>();
            bool hasChanges = false;

            foreach (var path in paths)
            {
                var value = GetValue(context.dataSource, path);
                if (value == null)
                {
                    Debug.LogWarning($"MultiDependencyBinding: Could not get value for path '{path}'");
                    continue;
                }

                values.Add(value);

                if (!_lastValues.TryGetValue(path, out var lastValue) || !Equals(value, lastValue))
                {
                    _lastValues[path] = value;
                    hasChanges = true;
                }
            }

            // Only update UI if values actually changed
            if (!hasChanges && _lastValues.Count > 0)
            {
                return new BindingResult(BindingStatus.Success);
            }

            if (values.Count != paths.Length)
            {
                return new BindingResult(BindingStatus.Failure, "Not all paths could be resolved");
            }

            // Execute callback if set
            _callback?.Invoke(values.ToArray());

            // If we have a converter, use it to update the binding
            if (customConverter != null)
            {
                var result = customConverter.Invoke(values.ToArray());

                // If the result is null or we're using a dummy binding ID, skip setting the value
                if (result == null || context.bindingId.ToString().StartsWith("__"))
                {
                    return new BindingResult(BindingStatus.Success);
                }

                var element = context.targetElement;
                if (ConverterGroups.TrySetValueGlobal(ref element, context.bindingId, result, out var errorCode))
                {
                    return new BindingResult(BindingStatus.Success);
                }

                return new BindingResult(BindingStatus.Failure, $"Binding failed: {errorCode}");
            }

            return new BindingResult(BindingStatus.Success);
        }
        catch (Exception ex)
        {
            Debug.LogError($"MultiDependencyBinding: Exception in Update: {ex.Message}\n{ex.StackTrace}");
            return new BindingResult(BindingStatus.Failure, ex.Message);
        }
    }

    private string[] GetPaths()
    {
        var paths = new List<string>();
        if (!string.IsNullOrEmpty(sourcePath)) paths.Add(sourcePath);
        if (dependencyPaths?.Length > 0) paths.AddRange(dependencyPaths.Where(p => !string.IsNullOrEmpty(p)));
        return paths.ToArray();
    }

    private object GetValue(object source, string path)
    {
        if (source == null || string.IsNullOrEmpty(path)) return null;

        try
        {
            var current = source;
            var parts = path.Split('.');

            foreach (var part in parts)
            {
                if (current == null) return null;

                if (PropertyContainer.TryGetValue<object, object>(ref current, new PropertyPath(part), out var value))
                {
                    current = value;
                }
                else
                {
                    Debug.LogWarning($"MultiDependencyBinding: Property '{part}' not found on type {current.GetType().Name}");
                    return null;
                }
            }

            return current;
        }
        catch (Exception ex)
        {
            Debug.LogError($"MultiDependencyBinding: Error getting value for path '{path}': {ex.Message}");
            return null;
        }
    }
}
