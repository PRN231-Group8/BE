using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.OtherObjects;

namespace Domain.Entities
{
    public class Location : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public WeatherStatus Status { get; set; }
        public double Temperature { get; set; }
        public ICollection<Photo> Photos { get; set; }
    }
}
