using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCBTracker.Domain.Entities
{
    public class Skid
    {
        public int SkidID { get; set; }
        public string SkidName { get; set; } = default!;
        public ICollection<Board> Boards { get; set; } = new List<Board>();
    }
}
