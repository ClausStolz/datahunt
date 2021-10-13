using DataHunt.Storage.Infrastructure.Interfaces;

namespace DataHunt.Storage.Infrastructure.Models
{
    public class Node<TV>
    {
        public Node<TV> Left { get; set; }
        
        public Node<TV> Right { get; set; }
        
        public TV Data { get; set; }

        public string Key { get; set; }

        public Node(string key, TV data)
        {
            Key = key;
            Data = data;
        }
    }
}