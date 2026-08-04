namespace Apha.Common.Utilities.ExcelExport
{
    public class ExcelSheetDefinition
    {
        public string SheetName { get; set; } = "Sheet";
        public IEnumerable<object> Data { get; set; } = Enumerable.Empty<object>();
        public Type DataType { get; set; } = typeof(object);

        /// <summary>
        /// When set, only properties whose names are in this list will be included as columns.
        /// When null, all public instance properties are included.
        /// </summary>
        public IEnumerable<string>? IncludedProperties { get; set; }

        /// <summary>
        /// Property names (not display names) whose cells should be locked against user edits,
        /// for every data row, once this sheet is protected (existing-row routing
        /// cells are reference-only — the API rejects any change regardless, this just makes the
        /// restriction visible at the point of entry). Every other cell in the sheet remains
        /// editable. Null/empty means no protection is applied to this sheet — existing exports
        /// that never set this are completely unaffected.
        /// </summary>
        public IReadOnlyCollection<string>? ProtectedColumnNames { get; set; }

        /// <summary>
        /// Maps a property name (not display name) to a formula template written as that
        /// column's live Excel formula for every data row, instead of a static value.
        /// A template references OTHER properties in the same sheet via <c>{PropertyName}</c>
        /// placeholders — each is resolved to that property's own column letter plus the
        /// current row number (e.g. "D2") before being written as the cell's FormulaA1, so the
        /// template stays correct regardless of column order or <see cref="IncludedProperties"/>
        /// filtering. The formula is for the user's visibility only — it is never read back by
        /// the upload parser, which always recalculates the equivalent value itself server-side.
        /// Null means no formula column — existing exports that never set this are completely
        /// unaffected.
        /// </summary>
        public IReadOnlyDictionary<string, string>? FormulaColumns { get; set; }
    }
}
