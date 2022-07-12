using Model;

namespace AVLTree
{
    public class MemTable<TKey, TValue>: GenericTable<TKey, TValue> where TKey : IComparable
    {
        private TKey? minKey { get; set; }
        private TKey? maxKey { get; set; }

        private int tableNum{get; set;} 
        AVL<TKey, TValue> avlTree;
        private string pathToSerialize{get; set;}
        private int currSize{get; set;}
        public MemTable(int tableNum, string path){
            avlTree = new AVL<TKey, TValue>();
            this.tableNum = tableNum; 
            this.pathToSerialize = path;
        }

        public Task<MyKeyValue<TKey, TValue>?> get(TKey key){
            return Task.FromResult(avlTree.getKey(key));
        }

        public Task<bool> put(TKey key, TValue value){
            if(avlTree.Insert(key, value) == null)
            {
                return Task.FromResult(false);
            }
            if(minKey == null){
                minKey = key;
            } else if(key.CompareTo(minKey)<0){
                minKey = key;
            }
            if(maxKey == null){
                maxKey = key;
            } else if(key.CompareTo(maxKey)>0){
                maxKey = key;
            }

            return Task.FromResult(true);
        }

        public Task<List<TValue>?> scan(){
            List<TValue> result = new List<TValue>();
            foreach(KeyValuePair<TKey,TValue> kv in avlTree){
                result.Add(kv.Value);
            }
            if(result.Count == 0)
                return Task.FromResult<List<TValue>?>(null);

            return Task.FromResult(result);
        }
        public async Task<bool> serialize(){
             return await SerializeUtil<TKey, TValue>.serializeKV(avlTree,pathToSerialize, tableNum.ToString());
        }
        
        private List<IndexEntry<TKey>>? _deserializeIndex(string pathToDeserialize){
            return SerializeUtil<TKey, TValue>.deserializeKIList(pathToDeserialize, tableNum.ToString());
        }

        public bool IsFull(long maxSize)
        {
            throw new NotImplementedException();
        }
    }
}