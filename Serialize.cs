using System.IO;
using System.Collections.Generic;
using System;
using System.Text.Json;
using SSTables;
using System.Threading.Tasks;
using System.Text;

namespace AVLTree{
    public class SerializeUtil<TKey, TValue> where TKey: IComparable{
        public async static void serializeKV(AVL<TKey, TValue> avlObj,string path, string fileName){
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
            
            List<IndexEntry<TKey>> idxList = new List<IndexEntry<TKey>>();
            
            using(FileStream dataFS = File.Create(dataFileName)){
                long offset = 0;
                foreach(KeyValuePair<TKey, TValue> kv in avlObj){
                    byte[] bufferData = serializeSingleKV(kv.Key,kv.Value);
                    dataFS.Seek(offset, SeekOrigin.Begin);
                    await dataFS.WriteAsync(bufferData,0,bufferData.Length);

                    IndexValue idxValue = new IndexValue(offset,bufferData.Length);
                    IndexEntry<TKey> idxPtr = new IndexEntry<TKey>(kv.Key, idxValue); 
                    idxList.Add(idxPtr);
                    offset += bufferData.Length;
                }
            }

            using(FileStream idxFS = File.Create(idxFileName)){
                long offset = 0;
                offset = await appendToStream("[", idxFS, offset);
                for(int i=0; i<idxList.Count;i++){
                    byte[] data = JsonSerializer.SerializeToUtf8Bytes(idxList[i]);
                    idxFS.Seek(offset, SeekOrigin.Begin);
                    await idxFS.WriteAsync(data,0,data.Length);
                    offset+= data.Length;
                    if(i!=idxList.Count-1)
                        offset = await appendToStream(",", idxFS, offset);
                }
                await appendToStream("]", idxFS, offset);
            }
        } 

        private async static Task<long> appendToStream(string inputStr, FileStream fs, long offset){
                byte[] startBytes= Encoding.UTF8.GetBytes(inputStr);
                await fs.WriteAsync(startBytes,0,startBytes.Length);
                offset += startBytes.Length;
                return offset;
        }

        public static byte[] serializeSingleKV<T1,T2>(T1 key, T2 value){
            MyKeyValue<T1, T2> customKV = new MyKeyValue<T1, T2>(key, value);
            return JsonSerializer.SerializeToUtf8Bytes<MyKeyValue<T1,T2>>(customKV);
        }

        public static byte[] serializeObj<T>(T obj){
            return JsonSerializer.SerializeToUtf8Bytes<T>(obj);
        }

        public async static Task<AVL<TKey, TValue>> deserializeKV(string dir, string fileName){
            if(!Directory.Exists(dir)) {
                return null;
            }

            string dataFile= dir + "/" + fileName+".data";
            if(!File.Exists(dataFile)){
                return null;
            }

            string idxFile = dir + "/" + fileName + ".idx";
            if(!File.Exists(idxFile)){
                return null;
            }
            AVL<TKey, TValue> avlTree = new AVL<TKey, TValue>();
            //foreach value in index
                //read data
            IndexEntry<TKey>[] idxArr = deserializeIdx(idxFile);
            using(FileStream dataFS = File.OpenRead(dataFile)){
                foreach(IndexEntry<TKey> idxEntry in idxArr){
                    var offset = idxEntry.v.o;
                    var length = idxEntry.v.l;

                    byte[] buffer = new byte[length];
                    await dataFS.ReadAsync(buffer, 0, length);
                    var obj = deserializeSingleKV(buffer); 
                    avlTree.Insert(obj.k,obj.v);
                }
            }
            return avlTree;
        }

        private static IndexEntry<TKey>[] deserializeIdx(string fileName){
            ReadOnlySpan<byte> jsonReadOnlySpan = File.ReadAllBytes(fileName);
            var obj = JsonSerializer.Deserialize<IndexEntry<TKey>[]>(jsonReadOnlySpan)!;
            return obj;
        }
        private static MyKeyValue<TKey,TValue> deserializeSingleKV(byte[] buffer){
            Utf8JsonReader utf8Reader = new Utf8JsonReader(buffer);
            return JsonSerializer.Deserialize<MyKeyValue<TKey,TValue>>(ref utf8Reader);
        }

        public static T deserializeSingleObj<T>(byte[] buffer){
            Utf8JsonReader utf8Reader = new Utf8JsonReader(buffer);
            return JsonSerializer.Deserialize<T>(ref utf8Reader);
        }
    }

}