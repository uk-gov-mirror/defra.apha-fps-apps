using Apha.PACT.Core.Entities;

namespace Apha.PACT.Core.Interfaces
{
    public interface IMonthRepository
    {
        Task<IEnumerable<Month>> GetAllMonthsAsync();        
    }
}
