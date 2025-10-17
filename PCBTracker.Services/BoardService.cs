using Microsoft.EntityFrameworkCore;
using PCBTracker.Data.Context;
using PCBTracker.Domain.DTOs;
using PCBTracker.Domain.Entities;
using PCBTracker.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PCBTracker.Services
{
    /// <summary>
    /// Board data operations. Updated to:
    ///  • Return dynamic types (DISTINCT from Boards)
    ///  • Store ONLY in Boards (no subtype writes), so new types work without republish
    /// </summary>
    public class BoardService : IBoardService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public BoardService(IDbContextFactory<AppDbContext> contextFactory) => _contextFactory = contextFactory;

        // ---- Dynamic board types from DB ----
        public async Task<IEnumerable<string>> GetBoardTypesAsync()
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            return await db.Boards
                .Where(b => b.BoardType != null && b.BoardType != "")
                .Select(b => b.BoardType.Trim())
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }

        public async Task<IEnumerable<Skid>> GetSkidsAsync()
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            return await db.Skids.ToListAsync();
        }

        public async Task<IEnumerable<SkidDto>> ExtractSkidsAsync()
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            return await db.Skids
                .Select(s => new SkidDto { SkidID = s.SkidID, SkidName = s.SkidName })
                .ToListAsync();
        }

        public async Task<Skid> CreateNewSkidAsync()
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            var max = await db.Skids.AnyAsync() ? await db.Skids.MaxAsync(s => s.SkidID) : 0;
            var skid = new Skid { SkidName = (max + 1).ToString() };
            db.Skids.Add(skid);
            await db.SaveChangesAsync();
            return skid;
        }

        public async Task SubmitBoardAsync(Board board)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            db.Boards.Add(board);
            await db.SaveChangesAsync();
        }

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
                SkidID = dto.SkidID,
                IsImported = false,
                CreatedAt = DateTime.Now
            };
            db.Boards.Add(board);
            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Store only in Boards, and set skid.designatedType if unset. (Removes subtype writes & unknown-type throws)
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
                CreatedAt = DateTime.Now,
                IsImported = false
            };

            db.Boards.Add(board);

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
        /// Query boards with optional filters; ordered by CreatedAt DESC.
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

            if (filter.IsShipped.HasValue)
                query = query.Where(b => b.IsShipped == filter.IsShipped.Value);

            if (filter.IsImported.HasValue)
                query = query.Where(b => b.IsImported == filter.IsImported.Value);

            if (filter.PrepDateFrom.HasValue)
                query = query.Where(b => b.PrepDate >= filter.PrepDateFrom.Value);
            if (filter.PrepDateTo.HasValue)
                query = query.Where(b => b.PrepDate <= filter.PrepDateTo.Value);

            if (filter.ShipDateFrom.HasValue)
                query = query.Where(b => b.ShipDate >= filter.ShipDateFrom.Value);
            if (filter.ShipDateTo.HasValue)
                query = query.Where(b => b.ShipDate <= filter.ShipDateTo.Value);

            query = query.OrderByDescending(b => b.CreatedAt);

            if (filter.PageNumber.HasValue && filter.PageSize.HasValue)
            {
                var skip = (filter.PageNumber.Value - 1) * filter.PageSize.Value;
                query = query.Skip(skip).Take(filter.PageSize.Value);
            }

            return await query.Select(b => new BoardDto
            {
                SerialNumber = b.SerialNumber,
                PartNumber = b.PartNumber,
                BoardType = b.BoardType,
                SkidID = b.SkidID,
                PrepDate = b.PrepDate,
                ShipDate = b.ShipDate,
                IsShipped = b.IsShipped,
                IsImported = b.IsImported
            }).ToListAsync();
        }

        public async Task<IEnumerable<Skid>> GetRecentSkidsAsync(int count)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            var recent = await db.Skids.OrderByDescending(s => s.SkidID).Take(count).ToListAsync();
            return recent.OrderBy(s => s.SkidID);
        }

        public async Task DeleteBoardBySerialAsync(string serialNumber)
        {
            using var db = await _contextFactory.CreateDbContextAsync();

            var all = await db.Boards.Where(b => b.SerialNumber == serialNumber).ToListAsync();
            if (!all.Any()) return;

            db.Boards.RemoveRange(all);

            // Clean up known subtype tables for legacy rows (no-op for new dynamic types)
            db.LE.RemoveRange(db.LE.Where(b => b.SerialNumber == serialNumber));
            db.LE_Upgrade.RemoveRange(db.LE_Upgrade.Where(b => b.SerialNumber == serialNumber));
            db.SAD.RemoveRange(db.SAD.Where(b => b.SerialNumber == serialNumber));
            db.SAD_Upgrade.RemoveRange(db.SAD_Upgrade.Where(b => b.SerialNumber == serialNumber));
            db.SAT.RemoveRange(db.SAT.Where(b => b.SerialNumber == serialNumber));
            db.SAT_Upgrade.RemoveRange(db.SAT_Upgrade.Where(b => b.SerialNumber == serialNumber));

            await db.SaveChangesAsync();
        }

        public async Task UpdateShipDateForSkidAsync(int skidId, DateTime shipDate)
        {
            using var db = await _contextFactory.CreateDbContextAsync();

            var boards = await db.Boards.Where(b => b.SkidID == skidId).ToListAsync();

            foreach (var board in boards)
            {
                board.ShipDate = shipDate;
                board.IsShipped = true;

                // Update legacy subtype tables only for known types (safe no-op for others)
                switch (board.BoardType)
                {
                    case "LE":
                        var le = await db.LE.FirstOrDefaultAsync(x => x.SerialNumber == board.SerialNumber);
                        if (le != null) { le.ShipDate = shipDate; le.IsShipped = true; }
                        break;
                    case "LE Upgrade":
                        var leu = await db.LE_Upgrade.FirstOrDefaultAsync(x => x.SerialNumber == board.SerialNumber);
                        if (leu != null) { leu.ShipDate = shipDate; leu.IsShipped = true; }
                        break;
                    case "SAD":
                        var sad = await db.SAD.FirstOrDefaultAsync(x => x.SerialNumber == board.SerialNumber);
                        if (sad != null) { sad.ShipDate = shipDate; sad.IsShipped = true; }
                        break;
                    case "SAD Upgrade":
                        var sadu = await db.SAD_Upgrade.FirstOrDefaultAsync(x => x.SerialNumber == board.SerialNumber);
                        if (sadu != null) { sadu.ShipDate = shipDate; sadu.IsShipped = true; }
                        break;
                    case "SAT":
                        var sat = await db.SAT.FirstOrDefaultAsync(x => x.SerialNumber == board.SerialNumber);
                        if (sat != null) { sat.ShipDate = shipDate; sat.IsShipped = true; }
                        break;
                    case "SAT Upgrade":
                        var satu = await db.SAT_Upgrade.FirstOrDefaultAsync(x => x.SerialNumber == board.SerialNumber);
                        if (satu != null) { satu.ShipDate = shipDate; satu.IsShipped = true; }
                        break;
                }
            }

            await db.SaveChangesAsync();
        }

        public async Task<int> DeleteBoardsBySkidAsync(int skidId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();

            var serials = await db.Boards
                .Where(b => b.SkidID == skidId)
                .Select(b => b.SerialNumber)
                .ToListAsync();

            if (serials.Count == 0) return 0;

            db.LE.RemoveRange(db.LE.Where(x => serials.Contains(x.SerialNumber)));
            db.LE_Upgrade.RemoveRange(db.LE_Upgrade.Where(x => serials.Contains(x.SerialNumber)));
            db.LE_Tray.RemoveRange(db.LE_Tray.Where(x => serials.Contains(x.SerialNumber)));
            db.SAD.RemoveRange(db.SAD.Where(x => serials.Contains(x.SerialNumber)));
            db.SAD_Upgrade.RemoveRange(db.SAD_Upgrade.Where(x => serials.Contains(x.SerialNumber)));
            db.SAT.RemoveRange(db.SAT.Where(x => serials.Contains(x.SerialNumber)));
            db.SAT_Upgrade.RemoveRange(db.SAT_Upgrade.Where(x => serials.Contains(x.SerialNumber)));

            var boards = db.Boards.Where(b => b.SkidID == skidId);
            db.Boards.RemoveRange(boards);

            return await db.SaveChangesAsync();
        }

        public async Task<int> UpdateIsImportedAsync(DateTime? date, bool? useShipDate, int? skidId, bool isImported)
        {
            using var db = await _contextFactory.CreateDbContextAsync();

            if (date == null && skidId == null) return 0;

            var query = db.Boards.AsQueryable();

            if (date != null && useShipDate.HasValue)
            {
                var d = date.Value.Date;
                query = useShipDate.Value
                    ? query.Where(b => b.ShipDate.HasValue && b.ShipDate.Value.Date == d)
                    : query.Where(b => b.PrepDate.Date == d);
            }

            if (skidId.HasValue)
                query = query.Where(b => b.SkidID == skidId.Value);

            var list = await query.ToListAsync();
            foreach (var b in list) b.IsImported = isImported;

            return await db.SaveChangesAsync();
        }
    }
}
