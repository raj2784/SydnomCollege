﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SydnomCollege.ModelView
{
	public class ChangePasswordViewModel
	{
		[Required]
		[Display(Name = "Current Password")]
		[DataType(DataType.Password)]
		public string CurrentPassword { get; set; }

		[Required]
		[Display(Name = "New Password")]
		[DataType(DataType.Password)]
		public string NewPassword { get; set; }

		[Required]
		[Compare("NewPassword",ErrorMessage ="New Password and Confrim Password should be same!")]
		[Display(Name = "Cofirm Password")]
		[DataType(DataType.Password)]
		public string ConfirmPassword { get; set; }
	}
}
