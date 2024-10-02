using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.BusinessObject.Utilities
{
	public interface IEmailVerify
	{
		bool SendVerifyAccountEmail(string email, string token);
	}
}
