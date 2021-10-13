using System;
using DataHunt.Storage.Infrastructure.Models;

namespace DataHunt.Storage.Implementation
{
    public partial class Storage<TV> //: IStorage<TK, TV>
    {
        private Node<TV> _root;

        public bool Insert(string key, TV data)
        {
            try
            {
                var newNode = new Node<TV>(key, data);
                _root = RecursiveInsert(_root, newNode);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public Node<TV> Search(string key)
        {
            return RecursiveSearch(key, _root);
        }

        public bool Remove(string key)
        {
            return RecursiveRemove(key, _root) != null;
        } 
    }
}