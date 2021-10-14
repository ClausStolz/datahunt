using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataHunt.Storage.Implementation;

namespace DataHunt.Storage.Infrastructure.Models
{
    public class Node<T>: ICollection<T>, IList<T> where T: IComparable<T>
    {
        public Storage<T> Storage { get; private set; }
        
        public T Value { get; private set; }
        
        public Node<T> Parent { get; private set; }
        
        public Node<T> LeftHand { get; private set; }
        
        public Node<T> RightHand { get; private set; }
        
        public int Level { get; set; }
        
        public int Count { get; private set; }
        
        public Node(T value, Storage<T> storage)
        {
            Value = value;
            Level = 1;
            Count = 1;
            Storage = storage;
        }
        
        public bool IsReadOnly => false;

        public void Add(T item)
        {
            var _ = item.CompareTo(Value) switch
            {
                < 0 => (Action<T>)delegate
                {
                    if (LeftHand == null)
                    {
                        ((LeftHand = new Node<T>(item, Storage)).Parent = this).Rebuild(true);
                    }
                    else
                    {
                        LeftHand.Add(item);
                    }
                },
                _ => (Action<T>)delegate
                {
                    if (RightHand == null)
                    {
                        ((RightHand = new Node<T>(item, Storage)).Parent = this).Rebuild(true);
                    }
                    else
                    {
                        RightHand.Add(item);
                    }
                }
            };
        }
        
        public void Insert(int index, T item) => throw new InvalidOperationException(); 
        
        public bool Contains(T item)
        {
            return item != null && item.CompareTo(Value) switch
            {
                < 0 => ((Func<bool>)(() => LeftHand?.Contains(item) ?? false))(),
                > 0 => ((Func<bool>)(() => RightHand?.Contains(item) ?? false))(),
                _ => true
            };
        }
        
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (LeftHand != null)
            {
                LeftHand.CopyTo(array, arrayIndex);
                arrayIndex += LeftHand.Count;
            }
            array[arrayIndex++] = Value;
            
            RightHand?.CopyTo(array, arrayIndex);
        }
        
        public bool Remove(T item)
        {
            return item != null && item.CompareTo(Value) switch
            {
                < 0 => LeftHand?.Remove(item) ?? false,
                > 0 => RightHand?.Remove(item) ?? false,
                _ => ((Func<bool>)(() =>
                {
                    if (LeftHand == null && RightHand == null)
                    {
                        if (Parent != null)
                        {
                            if (Parent.LeftHand == this)
                            {
                                Parent.LeftHand = null;
                            }
                            else
                            {
                                Parent.RightHand = null;
                            }
                            Parent.Rebuild(true);
                        }
                        else
                        {
                            Storage.Root = null;
                        }
                    }
                    else if (LeftHand == null || RightHand == null)
                    {
                        var child = LeftHand ?? RightHand;
                        
                        if (Parent != null)
                        {
                            if (Parent.LeftHand == this)
                            {
                                Parent.LeftHand = child;
                            }
                            else
                            {
                                Parent.RightHand = child;
                            }
                            (child.Parent = Parent).Rebuild(true);
                        }
                        else
                        {
                            (Storage.Root = child).Parent = null;
                        }
                    }
                    else
                    {
                        var replace = LeftHand;
                        while (replace.RightHand != null)
                        {
                            replace = replace.RightHand;
                        }
                        (Value, replace.Value) = (replace.Value, Value);
                        
                        return replace.Remove(replace.Value);
                    }
                    
                    Parent = LeftHand = RightHand = null;
                    
                    return true;
                }))()
            };
        }
        
        public void Clear()
        {
            LeftHand?.Clear();
            RightHand?.Clear();
            LeftHand = RightHand = null;
        }
        
