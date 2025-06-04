using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scripts.UI.Controls
{
    public class SimpleListView : VisualElement, IDisposable
    {
        private IList _itemsSource;
        private Func<VisualElement> _makeItem;
        private Action<VisualElement, int> _bindItem;
        private Action<VisualElement> _destroyItem;
        private Action<VisualElement, int> _unbindItem;
        private readonly VisualElement _itemsContainer;
        private List<VisualElement> _activeItems = new List<VisualElement>();
        private FlexDirection _layoutDirection = FlexDirection.Column;
        private bool _disposed;

        public FlexDirection layoutDirection
        {
            get => _layoutDirection;
            set
            {
                if (_layoutDirection != value)
                {
                    _layoutDirection = value;
                    UpdateLayoutDirection();
                }
            }
        }

        [CreateProperty]
        public IList itemsSource
        {
            get => _itemsSource;
            set
            {
                if (_itemsSource == value) return;
                _itemsSource = value;
                Debug.Log($"SimpleListView: data changed, refreshing items");
                if (_itemsSource == null)
                {
                    // Clear all items if source is null
                    foreach (var item in _activeItems)
                        DestroyItem(item);
                    _activeItems.Clear();
                    return;
                }
                RefreshItems();
            }
        }

        public Func<VisualElement> makeItem
        {
            get => _makeItem;
            set
            {
                if (_makeItem != value)
                {
                    _makeItem = value;
                    RefreshItems();
                }
            }
        }

        public Action<VisualElement, int> bindItem
        {
            get => _bindItem;
            set
            {
                if (_bindItem != value)
                {
                    _bindItem = value;
                    RefreshItems();
                }
            }
        }

        public Action<VisualElement> destroyItem
        {
            get => _destroyItem;
            set => _destroyItem = value;
        }

        public Action<VisualElement, int> unbindItem
        {
            get => _unbindItem;
            set => _unbindItem = value;
        }

        public SimpleListView()
        {
            // Create items container
            _itemsContainer = new VisualElement();
            Add(_itemsContainer);

            // Set default styles
            style.flexGrow = 1;
            _itemsContainer.style.flexGrow = 1;

            // Set initial layout direction
            UpdateLayoutDirection();
        }

        private void UpdateLayoutDirection()
        {
            // Update items container direction
            _itemsContainer.style.flexDirection = _layoutDirection;
        }

        private void DestroyItem(VisualElement item)
        {
            if (item != null)
            {
                item.RemoveFromHierarchy();
                if (_destroyItem != null)
                {
                    _destroyItem(item);
                }
            }
        }

        public void RefreshItems()
        {
            if (_itemsSource == null || _makeItem == null || _bindItem == null)
                return;

            // Unbind and destroy excess items if array got smaller
            for (int i = _itemsSource.Count; i < _activeItems.Count; i++)
            {
                if (_unbindItem != null)
                {
                    _unbindItem(_activeItems[i], i);
                }
                DestroyItem(_activeItems[i]);
            }
            if (_activeItems.Count > _itemsSource.Count)
                _activeItems.RemoveRange(_itemsSource.Count, _activeItems.Count - _itemsSource.Count);

            // Create or update items
            for (int i = 0; i < _itemsSource.Count; i++)
            {
                // Create new item if needed
                if (i >= _activeItems.Count || _activeItems[i] == null)
                {
                    var newItem = _makeItem();
                    _itemsContainer.Add(newItem);
                    if (i < _activeItems.Count)
                        _activeItems[i] = newItem;
                    else
                        _activeItems.Add(newItem);
                }
                else if (_unbindItem != null)
                {
                    // Unbind existing item before rebinding
                    _unbindItem(_activeItems[i], i);
                }

                // Always rebind the item to ensure it's up to date
                _bindItem(_activeItems[i], i);
            }
        }

        public new void MarkDirtyRepaint()
        {
            _itemsContainer.MarkDirtyRepaint();
        }

        void IDisposable.Dispose()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                // Clean up all items
                foreach (var item in _activeItems)
                {
                    DestroyItem(item);
                }
                _activeItems.Clear();
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}