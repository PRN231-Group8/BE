using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.BusinessObject.Entities
{
    public class TourTrip : BaseEntity
    {
        public Guid TourId { get; set; }
        public Tour Tour { get; set; }
    }
}
