// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Threading;

namespace CornerstoneRemoteControlClient.Helpers
{
    /// <summary>
    ///     Represents a list that allows cross thread collection and property binding.
    ///     Use AcquireLock for multithreaded scenarios.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObservableList<T> : IList<T>
    {
        private const String _lockMessage =
            "ObservableList must be locked before accessing.  This occurred when calling ";

        /// <summary>
        ///     The dispatcher that is used to notify the UI thread of changes
        /// </summary>
        private readonly Dispatcher _dispatcher;

        /// <summary>
        ///     The list used by a worker thread
        /// </summary>
        private readonly List<T> _list;

        /// <summary>
        ///     The ObservableCollection that UI controls should bind to
        /// </summary>
        private readonly RangeObservableCollection<T> _observableCollection;

        private TimedLock _lock;

        //Change callbacks

        /// <summary>
        ///     Creates a new instance of the ObservableBackgroundList class
        /// </summary>
        /// <param name="dispatcher">The dispatcher that is used to notify the UI thread of changes</param>
        public ObservableList(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _list = new List<T>();
            _observableCollection = new RangeObservableCollection<T>();
        }

        /// <summary>
        ///     Gets the ObservableCollection that UI controls should bind to
        /// </summary>
        public RangeObservableCollection<T> ObservableCollection
        {
            get
            {
                if (_dispatcher != null && _dispatcher.CheckAccess() == false)
                {
                    throw new InvalidOperationException("ObservableCollection only accessible from UI thread");
                }
                return _observableCollection;
            }
        }

        #region ICollection<T> Members

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        ///     Searches for the specified object and returns the zero-based index of the
        ///     first occurrence within the entire List.
        /// </summary>
        /// <param name="item">The object to locate in the List</param>
        /// <returns>
        ///     The zero-based index of the first occurrence of item within the entire List
        ///     if found; otherwise, –1.
        /// </returns>
        public int IndexOf(T item)
        {
            _lock.VerifyLocked(_lockMessage + "IndexOf.");
            return _list.IndexOf(item);
        }

        /// <summary>
        ///     Inserts an element into the List at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert. The value can be null for reference types.</param>
        public void Insert(int index, T item)
        {
            _lock.VerifyLocked(_lockMessage + "Insert.");

            _list.Insert(index, item);
            if (_dispatcher != null)
            {
                _dispatcher.BeginInvoke(DispatcherPriority.Send,
                                        new InsertItemCallback(InsertItemFromDispatcherThread),
                                        index,
                                        new object[] { item }
                    );
            }
        }

        /// <summary>
        ///     Removes the element at the specified index of the List
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        public void RemoveAt(int index)
        {
            _lock.VerifyLocked(_lockMessage + "RemoveAt.");
            _list.RemoveAt(index);
            if (_dispatcher != null)
            {
                _dispatcher.BeginInvoke(DispatcherPriority.Send,
                                        new RemoveAtCallback(RemoveAtFromDispatcherThread),
                                        index
                    );
            }
        }

        /// <summary>
        ///     Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        public T this[int index]
        {
            get
            {
                _lock.VerifyLocked(_lockMessage + "[] get operator.");
                return _list[index];
            }
            set
            {
                _lock.VerifyLocked(_lockMessage + "[] set operator.");
                _list[index] = value;
                if (_dispatcher != null)
                {
                    _dispatcher.BeginInvoke(DispatcherPriority.Send,
                                            new SetItemCallback(SetItemFromDispatcherThread),
                                            index,
                                            new object[] { value }
                        );
                }
            }
        }

        #endregion

        #region ICollection<T> Members

        /// <summary>
        ///     Adds an object to the end of the List.
        /// </summary>
        /// <param name="item">The object to be added to the end of the List</param>
        public void Add(T item)
        {
            _lock.VerifyLocked(_lockMessage + "Add.");
            _list.Add(item);
            if (_dispatcher != null)
            {
                _dispatcher.BeginInvoke(DispatcherPriority.Send,
                                        new AddCallback(AddFromDispatcherThread),
                                        item
                    );
            }
        }

        /// <summary>
        ///     Removes all elements from the List
        /// </summary>
        public void Clear()
        {
            _lock.VerifyLocked(_lockMessage + "Clear.");
            _list.Clear();
            if (_dispatcher != null)
            {
                _dispatcher.BeginInvoke(DispatcherPriority.Send,
                                        new ClearCallback(ClearFromDispatcherThread)
                    );
            }
        }

        /// <summary>
        ///     Determines whether an element is in the List
        /// </summary>
        /// <param name="item">The object to locate in the List</param>
        /// <returns>true if item is found in the List; otherwise, false</returns>
        public bool Contains(T item)
        {
            _lock.VerifyLocked(_lockMessage + "Contains.");
            return _list.Contains(item);
        }

        /// <summary>
        ///     Copies the entire List to a compatible one-dimensional
        ///     array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">
        ///     The one-dimensional System.Array that is the destination of the elements
        ///     copied from System.Collections.Generic.List
        ///     <T>
        ///         . The System.Array must have
        ///         zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _lock.VerifyLocked(_lockMessage + "CopyTo.");
            _list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        ///     Gets the number of elements actually contained in the List.
        /// </summary>
        public int Count
        {
            get
            {
                _lock.VerifyLocked(_lockMessage + "Count.");
                return _list.Count;
            }
        }

        /// <summary>
        ///     Removes the first occurrence of a specific object from the List.
        /// </summary>
        /// <param name="item">The object to remove from the List.</param>
        /// <returns>
        ///     true if item is successfully removed; otherwise, false. This method also
        ///     returns false if item was not found in the List
        /// </returns>
        public bool Remove(T item)
        {
            _lock.VerifyLocked(_lockMessage + "Remove.");
            bool result = _list.Remove(item);

            //only remove the item from the UI collection if it is removed from the worker collection
            if (result)
            {
                if (_dispatcher != null)
                {
                    _dispatcher.BeginInvoke(DispatcherPriority.Send,
                                            new RemoveCallback(RemoveFromDispatcherThread),
                                            item
                        );
                }
            }
            return result;
        }

        public void AddRange(IEnumerable<T> list)
        {
            _lock.VerifyLocked(_lockMessage + "AddRange.");

            foreach (T item in list)
            {
                _list.Add(item);
            }

            if (_dispatcher != null)
            {
                _dispatcher.BeginInvoke(DispatcherPriority.Send,
                                        new AddRangeCallback(AddRangeFromDispatcherThread),
                                        list
                    );
            }
        }

        #endregion

        #region IEnumerable<T> Members

        /// <summary>
        ///     Returns an enumerator that iterates through the List
        /// </summary>
        /// <returns>
        ///     A System.Collections.Generic.List
        ///     <T>
        ///         .Enumerator for the System.Collections.Generic.List<T>.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            _lock.VerifyLocked(_lockMessage + "GetEnumerator.");
            return _list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        ///     Returns an enumerator that iterates through the List
        /// </summary>
        /// <returns>Am Enumerator for the List.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            _lock.VerifyLocked(_lockMessage + "GetEnumerator.");
            return _list.GetEnumerator();
        }

