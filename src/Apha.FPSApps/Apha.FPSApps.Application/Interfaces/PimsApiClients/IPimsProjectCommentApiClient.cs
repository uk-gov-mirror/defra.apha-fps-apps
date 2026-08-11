using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    public interface IPimsProjectCommentApiClient
    {
       
        Task<ApiResponseDto<List<CommentDto>>> GetCommentsByProjectAsync(string project, int? year, string? topic, QueryParameters<string> query);

        
        Task<ApiResponseDto<CommentDto>> GetByIdAsync(int commentno);

        
        Task<ApiResponseDto<CommentDto>> CreateCommentAsync(CommentDto dto);

       
        Task<ApiResponseDto<CommentDto>> UpdateCommentAsync(int commentno, CommentDto dto);

       
        Task<ApiResponseDto<bool>> DeleteCommentAsync(int commentno);

        
        Task<ApiResponseDto<List<CommentTopicDto>>> GetCommentTopicsAsync();

        
        Task<ApiResponseDto<ProjectCommentForecastSpendDto>> GetForecastSpendByProjectAsync(string project);

        
        Task<ApiResponseDto<ProjectCommentForecastSpendDto>> UpdateForecastSpendByProjectAsync(string project, double? forecastSpend);
    }
}
