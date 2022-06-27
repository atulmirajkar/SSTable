using System;
using System.Collections.Generic;
using System.IO;
using AVLTree;
using Model;
namespace Level{
    public class Level<TKey,TValue> where TKey:IComparable{
        private int levelNum{get; set;}
        private TKey? minKey{get; set;}        
        private TKey? maxKey{get; set;}

        private string path{get; set;} 

        List<SSTable<TKey, TValue>>? ssList{get; set;}

        private Level(int levelNum, string path){
            this.levelNum = levelNum;
            this.path = path;
        }
        public static Level<TKey, TValue>? createLevel(int levelNum, string path){
            //create directory
            if(string.IsNullOrEmpty(path) || levelNum<0 || levelNum>100){
                return null;
            }
            try{
                string dirName = path +"/" + levelNum.ToString(); 
                if(Directory.Exists(dirName))
                {
                    return null;
                }
                Directory.CreateDirectory(path);
                //create metadata file
                LevelMetaData<TKey> mdObj = new LevelMetaData<TKey>{
                    n=levelNum,
                };
                string mdFile = dirName + "/" + levelNum.ToString() + ".md"; 
                using(FileStream fs = new FileStream(mdFile, FileMode.CreateNew)){
                    //serialize md object
                    //todo move serializeObj api to a different class, we dont need TKey and TValue generics for this
                    byte[] buffer = SerializeUtil<TKey,TValue>.serializeObj<LevelMetaData<TKey>>(mdObj);        
                    fs.WriteAsync(buffer, 0, buffer.Length);
                }
            } catch(Exception e){
                //todo implement logger    
            } 
            return new Level<TKey,TValue>(levelNum,path);

        }
    }
}