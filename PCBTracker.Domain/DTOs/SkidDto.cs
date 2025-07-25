using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCBTracker.Domain.DTOs
{
    /// <summary>
    /// Data Transfer Object representing a simplified view of a Skid entity.
    /// Used for UI filtering and lookup where only basic Skid identification is needed.
    /// </summary>
    public class SkidDto
    {
        /// <summary>
        /// The unique identifier of the skid.
        /// This corresponds to the primary key of the Skid entity in the database.
        /// </summary>
        public int SkidID { get; set; }

        /// <summary>
        /// The display name or label of the skid.
        /// Used in UI components such as Picker controls to identify skids by name.
        /// </summary>
        public string SkidName { get; set; } = default!;
    }
}
