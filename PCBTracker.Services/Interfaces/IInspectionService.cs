using PCBTracker.Domain.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PCBTracker.Services.Interfaces
{
    public interface IInspectionService
    {
        Task SubmitInspectionAsync(InspectionDto dto);
        Task<IEnumerable<InspectionDto>> GetInspectionsAsync(InspectionFilterDto filter);
    }
}
