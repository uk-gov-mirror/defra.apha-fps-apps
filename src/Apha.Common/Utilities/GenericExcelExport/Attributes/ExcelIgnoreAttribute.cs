namespace Apha.Common.Utilities.GenericExcelExport.Attributes
{
    /// <summary>
    /// Marks a property to be excluded from the generated Excel export.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ExcelIgnoreAttribute : Attribute
    {
    }
}
