using Microsoft.EntityFrameworkCore;
using PCBTracker.Data.Context;
using PCBTracker.Domain.DTOs;
using PCBTracker.Domain.Entities;
using PCBTracker.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
            var inspection = new Inspection
            {
                Date = dto.Date,
                ProductType = dto.ProductType,
                SerialNumber = dto.SerialNumber,
                IssueDescription = dto.IssueDescription,
                SeverityLevel = dto.SeverityLevel,
                ImmediateActionTaken = dto.ImmediateActionTaken,
                AdditionalNotes = dto.AdditionalNotes,
                AssembliesCompletedJson = JsonSerializer.Serialize(dto.AssembliesCompleted),
                CreatedAt = DateTime.Now
            };

            _db.Inspections.Add(inspection);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<InspectionDto>> GetInspectionsAsync(InspectionFilterDto filter)
        {
            var query = _db.Inspections.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SerialNumberContains))
                query = query.Where(i => i.SerialNumber.Contains(filter.SerialNumberContains));

            if (!string.IsNullOrWhiteSpace(filter.ProductType))
                query = query.Where(i => i.ProductType == filter.ProductType);

            if (!string.IsNullOrWhiteSpace(filter.SeverityLevel))
                query = query.Where(i => i.SeverityLevel == filter.SeverityLevel);

            if (filter.DateFrom.HasValue)
                query = query.Where(i => i.Date >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(i => i.Date <= filter.DateTo.Value);

            query = query.OrderByDescending(i => i.Date);

            if (filter.PageNumber.HasValue && filter.PageSize.HasValue)
            {
                var skip = (filter.PageNumber.Value - 1) * filter.PageSize.Value;
                query = query.Skip(skip).Take(filter.PageSize.Value);
            }

            var inspectionList = await query.ToListAsync();

            return inspectionList.Select(i => new InspectionDto
            {
                Date = i.Date,
                ProductType = i.ProductType,
                SerialNumber = i.SerialNumber,
                IssueDescription = i.IssueDescription,
                SeverityLevel = i.SeverityLevel,
                ImmediateActionTaken = i.ImmediateActionTaken,
                AdditionalNotes = i.AdditionalNotes,
                AssembliesCompleted = string.IsNullOrWhiteSpace(i.AssembliesCompletedJson)
                    ? new Dictionary<string, int>()
                    : JsonSerializer.Deserialize<Dictionary<string, int>>(i.AssembliesCompletedJson)
            });
        }

    }
}

