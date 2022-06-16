using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json;

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
            tree.Preorder();

            Console.WriteLine();
            tree.Delete(7);
            tree.Preorder();
        
            Console.WriteLine("Using enumerator");

            foreach(KeyValuePair<int, int> kv in tree){
                Console.WriteLine(kv.Key+":"+kv.Value);
            }

            Console.WriteLine("serialization");
            IFormatter bf = new BinaryFormatter();
            SerializeHelper<int, int>.SerializeObject(tree,"serialTest",bf);
            var deserialTree = (AVL<int,int>)SerializeHelper<int,int>.DeserializeObject("serialTest", bf);
            foreach(KeyValuePair<int, int> kv in tree){
                Console.WriteLine(kv.Key+":"+kv.Value);
            }


            Console.WriteLine("json formatter");
            string jsonString = JsonSerializer.Serialize<AVL<int,int>>(tree);
            Console.WriteLine(jsonString);

        }
    }
}
