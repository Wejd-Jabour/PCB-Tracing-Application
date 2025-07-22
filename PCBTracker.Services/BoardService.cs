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
    /// <summary>
    /// Implements IBoardService to handle board-related business logic and data persistence.
    /// </summary>
    public class BoardService : IBoardService
    {
        private readonly AppDbContext _db;

        /// <summary>
        /// AppDbContext is injected via DI to access the database.
        /// </summary>
        public BoardService(AppDbContext db)
            => _db = db;

        /// <summary>
        /// Returns a fixed set of board types; in the future, consider seeding these in the database
        /// or making them configurable.
        /// </summary>
        public Task<IEnumerable<string>> GetBoardTypesAsync()
        {
            // Hard-coded lookup values for UI Picker
            IEnumerable<string> types = new[] { "LE", "LE Upgrade", "SAD", "SAD Upgrade", "SAT", "SAT Upgrade" };
            return Task.FromResult(types);
        }

        /// <summary>
        /// Retrieves all Skid records from the database to populate a Picker.
        /// </summary>
        public async Task<IEnumerable<Skid>> GetSkidsAsync()
        {
            // Asynchronously fetch list of skids
            return await _db.Skids.ToListAsync();
        }

        /// <summary>
        /// Persists a fully-formed Board entity.
        /// This lower-level method is for advanced scenarios where the caller maps DTO to entity.
        /// </summary>
        public async Task SubmitBoardAsync(Board board)
        {
            _db.Boards.Add(board);
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Creates a new Skid with an auto-incremented name based on the current max ID.
        /// Ensures each SkidName is unique and sequential.
        /// </summary>
        public async Task<Skid> CreateNewSkidAsync()
        {
            // Determine the highest existing SkidID, or start from zero
            var max = await _db.Skids.AnyAsync()
                ? await _db.Skids.MaxAsync(s => s.SkidID)
                : 0;

            var next = max + 1;
            var skid = new Skid
            {
                // Using numeric naming; you could prefix with "Skid-" if desired.
                SkidName = next.ToString()
            };

            // Save the new Skid and return it for UI selection
            _db.Skids.Add(skid);
            await _db.SaveChangesAsync();
            return skid;
        }

        /// <summary>
        /// Maps a BoardDto to the Board entity, handles defaulting ShipDate,
        /// and persists the new Board record in one step.
        /// </summary>
        public async Task CreateBoardAsync(BoardDto dto)
        {
            // Map incoming DTO values to a new Board entity
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

            // Add to DbSet and save; EF Core tracks changes automatically
            _db.Boards.Add(board);
            await _db.SaveChangesAsync();
        }

        public async Task CreateBoardAndClaimSkidAsync(BoardDto dto)
        {
            // 1) Load the selected skid
            var skid = await _db.Skids.FindAsync(dto.SkidID);

            // 2) Claim it if unassigned
            if (skid != null && skid.designatedType is null)
            {
                skid.designatedType = dto.BoardType;
                _db.Skids.Update(skid);
            }

            // 3) Create the board
            var entity = new Board
            {
                SerialNumber = dto.SerialNumber,
                PartNumber = dto.PartNumber,
                BoardType = dto.BoardType,
                PrepDate = dto.PrepDate,
                IsShipped = dto.IsShipped,
                ShipDate = dto.IsShipped ? dto.ShipDate : null,
                SkidID = dto.SkidID
            };
            _db.Boards.Add(entity);

            // 4) Commit both changes atomically
            await _db.SaveChangesAsync();
        }
    }

}
