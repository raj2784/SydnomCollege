using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using SydnomCollege.Models;
using SydnomCollege.ModelView;

namespace SydnomCollege.ModelView
{
	public class CreateRoleViewModel
	{
		[Required]
		public string RoleName { get; set; }
	}
}
