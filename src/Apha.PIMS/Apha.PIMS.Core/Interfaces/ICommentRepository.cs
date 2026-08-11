using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.Core.Interfaces
{
    
    public interface ICommentRepository
    {
        
        Task<PagedData<Comment>> GetCommentsByProjectAsync(string project, int? year, PaginationParameters<string> query, string? topic = null);
        Task<Comment?> GetByIdAsync(int commentNo);
        Task<Comment> AddAsync(Comment entity);
        Task<Comment> UpdateAsync(Comment entity);
        Task<bool> DeleteAsync(int commentNo);
        Task<bool> ExistsAsync(string project, short year, string topic, int? excludeCommentNo = null);
        Task<IEnumerable<CommentTopic>> GetCommentTopicsAsync();
        Task<double?> GetForecastSpendByProjectAsync(string project);
        Task<double?> UpdateForecastSpendByProjectAsync(string project, double? forecastSpend);
    }
}
