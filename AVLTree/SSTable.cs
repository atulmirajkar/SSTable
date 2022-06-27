using Model;

namespace AVLTree
{
    public class SSTable<TKey, TValue> where TKey:IComparable 
    {
        private TKey? minKey { get; set; }
        private TKey? maxKey { get; set; }
        private int tableNum{get; set;} 
        List<IndexEntry<TKey>>? kiList;
        private string pathToDeserialize{get; set;} 
        private SSTable(int tableNum, string path, List<IndexEntry<TKey>>? kiList){
            this.tableNum = tableNum; 
            this.pathToDeserialize = path;
            this.kiList = kiList;
        }

        public SSTable<TKey, TValue>? BuildTable(int tableNum, string path){
            List<IndexEntry<TKey>>? kiList = _deserializeIndex(path);
            if(kiList == null)
                return null;
            
            this.minKey = kiList[0].k;
            this.maxKey = kiList[kiList.Count-1].k;
            return new SSTable<TKey, TValue>(tableNum, path, kiList);
        } 

        public async Task<MyKeyValue<TKey, TValue>?> get(TKey key){
            IndexValue? indexValue = _getIndexForKey(key);
            if(indexValue == null){
                return null;
            }
            return await SerializeUtil<TKey,TValue>.deserializeKVForIndex(pathToDeserialize, tableNum.ToString(), indexValue);
        }

        private IndexValue? _getIndexForKey(TKey key){
            if(kiList == null)
                return null;
            //do binary search on kiList - because it is sorted
            int start = 0;
            int end = kiList.Count-1;

            while(start <= end){
                int mid = start + ((end-start)>>1);
                if(key.CompareTo(kiList[mid].k)==0)
                {
                    return kiList[mid].v;
                }

                if(key.CompareTo(kiList[mid])<0)
                {
                    end = mid-1;
                } else{
                    start = mid+1;
                } 
            }
            return null;
        }
        //do we need to serialize?
        private async Task<List<MyKeyValue<TKey, TValue>>?> _deserialize(string pathToDeserialize){
            return await SerializeUtil<TKey,TValue>.deserializeKVList(pathToDeserialize,tableNum.ToString());
        }

        private List<IndexEntry<TKey>>? _deserializeIndex(string pathToDeserialize){
            return SerializeUtil<TKey, TValue>.deserializeKIList(pathToDeserialize, tableNum.ToString());
        }
    }
}