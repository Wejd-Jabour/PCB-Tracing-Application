using System.Collections.Generic;
using System.Threading.Tasks;
using PCBTracker.Data.Context;
using PCBTracker.Domain.DTOs;
using PCBTracker.Domain.Entities;

namespace PCBTracker.Services.Interfaces
{
    /// <summary>
    /// Defines the contract for board-related operations.
    /// This abstraction allows ViewModels to depend on an interface,
    /// making testing and swapping implementations easier.
    /// </summary>
    public interface IBoardService
    {
        /// <summary>
        /// Retrieves the list of available board type strings (e.g., "LE", "SAD").
        /// Typically used to populate a Picker in the UI.
        /// </summary>
        Task<IEnumerable<string>> GetBoardTypesAsync();

        /// <summary>
        /// Retrieves all Skid entities currently in the database.
        /// Used to populate a Picker and display existing skids.
        /// </summary>
        Task<IEnumerable<Skid>> GetSkidsAsync();

        /// <summary>
        /// Creates a new Skid with an auto-incremented SkidName (e.g. "Skid-1", "Skid-2").
        /// Returns the saved Skid entity for selection in the UI.
        /// </summary>
        Task<Skid> CreateNewSkidAsync();

        /// <summary>
        /// Persists a Board entity to the database. Expects the entity is fully populated.
        /// Used in advanced scenarios where mapping is done by the caller.
        /// </summary>
        Task SubmitBoardAsync(Board board);

        /// <summary>
        /// Maps a BoardDto to a Board entity, adds it to the context, and saves changes.
        /// This is the primary method used by ViewModels to create a new board entry.
        /// </summay>
        Task CreateBoardAsync(BoardDto boardDto);

        ///<summary>
        /// 
        /// </summary>
        Task CreateBoardAndClaimSkidAsync(BoardDto boardDto);

        /// Returns the most recently‐created skids, up to the specified count.
        /// </summary>
        Task<IEnumerable<Skid>> GetRecentSkidsAsync(int count);


        Task<IEnumerable<BoardDto>> GetBoardsAsync(BoardFilterDto filter);

    }
}
