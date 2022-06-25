using System;
using System.Collections.Generic;
using AVLTree;

namespace Level{
    public class LevelMeta<TKey,TValue> where TKey:IComparable{
        private int levelNum{get; set;}
        TKey? minKey{get; set;}        
        TKey? maxKey{get; set;}

        List<SSTable<TKey, TValue>>? ssList{get; set;}

        public LevelMeta(int levelNum){
           this.levelNum = levelNum;
            
        }

       
    }
}