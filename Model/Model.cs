namespace Model
{
    public class MyKeyValue<TKey, TValue>
    {
        public TKey k{get; set;} 
        public TValue v{get; set;}
        public MyKeyValue(){}
        public MyKeyValue(TKey key, TValue value){
            k = key;
            v = value;
        }
    }
    /*
        64 bits - 8 bytes internal memory
        2^64 - 18 billion billion - number lenght in string format is 20 digits 20 bytes required
    */ 
    public class IndexValue{
        public long o{get; set;}
        public int l{get; set;}

        public IndexValue(){}
        public IndexValue(long offset, int length){
            o = offset;
            l = length;
        }
    }

    public class IndexEntry<TKey>{
        public TKey k{get; set;}
        public IndexValue v{get; set;}

        public IndexEntry(){}
        public IndexEntry(TKey key, IndexValue value){
            k = key;
            v = value;
        }
    }

    [Serializable]
    public class LevelMetaData<TKey> where TKey:IComparable{
       //level num
       public int n{get; set;}
       //min key
       public TKey? minK{get; set;} 
       //max key
       public TKey? maxK{get; set;} 

    }
}