        public IEnumerator<T> GetEnumerator()
        {
            if (LeftHand != null)
            {
                foreach (var item in LeftHand)
                {
                    yield return item;
                }
            }
            yield return Value;
            
            if (RightHand != null)
            {
                foreach (var item in RightHand)
                {
                    yield return item;
                }
                
            }
        }
        
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        
        private void Rebuild(bool recursive)
        {
            Count = 1;

            var leftLevel = 0;
            var rightLevel = 0;
            
            if (LeftHand != null)
            {
                leftLevel = LeftHand.Level;
                Count += LeftHand.Count;
            }
            if (RightHand != null)
            {
                rightLevel = RightHand.Level;
                Count += RightHand.Count;
            }

            var _ = (leftLevel - rightLevel) switch
            {
                > 1 => (Action<T>)delegate
                {
                    var leftLeft = LeftHand.LeftHand?.Level ?? 0;
                    var leftRight = LeftHand.RightHand?.Level ?? 0;
                    
                    if (leftLeft >= leftRight)
                    {
                        LeftHand.Elevate();
                        this.Rebuild(true);
                    }
                    else
                    {
                        var pivot = LeftHand.RightHand;
                        pivot?.Elevate(); 
                        pivot?.Elevate();
                        pivot?.LeftHand.Rebuild(false);
                        pivot?.RightHand.Rebuild(true);
                    }
                },
                < -1 => (Action<T>)delegate
                {
                    var rightRight = this.RightHand.RightHand?.Level ?? 0;
                    var rightLeft = this.RightHand.LeftHand?.Level ?? 0;
                    
                    if (rightRight >= rightLeft)
                    {
                        RightHand.Elevate();
                        this.Rebuild(true);
                    }
                    else
                    {
                        var pivot = RightHand.LeftHand;
                        pivot?.Elevate(); 
                        pivot?.Elevate();
                        pivot?.LeftHand.Rebuild(false);
                        pivot?.RightHand.Rebuild(true);
                    }
                },
                _ => (Action<T>)delegate
                {
                    Level = Math.Max(leftLevel, rightLevel) + 1;
                    if (Parent != null && recursive)
                    {
                        Parent.Rebuild(true);   
                    }
                }
            };
        }
        
        private void Elevate()
        {
            var root = Parent;
            var parent = root.Parent;
            
            if ((Parent = parent) == null)
            {
                Storage.Root = this;
            }
            else
            {
                if (parent.LeftHand == root)
                {
                    parent.LeftHand = this;
                }
                else
                {
                    parent.RightHand = this;
                }
            }

            if (root.LeftHand == this)
            {
                root.LeftHand = RightHand;
                if (RightHand != null)
                {
                    RightHand.Parent = root;
                }
                
                RightHand = root;
                root.Parent = this;
            }
            else
            {
                root.RightHand = LeftHand;
                if (LeftHand != null)
                {
                    LeftHand.Parent = root;
                }
                
                LeftHand = root;
                root.Parent = this;
            }
        }
        
        public int IndexOf(T item)
        {
            return item.CompareTo(Value) switch
            {
                < 0 => LeftHand?.IndexOf(item) ?? -1,
                > 0 => RightHand?.IndexOf(item) ?? -1,
                _ => ((Func<int>)(() =>
                {
                    if (LeftHand == null)
                    {
                        return 0;
                    }

                    var temp = this.LeftHand.IndexOf(item);
                    return temp == -1 ? this.LeftHand.Count : temp;
                }))()
            };
        }
        
        public void RemoveAt(int index)
        {
            if (LeftHand != null)
                if (index < LeftHand.Count)
                {
                    LeftHand.RemoveAt(index);
                    return;
                }
                else
                {
                    index -= LeftHand.Count;
                }
            if (index-- == 0)
            {
                Remove(Value);
                return;
            }
            if (RightHand != null)
                if (index < RightHand.Count)
                {
                    RightHand.RemoveAt(index);
                    return;
                }
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        public T this[int index]
        {
            get
            {
                if (LeftHand != null)
                {
                    if (index < LeftHand.Count)
                    {
                        return this.LeftHand[index];
                    }
                    else index -= this.LeftHand.Count;
                    
                }

                if (index-- == 0)
                {
                    return this.Value;
                }

                if (RightHand != null)
                {
                    if (index < this.RightHand.Count)
                    {
                        return this.RightHand[index];
                    }
                }
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            set => throw new InvalidOperationException();
        }
    }
}