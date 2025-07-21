// PCBTracker.Services/Interfaces/IBoardService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using PCBTracker.Domain.Entities;

namespace PCBTracker.Services.Interfaces
{
    public interface IBoardService
    {
        /// <summary>
        /// Return a list of all board‐type values.
        /// </summary>
        Task<IEnumerable<string>> GetBoardTypesAsync();

        /// <summary>
        /// Return a list of all Skids.
        /// </summary>
        Task<IEnumerable<Skid>> GetSkidsAsync();

        /// <summary>
        /// Creates (and persists) a new Skid row with the next number, 
        /// and returns it.
        /// </summary>
        Task<Skid> CreateNewSkidAsync();


        /// <summary>
        /// Save a new Board record.
        /// </summary>
        Task SubmitBoardAsync(Board board);
    }
}
