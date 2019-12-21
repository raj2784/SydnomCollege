using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SydnomCollege.Models
{
	public class DateRangeAtribute : RangeAttribute
	{
		public DateRangeAtribute(string minimumValue)
		: base(typeof(DateTime), minimumValue, DateTime.Now.ToShortDateString())

		{

		}
	}
}
