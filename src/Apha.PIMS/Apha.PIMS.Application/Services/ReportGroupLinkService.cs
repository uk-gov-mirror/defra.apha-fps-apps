using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using AutoMapper;

namespace Apha.PIMS.Application.Services
{
    
    public class ReportGroupLinkService : IReportGroupLinkService
    {
        private readonly IReportGroupLinkRepository _repository;
        private readonly IReportRepository _reportRepository;
        private readonly IReportGroupRepository _reportGroupRepository;
        private readonly IMapper _mapper;

        public ReportGroupLinkService(
            IReportGroupLinkRepository repository,
            IReportRepository reportRepository,
            IReportGroupRepository reportGroupRepository,
            IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _reportRepository = reportRepository ?? throw new ArgumentNullException(nameof(reportRepository));
            _reportGroupRepository = reportGroupRepository ?? throw new ArgumentNullException(nameof(reportGroupRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        
        public async Task<List<ReportGroupLinkDto>> GetAllReportGroupLinksAsync()
        {
            List<ReportGroupLink> entities = await _repository.GetAllReportGroupLinksAsync();
            return _mapper.Map<List<ReportGroupLinkDto>>(entities);
        }

       
        public async Task<List<ReportGroupLinkDto>> GetReportGroupLinksByReportIdAsync(int reportId)
        {
            List<ReportGroupLink> entities = await _repository.GetReportGroupLinksByReportIdAsync(reportId);
            return _mapper.Map<List<ReportGroupLinkDto>>(entities);
        }

        
        public async Task<ReportGroupLinkDto?> GetReportGroupLinkByIdAsync(int reportId, int groupId)
        {
            ReportGroupLink? entity = await _repository.GetReportGroupLinkByIdAsync(reportId, groupId);
            return entity is null ? null : _mapper.Map<ReportGroupLinkDto>(entity);
        }

        private async Task<(string ReportName, string GroupName)> GetDisplayNamesAsync(int reportId, int groupId)
        {
            Report? report = await _reportRepository.GetReportByIdAsync(reportId);
            ReportGroup? reportGroup = await _reportGroupRepository.GetReportGroupByIdAsync(groupId);

            string reportName = report?.ReportName ?? $"reportid={reportId}";
            string groupName = reportGroup?.Description ?? $"groupid={groupId}";

            return (reportName, groupName);
        }

        
        public async Task<ReportGroupLinkDto> CreateReportGroupLinkAsync(ReportGroupLinkDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            bool alreadyExists = await _repository.ReportGroupLinkExistsAsync(dto.ReportId, dto.GroupId);
            if (alreadyExists)
            {
                (string reportName, string groupName) = await GetDisplayNamesAsync(dto.ReportId, dto.GroupId);
                throw new InvalidOperationException(
                    $"Report '{reportName}' and group '{groupName}' already exists.");
            }

            ReportGroupLink entity = _mapper.Map<ReportGroupLink>(dto);
            ReportGroupLink created = await _repository.AddReportGroupLinkAsync(entity);
            return _mapper.Map<ReportGroupLinkDto>(created);
        }

        
        public async Task<bool> DeleteReportGroupLinkAsync(int reportId, int groupId)
        {
            bool exists = await _repository.ReportGroupLinkExistsAsync(reportId, groupId);
            if (!exists)
            {
                (string reportName, string groupName) = await GetDisplayNamesAsync(reportId, groupId);
                throw new KeyNotFoundException(
                    $"Report '{reportName}' and group '{groupName}' was not found.");
            }

            return await _repository.DeleteReportGroupLinkAsync(reportId, groupId);
        }

        public async Task<bool> ReportGroupLinkExistsAsync(int reportId, int groupId)
        {
            return await _repository.ReportGroupLinkExistsAsync(reportId, groupId);
        }
    }
}
