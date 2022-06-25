using Model;

namespace AVLTree
{
    public class SSTable<TKey, TValue> where TKey:IComparable 
    {
        private TKey? minKey { get; set; }
        private TKey? maxKey { get; set; }

        private int tableNum{get; set;} 
        AVL<TKey, TValue>? avlTree;
        private string pathToDeserialize{get; set;} 
        private SSTable(int tableNum, string path, AVL<TKey, TValue>? avlTree){
            this.tableNum = tableNum; 
            this.pathToDeserialize = path;
            this.avlTree = avlTree;
        }

        public async Task<SSTable<TKey, TValue>> BuildTable(int tableNum, string path){
            AVL<TKey, TValue>? avlTree = await _deserialize(path);
            return new SSTable<TKey, TValue>(tableNum, path, avlTree);
        } 
        public MyKeyValue<TKey, TValue>? get(TKey key){
            return avlTree?.getKey(key);
        }

        private async Task<AVL<TKey, TValue>?> _deserialize(string pathToDeserialize){
            return await SerializeUtil<TKey,TValue>.deserializeKV(pathToDeserialize,tableNum.ToString());
        }
    }
}