using PCBTracker.Domain.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PCBTracker.Services.Interfaces
{
    public interface IAssemblyCompletionService
    {
        Task SubmitAssemblyAsync(AssemblyCompletionDto dto);
        Task<IEnumerable<AssemblyCompletionDto>> GetAllAsync();
    }
}
