using System;
using System.Reflection;

namespace Utility
{
    public class Utility
    {
        public static void Main()
        {
            DynamicClass dc = new DynamicClass("MyAssemblyName","MyClassName");
            dc.AddField("MyField",typeof(int));
            Type? myType = dc.CreateType();
            if(myType == null){
                Console.WriteLine("Could not create type");
                return;
            }
            Object? obj = Activator.CreateInstance(myType);
            if(obj == null){
                Console.WriteLine("Could not create object");
                return;
            }
            PropertyInfo? pi = myType.GetProperty("MyField");
            pi?.SetValue(obj,10);

            Console.WriteLine(pi?.GetValue(obj));
        }

    }
}
