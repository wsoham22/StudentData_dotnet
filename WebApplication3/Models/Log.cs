using System;

namespace MyAspNetCoreApp.Models
{
    public class Log
    {
        public int Id { get; set; } // Primary key
        public string Action { get; set; } // Action like Insert, Update, Delete
        public string Details { get; set; } // Details about the operation
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Timestamp for creation (non-nullable, default to current UTC time)
    }
}
