using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SydnomCollege.Models
{
	public class StudentContext : IdentityDbContext
	{
		public StudentContext(DbContextOptions<StudentContext> options) : base(options)
		{

		}
		public DbSet<Student> Students { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);


			foreach (var foreignKey in modelBuilder.Model.GetEntityTypes()
				.SelectMany(e => e.GetForeignKeys()))
			{
				foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
			}

		}

	}
}
