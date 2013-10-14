// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace CornerstoneRemoteControlClient.Helpers
{
    public class RangeObservableCollection<T> : ObservableCollection<T>
    {
        #region Fields

        private bool _suppressNotification;

        #endregion

        #region Method Overrides

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_suppressNotification)
            {
                base.OnCollectionChanged(e);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Adds a range to the end of this collection.
        /// </summary>
        /// <param name="elements">The elements to add.</param>
        /// <param name="reset">
        ///     When true, the list is reset after the range is added instead of firing the default range notification.
        ///     The default is false.
        /// </param>
        public void AddRange(IEnumerable<T> elements, Boolean reset = false)
        {
            InsertRange(Items.Count, elements, reset);
        }

        /// <summary>
        ///     Inserts a range at a given index.
        /// </summary>
        /// <param name="index">The index to insert at.</param>
        /// <param name="elements">The elements to insert.</param>
        /// <param name="reset">
        ///     When true, the list is reset after the range is added instead of firing the default range notification.
        ///     The default is false.
        /// </param>
        public void InsertRange(Int32 index, IEnumerable<T> elements, Boolean reset = false)
        {
            if (index < 0 || index > Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (elements == null)
            {
                throw new ArgumentNullException("elements");
            }

            _suppressNotification = true;

            // Insert the items in reverse order.
            var items = new List<T>(elements);
            foreach (T item in ((IEnumerable<T>) items).Reverse())
            {
                Insert(index, item);
            }

            _suppressNotification = false;

            // Notify listeners of this change.
            if (items.Count > 0)
            {
                if (reset && items.Count > 1)
                {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
                else
                {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items,
                        index));
                }
            }
        }

        /// <summary>
        ///     Removes a range, starting at a given index.
        /// </summary>
        /// <param name="index">The index to remove at.</param>
        /// <param name="count">The count of elements to remove.</param>
        /// <param name="reset">
        ///     When true, the list is reset after the range is removed instead of firing the default range notification.
        ///     The default is false.
        /// </param>
        public void RemoveRange(Int32 index, Int32 count, Boolean reset = false)
        {
            if (index < 0 || index > Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (count < 0 || index + count > Count)
            {
                throw new ArgumentNullException("count");
            }

            _suppressNotification = true;

            // Remove the items in the range.
            var removedItems = new List<T>();
            for (Int32 i = count; i > 0; i--)
            {
                removedItems.Add(Items[index]);
                Items.RemoveAt(index);
            }

            _suppressNotification = false;

            // Notify listeners of this change.
            if (removedItems.Count > 0)
            {
                if (reset && removedItems.Count > 1)
                {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
                else
                {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                        removedItems, index));
                }
            }
        }

        /// <summary>
        ///     Removes a set of objects.
        /// </summary>
        /// <param name="items">The items to be removed.</param>
        /// <remarks>Only supports resetting the list at this time.</remarks>
        public void RemoveRange(IEnumerable<T> items)
        {
            _suppressNotification = true;

            // Remove the items in the range.
            Boolean removed = items.Aggregate(false, (current, item) => current | Items.Remove(item));

            _suppressNotification = false;

            // Notify listeners of this range change.
            if (removed)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        /// <summary>
        ///     Clears the contents of the list, replacing it with new items.
        /// </summary>
        /// <param name="items">The items to use as the new contents of the list.</param>
        public void ResetContents(ICollection<T> items)
        {
            if (Items.Count == 0 && items.Count == 0)
            {
                return;
            }

            _suppressNotification = true;

            // Remove the items in the range.
            Items.Clear();
            foreach (var item in items)
            {
                Items.Add(item);
            }

            _suppressNotification = false;

            // Notify listeners of this change.
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        ///     Raises the reset event on the list.
        /// </summary>
        public void RaiseResetEvent()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        #endregion
    }
}
