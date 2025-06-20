﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.BusinessObject.Utilities
{
    public class TokenGenerator
    {
		public static Func<string> CreateRandomTokenDelegate = DefaultCreateRandomToken;

		public static string CreateRandomToken()
        {
			//return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
			return CreateRandomTokenDelegate();
		}
		private static string DefaultCreateRandomToken()
		{
			return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
		}
	}
}
