using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.BusinessObject.Models.Response
{
    public class TourResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal TotalPrice { get; set; }
        public TourStatus Status { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public ICollection<Transportation> Transportations { get; set; }
        public ICollection<TourTimestamp> TourTimestamps { get; set; }
        public ICollection<LocationInTour> LocationInTours { get; set; }
        public ICollection<TourMood> TourMoods { get; set; }
        public ICollection<TourTrip> TourTrips { get; set; }
    }
}
