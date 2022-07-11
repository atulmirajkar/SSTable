using System.Threading.Tasks;

namespace Table
{
    internal class Program
    {
        async static Task Main(string[] args)
        {
            Table? table = await Table.createTable("/Users/atulmirajkar/data", "testTable", "ID",Model.ColumnType.Int32);
            if(table == null){
                return;
            }

            await table.AddColumn("Value", Model.ColumnType.Varchar);
            table.Add(@"{""ID"":""123"",""value"":""sdklfj""}");
        }
    }
}