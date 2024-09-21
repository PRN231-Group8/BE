using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Comments : BaseEntity
    {
        public string Content { get; set; }
        public Guid PostId { get; set; }
        public Posts Post { get; set; }
    }
}
