﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCBTracker.Domain.Entities
{
    public class Board
    {
        [Key]
        public string SerialNumber { get; set; } = default!;

        public string PartNumber { get; set; } = default!;
        public string BoardType { get; set; } = default!;
        public int SkidID { get; set; }
        public DateTime PrepDate { get; set; }
        public DateTime? ShipDate { get; set; }
        public bool IsShipped { get; set; }

        public Skid? Skid { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
