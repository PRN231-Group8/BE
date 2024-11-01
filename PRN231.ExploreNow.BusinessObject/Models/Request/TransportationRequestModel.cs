using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.BusinessObject.Models.Request
{
    public class TransportationRequestModel
    {
        public TransportationType Type { get; set; }
        public decimal Price { get; set; }
        public int Capacity { get; set; }
        public Guid TourId { get; set; }
    }
}
