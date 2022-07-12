using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AVLTree;
using Model;
using Utility;

namespace Table
{
    public class Table
    {
        private string path { get; set; }
        private string tableName { get; set; }
        private string primaryKey { get; set; }
        private ColumnType pkColumnType { get; set; }
        private readonly long ssTableSize;
        private Dictionary<string, ColumnType> colMap { get; set; }

        private List<MemTable<ConcreteKey, Dictionary<string, string>>>? memTableList { get; set; }
        private Table(string path, string tableName, string primaryKey, ColumnType columnType,List<MemTable<ConcreteKey, Dictionary<string, string>>>? memTableList, long ssTableSize)
        {
            this.path = path;
            this.tableName = tableName;
            this.primaryKey = primaryKey;
            this.pkColumnType = columnType;
            this.colMap = new Dictionary<string, ColumnType>();
            colMap.Add(primaryKey, columnType);
            this.memTableList = memTableList;
            this.ssTableSize = ssTableSize;
        }

        public async static Task<Table?> createTable(string path, string tableName, string primaryKey, ColumnType columnType, long ssTableSize=163840)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(primaryKey))
            {
                return null;
            }
            string dirName = Path.Combine(path, tableName);
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            var columnList = new List<Tuple<string, ColumnType>>();
            columnList.Add(Tuple.Create(primaryKey, columnType));
            TableMetaData tObj = new TableMetaData(primaryKey, columnType, columnList);
            string mdFile = Path.Combine(dirName, "Table.md");
            await serializeTableMD(tObj, mdFile);
            //check if there are memTables
            List<MemTable<ConcreteKey, Dictionary<string, string>>>? memTableList = getMemTableList(path, tableName, columnType);
            //todo - check if there are levels
            return new Table(path, tableName, primaryKey, columnType, memTableList,ssTableSize);
        }
        ///<summary>
        /// function to read memtables already created on the disk
        ///</summary> 
        public static List<MemTable<ConcreteKey, Dictionary<string, string>>>? getMemTableList(string path, string tableName, ColumnType pkColumnType)
        {
            Type? primaryKeyType = ModelUtil.getRuntimeType(pkColumnType);
            if (primaryKeyType == null)
            {
                return null;
            }
            List<string>? memList = DirectoryUtils.getFilesWithExt(Path.Combine(path, tableName), "data");
            if (memList == null)
            {
                return null;
            }
            List<MemTable<ConcreteKey, Dictionary<string, string>>>? memTableList = createMemTableList();
            if (memTableList == null)
            {
                return null;
            }
            //e.g. C:\SSData\testTable\1.data
            //may be we can just use the count of the list to get current number of sstables
            for (int i = 0; i < memList.Count; i++)
            {
                var tempTable = createMemTable(i, Path.Combine(path, tableName), primaryKeyType);
                if(tempTable==null){
                    continue;
                }
                memTableList.Add(tempTable);
            }
            return memTableList;
        }
        public async Task AddColumn(string columnName, ColumnType columnType)
        {
            if (string.IsNullOrEmpty(columnName))
            {
                return;
            }
            if (colMap.ContainsKey(columnName))
            {
                return;
            }
            colMap[columnName] = columnType;
            Type? runTimeColumnType = ModelUtil.getRuntimeType(columnType);
            List<Tuple<string, ColumnType>> columnList = new List<Tuple<string, ColumnType>>();
            foreach (var kv in colMap)
            {
                columnList.Add(Tuple.Create(kv.Key, kv.Value));
            }
            TableMetaData tObj = new TableMetaData(primaryKey, columnType, columnList);
            string mdFile = Path.Combine(this.path, this.tableName);
            mdFile = Path.Combine(mdFile, "Table.md");
            await serializeTableMD(tObj, mdFile);
        }
        private static async Task serializeTableMD(TableMetaData tObj, string fileName)
        {
            try
            {
                using (FileStream fs = File.Create(fileName))
                {
                    //serialize md object
                    //int, int template is not required below. Need to refactor SerializeUtil.
                    byte[] buffer = SerializeUtil.serializeObj<TableMetaData>(tObj);
                    await fs.WriteAsync(buffer, 0, buffer.Length);
                }
            }
            catch (Exception e)
            {
                //todo implement logger
                Console.WriteLine("Something went wrong." + e?.ToString());
            }
        }

        //public
        ///<summary>
        /// Adds a row to the table
        /// Should be in json forma
        ///{key1:value1, key2: value2} 
        /// Algo: 
        ///     SSTables are immutable
        ///     Once an SSTable is full, you have to write to another SSTable
        ///     keys within SSTable are sorted
        ///     MemTables SSTables can overlap
        ///     Once We have reached the max MemTables, we have to merge them to the next level
        ///</summary>
        ///<param>
        /// jsonString - input json string
        ///</param>
        public async void Add(string jsonString)
        {
            Dictionary<string, string>? map = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
            if (map == null)
            {
                return;
            }
            //todo - make sure that primary key column is there and that column names are valid

            Type? primaryKeyType = ModelUtil.getRuntimeType(pkColumnType);
            if (primaryKeyType == null)
                return;

            MemTable<ConcreteKey, Dictionary<string, string>>? memTable = null;
            if (memTableList == null)
            {
                memTable = createMemTable(1, Path.Combine(path, tableName), primaryKeyType);
                if (memTable == null)
                {
                    return;
                }
                //do we need to maintain memtablelist?
                //useful if batching adds
                memTableList = createMemTableList();
                memTableList?.Add(memTable);
            }
            else
            {
                memTable = memTableList[memTableList.Count - 1];
                //check if memtable is full?
                if (memTable.IsFull(ssTableSize))
                {
                    //create a new memtable
                    memTable=createMemTable(memTableList.Count, Path.Combine(path,tableName),primaryKeyType);
                    if(memTable==null){
                        return;
                    }
                    //do we need to maintain memtablelist?
                    //useful if batching adds
                    memTableList?.Add(memTable);
                }
            }
            if (memTable == null)
            {
                return;
            }
            await memTable.put(new ConcreteKey(map[primaryKey], primaryKeyType), map);
            await memTable.serialize();
        }

        public string Get(string jsonString)
        {
            if (memTableList == null || memTableList.Count == 0)
            {
                return "";
            }
            Dictionary<string, string>? req = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
            if (req == null || !req.ContainsKey(primaryKey))
            {
                return "";
            }
            Type? pkType = ModelUtil.getRuntimeType(pkColumnType);
            if (pkType == null)
            {
                return "";
            }
            ConcreteKey ck = new ConcreteKey(req[primaryKey], pkType);
            for (int i = memTableList.Count - 1; i >= 0; i--)
            {
                MyKeyValue<ConcreteKey, Dictionary<string, string>>? kv = memTableList[i].get(ck);
                if (kv != null)
                {
                    return JsonSerializer.Serialize(kv.v);
                }
            }
            return "";
        }

        ///<summary>
        /// todo add start and end range values
        ///</summary>
        public async Task<string> Scan()
        {
            if (memTableList == null || memTableList.Count == 0)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            for (int i = memTableList.Count - 1; i >= 0; i--)
            {
                List<Dictionary<string, string>> rowList = await memTableList[i].scan();
                sb.Append(JsonSerializer.Serialize(rowList));
                if (i > 0)
                    sb.Append(",");
            }
            return sb.ToString();
        }
        public static List<MemTable<ConcreteKey, Dictionary<string, string>>>? createMemTableList()
        {
            var memTableType = typeof(List<>);
            var constructedType = memTableType.MakeGenericType(typeof(MemTable<ConcreteKey, Dictionary<string, string>>));
            return (List<MemTable<ConcreteKey, Dictionary<string, string>>>?)Activator.CreateInstance(constructedType);
        }
        public static MemTable<ConcreteKey, Dictionary<string, string>>? createMemTable(int tableNum, string path, Type? keyType)
        {
            var memTableType = typeof(MemTable<,>);
            var constructedType = memTableType.MakeGenericType(typeof(ConcreteKey), typeof(Dictionary<string, string>));

            return (MemTable<ConcreteKey, Dictionary<string, string>>?)Activator.CreateInstance(constructedType, tableNum, path);
        }
    }
}