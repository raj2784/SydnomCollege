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
	public class StudentEditViewModel : StudentCreateViewModel
	{

		public int Id { get; set; }

		public string ExistingPhotoPath { get; set; }
	}
}
