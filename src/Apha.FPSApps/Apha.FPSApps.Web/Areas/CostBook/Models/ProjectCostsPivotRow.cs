using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

namespace Apha.FPSApps.Web.Areas.CostBook.Models
{
    public class ProjectCostsPivotRow
    {
        private static readonly AssemblyBuilder DynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName("Apha.FPSApps.Web.DynamicProjectCostsRows"),
            AssemblyBuilderAccess.Run);

        private static readonly ModuleBuilder DynamicModule = DynamicAssembly.DefineDynamicModule("Main");
        private static readonly ConcurrentDictionary<int, Type> DynamicRowTypes = new();
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> YearPropertiesByType = new();

        public string Project { get; set; } = null!;
        public string Category { get; set; } = null!;
        public decimal Total { get; set; }

        public static ProjectCostsPivotRow Create(int yearCount)
        {
            if (yearCount <= 0)
            {
                return new ProjectCostsPivotRow();
            }

            var dynamicType = DynamicRowTypes.GetOrAdd(yearCount, CreateDynamicRowType);
            return (ProjectCostsPivotRow)Activator.CreateInstance(dynamicType)!;
        }

        public void SetYearValue(int yearIndex, decimal? value)
        {
            if (yearIndex <= 0)
            {
                return;
            }

            var yearProperties = YearPropertiesByType.GetOrAdd(GetType(), static type =>
                type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.Name.Length > 1 && p.Name[0] == 'Y' && int.TryParse(p.Name[1..], out _))
                    .OrderBy(p => int.Parse(p.Name[1..]))
                    .ToArray());

            if (yearIndex <= yearProperties.Length)
            {
                yearProperties[yearIndex - 1].SetValue(this, value);
            }
        }

        private static Type CreateDynamicRowType(int yearCount)
        {
            var typeBuilder = DynamicModule.DefineType(
                $"ProjectCostsPivotRow_{yearCount}",
                TypeAttributes.Public | TypeAttributes.Class,
                typeof(ProjectCostsPivotRow));

            for (int i = 1; i <= yearCount; i++)
            {
                CreateAutoProperty(typeBuilder, $"Y{i}");
            }

            return typeBuilder.CreateTypeInfo()!.AsType();
        }

        private static void CreateAutoProperty(TypeBuilder typeBuilder, string propertyName)
        {
            var propertyType = typeof(decimal?);
            var fieldBuilder = typeBuilder.DefineField($"_{propertyName}", propertyType, FieldAttributes.Private);

            var propertyBuilder = typeBuilder.DefineProperty(
                propertyName,
                PropertyAttributes.HasDefault,
                propertyType,
                null);

            var methodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

            var getterBuilder = typeBuilder.DefineMethod($"get_{propertyName}", methodAttributes, propertyType, Type.EmptyTypes);
            var getterIl = getterBuilder.GetILGenerator();
            getterIl.Emit(OpCodes.Ldarg_0);
            getterIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getterIl.Emit(OpCodes.Ret);

            var setterBuilder = typeBuilder.DefineMethod($"set_{propertyName}", methodAttributes, null, new[] { propertyType });
            var setterIl = setterBuilder.GetILGenerator();
            setterIl.Emit(OpCodes.Ldarg_0);
            setterIl.Emit(OpCodes.Ldarg_1);
            setterIl.Emit(OpCodes.Stfld, fieldBuilder);
            setterIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getterBuilder);
            propertyBuilder.SetSetMethod(setterBuilder);
        }
    }
}
