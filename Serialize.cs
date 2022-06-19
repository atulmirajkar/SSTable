using System.ComponentModel.Design;
using System.IO;
using System.Collections.Generic;
using System;
using System.Text.Json;

namespace AVLTree{
    public class SerializeUtil<TKey, TValue> where TKey: IComparable{
        public static void serialize(AVL<TKey, TValue> avlObj,string path, string fileName){
            if(string.IsNullOrEmpty(fileName)){
                return;
            }
            //check if path exists
            if(!Directory.Exists(path)){
                return;
            }
            string dataFileName = path + "/" + fileName+".data";
            string idxFileName = path+"/"+fileName+".idx";
            string checkFileName = path + "/" + fileName + ".csm";
            //check if file exists - if exists then delete?
            if(File.Exists(dataFileName)){
                File.Delete(dataFileName);
            }
            if(File.Exists(idxFileName)){
                File.Delete(idxFileName);
            }
            if(File.Exists(checkFileName)){
                File.Delete(checkFileName);
            }
            List<KeyValuePair<TKey,Tuple<long,int>>> idxList = new List<KeyValuePair<TKey,Tuple<long,int>>>();
            
            using(FileStream fsData = File.Create(dataFileName)){
                long offset = 0;
                foreach(KeyValuePair<TKey, TValue> kv in avlObj){
                    byte[] bufferData = JsonSerializer.SerializeToUtf8Bytes<KeyValuePair<TKey,TValue>>(kv);
                    fsData.Seek(offset, SeekOrigin.Begin);
                    fsData.Write(bufferData,0,bufferData.Length);

                    var tuple = Tuple.Create(offset,bufferData.Length);
                    KeyValuePair<TKey, Tuple<long,int>> idxPtr = new KeyValuePair<TKey, Tuple<long,int>>(kv.Key, tuple); 
                    idxList.Add(idxPtr);
                    offset += bufferData.Length;
                }
            }

            using(FileStream idxData = File.Create(idxFileName)){
                long offset = 0;
                foreach(KeyValuePair<TKey, Tuple<long,int>> kv in idxList){
                    byte[] data = JsonSerializer.SerializeToUtf8Bytes<KeyValuePair<TKey,Tuple<long,int>>>(kv);
                    idxData.Seek(offset, SeekOrigin.Begin);
                    idxData.Write(data,0,data.Length);
                    offset+= data.Length;
                }
            }
        } 
        public static void deserialize(Type toType, string fileName){

        }
    }

}