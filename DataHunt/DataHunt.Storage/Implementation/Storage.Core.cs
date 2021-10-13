using System;
using DataHunt.Storage.Infrastructure.Models;

namespace DataHunt.Storage.Implementation
{
    public partial class Storage<TV>
    {
        private Node<TV> RecursiveInsert(Node<TV> current, Node<TV> newNode)
        {
            return current switch
            {
                null => ((Func<Node<TV>>)(() =>
                {
                    current = newNode;
                    return current;
                }))(),
                var _ when string.CompareOrdinal(current.Key, newNode.Key) < 0 => 
                    ((Func<Node<TV>>)(() => 
                    {
                        current.Left = RecursiveInsert(current.Left, newNode);
                        return BalanceTree(current);
                    }))(), 
                var _ when string.CompareOrdinal(current.Key, newNode.Key) > 0 => 
                    ((Func<Node<TV>>)(() =>
                    {
                        current.Right = RecursiveInsert(current.Right, newNode);
                        return BalanceTree(current);
                    }))(),
                _ => throw new ArgumentOutOfRangeException(nameof(current), current, null)
            };
        }

        private Node<TV> RecursiveSearch(string key, Node<TV> current)
        {
            return current switch
            {
                null => null,
                var _ when key.Equals(current.Key) => current,
                var _ when string.CompareOrdinal(key, current.Key) < 0 => RecursiveSearch(key, current.Left),
                _ => RecursiveSearch(key, current.Right)
                
            };
        }

        private Node<TV> RecursiveRemove(string key, Node<TV> current)
        {
            if (current == null)
            {
                return null;
            }

            return key switch
            {
                var _ when string.CompareOrdinal(key, current.Key) < 0 =>
                    ((Func<Node<TV>>)(() =>
                    {
                        current.Left = RecursiveRemove(key, current.Left);
                        return BalanceFactor(current) switch
                        {
                            -2 => BalanceFactor(current.Right) <= 0
                                ? RotateRR(current)
                                : RotateRL(current),
                            _ => current
                        };
                    }))(),
                var _ when string.CompareOrdinal(key, current.Key) > 0 =>
                    ((Func<Node<TV>>)(() =>
                    {
                        current.Right = RecursiveRemove(key, current.Right);
                        return BalanceFactor(current) switch
                        {
                            2 => BalanceFactor(current.Left) >= 0
                                ? RotateLL(current)
                                : RotateLR(current),
                            _ => current
                        };
                    }))(),
                _ => ((Func<Node<TV>>)(() =>
                {
                    return current.Right switch
                    {
                        null => current.Left,
                        _ => ((Func<Node<TV>>)(() =>
                        {
                            var parent = current.Right;
                            while (parent.Left != null)
                            {
                                parent = parent.Left;
                            }

                            current.Key = parent.Key;
                            current.Data = parent.Data;

                            current.Right = RecursiveRemove(parent.Key, current.Right);
                            return BalanceFactor(current) switch
                            {
                                2 => BalanceFactor(current.Left) >= 0
                                    ? RotateLL(current)
                                    : RotateLR(current),
                                _ => current
                            };
                        }))()
                    };
                }))()
            };
        }
    }
}