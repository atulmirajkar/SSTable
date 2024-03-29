﻿namespace AVLTree
{
    class Treebehavior
    {
        async static Task Main(string[] args)
        {
            AVL<int, int> tree = new AVL<int, int>();
            tree.Insert(10,100);
            tree.Insert(5,50);
            tree.Insert(7,70);
            tree.Preorder();

            Console.WriteLine();
            tree.Delete(7);
            tree.Preorder();
        
            Console.WriteLine("Using enumerator");

            foreach(KeyValuePair<int, int> kv in tree){
                Console.WriteLine(kv.Key+":"+kv.Value);
            }

            Console.WriteLine("serialization");
            await SerializeUtil<int, int>.serializeKV(tree,"./data","one");

            AVL<int,int>? deserialTree = await SerializeUtil<int, int>.deserializeKV("./data","one");
            if(deserialTree==null)
                return;
                
            foreach(KeyValuePair<int, int> kv in deserialTree){
                Console.WriteLine(kv.Key+":"+kv.Value);
            }
            
            return;
        }
    }
}
