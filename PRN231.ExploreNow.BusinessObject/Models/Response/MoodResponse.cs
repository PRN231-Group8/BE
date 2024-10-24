using PRN231.ExploreNow.BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.BusinessObject.Models.Response
{
    public  class MoodResponse
    {
        public Guid Id { get; set; }
        public string MoodTag { get; set; }
        public string IconName {  get; set; }
        public List<TourMood> TourMoods { get; set; }
    }
}
