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
    /// Service implementation for board-related business logic and data operations.
    /// Handles creation, lookup, deletion, and batch updates of boards and skids.
    /// All operations use Entity Framework Core via AppDbContext.
    /// </summary>
    public class BoardService : IBoardService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        /// <summary>
        /// Initializes the BoardService with an injected AppDbContext.
        /// </summary>
        public BoardService(IDbContextFactory<AppDbContext> contextFactory) => _contextFactory = contextFactory;

        /// <summary>
        /// Returns a hardcoded list of supported board types.
        /// These values are used for board classification in the UI and logic layers.
        /// </summary>
        public Task<IEnumerable<string>> GetBoardTypesAsync()
            => Task.FromResult((IEnumerable<string>)new[] { "LE", "LE Upgrade", "LE Tray", "SAD", "SAD Upgrade", "SAT", "SAT Upgrade" });

        /// <summary>
        /// Retrieves all Skid entities from the database.
        /// </summary>
        public async Task<IEnumerable<Skid>> GetSkidsAsync()
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            return await db.Skids.ToListAsync();
        }

        /// <summary>
        /// Projects Skid entities into SkidDto view models with ID and name only.
        /// Used in extract filter dropdowns.
        /// </summary>
        public async Task<IEnumerable<SkidDto>> ExtractSkidsAsync()
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            return await db.Skids
                        .Select(s => new SkidDto
                        {
                            SkidID = s.SkidID,
                            SkidName = s.SkidName
                        })
                        .ToListAsync();
        }

        /// <summary>
        /// Creates a new Skid record with an incremented numeric SkidName based on max existing SkidID.
        /// </summary>
        public async Task<Skid> CreateNewSkidAsync()
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            var max = await db.Skids.AnyAsync()
                ? await db.Skids.MaxAsync(s => s.SkidID)
                : 0;
            var skid = new Skid { SkidName = (max + 1).ToString() };
            db.Skids.Add(skid);
            await db.SaveChangesAsync();
            return skid;
        }

        /// <summary>
        /// Persists a fully defined Board entity to the database.
        /// </summary>
        public async Task SubmitBoardAsync(Board board)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            db.Boards.Add(board);
            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Converts a BoardDto into a Board entity and saves it to the Boards table.
        /// ShipDate is conditionally assigned if IsShipped is true.
        /// </summary>
        public async Task CreateBoardAsync(BoardDto dto)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            var board = new Board
            {
                SerialNumber = dto.SerialNumber,
                PartNumber = dto.PartNumber,
                BoardType = dto.BoardType,
                PrepDate = dto.PrepDate,
                ShipDate = dto.IsShipped ? dto.ShipDate ?? dto.PrepDate : null,
                IsShipped = dto.IsShipped,
                SkidID = dto.SkidID
            };
            db.Boards.Add(board);
            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Adds a Board to both the Boards table and its corresponding type-specific table.
        /// Also sets the Skid's designatedType if not already assigned.
        /// Clears the EF Core ChangeTracker after save to reset internal state.
        /// </summary>
        public async Task CreateBoardAndClaimSkidAsync(BoardDto dto)
        {
            using var db = await _contextFactory.CreateDbContextAsync();

            var skid = await db.Skids.FindAsync(dto.SkidID);
            if (skid != null && skid.designatedType == null)
                skid.designatedType = dto.BoardType;

            var board = new Board
            {
                SerialNumber = dto.SerialNumber,
                PartNumber = dto.PartNumber,
                BoardType = dto.BoardType,
                PrepDate = dto.PrepDate,
                ShipDate = dto.IsShipped ? dto.ShipDate ?? dto.PrepDate : null,
                IsShipped = dto.IsShipped,
                SkidID = dto.SkidID,
                CreatedAt = DateTime.Now
            };

            db.Boards.Add(board);

            switch (dto.BoardType)
            {
                case "LE":
                    db.LE.Add(new LE { SerialNumber = dto.SerialNumber, PartNumber = dto.PartNumber, BoardType = dto.BoardType, PrepDate = dto.PrepDate, IsShipped = dto.IsShipped, ShipDate = dto.IsShipped ? dto.ShipDate ?? dto.PrepDate : null, SkidID = dto.SkidID });
                    break;
                case "LE Upgrade":
                    db.LE_Upgrade.Add(new LE_Upgrade { SerialNumber = dto.SerialNumber, PartNumber = dto.PartNumber, BoardType = dto.BoardType, PrepDate = dto.PrepDate, IsShipped = dto.IsShipped, ShipDate = dto.IsShipped ? dto.ShipDate ?? dto.PrepDate : null, SkidID = dto.SkidID });
                    break;
                case "LE Tray":
                    db.LE_Tray.Add(new LE_Tray{ SerialNumber = dto.SerialNumber, PartNumber = dto.PartNumber, BoardType = dto.BoardType, PrepDate = dto.PrepDate, IsShipped = dto.IsShipped, ShipDate = dto.IsShipped ? dto.ShipDate ?? dto.PrepDate : null, SkidID = dto.SkidID });
                    break;
                case "SAD":
                    db.SAD.Add(new SAD { SerialNumber = dto.SerialNumber, PartNumber = dto.PartNumber, BoardType = dto.BoardType, PrepDate = dto.PrepDate, IsShipped = dto.IsShipped, ShipDate = dto.IsShipped ? dto.ShipDate ?? dto.PrepDate : null, SkidID = dto.SkidID });
                    break;
                case "SAD Upgrade":
                    db.SAD_Upgrade.Add(new SAD_Upgrade { SerialNumber = dto.SerialNumber, PartNumber = dto.PartNumber, BoardType = dto.BoardType, PrepDate = dto.PrepDate, IsShipped = dto.IsShipped, ShipDate = dto.IsShipped ? dto.ShipDate ?? dto.PrepDate : null, SkidID = dto.SkidID });
                    break;
                case "SAT":
                    db.SAT.Add(new SAT { SerialNumber = dto.SerialNumber, PartNumber = dto.PartNumber, BoardType = dto.BoardType, PrepDate = dto.PrepDate, IsShipped = dto.IsShipped, ShipDate = dto.IsShipped ? dto.ShipDate ?? dto.PrepDate : null, SkidID = dto.SkidID });
                    break;
                case "SAT Upgrade":
                    db.SAT_Upgrade.Add(new SAT_Upgrade { SerialNumber = dto.SerialNumber, PartNumber = dto.PartNumber, BoardType = dto.BoardType, PrepDate = dto.PrepDate, IsShipped = dto.IsShipped, ShipDate = dto.IsShipped ? dto.ShipDate ?? dto.PrepDate : null, SkidID = dto.SkidID });
                    break;
                default:
                    throw new InvalidOperationException($"Unknown board type: {dto.BoardType}");
            }

            try
            {
                await db.SaveChangesAsync();
            }
            finally
            {
                db.ChangeTracker.Clear();
            }
        }

        /// <summary>
        /// Queries board records with optional filters on serial, type, dates, skid, etc.
        /// Results are returned in descending order of creation date.
        /// </summary>
        public async Task<IEnumerable<BoardDto>> GetBoardsAsync(BoardFilterDto filter)
        {
            using var db = await _contextFactory.CreateDbContextAsync();

            var query = db.Boards.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SerialNumber))
                query = query.Where(b => b.SerialNumber.Contains(filter.SerialNumber));

            if (!string.IsNullOrWhiteSpace(filter.BoardType))
                query = query.Where(b => b.BoardType == filter.BoardType);

            if (filter.SkidId.HasValue)
                query = query.Where(b => b.SkidID == filter.SkidId.Value);

            if (filter.PrepDateFrom.HasValue)
                query = query.Where(b => b.PrepDate >= filter.PrepDateFrom.Value);

            if (filter.PrepDateTo.HasValue)
                query = query.Where(b => b.PrepDate <= filter.PrepDateTo.Value);

            if (filter.ShipDateFrom.HasValue)
                query = query.Where(b => b.ShipDate >= filter.ShipDateFrom.Value);

            if (filter.ShipDateTo.HasValue)
                query = query.Where(b => b.ShipDate <= filter.ShipDateTo.Value);

            if (filter.IsShipped.HasValue)
                query = query.Where(b => b.IsShipped == filter.IsShipped.Value);

            query = query.OrderByDescending(b => b.CreatedAt);

            if (filter.PageNumber.HasValue && filter.PageSize.HasValue)
            {
                int skip = (filter.PageNumber.Value - 1) * filter.PageSize.Value;
                query = query.Skip(skip).Take(filter.PageSize.Value);
            }

            return await query.Select(b => new BoardDto
            {
                SerialNumber = b.SerialNumber,
                PartNumber = b.PartNumber,
                BoardType = b.BoardType,
                PrepDate = b.PrepDate,
                ShipDate = b.ShipDate,
                IsShipped = b.IsShipped,
                SkidID = b.SkidID
            }).ToListAsync();
        }

        /// <summary>
        /// Retrieves the latest N skids sorted by descending SkidID,
        /// then returns them sorted in ascending order.
        /// </summary>
        public async Task<IEnumerable<Skid>> GetRecentSkidsAsync(int count)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            var recent = await db.Skids.OrderByDescending(s => s.SkidID).Take(count).ToListAsync();
            return recent.OrderBy(s => s.SkidID);
        }

        /// <summary>
        /// Deletes all board and subtype table records associated with the given serial number.
        /// </summary>
        public async Task DeleteBoardBySerialAsync(string serialNumber)
        {
            using var db = await _contextFactory.CreateDbContextAsync();

            var all = await db.Boards.Where(b => b.SerialNumber == serialNumber).ToListAsync();
            if (!all.Any()) return;

            db.Boards.RemoveRange(all);

            db.LE.RemoveRange(db.LE.Where(b => b.SerialNumber == serialNumber));
            db.LE_Upgrade.RemoveRange(db.LE_Upgrade.Where(b => b.SerialNumber == serialNumber));
            db.SAD.RemoveRange(db.SAD.Where(b => b.SerialNumber == serialNumber));
            db.SAD_Upgrade.RemoveRange(db.SAD_Upgrade.Where(b => b.SerialNumber == serialNumber));
            db.SAT.RemoveRange(db.SAT.Where(b => b.SerialNumber == serialNumber));
            db.SAT_Upgrade.RemoveRange(db.SAT_Upgrade.Where(b => b.SerialNumber == serialNumber));

            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Updates the ShipDate and IsShipped flag for all boards on a given skid.
        /// </summary>
        public async Task UpdateShipDateForSkidAsync(int skidId, DateTime shipDate)
        {
            using var db = await _contextFactory.CreateDbContextAsync();

            var boards = await db.Boards.Where(b => b.SkidID == skidId).ToListAsync();

            foreach (var board in boards)
            {
                board.ShipDate = shipDate;
                board.IsShipped = true;

                switch (board.BoardType)
                {
                    case "LE":
                        var le = await db.LE.FirstOrDefaultAsync(x => x.SerialNumber == board.SerialNumber);
                        if (le != null)
                        {
                            le.ShipDate = shipDate;
                            le.IsShipped = true;
                        }
                        break;
                    case "LE Upgrade":
                        var leu = await db.LE_Upgrade.FirstOrDefaultAsync(x => x.SerialNumber == board.SerialNumber);
                        if (leu != null)
                        {
                            leu.ShipDate = shipDate;
                            leu.IsShipped = true;
                        }
                        break;
                    case "SAD":
                        var sad = await db.SAD.FirstOrDefaultAsync(x => x.SerialNumber == board.SerialNumber);
                        if (sad != null)
                        {
                            sad.ShipDate = shipDate;
                            sad.IsShipped = true;
                        }
                        break;
                    case "SAD Upgrade":
                        var sadu = await db.SAD_Upgrade.FirstOrDefaultAsync(x => x.SerialNumber == board.SerialNumber);
                        if (sadu != null)
                        {
                            sadu.ShipDate = shipDate;
                            sadu.IsShipped = true;
                        }
                        break;
                    case "SAT":
                        var sat = await db.SAT.FirstOrDefaultAsync(x => x.SerialNumber == board.SerialNumber);
                        if (sat != null)
                        {
                            sat.ShipDate = shipDate;
                            sat.IsShipped = true;
                        }
                        break;
                    case "SAT Upgrade":
                        var satu = await db.SAT_Upgrade.FirstOrDefaultAsync(x => x.SerialNumber == board.SerialNumber);
                        if (satu != null)
                        {
                            satu.ShipDate = shipDate;
                            satu.IsShipped = true;
                        }
                        break;
                }
            }

            await db.SaveChangesAsync();
        }

    }
}