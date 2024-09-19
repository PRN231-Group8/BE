using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Tour : BaseEntity
    { 
        public string Title { get; set; }
        public string Description { get; set; }
        public ICollection<Booking> Bookings { get; set; }

        
        public ICollection<TourTimestamp> TourTimestamps { get; set; }

        
        public ICollection<LocationRequest> LocationRequests { get; set; }

        
        public ICollection<Moods> Moods { get; set; }
    }
}
