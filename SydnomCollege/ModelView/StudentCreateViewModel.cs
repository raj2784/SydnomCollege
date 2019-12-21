using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using SydnomCollege.Models;

namespace SydnomCollege.ModelView
{
	public class StudentCreateViewModel
	{
		public int StudentId { get; set; }
		[Required]
		[Column(TypeName = "nvarchar(250)")]
		public string Name { get; set; }
		[Column(TypeName = "nvarchar(50)")]
		[Required]
		[EmailAddress]
		public string Email { get; set; }
		[Required]
		[DateRangeAtribute("01/07/2000")]
		[DataType(DataType.DateTime)]
		[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
		public DateTime Dob { get; set; }
		[Required]
		[Range(50, 99, ErrorMessage = "Grade Accepted from 50 to 100 only")]
		public int Grade { get; set; }
		public string City { get; set; }
		//[Required(ErrorMessage = "Please select file.")]
		//[RegularExpression(@"([a-zA-Z0-9\s_\\.\-:])+(.png|.jpg|.gif)$", ErrorMessage = "Only Image files allowed.")]
		public IFormFile Photo { get; set; }
	}
}
