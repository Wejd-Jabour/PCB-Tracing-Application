// InspectionService.cs
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
    public class InspectionService : IInspectionService
    {
        private readonly AppDbContext _db;

        public InspectionService(AppDbContext db)
        {
            _db = db;
        }

        public async Task SubmitInspectionAsync(InspectionDto dto)
        {
            var entity = new Inspection
            {
                Date = dto.Date,
                ProductType = dto.ProductType,
                SerialNumber = dto.SerialNumber,
                IssueDescription = dto.IssueDescription,
                SeverityLevel = dto.SeverityLevel,
                ImmediateActionTaken = dto.ImmediateActionTaken,
                AdditionalNotes = dto.AdditionalNotes,
                CreatedAt = DateTime.UtcNow
            };

            _db.Inspections.Add(entity);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<InspectionDto>> GetInspectionsAsync(InspectionFilterDto filter)
        {
            var query = _db.Inspections.AsQueryable();

            if (filter.DateFrom.HasValue)
                query = query.Where(x => x.Date >= filter.DateFrom);
            if (filter.DateTo.HasValue)
                query = query.Where(x => x.Date <= filter.DateTo);
            if (!string.IsNullOrEmpty(filter.ProductType))
                query = query.Where(x => x.ProductType == filter.ProductType);
            if (!string.IsNullOrEmpty(filter.SerialNumberContains))
                query = query.Where(x => x.SerialNumber.Contains(filter.SerialNumberContains));
            if (!string.IsNullOrEmpty(filter.SeverityLevel))
                query = query.Where(x => x.SeverityLevel == filter.SeverityLevel);

            query = query.OrderByDescending(x => x.Date);

            if (filter.PageNumber.HasValue && filter.PageSize.HasValue)
            {
                var skip = (filter.PageNumber.Value - 1) * filter.PageSize.Value;
                query = query.Skip(skip).Take(filter.PageSize.Value);
            }

            return await query
                .Select(x => new InspectionDto
                {
                    Date = x.Date,
                    ProductType = x.ProductType,
                    SerialNumber = x.SerialNumber,
                    IssueDescription = x.IssueDescription,
                    SeverityLevel = x.SeverityLevel,
                    ImmediateActionTaken = x.ImmediateActionTaken,
                    AdditionalNotes = x.AdditionalNotes
                })
                .ToListAsync();
        }
    }
}