        #endregion

        //change callbacks
        private void InsertItemFromDispatcherThread(int index, T item)
        {
            _observableCollection.Insert(index, item);
        }

        private void RemoveAtFromDispatcherThread(int index)
        {
            _observableCollection.RemoveAt(index);
        }

        private void SetItemFromDispatcherThread(int index, T item)
        {
            _observableCollection[index] = item;
        }

        private void AddFromDispatcherThread(T item)
        {
            _observableCollection.Add(item);
        }

        private void AddRangeFromDispatcherThread(IEnumerable<T> list)
        {
            _observableCollection.AddRange(list, true);
        }

        private void ClearFromDispatcherThread()
        {
            _observableCollection.Clear();
        }

        private void RemoveFromDispatcherThread(T item)
        {
            _observableCollection.Remove(item);
        }

        /// <summary>
        ///     Acquires an object that locks on the collection. The lock is released when the object is disposed
        /// </summary>
        /// <returns>A disposable object that unlocks the collection when disposed</returns>
        public TimedLock AcquireLock()
        {
            _lock = new TimedLock(((ICollection)_list).SyncRoot);
            return _lock;
        }

        private delegate void AddCallback(T item);

        private delegate void AddRangeCallback(IEnumerable<T> list);

        private delegate void ClearCallback();

        private delegate void InsertItemCallback(int index, T item);

        private delegate void PropertyChangedCallback(T item, PropertyChangedEventArgs e);

        private delegate void RemoveAtCallback(int index);

        private delegate void RemoveCallback(T item);

        private delegate void SetItemCallback(int index, T item);
    }
}
