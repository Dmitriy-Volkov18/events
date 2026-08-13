using System;

namespace Domain
{
    public class ActivityAttendee
    {
        public string AppUserId { get; set; } = string.Empty;
        public AppUser AppUser { get; set; } = null!;
        public Guid ActivityId { get; set; }
        public Activity Activity { get; set; } = null!;
        public bool isHost { get; set; }

    }
}