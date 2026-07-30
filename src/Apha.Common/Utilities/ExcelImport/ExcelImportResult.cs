namespace Apha.Common.Utilities.ExcelImport
{    
    public class ExcelImportResult<T>
    {        
        public List<T> Rows { get; set; } = new List<T>();
        public int TotalRows { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> MissingHeaders { get; set; } = new List<string>();
    }
}
