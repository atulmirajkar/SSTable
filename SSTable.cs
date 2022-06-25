using System;

namespace AVLTree
{
    public class SSTable<TKey, TValue> where TKey:IComparable 
    {
        private TKey minKey{get; set;}
        private TKey maxKey{get; set;}
        private int level{get; set;}

        private string filePath{get; set;}

    }
}