using System.Reflection.Emit;
using System.Text.Json.Serialization;

namespace Model
{
    public static class ModelUtil
    {

        public static Type? getRuntimeType(ColumnType columnType)
        {
            switch (columnType)
            {
                case ColumnType.Varchar:
                    return typeof(string);
                case ColumnType.Int32:
                    return typeof(int);
                case ColumnType.Int64:
                    return typeof(Int64);
                case ColumnType.UUID:
                    return typeof(Guid);
                case ColumnType.Boolean:
                    return typeof(bool);
            }
            return null;
        }

        // public static string serializeType(Type inputType){
        //     string? result = null;
        //     if(typeof(string) == inputType){
        //         result =  Enum.GetName(typeof(ColumnType), ColumnType.Varchar);
        //     } else if(typeof(int) == inputType){
        //         result =  Enum.GetName(typeof(ColumnType), ColumnType.Int32);
        //     } else if(typeof(Int64) == inputType){
        //         result =  Enum.GetName(typeof(ColumnType), ColumnType.Int64);
        //     } else if(typeof(Guid) == inputType){
        //         result =  Enum.GetName(typeof(ColumnType), ColumnType.UUID);
        //     } else if(typeof(bool) == inputType){
        //         result =  Enum.GetName(typeof(ColumnType), ColumnType.Boolean);
        //     } 
        //     return result != null? result : "";
        // }
    }

    public interface GenericTable<TKey, TValue>{
        public Task<MyKeyValue<TKey, TValue>?> get(TKey key);
        public Task<bool> put(TKey key, TValue value);
        public Task<List<TValue>?> scan();

        public bool IsFull(long maxSize);
    }
    public class MyKeyValue<TKey, TValue>
    {
        public TKey k { get; set; }
        public TValue v { get; set; }
        public MyKeyValue() { }
        public MyKeyValue(TKey key, TValue value)
        {
            k = key;
            v = value;
        }
    }
    /*
        64 bits - 8 bytes internal memory
        2^64 - 18 billion billion - number lenght in string format is 20 digits 20 bytes required
    */
    public class IndexValue
    {
        public long o { get; set; }
        public int l { get; set; }

        public IndexValue() { }
        public IndexValue(long offset, int length)
        {
            o = offset;
            l = length;
        }
    }

    public class IndexEntry<TKey>
    {
        public TKey k { get; set; }
        public IndexValue v { get; set; }

        public IndexEntry() { }
        public IndexEntry(TKey key, IndexValue value)
        {
            k = key;
            v = value;
        }
    }

    [Serializable]
    public class LevelMetaData<TKey> where TKey : IComparable
    {
        //level num
        public int n { get; set; }
        //min key
        public TKey? minK { get; set; }
        //max key
        public TKey? maxK { get; set; }

    }

    public enum ColumnType
    {
        Varchar,
        Int32,
        Int64,
        UUID,
        Boolean   // cannot be keytype
    }


    public class TableMetaData
    {
        public List<Tuple<string, ColumnType>> columnList { get; set; }
        public string primaryKey { get; set; }
        private ColumnType pkColumnType { get; set; }
        public TableMetaData(string primaryKey, ColumnType columnType, List<Tuple<string, ColumnType>> columnList)
        {
            this.primaryKey = primaryKey;
            this.columnList = columnList;
            this.pkColumnType = columnType;
        }
    }


    public class ConcreteKey : IComparable
    {
        [JsonIgnore]
        public Type keyType { get; set; }
        public string key { get; set; }

        public ConcreteKey(string key, Type keyType)
        {
            this.key = key;
            this.keyType = keyType;
        }
        public int CompareTo(ConcreteKey? other)
        {
            if (keyType == typeof(int))
            {
                var currObj = Convert.ToInt32(key);
                var otherObj = Convert.ToInt32(other?.key);
                return currObj.CompareTo(otherObj);
            }
            else if (keyType == typeof(string))
            {
                key.CompareTo(other?.key);
            }
            else if (keyType == typeof(Int64))
            {
                var currObj = Convert.ToInt32(key);
                var otherObj = Convert.ToInt32(other?.key);
                return currObj.CompareTo(otherObj);
            }
            else if (keyType == typeof(Guid))
            {
                var currObj = Guid.Parse(key);
                Guid otherObj = Guid.Empty;
                if (other != null)
                {
                    otherObj = Guid.Parse(other.key);
                }
                return currObj.CompareTo(otherObj);
            }
            //what to return if error
            return 1;
        }

        public int CompareTo(object? obj)
        {
            var otherObj = obj as ConcreteKey;
            return this.CompareTo(otherObj);
        }


    }

    public class GetModel
    {
        public string columnName { get; set; }
        public string columnValue { get; set; }
        public GetModel(string columnName, string columnValue)
        {
            this.columnName = columnName;
            this.columnValue = columnValue;
        }
    }

}