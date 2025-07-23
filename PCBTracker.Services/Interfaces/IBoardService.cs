using PCBTracker.Domain.DTOs;
using PCBTracker.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PCBTracker.Services.Interfaces
{
    /// <summary>
    /// Defines the contract for board-related operations.
    /// </summary>
    public interface IBoardService
    {
        Task<IEnumerable<string>> GetBoardTypesAsync();

        /// <summary>
        /// (Existing) Used by submit flows to list entity-based skids.
        /// </summary>
        Task<IEnumerable<Skid>> GetSkidsAsync();

        /// <summary>
        /// (New) Used by the extract page to populate its Skid filter.
        /// </summary>
        Task<IEnumerable<SkidDto>> ExtractSkidsAsync();

        Task<Skid> CreateNewSkidAsync();
        Task SubmitBoardAsync(Board board);
        Task CreateBoardAsync(BoardDto boardDto);
        Task CreateBoardAndClaimSkidAsync(BoardDto boardDto);
        Task<IEnumerable<Skid>> GetRecentSkidsAsync(int count);
        Task<IEnumerable<BoardDto>> GetBoardsAsync(BoardFilterDto filter);
    }
}
