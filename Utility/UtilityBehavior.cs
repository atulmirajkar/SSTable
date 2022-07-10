using System;
using System.Reflection;

namespace Utility
{
    public class Utility
    {
        public static void Main()
        {
            DynamicClass dc = new DynamicClass("MyAssemblyName","MyClassName");
            dc.AddField("MyIntField",typeof(int));
            dc.AddField("MyStringField",typeof(string));
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
            PropertyInfo? pi = myType.GetProperty("MyIntField");
            pi?.SetValue(obj,10);
            
            PropertyInfo? piStr = myType.GetProperty("MyStringField");
            piStr?.SetValue(obj,"someValue");

            Console.WriteLine(pi?.GetValue(obj));
            Console.WriteLine(piStr?.GetValue(obj));
        }

    }
}
