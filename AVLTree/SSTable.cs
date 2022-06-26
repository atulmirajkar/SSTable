using Model;

namespace AVLTree
{
    public class SSTable<TKey, TValue> where TKey:IComparable 
    {
        private TKey? minKey { get; set; }
        private TKey? maxKey { get; set; }
        private int tableNum{get; set;} 
        List<MyKeyValue<TKey, TValue>>? kvList;
        private string pathToDeserialize{get; set;} 
        private SSTable(int tableNum, string path, List<MyKeyValue<TKey, TValue>>? kvList){
            this.tableNum = tableNum; 
            this.pathToDeserialize = path;
            this.kvList = kvList;
        }

        public async Task<SSTable<TKey, TValue>> BuildTable(int tableNum, string path){
            List<MyKeyValue<TKey, TValue>>? kvList = await _deserialize(path);
            if(kvList == null)
                return null;
            
            this.minKey = kvList[0].k;
            this.maxKey = kvList[kvList.Count-1].k;
            return new SSTable<TKey, TValue>(tableNum, path, kvList);
        } 

        public MyKeyValue<TKey, TValue>? get(TKey key){
            if(kvList == null)
                return null;
            //do binary search on kvList - because it is sorted
            int start = 0;
            int end = kvList.Count-1;

            while(start <= end){
                int mid = start + ((end-start)>>1);
                if(key.CompareTo(kvList[mid])==0)
                {
                    return kvList[mid];
                }

                if(key.CompareTo(kvList[mid])<0)
                {
                    end = mid-1;
                } else{
                    start = mid+1;
                } 
            }
            return null;
        }
        //do we need to serialize 
        private async Task<List<MyKeyValue<TKey, TValue>>?> _deserialize(string pathToDeserialize){
            return await SerializeUtil<TKey,TValue>.deserializeKVList(pathToDeserialize,tableNum.ToString());
        }
    }
}