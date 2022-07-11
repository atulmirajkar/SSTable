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
        private Dictionary<string, ColumnType> colMap { get; set; }
        private DynamicClass dynamicClass { get; set; }

        //todo  - creating memtable
        private List<MemTable<ConcreteKey, Dictionary<string, string>>>? memTableList { get; set; }
        private Table(string path, string tableName, string primaryKey, ColumnType columnType)
        {
            this.path = path;
            this.tableName = tableName;
            this.primaryKey = primaryKey;
            this.pkColumnType = columnType;
            this.colMap = new Dictionary<string, ColumnType>();
            colMap.Add(primaryKey, columnType);
            dynamicClass = new DynamicClass(tableName, tableName);
        }

        public async static Task<Table?> createTable(string path, string tableName, string primaryKey, ColumnType columnType)
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
            return new Table(path, tableName, primaryKey, columnType);
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
            if (runTimeColumnType != null)
                dynamicClass.AddField(columnName, runTimeColumnType);

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
        //todo deserialize 
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
        //accept value as a json
        //{key1:value1, key2: value2}
        public async void Add(string jsonString)
        {
            Dictionary<string, string>? map = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
            if (map == null)
            {
                return;
            }
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
                memTableList = createMemTableList();
                memTableList?.Add(memTable);
            }
            else
            {
                memTable = memTableList[memTableList.Count - 1];
            }
            if (memTable == null)
            {
                return;
            }
            memTable.put(new ConcreteKey(map[primaryKey], primaryKeyType), map);
            await memTable.serialize();
        }

        public string Get(string jsonString)
        {
            if(memTableList == null || memTableList.Count==0){
                return "";
            }
            Dictionary<string, string>? req = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
            if (req == null || !req.ContainsKey(primaryKey))
            {
                return "";
            }
            Type? pkType = ModelUtil.getRuntimeType(pkColumnType);
            if(pkType==null){
                return "";
            }
            ConcreteKey ck = new ConcreteKey(req[primaryKey], pkType);
            for(int i=memTableList.Count-1; i>=0; i--){
                MyKeyValue<ConcreteKey, Dictionary<string, string>>? kv = memTableList[i].get(ck);
                if(kv!=null){
                    return JsonSerializer.Serialize(kv.v);
                }
            }
            return "";
        }

        public string Scan(){
            if(memTableList == null || memTableList.Count == 0){
                return "";
            }
            
            StringBuilder sb = new StringBuilder();
            for(int i=memTableList.Count-1; i>=0; i--){
                List<Dictionary<string, string>> rowList = memTableList[i].scan();
                sb.Append(JsonSerializer.Serialize(rowList));
                if(i>0)
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