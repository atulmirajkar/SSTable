using Model;

namespace AVLTree
{
    public class MemTable<TKey, TValue> where TKey : IComparable
    {
        private TKey? minKey { get; set; }
        private TKey? maxKey { get; set; }

        private int tableNum{get; set;} 
        AVL<TKey, TValue> avlTree;
        private string pathToSerialize{get; set;}
        public MemTable(int tableNum, string path){
            avlTree = new AVL<TKey, TValue>();
            this.tableNum = tableNum; 
            this.pathToSerialize = path;
        }

        public MyKeyValue<TKey, TValue>? get(TKey key){
            return avlTree.getKey(key);
        }

        public bool put(TKey key, TValue value){
            if(avlTree.Insert(key, value) == null)
            {
                return false;
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

            return true;
        }

        public async Task<bool> serialize(){
             return await SerializeUtil<TKey, TValue>.serializeKV(avlTree,pathToSerialize, tableNum.ToString());
        }
    }
}