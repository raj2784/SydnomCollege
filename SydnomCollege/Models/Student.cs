using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using SydnomCollege.ModelView;

namespace SydnomCollege.Models
{
	public class Student
	{
		[Key]
		public int StudentId { get; set; }
		[Required]
		[Column(TypeName = "nvarchar(250)")]
		public string Name { get; set; }
		[Column(TypeName = "nvarchar(50)")]
		[EmailAddress]
		public string Email { get; set; }
		[DateRangeAtribute ("01/07/2000")]
		[DataType(DataType.DateTime)]
		[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
		public DateTime Dob { get; set; }
		[Required]
		[Range(50,99,ErrorMessage ="Grade would be Accepted from 50 to 100 only")]
		public int Grade { get; set; }
		public string City { get; set; }
		public string PhotoPath { get; set; }

	}
}
