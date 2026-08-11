using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.PIMS.DataAccess.Repository
{
    public class PublicationTypeRepository : BaseRepository, IPublicationTypeRepository
    {
        private readonly PimsDbContext _dbContext;

        public PublicationTypeRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<PublicationType>> GetAllPublicationTypesAsync()
        {
            return await _dbContext.PublicationTypes
                .AsNoTracking()
                .OrderBy(p => p.Type)
                .ToListAsync();
        }

        public async Task<PagedData<PublicationType>> GetPagedPublicationTypesAsync(PaginationParameters<string> query)
        {
            var baseQuery = _dbContext.PublicationTypes.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.Filter))
            {
                var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(query.Filter)
                    ?? new Dictionary<string, string>();

                if (filters.TryGetValue("Type", out var typeFilter)
                    && !string.IsNullOrWhiteSpace(typeFilter))
                {
                    var value = typeFilter.Trim();
                    baseQuery = baseQuery.Where(p => EF.Functions.ILike(p.Type, $"%{value}%"));
                }

                if (filters.TryGetValue("Description", out var descFilter)
                    && !string.IsNullOrWhiteSpace(descFilter))
                {
                    var value = descFilter.Trim();
                    baseQuery = baseQuery.Where(p => p.Description != null && EF.Functions.ILike(p.Description, $"%{value}%"));
                }
            }

            baseQuery = (query.SortBy, query.Descending) switch
            {
                ("Type", true)        => baseQuery.OrderByDescending(p => p.Type),
                ("Type", false)       => baseQuery.OrderBy(p => p.Type),
                ("Description", true) => baseQuery.OrderByDescending(p => p.Description),
                ("Description", false)=> baseQuery.OrderBy(p => p.Description),
                (_, true)             => baseQuery.OrderByDescending(p => p.Type),
                _                     => baseQuery.OrderBy(p => p.Type)
            };

            var page = query.Page > 0 ? query.Page : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;
            return await ApplyPaging(baseQuery, page, pageSize);
        }

        public async Task<PublicationType?> GetPublicationTypeByCodeAsync(string type)
        {
            return await _dbContext.PublicationTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Type == type);
        }

        public async Task<PublicationType> AddPublicationTypeAsync(PublicationType entity)
        {
            _dbContext.PublicationTypes.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<PublicationType> UpdatePublicationTypeAsync(PublicationType entity)
        {
            _dbContext.PublicationTypes.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeletePublicationTypeAsync(string type)
        {
            int rowsAffected = await _dbContext.PublicationTypes
                .Where(p => p.Type == type)
                .ExecuteDeleteAsync();

            return rowsAffected > 0;
        }

        public async Task<bool> PublicationTypeExistsAsync(string type)
        {
            return await _dbContext.PublicationTypes
                .AnyAsync(p => p.Type == type);
        }
    }
}
