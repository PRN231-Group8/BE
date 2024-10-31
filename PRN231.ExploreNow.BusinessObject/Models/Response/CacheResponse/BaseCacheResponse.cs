using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.BusinessObject.Models.Response.CacheResponse
{
    public class BaseCacheResponse<T>
    {
        public List<T> ResponseList { get; set; } = new List<T>();
        public int TotalElements { get; set; }
    }

}
