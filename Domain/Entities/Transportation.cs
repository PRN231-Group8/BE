using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.OtherObjects;

namespace Domain.Entities
{
    public class Transportation : BaseEntity
    {
        public decimal Price { get; set; }
        public int Capacity { get; set; }
        public TransportationType Type { get; set; }
        public Guid BookingId { get; set; }
        public Booking Booking { get; set; }
    }
}
