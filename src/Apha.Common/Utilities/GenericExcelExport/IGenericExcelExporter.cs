namespace Apha.Common.Utilities.GenericExcelExport
{
    /// <summary>
    /// Generic, reflection-based Excel exporter. Accepts any collection and produces
    /// a .xlsx byte array. Column headers are resolved from, in order of precedence:
    ///   1. [ExcelColumn(Name = "...")]
    ///   2. [Display(Name = "...")]
    ///   3. the property name.
    /// Properties decorated with [ExcelIgnore] are skipped.
    /// </summary>
    public interface IGenericExcelExporter
    {
        /// <summary>
        /// Exports a collection to a single-sheet Excel workbook.
        /// </summary>
        /// <typeparam name="T">Row type. Any class/record with public instance properties.</typeparam>
        /// <param name="data">The rows to export. A null value is treated as empty.</param>
        /// <param name="sheetName">The worksheet name.</param>
        /// <returns>The generated .xlsx file as a byte array.</returns>
        byte[] Export<T>(IEnumerable<T> data, string sheetName = "Sheet1");
    }
}
