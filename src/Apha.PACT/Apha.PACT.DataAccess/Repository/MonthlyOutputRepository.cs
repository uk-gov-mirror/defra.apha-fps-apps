using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PACT.DataAccess.Repository
{
    public class MonthlyOutputRepository : BaseRepository, IMonthlyOutputRepository
    {
        public MonthlyOutputRepository(FpsDbContext context) : base(context)
        {
        }

        public async Task<PagedData<MonthlyOutputLog>> GetMonthlyOutputLogAsync(
            PaginationParameters<string> query,
            string? workGroup,
            string? testCode,
            string? buyer,
            DateTime? dateImported,
            double? month,
            string? userId,
            string? insertDelete)
        {
            var baseQuery = _context.MonthlyOutputLogs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(workGroup))
                baseQuery = baseQuery.Where(x => x.WorkGroup == workGroup);

            if (!string.IsNullOrWhiteSpace(testCode))
                baseQuery = baseQuery.Where(x => x.TestCode == testCode);

            if (!string.IsNullOrWhiteSpace(buyer))
                baseQuery = baseQuery.Where(x => x.Buyer == buyer);

            if (dateImported.HasValue)
            {
                var dateOnly = dateImported.Value.Date;
                baseQuery = baseQuery.Where(x => x.DateTime.HasValue
                    && x.DateTime.Value.Date == dateOnly);
            }

            if (month.HasValue)
                baseQuery = baseQuery.Where(x => x.Month.HasValue && (int)x.Month.Value == (int)month.Value);

            if (!string.IsNullOrWhiteSpace(userId))
                baseQuery = baseQuery.Where(x => x.UserId != null && x.UserId.Contains(userId));

            if (!string.IsNullOrWhiteSpace(insertDelete))
                baseQuery = baseQuery.Where(x => x.InsertDelete != null
                    && x.InsertDelete.StartsWith(insertDelete));

            baseQuery = baseQuery.OrderByDescending(x => x.DateTime).ThenBy(x => x.SequenceNo);

            return await ApplyPaging(baseQuery, query.Page, query.PageSize);
        }

        public async Task<bool> ExistsByTestCodeAndWorkGroupAsync(string testCode, string workGroup)
        {
            return await _context.MonthlyOutputs
                .AsNoTracking()
                .AnyAsync(m => m.TestCode == testCode && m.WorkGroup == workGroup);
        }
    }
}
