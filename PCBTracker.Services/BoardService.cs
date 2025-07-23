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
            // 1) Load & (maybe) claim the skid
            var skid = await _db.Skids.FindAsync(dto.SkidID);
            if (skid != null && skid.designatedType == null)
            {
                skid.designatedType = dto.BoardType;
                // no need to call _db.Skids.Update(skid); FindAsync gives you a tracked entity
            }

            // 2) Create the new Board entity
            var board = new Board
            {
                SerialNumber = dto.SerialNumber,
                PartNumber = dto.PartNumber,
                BoardType = dto.BoardType,
                PrepDate = dto.PrepDate,
                IsShipped = dto.IsShipped,
                ShipDate = dto.IsShipped
                                 ? dto.ShipDate ?? dto.PrepDate
                                 : null,
                SkidID = dto.SkidID
            };
            _db.Boards.Add(board);


            // 3) Upload it also to its respective table based on the board type
            switch(dto.BoardType)
            {
                case "LE":
                    _db.LE.Add( new LE
                    {
                        SerialNumber = dto.SerialNumber,
                        PartNumber = dto.PartNumber,
                        BoardType = dto.BoardType,
                        PrepDate = dto.PrepDate,
                        IsShipped = dto.IsShipped,
                        ShipDate = dto.IsShipped
                                 ? dto.ShipDate ?? dto.PrepDate
                                 : null,
                        SkidID = dto.SkidID
                    });
                    break;
                
                case "LE Upgrade":
                    _db.LE_Upgrade.Add(new LE_Upgrade
                    {
                        SerialNumber = dto.SerialNumber,
                        PartNumber = dto.PartNumber,
                        BoardType = dto.BoardType,
                        PrepDate = dto.PrepDate,
                        IsShipped = dto.IsShipped,
                        ShipDate = dto.IsShipped
                                 ? dto.ShipDate ?? dto.PrepDate
                                 : null,
                        SkidID = dto.SkidID
                    });
                    break;
                
                case "SAD":
                    _db.SAD.Add( new SAD
                    {
                        SerialNumber = dto.SerialNumber,
                        PartNumber = dto.PartNumber,
                        BoardType = dto.BoardType,
                        PrepDate = dto.PrepDate,
                        IsShipped = dto.IsShipped,
                        ShipDate = dto.IsShipped
                                 ? dto.ShipDate ?? dto.PrepDate
                                 : null,
                        SkidID = dto.SkidID
                    });
                    break;
                
                case "SAD Upgrade":
                    _db.SAD_Upgrade.Add( new SAD_Upgrade
                    {
                        SerialNumber = dto.SerialNumber,
                        PartNumber = dto.PartNumber,
                        BoardType = dto.BoardType,
                        PrepDate = dto.PrepDate,
                        IsShipped = dto.IsShipped,
                        ShipDate = dto.IsShipped
                                 ? dto.ShipDate ?? dto.PrepDate
                                 : null,
                        SkidID = dto.SkidID
                    });
                    break;
                
                case "SAT":
                    _db.SAT.Add( new SAT
                    {
                        SerialNumber = dto.SerialNumber,
                        PartNumber = dto.PartNumber,
                        BoardType = dto.BoardType,
                        PrepDate = dto.PrepDate,
                        IsShipped = dto.IsShipped,
                        ShipDate = dto.IsShipped
                                 ? dto.ShipDate ?? dto.PrepDate
                                 : null,
                        SkidID = dto.SkidID
                    });
                    break;
                
                case "SAT Upgrade":
                    _db.SAT_Upgrade.Add( new SAT_Upgrade
                    {
                        SerialNumber = dto.SerialNumber,
                        PartNumber = dto.PartNumber,
                        BoardType = dto.BoardType,
                        PrepDate = dto.PrepDate,
                        IsShipped = dto.IsShipped,
                        ShipDate = dto.IsShipped
                                 ? dto.ShipDate ?? dto.PrepDate
                                 : null,
                        SkidID = dto.SkidID
                    });
                    break;

                default:
                    throw new InvalidOperationException($"Unknown board type: {dto.BoardType}");
            }
                



            // 4) Save inside a try/finally so we ALWAYS clear the tracker
            try
            {
                await _db.SaveChangesAsync();
            }
            finally
            {
                // EF Core 6+:
                _db.ChangeTracker.Clear();
                
            }
        }





        public async Task<IEnumerable<Skid>> GetRecentSkidsAsync(int count)
        {
            // 1) order by descending ID (newest first)
            var recent = await _db.Skids
                                  .OrderByDescending(s => s.SkidID)
                                  .Take(count)
                                  .ToListAsync();

            // 2) flip to ascending if you want oldest‐of‐the‐last first
            return recent.OrderBy(s => s.SkidID);
        }


        public async Task<IEnumerable<BoardDto>> GetBoardsAsync(BoardFilterDto filter)
        {
            var query = _db.Boards.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SerialNumber))
                query = query.Where(b => b.SerialNumber.Contains(filter.SerialNumber));

            if (!string.IsNullOrWhiteSpace(filter.BoardType))
                query = query.Where(b => b.BoardType == filter.BoardType);

            if (filter.PrepDateFrom.HasValue)
                query = query.Where(b => b.PrepDate >= filter.PrepDateFrom.Value);

            if (filter.PrepDateTo.HasValue)
                query = query.Where(b => b.PrepDate <= filter.PrepDateTo.Value);

            if (filter.ShipDateFrom.HasValue)
                query = query.Where(b => b.ShipDate >= filter.ShipDateFrom.Value);

            if (filter.ShipDateTo.HasValue)
                query = query.Where(b => b.ShipDate <= filter.ShipDateTo.Value);

            return await query
                .Select(b => new BoardDto
                {
                    SerialNumber = b.SerialNumber,
                    PartNumber = b.PartNumber,
                    BoardType = b.BoardType,
                    PrepDate = b.PrepDate,
                    ShipDate = b.ShipDate,
                    IsShipped = b.IsShipped,
                    SkidID = b.SkidID
                })
                .ToListAsync();
        }


    }

}
