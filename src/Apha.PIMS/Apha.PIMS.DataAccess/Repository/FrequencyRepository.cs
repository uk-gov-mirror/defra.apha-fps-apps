using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.PIMS.DataAccess.Repository
{
    public class FrequencyRepository : BaseRepository, IFrequencyRepository
    {
        private readonly PimsDbContext _dbContext;

        public FrequencyRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Frequency>> GetAllFrequenciesAsync()
        {
            return await _dbContext.Frequencies
                .AsNoTracking()
                .OrderBy(f => f.FrequencyId)
                .ToListAsync();
        }

        public async Task<PagedData<Frequency>> GetPagedFrequenciesAsync(PaginationParameters<string> query)
        {
            var baseQuery = _dbContext.Frequencies.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.Filter))
            {
                var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(query.Filter)
                    ?? new Dictionary<string, string>();

                if (filters.TryGetValue("Frequencyid", out var frequencyIdFilter)
                    && int.TryParse(frequencyIdFilter, out var frequencyId))
                {
                    baseQuery = baseQuery.Where(f => f.FrequencyId == frequencyId);
                }

                if (filters.TryGetValue("FrequencyValue", out var frequencyValueFilter)
                    && !string.IsNullOrWhiteSpace(frequencyValueFilter))
                {
                    var value = frequencyValueFilter.Trim();
                    baseQuery = baseQuery.Where(f => f.FrequencyValue != null &&
                        EF.Functions.ILike(f.FrequencyValue, $"%{value}%"));
                }
            }

            baseQuery = (query.SortBy, query.Descending) switch
            {
                ("FrequencyId", true) => baseQuery.OrderByDescending(f => f.FrequencyId),
                ("FrequencyId", false) => baseQuery.OrderBy(f => f.FrequencyId),
                ("FrequencyValue", true) => baseQuery.OrderByDescending(f => f.FrequencyValue),
                ("FrequencyValue", false) => baseQuery.OrderBy(f => f.FrequencyValue),
                (_, true) => baseQuery.OrderByDescending(f => f.FrequencyId),
                _ => baseQuery.OrderBy(f => f.FrequencyId)
            };

            var page = query.Page > 0 ? query.Page : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;
            return await ApplyPaging(baseQuery, page, pageSize);
        }

        public async Task<Frequency?> GetFrequencyByIdAsync(int frequencyId)
        {
            return await _dbContext.Frequencies
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.FrequencyId == frequencyId);
        }

        public async Task<Frequency> AddFrequencyAsync(Frequency entity)
        {
            _dbContext.Frequencies.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<Frequency> UpdateFrequencyAsync(Frequency entity)
        {
            _dbContext.Frequencies.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteFrequencyAsync(int frequencyId)
        {
            int rowsAffected = await _dbContext.Frequencies
                .Where(f => f.FrequencyId == frequencyId)
                .ExecuteDeleteAsync();

            return rowsAffected > 0;
        }

        public async Task<bool> FrequencyExistsAsync(int frequencyId)
        {
            return await _dbContext.Frequencies
                .AnyAsync(f => f.FrequencyId == frequencyId);
        }
    }
}
