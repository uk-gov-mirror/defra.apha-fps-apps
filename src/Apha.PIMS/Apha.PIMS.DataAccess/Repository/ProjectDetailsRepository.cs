using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PIMS.DataAccess.Repository
{
    public class ProjectDetailsRepository : BaseRepository, IProjectDetailsRepository
    {
        private readonly PimsDbContext _dbContext;

        public ProjectDetailsRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ProjectDetail?> GetPimsDetailAsync(string parentproject)
        {
            return await (
                from rd in _dbContext.ProjectRadTrackData
                join risk in _dbContext.Risks
                    on rd.Riskid equals risk.RiskId into riskGroup
                from risk in riskGroup.DefaultIfEmpty()     // LEFT JOIN — Riskid is nullable
                where rd.Parentproject == parentproject
                select new ProjectDetail
                {
                    Parentproject   = rd.Parentproject,
                    Version         = rd.Version,
                    FileRef         = rd.Fileref,
                    CustomerRef     = rd.Customerref,
                    StartDate       = rd.Startdate,
                    EndDate         = rd.Enddate,
                    CostbookNumber  = rd.Costbooknumber,
                    Riskid          = rd.Riskid,
                    UseProjectYears = rd.Useprojectyear != 0,
                    RevisedEndDate  = rd.Revisedenddate,
                    ClosedDate      = rd.Closeddate
                }
            ).FirstOrDefaultAsync();
        }

        // WRITE: Insert into g_tlkpproject_radtrackdata using EF change tracking
        public async Task<ProjectDetail> AddPimsDetailAsync(ProjectDetail entity)
        {
            var radtrackData = new ProjectRadTrackData
            {
                Parentproject  = entity.Parentproject!,
                Version        = entity.Version,
                Fileref        = entity.FileRef,
                Customerref    = entity.CustomerRef,
                Startdate      = entity.StartDate,
                Enddate        = entity.EndDate,
                Costbooknumber = entity.CostbookNumber,
                Riskid         = entity.Riskid,
                Useprojectyear = (short)(entity.UseProjectYears ? 1 : 0),
                Revisedenddate = entity.RevisedEndDate,
                Closeddate     = entity.ClosedDate
            };

            _dbContext.ProjectRadTrackData.Add(radtrackData);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        // WRITE: Update g_tlkpproject_radtrackdata using EF change tracking
        public async Task<ProjectDetail> UpdatePimsDetailAsync(ProjectDetail entity)
        {
            var existing = await _dbContext.ProjectRadTrackData
                .FirstOrDefaultAsync(r => r.Parentproject == entity.Parentproject);

            if (existing is not null)
            {
                existing.Version        = entity.Version;
                existing.Fileref        = entity.FileRef;
                existing.Customerref    = entity.CustomerRef;
                existing.Startdate      = entity.StartDate;
                existing.Enddate        = entity.EndDate;
                existing.Costbooknumber = entity.CostbookNumber;
                existing.Riskid         = entity.Riskid;
                existing.Useprojectyear = (short)(entity.UseProjectYears ? 1 : 0);
                existing.Revisedenddate = entity.RevisedEndDate;
                existing.Closeddate     = entity.ClosedDate;
                await _dbContext.SaveChangesAsync();
            }

            return entity;
        }

        // ProposedProject methods — unchanged
        public async Task<ProposedProject?> GetProposedProjectAsync(string parentproject)
        {
            return await _dbContext.ProposedProjects
                .FirstOrDefaultAsync(p => p.Parentproject == parentproject);
        }

        public async Task<ProposedProject> UpdateProposedProjectAsync(ProposedProject entity, string transferTo)
        {
            if (entity.Parentproject != transferTo)
            {
                bool codeChanged = await ChangeProjectCodeAsync(entity.Parentproject!, transferTo);
                if (!codeChanged)
                    throw new InvalidOperationException("Failed to change project code for proposed project update.");
                entity.Parentproject = transferTo;
            }
            else {
                _dbContext.ProposedProjects.Update(entity);
                await _dbContext.SaveChangesAsync();
            }
            
            return entity;
        }

        public async Task<List<Risk>> GetAllRiskAsync()
        {
            var risks = await _dbContext.Risks.ToListAsync();

            return risks;
        }

        public async Task<List<Year>> GetAllYearAsync()
        {
            var years = await _dbContext.Years.ToListAsync();

            return years;
        }

        public async Task<Project?> GetFpsProjectByIdAsync(string parentproject)
        {
            return await _dbContext.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Parentproject == parentproject);
        }
        private async Task<bool> ChangeProjectCodeAsync(string oldCode, string newCode)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();

                var comments = await _dbContext.Comments
                    .Where(x => x.Project == oldCode)
                    .ToListAsync();

                if (comments.Count != 0)
                {
                    comments.ForEach(x => x.Project = newCode);
                }

                var proposed = await _dbContext.ProposedProjects
                    .Where(x => x.Parentproject == oldCode)
                    .ToListAsync();

                if (proposed.Count != 0)
                {
                    proposed.ForEach(x => x.Parentproject = newCode);
                }

                var radTrackOld = await _dbContext.ProjectRadTrackData
                    .Where(x => x.Parentproject == oldCode)
                    .ToListAsync();
               

                if (radTrackOld.Count != 0)
                {
                    radTrackOld.ForEach(x => x.Parentproject = newCode);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            });
        }
    }
}
