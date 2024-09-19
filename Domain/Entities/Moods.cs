using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Moods : BaseEntity
    {
        public string MoodTag { get; set; }
        public Guid TourId { get; set; }
        public Tour Tour { get; set; }
    }
}
