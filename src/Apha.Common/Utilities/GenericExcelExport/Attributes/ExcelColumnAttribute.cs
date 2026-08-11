namespace Apha.Common.Utilities.GenericExcelExport.Attributes
{
    /// <summary>
    /// Optional per-property configuration for the generic Excel export.
    /// Any value left unset falls back to sensible defaults (see remarks on each member).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ExcelColumnAttribute : Attribute
    {
        /// <summary>
        /// Overrides the column header text. When null/empty the exporter falls back to
        /// [Display(Name = ...)] and finally the property name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Controls the left-to-right ordering of columns (ascending).
        /// Columns without an explicit order are placed after ordered ones, in reflection order.
        /// </summary>
        public int Order { get; set; } = int.MaxValue;

        /// <summary>
        /// Excel number format string (e.g. "#,##0.00", "£#,##0.00", "dd/MM/yyyy").
        /// When null the exporter falls back to [DisplayFormat(DataFormatString = ...)] if present.
        /// </summary>
        public string? Format { get; set; }

        /// <summary>
        /// Optional fixed column width. When null the column auto-fits to content.
        /// </summary>
        public double Width { get; set; } = 0;
    }
}
