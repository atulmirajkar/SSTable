using System.Reflection;
using System.Reflection.Emit;
using System;
using Model;

namespace Utility
{
    public class DynamicClass
    {
        private TypeBuilder? typeBuilder { get; set; }
        public DynamicClass(string assemblyName, string className)
        {
            AssemblyName aName = new AssemblyName(assemblyName);
            AssemblyBuilder aBuilder = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = aBuilder.DefineDynamicModule(aName.Name != null ? aName.Name : "");
            this.typeBuilder = moduleBuilder.DefineType(className, TypeAttributes.Public);
        }

        public void AddField(string name, Type type)
        {
            if (typeBuilder == null)
            {
                return;
            }
            Type propertyType = type;
            string propertyName = name;
            string fieldName = $"_{propertyName}";

            FieldBuilder fieldBuilder = typeBuilder.DefineField(fieldName, propertyType, FieldAttributes.Private);

            // The property set and get methods require a special set of attributes.
            MethodAttributes getSetAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

            // Define the 'get' accessor method.
            MethodBuilder getMethodBuilder = typeBuilder.DefineMethod($"get_{propertyName}", getSetAttributes, propertyType, Type.EmptyTypes);
            ILGenerator propertyGetGenerator = getMethodBuilder.GetILGenerator();
            propertyGetGenerator.Emit(OpCodes.Ldarg_0);
            propertyGetGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
            propertyGetGenerator.Emit(OpCodes.Ret);

            // Define the 'set' accessor method.
            MethodBuilder setMethodBuilder = typeBuilder.DefineMethod($"set_{propertyName}", getSetAttributes, null, new Type[] { propertyType });
            ILGenerator propertySetGenerator = setMethodBuilder.GetILGenerator();
            propertySetGenerator.Emit(OpCodes.Ldarg_0);
            propertySetGenerator.Emit(OpCodes.Ldarg_1);
            propertySetGenerator.Emit(OpCodes.Stfld, fieldBuilder);
            propertySetGenerator.Emit(OpCodes.Ret);

            // Lastly, we must map the two methods created above to a PropertyBuilder and their corresponding behaviors, 'get' and 'set' respectively.
            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            propertyBuilder.SetGetMethod(getMethodBuilder);
            propertyBuilder.SetSetMethod(setMethodBuilder);
        }

        public Type? CreateType()
        {
            return typeBuilder?.CreateType();
        }
    }
}