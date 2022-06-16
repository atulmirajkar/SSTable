using System;
using System.Collections.Generic;

namespace AVLTree
{
    class Treebehavior
    {
        static void Main(string[] args)
        {
            AVL<int, int> tree = new AVL<int, int>();
            tree.Insert(10,100);
            tree.Insert(5,50);
            tree.Insert(7,70);
            tree.PreOrder();

            Console.WriteLine();
            tree.Delete(7);
            tree.PreOrder();
        
            Console.WriteLine("Using enumerator");

            foreach(KeyValuePair<int, int> kv in tree){
                Console.WriteLine(kv.Key+":"+kv.Value);
            }
        }
    }
}
