using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.Application.Interfaces
{
    public interface ICommentService
    {
        
        Task<PaginatedResult<CommentDto>> GetCommentsByProjectAsync(string project, int? year, QueryParameters<string> query, string? topic = null);
        Task<CommentDto?> GetByIdAsync(int commentno);
        Task<CommentDto> AddAsync(CommentDto dto);
        Task<CommentDto> UpdateAsync(CommentDto dto);
        Task<bool> DeleteAsync(int commentno);
        Task<IEnumerable<CommentTopicDto>> GetCommentTopicsAsync();
        Task<double?> GetForecastSpendByProjectAsync(string project);
        Task<double?> UpdateForecastSpendByProjectAsync(string project, double? forecastSpend);
    }
}
