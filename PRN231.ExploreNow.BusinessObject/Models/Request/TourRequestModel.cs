using Microsoft.EntityFrameworkCore.Query.Internal;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.BusinessObject.Models.Request
{
    public class TourRequestModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal TotalPrice { get; set; }
        public TourStatus Status { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<Guid> TourMoods { get; set; } 
        public List<Guid> Transports { get; set; } 
        public List<Guid> LocationInTours { get; set; }
    }
}
