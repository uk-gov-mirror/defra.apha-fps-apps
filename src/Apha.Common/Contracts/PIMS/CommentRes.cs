using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.Common.Contracts.PIMS
{
    public class CommentRes
    {
        
        public int CommentNo { get; set; }

       
        public string? Project { get; set; }

        
        public int? Year { get; set; }

        
        public string? Topic { get; set; }

        
        public string? Comment { get; set; }

        
        public string? MadeBy { get; set; }

        
        public DateTime? DateEntered { get; set; }

       
        public string? CommentText { get; set; }
    }
}
