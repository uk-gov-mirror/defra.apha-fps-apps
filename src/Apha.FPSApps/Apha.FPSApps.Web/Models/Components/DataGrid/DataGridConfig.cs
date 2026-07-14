namespace Apha.FPSApps.Web.Models.Components.DataGrid
{
    public class DataGridConfig<T> where T : class
    {
        public string GridId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public List<DataGridColumn> Columns { get; set; }
        public List<T> Data { get; set; }
        public bool ShowCheckboxColumn { get; set; }
        public bool ShowPagination { get; set; }
        public string KeyProperty { get; set; } = string.Empty;
        public bool AllowAdd { get; set; }
        public bool AllowCopy { get; set; }
        public bool AllowEdit { get; set; }
        public bool AllowDelete { get; set; }
        public bool AllowRowSelection { get; set; }
        public bool AllowBulkCopy { get; set; }
        public bool AllowBulkDelete { get; set; }
        public bool AllowView { get; set; }
        public bool AllowExport { get; set; }
        public string BulkCopyButtonText { get; set; } = "Copy";
        public string BulkCopyFunction { get; set; } = string.Empty;
        public string BulkDeleteFunction { get; set; } = string.Empty;
        public string RowSelectFunction { get; set; } = string.Empty;
        public string AddFunction { get; set; } = string.Empty;
        public string CopyFunction { get; set; } = string.Empty;
        public string EditFunction { get; set; } = string.Empty;
        public string DeleteFunction { get; set; } = string.Empty;
        public string ViewFunction { get; set; } = string.Empty;
        public string ExportUrl { get; set; } = string.Empty;
        public string BindGridUrl { get; set; } = string.Empty;
        public string ExtraFilterMethod { get; set; } = string.Empty;
        public PaginationModel Pagination { get; set; } = new PaginationModel();
        public string? CurrentSearch { get; set; }
        public Dictionary<string, string>? CurrentFilters { get; set; } = null;

        /// <summary>
        /// Optional column-group header row rendered above the main header.
        /// Each entry spans the given number of visible columns (left to right).
        /// Use an empty Label for ungrouped filler columns.
        /// </summary>
        public List<DataGridColumnGroup>? ColumnGroups { get; set; }

        public DataGridConfig()
        {
            Columns = new List<DataGridColumn>();
            Data = new List<T>();
            ShowCheckboxColumn = false;
            ShowPagination = true;
            AllowAdd = true;
            AllowCopy = false;
            AllowEdit = true;
            AllowDelete = true;
            AllowRowSelection = false;
            AllowBulkCopy = false;
            AllowBulkDelete = false;
            AllowView = false;
            AllowExport = false;
        }
    }
}
