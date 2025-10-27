using PCBTracker.Domain.DTOs;
using PCBTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PCBTracker.Services.Interfaces
{
    public interface IBoardService
    {
        // Existing APIs used across the app
        Task<IEnumerable<string>> GetBoardTypesAsync();
        Task<IEnumerable<Skid>> GetSkidsAsync();
        Task<IEnumerable<SkidDto>> ExtractSkidsAsync();
        Task<Skid> CreateNewSkidAsync();

        Task SubmitBoardAsync(Board board);
        Task CreateBoardAsync(BoardDto dto);
        Task CreateBoardAndClaimSkidAsync(BoardDto dto);

        Task<IEnumerable<BoardDto>> GetBoardsAsync(BoardFilterDto filter);
        Task<IEnumerable<Skid>> GetRecentSkidsAsync(int count);

        Task DeleteBoardBySerialAsync(string serialNumber);
        Task UpdateShipDateForSkidAsync(int skidId, DateTime shipDate);
        Task<int> DeleteBoardsBySkidAsync(int skidId);
        Task<int> UpdateIsImportedAsync(DateTime? date, bool? useShipDate, int? skidId, bool isImported);

        // === NEW minimal read helpers for Submit flow ===
        Task<IEnumerable<BoardDto>> GetBoardsBySkidAsync(int skidId);
        Task<int> GetMaxSkidIdAsync();

        Task<int> ClearShipDateForSkidAsync(int skidId);
    }
}
