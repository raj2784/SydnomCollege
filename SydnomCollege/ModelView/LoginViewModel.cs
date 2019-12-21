using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;

namespace SydnomCollege.ModelView
{
	public class LoginViewModel
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }
		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }
		[Display(Name = "Remember Me")]
		public bool Rememberme { get; set; }
		public string ReturnUrl { get; set; }
		public IList<AuthenticationScheme> ExternalLogins { get; set; }
	}
}
