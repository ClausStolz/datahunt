using System;
using System.Collections;
using System.Collections.Generic;
using DataHunt.Storage.Infrastructure.Models;

namespace DataHunt.Storage.Implementation
{
    public class Storage<T> : ICollection<T>, IList<T> where T : IComparable<T>
    {
        public Node<T> Root { get; set; }

        public void Add(T item)
        {
            if (Root == null)
            {
                Root = new Node<T>(item, this);
            }
            else
            {
                Root.Add(item);
            }
        }

        public void Clear()
        {
            if (Root == null)
            {
                return;
            }
            
            Root.Clear();
            Root = null;
        }

        public bool Contains(T item) => Root?.Contains(item) ?? false;

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }
            if ((array.Length <= arrayIndex) || (Root != null && array.Length < arrayIndex + Root.Count))
            {
                throw new ArgumentException();
            }

            Root?.CopyTo(array, arrayIndex);
        }

        public int Count => Root.Count;

        public bool IsReadOnly => false;

        public bool Remove(T item) => Root?.Remove(item) ?? false;

        public IEnumerator<T> GetEnumerator()
        {
            if (Root != null)
            {
                foreach (var item in Root)
                {
                    yield return item;
                }
            }
            else
            {
                yield break;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator(); 

        public int IndexOf(T item) => Root?.IndexOf(item) ?? -1; 

        public void Insert(int index, T item) => throw new InvalidOperationException();

        public void RemoveAt(int index) => Root?.RemoveAt(index);

        public T this[int index]
        {
            get
            {
                if (Root != null)
                {
                    return Root[index];
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
            set => throw new InvalidOperationException();
        }
    }
}