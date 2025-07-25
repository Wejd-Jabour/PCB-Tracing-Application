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
        public BoardService(AppDbContext db) => _db = db;

        public Task<IEnumerable<string>> GetBoardTypesAsync()
            => Task.FromResult((IEnumerable<string>)new[] { "LE", "LE Upgrade", "SAD", "SAD Upgrade", "SAT", "SAT Upgrade" });

        public async Task<IEnumerable<Skid>> GetSkidsAsync()
            => await _db.Skids.ToListAsync();

        /// <summary>
        /// Project each Skid entity into a SkidDto for the extract page.
        /// </summary>
        public async Task<IEnumerable<SkidDto>> ExtractSkidsAsync()
            => await _db.Skids
                        .Select(s => new SkidDto
                        {
                            SkidID = s.SkidID,
                            SkidName = s.SkidName
                        })
                        .ToListAsync();

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

        public async Task SubmitBoardAsync(Board board)
        {
            _db.Boards.Add(board);
            await _db.SaveChangesAsync();
        }

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
                    _db.SAD.Add(new SAD
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
                    _db.SAD_Upgrade.Add(new SAD_Upgrade
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
                    _db.SAT.Add(new SAT
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
                    _db.SAT_Upgrade.Add(new SAT_Upgrade
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

            query = query.OrderByDescending(b => b.CreatedAt);

            // Project to DTO
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


        public async Task<IEnumerable<Skid>> GetRecentSkidsAsync(int count)
        {
            var recent = await _db.Skids
                                  .OrderByDescending(s => s.SkidID)
                                  .Take(count)
                                  .ToListAsync();
            return recent.OrderBy(s => s.SkidID);
        }
    }
}
