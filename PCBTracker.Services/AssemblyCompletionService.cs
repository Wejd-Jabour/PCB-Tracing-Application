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
    public class AssemblyCompletionService : IAssemblyCompletionService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public AssemblyCompletionService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task SubmitAssemblyAsync(AssemblyCompletionDto dto)
        {
            using var db = await _contextFactory.CreateDbContextAsync();

            var entity = new AssemblyCompletion
            {
                Date = dto.Date,
                BoardType = dto.BoardType,
                Count = dto.Count
            };

            db.AssembliesCompleted.Add(entity);
            await db.SaveChangesAsync();
        }

        public async Task<IEnumerable<AssemblyCompletionDto>> GetAllAsync()
        {
            using var db = await _contextFactory.CreateDbContextAsync();

            return await db.AssembliesCompleted
                .OrderByDescending(x => x.Date)
                .Select(x => new AssemblyCompletionDto
                {
                    Date = x.Date,
                    BoardType = x.BoardType,
                    Count = x.Count
                })
                .ToListAsync();
        }
    }
}
