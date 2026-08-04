using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;
using System.Linq.Expressions;

namespace Apha.PIMS.DataAccess.Repository
{
    public class CommentRepository:BaseRepository, ICommentRepository
    {
        private readonly PimsDbContext _dbContext;

        public CommentRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        
        public async Task<PagedData<Comment>> GetCommentsByProjectAsync(string project, int? year, PaginationParameters<string> query, string? topic = null)
        {
            IQueryable<Comment> baseQuery = _dbContext.Comments
                .AsNoTracking()
                .Where(c => c.Project == project);

            if (year.HasValue)
                baseQuery = baseQuery.Where(c => c.Year == year.Value);

            
            if (!string.IsNullOrEmpty(topic))
                baseQuery = baseQuery.Where(c => EF.Functions.ILike(c.Topic, topic));

            
            baseQuery = ApplyFilter(baseQuery, query.Filter);
            baseQuery = ApplySorting(baseQuery, query.SortBy, query.Descending);

            return await ApplyPaging(baseQuery, query.Page, query.PageSize);
        }

        public async Task<Comment?> GetByIdAsync(int commentNo)
        {
            return await _dbContext.Comments
                .FirstOrDefaultAsync(c => c.CommentNo == commentNo);
        }

        public async Task<bool> ExistsAsync(string project, short year, string topic, int? excludeCommentNo = null)
        {
            IQueryable<Comment> query = _dbContext.Comments
                .Where(c => c.Project == project && c.Year == year && c.Topic == topic);

            if (excludeCommentNo.HasValue)
                query = query.Where(c => c.CommentNo != excludeCommentNo.Value);

            return await query.AnyAsync();
        }

        public async Task<Comment> AddAsync(Comment entity)
        {
            _dbContext.Comments.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<Comment> UpdateAsync(Comment entity)
        {
            _dbContext.Comments.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int commentNo)
        {
            Comment? entity = await _dbContext.Comments.FindAsync(commentNo);
            if (entity is null) return false;
            _dbContext.Comments.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CommentTopic>> GetCommentTopicsAsync()
        { 
            return await _dbContext.CommentTopics.ToListAsync();
        }

        public async Task<double?> GetForecastSpendByProjectAsync(string project)
        {
            return await _dbContext.ProjectRadTrackData
                .AsNoTracking()
                .Where(x => x.Parentproject == project)
                .Select(x => x.Pcforecastspend)
                .FirstOrDefaultAsync();
        }

        public async Task<double?> UpdateForecastSpendByProjectAsync(string project, double? forecastSpend)
        {
            ProjectRadTrackData? entity = await _dbContext.ProjectRadTrackData
                .FirstOrDefaultAsync(x => x.Parentproject == project);

            if (entity is null)
                return null;

            entity.Pcforecastspend = forecastSpend;
            await _dbContext.SaveChangesAsync();
            return entity.Pcforecastspend;
        }

        private static IQueryable<Comment> ApplyFilter(IQueryable<Comment> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter) || filter == "{}")
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("Year", out var year) && year != null && int.TryParse(year.ToString(), out int yearVal))
                query = query.Where(x => x.Year == yearVal);

            if (dict.TryGetValue("Topic", out var topic) && topic != null)
            {
                string val = topic.ToString()!;
                query = query.Where(x => EF.Functions.ILike(x.Topic, $"%{val}%"));
            }

            if (dict.TryGetValue("Comment", out var comment) && comment != null)
            {
                string val = comment.ToString()!;
                query = query.Where(x => x.CommentText != null && EF.Functions.ILike(x.CommentText, $"%{val}%"));
            }

            if (dict.TryGetValue("MadeBy", out var madeBy) && madeBy != null)
            {
                string val = madeBy.ToString()!;
                query = query.Where(x => x.MadeBy != null && EF.Functions.ILike(x.MadeBy, $"%{val}%"));
            }

            if (dict.TryGetValue("DateEntered", out var dateEntered) && dateEntered != null
                && DateTime.TryParse(dateEntered.ToString(), out DateTime dateVal))
            {
                DateTime from = dateVal.Date;
                DateTime to = from.AddDays(1);
                query = query.Where(x => x.DateEntered.HasValue && x.DateEntered.Value >= from && x.DateEntered.Value < to);
            }

            return query;
        }

        private static IQueryable<Comment> ApplySorting(IQueryable<Comment> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderByDescending(c => c.Year).ThenByDescending(c => c.CommentNo);

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable<Comment> ApplySortingByProperty(IQueryable<Comment> query, string property, bool descending)
        {
            return property switch
            {
                "commentno" => ApplyOrder(query, c => c.CommentNo, descending),
                "project" => ApplyOrder(query, c => c.Project, descending),
                "year" => ApplyOrder(query, c => c.Year, descending),
                "topic" => ApplyOrder(query, c => c.Topic, descending),
                "dateentered" => ApplyOrder(query, c => c.DateEntered, descending),
                "madeby" => ApplyOrder(query, c => c.MadeBy, descending),
                _ => query.OrderByDescending(c => c.Year).ThenByDescending(c => c.CommentNo)
            };
        }

        private static IQueryable<Comment> ApplyOrder<T>(IQueryable<Comment> query, Expression<Func<Comment, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }
    }
}


