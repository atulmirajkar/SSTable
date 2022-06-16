using System;
using System.IO;
using System.Runtime.Serialization;

namespace AVLTree{
    public static class SerializeHelper<TKey,TValue> where TKey:IComparable{
        public static void SerializeObject(AVL<TKey,TValue> obj, string fileName, IFormatter formatter){
           FileStream s = new FileStream(fileName, FileMode.Create);
           formatter.Serialize(s, obj); 
           s.Close();
        }    
        public static object DeserializeObject(string fileName, IFormatter formatter){

            FileStream s = new FileStream(fileName, FileMode.Open);
            Object obj = formatter.Deserialize(s);
            return obj;
        }

    }
}