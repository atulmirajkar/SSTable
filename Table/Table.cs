using System;
using System.Collections.Generic;
using System.IO;
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
        private List<MemTable<string, Dictionary<string, string>>> memTableList { get; set; }
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
            string dirName = path + "/" + tableName;
            if (Directory.Exists(dirName))
            {
                return null;
            }
            Directory.CreateDirectory(path);
            var columnList = new List<Tuple<string, ColumnType>>();
            columnList.Add(Tuple.Create(primaryKey, columnType));
            TableMetaData tObj = new TableMetaData(primaryKey, columnType, columnList);
            string mdFile = dirName + "/" + "Table.md";
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
            string mdFile = this.path + "/" + this.tableName + "/" + "Table.md";
            await serializeTableMD(tObj, mdFile);
        }
        //todo deserialize 
        private static async Task serializeTableMD(TableMetaData tObj, string dirName)
        {
            try
            {
                string mdFile = dirName + "/" + "Table.md";
                using (FileStream fs = new FileStream(mdFile, FileMode.CreateNew))
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
            }
        }

        //public
        //accept value as a json
        //{key1:value1, key2: value2}
        public void Add(string jsonString)
        {
            Dictionary<string, string>? map = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
            if (map == null)
            {
                return;
            }
            // Type? myValueType = dynamicClass.CreateType();
            // if (myValueType == null)
            // {
            //     return;
            // }
            // Object? obj = Activator.CreateInstance(myValueType) as Object;
            // foreach (var kv in map)
            // {
            //     if (kv.Key == primaryKey)
            //         continue;
            //     PropertyInfo? pi = myValueType.GetProperty(kv.Key);
            //     //todo correctly convert to type and then setvalue
            //     pi?.SetValue(obj, kv.Value);
            // }
            Type? primaryKeyType = ModelUtil.getRuntimeType(pkColumnType);
            if (primaryKeyType == null)
                return;

            MemTable<string, Dictionary<string, string>>? memTable = null;
            if (memTableList == null)
            {
                memTable = createMemTable(1, path + "/" + tableName);
                if (memTable == null)
                {
                    return;
                }
                //memTableList.Add(memTable);
            }
            else
            {
                memTable = memTableList[memTableList.Count - 1];
            }
            if (memTable == null)
            {
                return;
            }
            memTable.put(map[primaryKey], map);
        }
        public static dynamic Cast(dynamic source, Type dest)
        {
            return Convert.ChangeType(source, dest);
        }
        public static List<MemTable<string, Object>>? createMemTableList(Type type)
        {
            var memTableType = typeof(List<MemTable<string, Object>>);
            var constructedType = memTableType.MakeGenericType(type);
            return (List<MemTable<string, Object>>?)Activator.CreateInstance(constructedType);
        }
        public static MemTable<string, Dictionary<string, string>>? createMemTable(int tableNum, string path)
        {
            var memTableType = typeof(MemTable<,>);
            var constructedType = memTableType.MakeGenericType(typeof(string), typeof(Dictionary<string, string>));
            return (MemTable<string, Dictionary<string, string>>?)Activator.CreateInstance(constructedType, tableNum, path);
        }
    }
}