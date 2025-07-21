// PCBTracker.Services/BoardService.cs
using Microsoft.EntityFrameworkCore;
using PCBTracker.Data.Context;
using PCBTracker.Domain.DTOs;
using PCBTracker.Domain.Entities;
using PCBTracker.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PCBTracker.Services
{
    public class BoardService : IBoardService
    {
        private readonly AppDbContext _db;

        public BoardService(AppDbContext db)
            => _db = db;

        public Task<IEnumerable<string>> GetBoardTypesAsync()
        {
            IEnumerable<string> types = new[] { "LE", "LE Upgrade", "SAD", "SAD Upgrade", "SAT", "SAT Upgrade" };
            return Task.FromResult(types);

        }

        public async Task<IEnumerable<Skid>> GetSkidsAsync()
        {
            // now uses await, so no warning
            return await _db.Skids.ToListAsync();
        }

        public async Task SubmitBoardAsync(Board board)
        {
            _db.Boards.Add(board);
            await _db.SaveChangesAsync();
        }

        public async Task<Skid> CreateNewSkidAsync()
        {
            // Figure out the next SkidID (or start at 1 if none)
            var max = await _db.Skids.AnyAsync()
                ? await _db.Skids.MaxAsync(s => s.SkidID)
                : 0;

            var next = max + 1;
            var skid = new Skid
            {
                SkidName = next.ToString()  // or $"Skid {next}"
            };

            _db.Skids.Add(skid);
            await _db.SaveChangesAsync();

            return skid;
        }

        public async Task CreateBoardAsync(BoardDto dto)
        {
            // map DTO → entity
            var board = new Board
            {
                SerialNumber = dto.SerialNumber,
                PartNumber = dto.PartNumber,
                BoardType = dto.BoardType,
                PrepDate = dto.PrepDate,
                ShipDate = dto.IsShipped
                                 ? dto.ShipDate ?? dto.PrepDate
                                 : null,
                IsShipped = dto.IsShipped,
                SkidID = dto.SkidID
            };

            _db.Boards.Add(board);
            await _db.SaveChangesAsync();
        }
    }
}
