using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.FPSApps.Application.Services.PIMS
{
    public class ProjectCommentService : IProjectCommentService
    {
        private readonly IPimsApiClient _client;

        public ProjectCommentService(IPimsApiClient client)
        {
            _client = client;
        }

       
        public async Task<ApiResponseDto<List<CommentDto>>> GetCommentsByProjectAsync(string project, int? year, string? topic, QueryParameters<string> query)
            => await _client.PimsProjectComment.GetCommentsByProjectAsync(project, year, topic, query);

        public async Task<ApiResponseDto<CommentDto>> GetByIdAsync(int commentno)
            => await _client.PimsProjectComment.GetByIdAsync(commentno);

        public async Task<ApiResponseDto<CommentDto>> CreateCommentAsync(CommentDto dto)
            => await _client.PimsProjectComment.CreateCommentAsync(dto);

        public async Task<ApiResponseDto<CommentDto>> UpdateCommentAsync(int commentno, CommentDto dto)
            => await _client.PimsProjectComment.UpdateCommentAsync(commentno, dto);

        public async Task<ApiResponseDto<bool>> DeleteCommentAsync(int commentno)
            => await _client.PimsProjectComment.DeleteCommentAsync(commentno);

        public async Task<ApiResponseDto<List<CommentTopicDto>>> GetCommentTopicsAsync()
            => await _client.PimsProjectComment.GetCommentTopicsAsync();

        public async Task<ApiResponseDto<ProjectCommentForecastSpendDto>> GetForecastSpendByProjectAsync(string project)
            => await _client.PimsProjectComment.GetForecastSpendByProjectAsync(project);

        public async Task<ApiResponseDto<ProjectCommentForecastSpendDto>> UpdateForecastSpendByProjectAsync(string project, double? forecastSpend)
            => await _client.PimsProjectComment.UpdateForecastSpendByProjectAsync(project, forecastSpend);
    }
}
