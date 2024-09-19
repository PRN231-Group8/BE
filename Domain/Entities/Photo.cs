using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Photo : BaseEntity
    {
        public string Url { get; set; }
        public string Alt { get; set; }
        public Guid LocationId { get; set; }
        public Location Location { get; set; }
    }
}
