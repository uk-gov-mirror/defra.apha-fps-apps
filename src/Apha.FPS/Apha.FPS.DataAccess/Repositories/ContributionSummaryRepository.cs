using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    /// <summary>
    /// Repository for the Income/Contribution from Time Sales summary view (frmTimeSellerPC).
    /// </summary>
    public class ContributionSummaryRepository : IContributionSummaryRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public ContributionSummaryRepository(FpsDbContext dbContext, IFpsRequestContext requestContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
        }

        /// <inheritdoc/>
        public async Task<List<ContributionSummaryView>> GetBySellingPcAsync(string sellingPc)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sellingPc);

            return await _dbContext.VQryFrmTimeSellerPcViews
                .AsNoTracking()
                .Where(x => x.SellingPc == sellingPc
                    && x.UserEmail != null
                    && x.UserEmail.ToLower() == _requestContext.UserEmailId)
                .OrderBy(x => x.WorkGroup)
                .ThenBy(x => x.WgGrade)
                .ToListAsync();
        }
    }
}
