using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.BusinessObject.OtherObjects
{
	public class CreateException : Exception
	{
		public CreateException(string message) : base(message) { }
	}
}
