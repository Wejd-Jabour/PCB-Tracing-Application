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
        public string Role { get; set; } = default!; // “Admin” or “Standard”

    }
}
