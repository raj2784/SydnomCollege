using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace SydnomCollege.Security
{
	public class CustomEmailConfirmationTokenProvider<TUser>
		: DataProtectorTokenProvider<TUser> where TUser : class
	{
		public CustomEmailConfirmationTokenProvider(IDataProtectionProvider dataProtectionProvider,
							   IOptions<CustomEmailConfirmationTokenProviderOptions> options)
			: base(dataProtectionProvider, options)
		{

		}
	}
}
