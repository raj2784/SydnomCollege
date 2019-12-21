using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;
using SydnomCollege.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using SydnomCollege.ModelView;
using Microsoft.Extensions.Logging;

namespace SydnomCollege.Controllers
{
	[Authorize]
	public class StudentsController : Controller
	{
		private readonly StudentContext db;
		private readonly IHostingEnvironment hostingEnvironment;
		private readonly ILogger logger;

		public StudentsController(StudentContext context,
								IHostingEnvironment hostingEnvironment,
								ILogger<StudentsController>logger)
		{
			db = context;
			this.hostingEnvironment = hostingEnvironment;
			this.logger = logger;
		}
		private string FileProccesingUpload(StudentCreateViewModel model)
		{
			string PhotoName = null;
			if (model.Photo != null)

			{
				string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "Images");
				PhotoName = DateTime.Now.ToString("yyyyMMddhhmmss") + "-" + model.Photo.FileName;
				// to get uniq filename use either time stamp or GUID 
				//PhotoName = Guid.NewGuid().ToString() + "-" + model.Photo.FileName;
				string PhotoPath = Path.Combine(uploadsFolder, PhotoName);
				using (var fielstream = new FileStream(PhotoPath, FileMode.Create))
				{
					model.Photo.CopyTo(fielstream);
				}

			}
			return PhotoName;
		}

		// GET: Students
		[AllowAnonymous]
		public IActionResult Index()
		{
			return View(db.Students.ToList());
		}


		// GET: Students/Details/5
		[AllowAnonymous]
		public IActionResult Details(int? id)
		{
			logger.LogTrace("Trace Log");
			logger.LogDebug("Debug Log");
			logger.LogInformation("Information Log");
			logger.LogWarning("Warning Log");
			logger.LogError("Error Log");
			logger.LogCritical("Critical Log");

			if (id == null)
			{
				return NotFound();
			}

			var student = db.Students.Where(m => m.StudentId == id).FirstOrDefault();

			if (student == null)
			{
				return NotFound();
			}

			return View(student);
		}

		// GET: Students/Create
		[HttpGet]
		public IActionResult Create()
		{
			ViewBag.Success = TempData["Success"];
			return View();
		}

		// POST: Students/Create
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		//[ValidateAntiForgeryToken]
		public IActionResult Create(StudentCreateViewModel model)
		{

			if (ModelState.IsValid)
			{

				string PhotoName = FileProccesingUpload(model);
							   				 
				Student newStudent = new Student
				{
					Name = model.Name,
					Email = model.Email,
					Dob = model.Dob,
					Grade = model.Grade,
					City = model.City,
					PhotoPath = PhotoName

				};

				db.Add(newStudent);
				db.SaveChanges();
				TempData["Success"] = "Student data added successfully.";
				//return RedirectToAction("Index","Students");
				return RedirectToAction(nameof(Index));
			}
			return View();

		}

		[HttpGet]
		public IActionResult Edit(int id)
		{
			Student std = db.Students.Find(id);
			StudentEditViewModel model = new StudentEditViewModel
			{
				Id = std.StudentId,
				Name = std.Name,
				Email = std.Email,
				Dob = std.Dob,
				City = std.City,
				Grade = std.Grade,
				ExistingPhotoPath = std.PhotoPath,

			};
			return View(model);
		}
		[HttpPost]
		public IActionResult Edit(StudentEditViewModel model)
		{
			//if(ModelState.IsValid)
			//{ 
			Student std = db.Students.Find(model.Id);
			std.Name = model.Name;
			std.Email = model.Email;
			std.Dob = model.Dob;
			std.Grade = model.Grade;
			std.City = model.City;
			if (model.Photo != null)
			{
				if (model.ExistingPhotoPath != null)
				{
					string PhotoPath = Path.Combine(hostingEnvironment.WebRootPath, "Images", model.ExistingPhotoPath);
					System.IO.File.Delete(PhotoPath);
				}
				std.PhotoPath = FileProccesingUpload(model);
			}

			db.Update(std);
			db.SaveChanges();
			TempData["Success"] = "Student data Updated successfully.";
			return RedirectToAction("Index", "Students");
			//return RedirectToAction(nameof(Index));

			//}
			//return View();

		}
		[HttpGet]
		public ActionResult Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}
			var student = db.Students.Where(m => m.StudentId == id).FirstOrDefault();
			if (student == null)
			{
				return NotFound();
			}

			return View(student);
		}

		// POST: Students/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var student = await db.Students.FindAsync(id);
			db.Students.Remove(student);
			await db.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool StudentExists(int id)
		{
			return db.Students.Any(e => e.StudentId == id);
		}
	}
}
