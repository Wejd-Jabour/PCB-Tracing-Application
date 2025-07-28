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
        private readonly AppDbContext _db;

        /// <summary>
        /// Initializes the BoardService with an injected AppDbContext.
        /// </summary>
        public BoardService(AppDbContext db) => _db = db;

        /// <summary>
        /// Returns a hardcoded list of supported board types.
        /// These values are used for board classification in the UI and logic layers.
        /// </summary>
        public Task<IEnumerable<string>> GetBoardTypesAsync()
            => Task.FromResult((IEnumerable<string>)new[] { "LE", "LE Upgrade", "SAD", "SAD Upgrade", "SAT", "SAT Upgrade" });

        /// <summary>
        /// Retrieves all Skid entities from the database.
        /// </summary>
        public async Task<IEnumerable<Skid>> GetSkidsAsync()
            => await _db.Skids.ToListAsync();

        /// <summary>
        /// Projects Skid entities into SkidDto view models with ID and name only.
        /// Used in extract filter dropdowns.
        /// </summary>
        public async Task<IEnumerable<SkidDto>> ExtractSkidsAsync()
            => await _db.Skids
                        .Select(s => new SkidDto
                        {
                            SkidID = s.SkidID,
                            SkidName = s.SkidName
                        })
                        .ToListAsync();

        /// <summary>
        /// Creates a new Skid record with an incremented numeric SkidName based on max existing SkidID.
        /// </summary>
        public async Task<Skid> CreateNewSkidAsync()
        {
            var max = await _db.Skids.AnyAsync()
                ? await _db.Skids.MaxAsync(s => s.SkidID)
                : 0;
            var skid = new Skid { SkidName = (max + 1).ToString() };
            _db.Skids.Add(skid);
            await _db.SaveChangesAsync();
            return skid;
        }

        /// <summary>
        /// Persists a fully defined Board entity to the database.
        /// </summary>
        public async Task SubmitBoardAsync(Board board)
        {
            _db.Boards.Add(board);
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Converts a BoardDto into a Board entity and saves it to the Boards table.
        /// ShipDate is conditionally assigned if IsShipped is true.
        /// </summary>
        public async Task CreateBoardAsync(BoardDto dto)
        {
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
            _db.Boards.Add(board);
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Adds a Board to both the Boards table and its corresponding type-specific table.
        /// Also sets the Skid's designatedType if not already assigned.
        /// Clears the EF Core ChangeTracker after save to reset internal state.
        /// </summary>
        public async Task CreateBoardAndClaimSkidAsync(BoardDto dto)
        {
            var skid = await _db.Skids.FindAsync(dto.SkidID);
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

            _db.Boards.Add(board);

            switch (dto.BoardType)
            {
                case "LE":
                    _db.LE.Add(new LE
                    {
                        SerialNumber = dto.SerialNumber,
                        PartNumber = dto.PartNumber,
                        BoardType = dto.BoardType,
                        PrepDate = dto.PrepDate,
                        IsShipped = dto.IsShipped,
                        ShipDate = dto.IsShipped ? dto.ShipDate ?? dto.PrepDate : null,
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
                        ShipDate = dto.IsShipped ? dto.ShipDate ?? dto.PrepDate : null,
                        SkidID = dto.SkidID
                    });
                    break;

                case "SAD":
                    _db.SAD.Add(new SAD
                    {
                        SerialNumber = dto.SerialNumber,
                        PartNumber = dto.PartNumber,
                        BoardType = dto.BoardType,
                        PrepDate = dto.PrepDate,
                        IsShipped = dto.IsShipped,
                        ShipDate = dto.IsShipped ? dto.ShipDate ?? dto.PrepDate : null,
                        SkidID = dto.SkidID
                    });
                    break;

                case "SAD Upgrade":
                    _db.SAD_Upgrade.Add(new SAD_Upgrade
                    {
                        SerialNumber = dto.SerialNumber,
                        PartNumber = dto.PartNumber,
                        BoardType = dto.BoardType,
                        PrepDate = dto.PrepDate,
                        IsShipped = dto.IsShipped,
                        ShipDate = dto.IsShipped ? dto.ShipDate ?? dto.PrepDate : null,
                        SkidID = dto.SkidID
                    });
                    break;

                case "SAT":
                    _db.SAT.Add(new SAT
                    {
                        SerialNumber = dto.SerialNumber,
                        PartNumber = dto.PartNumber,
                        BoardType = dto.BoardType,
                        PrepDate = dto.PrepDate,
                        IsShipped = dto.IsShipped,
                        ShipDate = dto.IsShipped ? dto.ShipDate ?? dto.PrepDate : null,
                        SkidID = dto.SkidID
                    });
                    break;

                case "SAT Upgrade":
                    _db.SAT_Upgrade.Add(new SAT_Upgrade
                    {
                        SerialNumber = dto.SerialNumber,
                        PartNumber = dto.PartNumber,
                        BoardType = dto.BoardType,
                        PrepDate = dto.PrepDate,
                        IsShipped = dto.IsShipped,
                        ShipDate = dto.IsShipped ? dto.ShipDate ?? dto.PrepDate : null,
                        SkidID = dto.SkidID
                    });
                    break;

                default:
                    throw new InvalidOperationException($"Unknown board type: {dto.BoardType}");
            }

            try
            {
                await _db.SaveChangesAsync();
            }
            finally
            {
                _db.ChangeTracker.Clear();
            }
        }

        /// <summary>
        /// Queries board records with optional filters on serial, type, dates, skid, etc.
        /// Results are returned in descending order of creation date.
        /// </summary>
        public async Task<IEnumerable<BoardDto>> GetBoardsAsync(BoardFilterDto filter)
        {
            var query = _db.Boards.AsQueryable();

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

            // Apply pagination
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
            var recent = await _db.Skids
                                  .OrderByDescending(s => s.SkidID)
                                  .Take(count)
                                  .ToListAsync();
            return recent.OrderBy(s => s.SkidID);
        }

        /// <summary>
        /// Deletes all board and subtype table records associated with the given serial number.
        /// </summary>
        public async Task DeleteBoardBySerialAsync(string serialNumber)
        {
            var all = await _db.Boards.Where(b => b.SerialNumber == serialNumber).ToListAsync();
            if (!all.Any()) return;

            _db.Boards.RemoveRange(all);

            _db.LE.RemoveRange(_db.LE.Where(b => b.SerialNumber == serialNumber));
            _db.LE_Upgrade.RemoveRange(_db.LE_Upgrade.Where(b => b.SerialNumber == serialNumber));
            _db.SAD.RemoveRange(_db.SAD.Where(b => b.SerialNumber == serialNumber));
            _db.SAD_Upgrade.RemoveRange(_db.SAD_Upgrade.Where(b => b.SerialNumber == serialNumber));
            _db.SAT.RemoveRange(_db.SAT.Where(b => b.SerialNumber == serialNumber));
            _db.SAT_Upgrade.RemoveRange(_db.SAT_Upgrade.Where(b => b.SerialNumber == serialNumber));

            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Updates the ShipDate and IsShipped flag for all boards on a given skid.
        /// </summary>
        public async Task UpdateShipDateForSkidAsync(int skidId, DateTime shipDate)
        {
            var boards = await _db.Boards
                                  .Where(b => b.SkidID == skidId)
                                  .ToListAsync();

            foreach (var b in boards)
            {
                b.ShipDate = shipDate;
                b.IsShipped = true;
            }

            await _db.SaveChangesAsync();
        }
    }
}
