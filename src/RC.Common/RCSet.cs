using System;
using System.Collections;
using System.Collections.Generic;

namespace RC.Common
{
    /// <summary>
    /// Represents a set that preserves insertion order.
    /// </summary>
    /// <typeparam name="T">The type of the elements of this set.</typeparam>
    public class RCSet<T> : ISet<T>
    {
        /// <summary>
        /// Constructs an RCSet with the default equality comparer.
        /// </summary>
        public RCSet()
            : this(EqualityComparer<T>.Default)
        {
        }

        /// <summary>
        /// Constructs an RCSet and adds the elements from the specified collection.
        /// </summary>
        /// <param name="collection">The collection of the initial elements.</param>
        public RCSet(IEnumerable<T> collection)
            : this(collection, EqualityComparer<T>.Default)
        {
        }

        /// <summary>
        /// Constructs an RCSet with the given equality comparer and adds the elements from the specified collection.
        /// </summary>
        /// <param name="collection">The collection of the initial elements.</param>
        /// <param name="comparer">The comparer to be used.</param>
        public RCSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
            : this(comparer)
        {
            foreach (T item in collection) { this.Add(item); }
        }

        /// <summary>
        /// Constructs an RCSet with the given equality comparer.
        /// </summary>
        /// <param name="comparer">The comparer to be used.</param>
        public RCSet(IEqualityComparer<T> comparer)
        {
            this.underlyingDictionary = new Dictionary<T, LinkedListNode<T>>(comparer);
            this.underlyingList = new LinkedList<T>();
        }

        #region IEnumerator members

        /// <see cref="IEnumerator.GetEnumerator"/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion IEnumerator members

        #region IEnumerator<T> members

        /// <see cref="IEnumerator&lt;T&gt;.GetEnumerator"/>
        public IEnumerator<T> GetEnumerator()
        {
            return this.underlyingList.GetEnumerator();
        }

        #endregion IEnumerator<T> members

        #region ICollection<T> members

        /// <see cref="ICollection&lt;T&gt;.Count"/>
        public int Count
        {
            get { return this.underlyingDictionary.Count; }
        }

        /// <see cref="ICollection&lt;T&gt;.IsReadOnly"/>
        public virtual bool IsReadOnly
        {
            get { return this.underlyingDictionary.IsReadOnly; }
        }

        /// <see cref="ICollection&lt;T&gt;.Add"/>
        void ICollection<T>.Add(T item)
        {
            this.Add(item);
        }

        /// <see cref="ICollection&lt;T&gt;.Clear"/>
        public void Clear()
        {
            this.underlyingList.Clear();
            this.underlyingDictionary.Clear();
        }

        /// <see cref="ICollection&lt;T&gt;.Contains"/>
        public bool Contains(T item)
        {
            return this.underlyingDictionary.ContainsKey(item);
        }
        
        /// <see cref="ICollection&lt;T&gt;.CopyTo"/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            this.underlyingList.CopyTo(array, arrayIndex);
        }

        /// <see cref="ICollection&lt;T&gt;.Remove"/>
        public bool Remove(T item)
        {
            LinkedListNode<T> node;
            bool found = this.underlyingDictionary.TryGetValue(item, out node);
            if (!found) { return false; }
            this.underlyingDictionary.Remove(item);
            this.underlyingList.Remove(node);
            return true;
        }

        #endregion ICollection<T> members

        #region ISet<T> members

        /// <see cref="ISet&lt;T&gt;.Add"/>
        public bool Add(T item)
        {
            if (this.underlyingDictionary.ContainsKey(item)) { return false; }
            LinkedListNode<T> node = this.underlyingList.AddLast(item);
            this.underlyingDictionary.Add(item, node);
            return true;
        }

        /// <see cref="ISet&lt;T&gt;.UnionWith"/>
        public void UnionWith(IEnumerable<T> other)
        {
            if (other == null) { throw new ArgumentNullException("other"); }
            foreach (T element in other) { this.Add(element); }
        }

        /// <see cref="ISet&lt;T&gt;.IntersectWith"/>
        public void IntersectWith(IEnumerable<T> other)
        {
            HashSet<T> thisSetMinusOther = new HashSet<T>(this);
            thisSetMinusOther.ExceptWith(other);
            foreach (T elementToRemove in thisSetMinusOther) { this.Remove(elementToRemove); }
        }

        /// <see cref="ISet&lt;T&gt;.ExceptWith"/>
        public void ExceptWith(IEnumerable<T> other)
        {
            foreach (T element in other) { this.Remove(element); }
        }

        /// <see cref="ISet&lt;T&gt;.SymmetricExceptWith"/>
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (other == null) { throw new ArgumentNullException("other"); }
            foreach (T element in other)
            {
                if (this.Contains(element)) { this.Remove(element); }
                else { this.Add(element); }
            }
        }

        /// <see cref="ISet&lt;T&gt;.IsSubsetOf"/>
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            if (other == null) { throw new ArgumentNullException("other"); }
            HashSet<T> otherHashset = new HashSet<T>(other);
            return otherHashset.IsSupersetOf(this);
        }

        /// <see cref="ISet&lt;T&gt;.IsSupersetOf"/>
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            if (other == null) { throw new ArgumentNullException("other"); }
            HashSet<T> otherHashSet = new HashSet<T>(other);
            return otherHashSet.IsSubsetOf(this);
        }

        /// <see cref="ISet&lt;T&gt;.IsProperSubsetOf"/>
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            if (other == null) { throw new ArgumentNullException("other"); }
            HashSet<T> otherHashSet = new HashSet<T>(other);
            return otherHashSet.IsProperSupersetOf(this);
        }

        /// <see cref="ISet&lt;T&gt;.IsProperSupersetOf"/>
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            if (other == null) { throw new ArgumentNullException("other"); }
            HashSet<T> otherHashset = new HashSet<T>(other);
            return otherHashset.IsProperSubsetOf(this);
        }

        /// <see cref="ISet&lt;T&gt;.Overlaps"/>
        public bool Overlaps(IEnumerable<T> other)
        {
            if (other == null) { throw new ArgumentNullException("other"); }
            if (this.Count == 0) { return false; }

            foreach (T element in other)
            {
                if (this.Contains(element)) { return true; }
            }
            return false;
        }

        /// <see cref="ISet&lt;T&gt;.SetEquals"/>
        public bool SetEquals(IEnumerable<T> other)
        {
            if (other == null) { throw new ArgumentNullException("other"); }
            HashSet<T> otherHashset = new HashSet<T>(other);
            return otherHashset.SetEquals(this);
        }

        #endregion ISet<T> members

        /// <summary>
        /// The underlying dictionary of elements.
        /// </summary>
        private readonly IDictionary<T, LinkedListNode<T>> underlyingDictionary;

        /// <summary>
        /// The underlying linked list of elements.
        /// </summary>
        private readonly LinkedList<T> underlyingList;
    }
}