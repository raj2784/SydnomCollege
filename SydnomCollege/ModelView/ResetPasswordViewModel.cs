using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SydnomCollege.ModelView
{
	public class ResetPasswordViewModel
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Display(Name ="Confirm Password")]
		[Compare("Password",ErrorMessage ="Password and Confirm password must be same")]
		public string  ConfirmPassword { get; set; }

		public string Token { get; set; }
	}
}
