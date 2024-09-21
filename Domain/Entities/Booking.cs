using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.OtherObjects;

namespace Domain.Entities
{
    public class Booking : BaseEntity
    {
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public Guid TourId { get; set; }
        public Tour Tour { get; set; }
        public ICollection<Transportation> Transportations { get; set; }
    }
}
