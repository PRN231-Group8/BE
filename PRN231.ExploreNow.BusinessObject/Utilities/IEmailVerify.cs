using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.BusinessObject.Utilities
{
	// Create interface Email Verify for writing Unit Tests
	public interface IEmailVerify
	{
		bool SendVerifyAccountEmail(string email, string token);
	}
}
