using PCBTracker.Domain.DTOs;
using PCBTracker.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PCBTracker.Services.Interfaces
{
    /// <summary>
    /// Declares the interface for board-related service operations.
    /// Implementations of this interface handle all application logic
    /// for reading, creating, modifying, and deleting board and skid data.
    /// </summary>
    public interface IBoardService
    {
        /// <summary>
        /// Returns a list of supported board type names.
        /// These values are used in UI pickers and board validation logic.
        /// </summary>
        Task<IEnumerable<string>> GetBoardTypesAsync();

        /// <summary>
        /// Returns all Skid entities from the database.
        /// Used in submission workflows to assign or select available skids.
        /// </summary>
        Task<IEnumerable<Skid>> GetSkidsAsync();

        /// <summary>
        /// Returns simplified skid representations (SkidDto) for filtering use in the extract page.
        /// Each entry includes only the ID and name.
        /// </summary>
        Task<IEnumerable<SkidDto>> ExtractSkidsAsync();

        /// <summary>
        /// Creates a new Skid record in the database with a sequentially generated name.
        /// </summary>
        Task<Skid> CreateNewSkidAsync();

        /// <summary>
        /// Submits a Board entity directly to the database.
        /// This method is used when a Board instance is already constructed.
        /// </summary>
        Task SubmitBoardAsync(Board board);

        /// <summary>
        /// Accepts a BoardDto and persists a new Board to the database.
        /// Handles mapping from DTO to entity format.
        /// </summary>
        Task CreateBoardAsync(BoardDto boardDto);


        /// <summary>
        /// Persists a BoardDto to the database and updates the target skid
        /// with a designated board type if it has not been assigned one.
        /// Also inserts the board into a subtype table based on its BoardType value.
        /// </summary>
        Task CreateBoardAndClaimSkidAsync(BoardDto boardDto);

        /// <summary>
        /// Retrieves the most recent skid records, sorted by descending SkidID,
        /// and limited by the specified count.
        /// </summary>
        Task<IEnumerable<Skid>> GetRecentSkidsAsync(int count);

        /// <summary>
        /// Returns a filtered list of BoardDto results matching the given filter parameters.
        /// Supports paging and conditional filtering on fields such as type, date range, and skid.
        /// </summary>
        Task<IEnumerable<BoardDto>> GetBoardsAsync(BoardFilterDto filter);

        /// <summary>
        /// Deletes a board record based on its serial number.
        /// Removes the board from the database if it exists.
        /// </summary>
        Task DeleteBoardBySerialAsync(string serialNumber);

        /// <summary>
        /// Updates the ShipDate of all boards assigned to the specified skid.
        /// Applies the same date to each associated board.
        /// </summary>
        Task UpdateShipDateForSkidAsync(int skidId, DateTime shipDate);

        /// <summary>
        /// Clears the ShipDate (sets to null) and sets IsShipped = false
        /// for all boards on the specified skid. Implementations should also
        /// mirror this to legacy subtype tables where applicable.
        /// </summary>
        Task ClearShipDateForSkidAsync(int skidId);

        Task<int> DeleteBoardsBySkidAsync(int skidId);

        /// <summary>
        /// Updates IsImported for boards filtered by optional date (prep or ship) and/or skid.
        /// - If <paramref name="date"/> is provided, the date comparison uses date-only semantics.
        /// - If <paramref name="useShipDate"/> is true, compares ShipDate; otherwise compares PrepDate.
        /// - If <paramref name="skidId"/> is provided, also filters by that skid.
        /// Returns the number of affected rows.
        /// </summary>
        Task<int> UpdateIsImportedAsync(DateTime? date, bool? useShipDate, int? skidId, bool isImported);
    }
}
