using System;
using DataHunt.Storage.Infrastructure.Models;

namespace DataHunt.Storage.Implementation
{
    public partial class Storage<TV>
    {
        private int GetHeight(Node<TV> current)
        {
            return current switch
            {
                null => 0,
                _ => ((Func<int>)(() =>
                {
                    var leftHeight = GetHeight(current.Left);
                    var rightHeight = GetHeight(current.Right);
                    var maximumHeight = Math.Max(leftHeight, rightHeight);
                    return maximumHeight + 1;
                }))()
            };
        }
        
        private int BalanceFactor(Node<TV> current)
        {
            var leftHeight = GetHeight(current.Left);
            var rightHeight = GetHeight(current.Right);
            return leftHeight - rightHeight;
        }
        
        private Node<TV> BalanceTree(Node<TV> current)
        {
            var balanceFactor = BalanceFactor(current);
            return balanceFactor switch
            {
                > 1 => ((Func<Node<TV>>)(() =>
                {
                    return BalanceFactor(current.Left) switch
                    {
                        > 0 => RotateLL(current),
                        _ => RotateLR(current)
                    };
                }))(),
                < -1 => ((Func<Node<TV>>)(() =>
                {
                    return BalanceFactor(current.Right) switch
                    {
                        > 0 => RotateRL(current),
                        _ => RotateRR(current)
                    };
                }))()
            };
        }

        private Node<TV> RotateRR(Node<TV> parent)
        {
            var pivot = parent.Right;
            parent.Right = pivot.Left;
            pivot.Left = parent;
            return pivot;
        }
        private Node<TV> RotateLL(Node<TV> parent)
        {
            var pivot = parent.Left;
            parent.Left = pivot.Right;
            pivot.Right = parent;
            return pivot;
        }
        private Node<TV> RotateLR(Node<TV> parent)
        {
            var pivot = parent.Left;
            parent.Left = RotateRR(pivot);
            return RotateLL(parent);
        }
        private Node<TV> RotateRL(Node<TV> parent)
        {
            var pivot = parent.Right;
            parent.Right = RotateLL(pivot);
            return RotateRR(parent);
        }
    }
}