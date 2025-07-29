using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCBTracker.Domain.Entities
{
    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public bool Admin { get; set; } = false; //Full setting access
        public bool Scan { get; set; } = false; //Can submit boards
        public bool Edit { get; set; } = false; //Can delete/update boards
        public bool Inspection { get; set; } = false; //Can add board inspection

    }
}
