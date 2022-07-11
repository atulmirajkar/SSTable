using System.Text.Json;
using System.Text;
using Model;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Linq;


//todo move this to a util project, or remame Model project to util and keep both model.cs and util files in that project
namespace Utility{
    
    public class SerializeUtil{

        public static T? deserializeSingleObj<T>(byte[] buffer){
            Utf8JsonReader utf8Reader = new Utf8JsonReader(buffer);
            return JsonSerializer.Deserialize<T>(ref utf8Reader);
        }
        public static byte[] serializeObj<T>(T obj){
            return JsonSerializer.SerializeToUtf8Bytes<T>(obj);
        }

        public async static Task<long> appendToStream(string inputStr, FileStream fs, long offset){
                byte[] startBytes= Encoding.UTF8.GetBytes(inputStr);
                await fs.WriteAsync(startBytes,0,startBytes.Length);
                offset += startBytes.Length;
                return offset;
        }

    }
}