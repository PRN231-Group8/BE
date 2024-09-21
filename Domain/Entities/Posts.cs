using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Posts : BaseEntity
    {
        public string Content { get; set; }
        public int Rating { get; set; }
    }
}
