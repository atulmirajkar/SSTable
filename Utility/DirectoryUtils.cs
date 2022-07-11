using System.Collections.Generic;
using System.IO;

namespace Utility{
    public static class DirectoryUtils{
        public static List<string>? getFilesWithExt(string path, string ext){
            if(!Directory.Exists(path)){
                return null;
            }
            List<string> result = new List<string>();
            foreach(var file in Directory.GetFiles(path,"*."+ext,SearchOption.TopDirectoryOnly)){
                result.Add(file);
            }
            return result;
        }
    }
}