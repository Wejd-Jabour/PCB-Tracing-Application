using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCBTracker.Domain.DTOs
{
    public class SkidDto
    {
        public int SkidID { get; set; }
        public string SkidName { get; set; } = default!;
    }
}
