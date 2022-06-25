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
            return avlTree.Insert(key, value)!=null;
        }

        public async Task<bool> serialize(){
             return await SerializeUtil<TKey, TValue>.serializeKV(avlTree,pathToSerialize, tableNum.ToString());
        }
    }
}