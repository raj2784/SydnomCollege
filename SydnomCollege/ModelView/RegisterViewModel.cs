using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SydnomCollege.ModelView
{
	public class RegisterViewModel
	{
		[Required]
		[EmailAddress]
		[Remote(action:"IsEmailInUse",controller:"Account")]
		public string Email { get; set; }
		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }
		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Confirm Password")]
		[Compare("Password", ErrorMessage = "Password Should be matched")]
		public string ConfirmPassword { get; set; }
	}
}
