using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SydnomCollege.Models;
using SydnomCollege.ModelView;

namespace SydnomCollege.Controllers
{
	[Authorize(Policy = "RolesPolicy")]
	public class AdministrationController : Controller
	{
		private readonly RoleManager<IdentityRole> roleManager;
		private readonly UserManager<IdentityUser> userManager;
		private readonly ILogger<AdministrationController> logger;

		public AdministrationController(RoleManager<IdentityRole> roleManager,
										UserManager<IdentityUser> userManager,
										ILogger<AdministrationController> logger)

		{
			this.roleManager = roleManager;
			this.userManager = userManager;
			this.logger = logger;
		}
		[HttpGet]
		[Authorize(Policy = "ClaimsPolicy")]
		public IActionResult CreateRole()
		{
			return View();
		}
		[HttpPost]
		[Authorize(Policy = "ClaimsPolicy")]
		public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
		{
			if (ModelState.IsValid)
			{
				IdentityRole identityRole = new IdentityRole
				{
					Name = model.RoleName
				};
				IdentityResult result = await roleManager.CreateAsync(identityRole);
				if (result.Succeeded)
				{
					return RedirectToAction("ListRoles", "Administration");
				}
				foreach (IdentityError error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
			}
			return View(model);
		}
		[HttpGet]
		public IActionResult ListUsers()
		{
			var users = userManager.Users;
			return View(users);
		}
		[HttpGet]
		public IActionResult ListRoles()
		{
			var roles = roleManager.Roles;
			return View(roles);
		}
		[HttpGet]
		public async Task<IActionResult> EditUser(string id)
		{
			var user = await userManager.FindByIdAsync(id);
			if (user == null)
			{
				ViewBag.ErrorMessage = $"User with Id = {id} is not found!";
				return View("NotFound");
			}
			var userClaims = await userManager.GetClaimsAsync(user);
			var userRoles = await userManager.GetRolesAsync(user);

			var model = new EditUserViewModel
			{
				Id = user.Id,
				Email = user.Email,
				UserName = user.Email,
				Claims = userClaims.Select(c => c.Type + ":" + c.Value).ToList(),
				Roles = userRoles
			};
			return View(model);
		}
		[HttpPost]
		public async Task<IActionResult> EditUser(EditUserViewModel model)
		{
			var user = await userManager.FindByIdAsync(model.Id);
			if (user == null)
			{
				ViewBag.ErrorMessage = $"User with Id = {model.Id} is not found!";
				return View("NotFound");
			}
			else
			{
				user.Email = model.Email;
				user.UserName = model.UserName;

				var result = await userManager.UpdateAsync(user);
				if (result.Succeeded)
				{
					return RedirectToAction("ListUsers");
				}
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);

				}
				return View(model);
			}

		}
		[HttpPost]
		public async Task<IActionResult> DeleteUser(string id)
		{
			var user = await userManager.FindByIdAsync(id);
			if (user == null)
			{
				ViewBag.ErrorMessage = $"User with Id ={id} is not found!";
				return View("NotFound");
			}
			else
			{
				var result = await userManager.DeleteAsync(user);

				if (result.Succeeded)
				{
					return RedirectToAction("ListUsers");
				}
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
				return View("ListUsers");

			}
		}
		[Authorize(Policy = "ClaimsPolicy")]
		[HttpGet]
		public async Task<IActionResult> EditRole(string id)
		{
			var role = await roleManager.FindByIdAsync(id);
			if (role == null)
			{
				ViewBag.ErrorMessage = $"Role with Id = {id} is not found!";
				return View("NotFound");
			}
			var model = new EditRoleViewModel
			{
				Id = role.Id,
				RoleName = role.Name,

			};
			foreach (var user in userManager.Users)
			{
				if (await userManager.IsInRoleAsync(user, role.Name))
				{
					model.Users.Add(user.UserName);
				}
			}
			return View(model);

		}
		[Authorize(Policy = "ClaimsPolicy")]
		[HttpPost]
		public async Task<IActionResult> EditRole(EditRoleViewModel model)
		{
			var role = await roleManager.FindByIdAsync(model.Id);
			if (role == null)
			{
				ViewBag.ErrorMessage = $"Role with Id = {model.Id} is not found!";
				return View("NotFound");
			}
			else
			{
				role.Name = model.RoleName;
				var result = await roleManager.UpdateAsync(role);

				if (result.Succeeded)
				{
					return RedirectToAction("ListRoles");
				}
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
				return View(model);

			}

		}
		[HttpGet, HttpPost]
		[Authorize(Policy = "ClaimsPolicy")]
		public async Task<IActionResult> DeleteRole(string id)
		{
			var role = await roleManager.FindByIdAsync(id);
			if (role == null) 
			{
				ViewBag.ErrorMessage = $"Role with Id ={id} is not found!";
				return View("NotFound");
			}
			else
			{
				try
				{
					var result = await roleManager.DeleteAsync(role);

					if (result.Succeeded)
					{
						return RedirectToAction("ListRoles");
					}
					foreach (var error in result.Errors)
					{
						ModelState.AddModelError("", error.Description);
					}
					return View("ListRoles");

				}
				catch (DbUpdateException ex)

				{
					logger.LogError($"Error Deleting Role{ex}");

					ViewBag.ErrorTitle = $"{role.Name} role is in use";
					ViewBag.ErrorMessage = $"{role.Name} role can't deleted as there are users in that role first remove the users"
						+ $"in this role. if you want to delete this role please remove the users form" + $"the role and then try delete";
					return View("Error");
				}
			}
		}

		[HttpGet]
		public async Task<IActionResult> EditUsersInRole(string roleId)
		{
			ViewBag.roleId = roleId;
			var role = await roleManager.FindByIdAsync(roleId);
			if (role == null)
			{
				ViewBag.ErrorMessage = $"Role With Id = {roleId} not found!";
				return View("NotFound");
			}

			var model = new List<EditUsersInRoleViewModel>();
			foreach (var user in userManager.Users)
			{
				var editUsersInRoleViewModel = new EditUsersInRoleViewModel
				{
					UserId = user.Id,
					UserName = user.UserName
				};
				if (await userManager.IsInRoleAsync(user, role.Name))
				{
					editUsersInRoleViewModel.IsSleceted = true;
				}
				else
				{
					editUsersInRoleViewModel.IsSleceted = false;
				}
				model.Add(editUsersInRoleViewModel);
			}
			return View(model);

		}
		[HttpPost]
		public async Task<IActionResult> EditUsersInRole(List<EditUsersInRoleViewModel> model, string roleId)
		{
			var role = await roleManager.FindByIdAsync(roleId);
			if (role == null)
			{
				ViewBag.ErrorMessage = $"Role With Id ={roleId} not found!";
				return View("NotFound");
			}
			for (int i = 0; i < model.Count; i++)
			{
				var user = await userManager.FindByIdAsync(model[i].UserId);

				IdentityResult result = null;

				if (model[i].IsSleceted && !(await userManager.IsInRoleAsync(user, role.Name)))
				{
					result = await userManager.AddToRoleAsync(user, role.Name);
				}
				else if (!model[i].IsSleceted && (await userManager.IsInRoleAsync(user, role.Name)))
				{
					result = await userManager.RemoveFromRoleAsync(user, role.Name);
				}
				else
				{
					continue;
				}
				if (result.Succeeded)
				{
					if (i < (model.Count - 1))
						continue;
					else
						return RedirectToAction("EditRole", new { Id = roleId });
				}
			}
			return RedirectToAction("EditRole", new { Id = roleId });
		}
		[HttpGet]
		public async Task<IActionResult> EditRolesInUser(string userId)
		{
			ViewBag.userId = userId;
			var user = await userManager.FindByIdAsync(userId);
			if (user == null)
			{
				ViewBag.ErrorMassege = $"User with {userId} is not Found!";
				return View("NotFound");
			}
			else
			{
				var model = new List<EditRolesInUserViewModel>();
				foreach (var role in roleManager.Roles)
				{
					var editRolesInUserViewModel = new EditRolesInUserViewModel
					{
						RoleId = role.Id,
						RoleName = role.Name
					};
					if (await userManager.IsInRoleAsync(user, role.Name))
					{
						editRolesInUserViewModel.IsSelected = true;
					}
					else
					{
						editRolesInUserViewModel.IsSelected = false;
					}
					model.Add(editRolesInUserViewModel);
				}
				return View(model);

			}
		}
		[HttpPost]
		public async Task<IActionResult> EditRolesInUser(List<EditRolesInUserViewModel> model, string userId)
		{
			var user = await userManager.FindByIdAsync(userId);
			if (user == null)
			{
				ViewBag.ErrorMessage = $"User is with {userId} not found!";
				return View("NotFound");
			}

			var roles = await userManager.GetRolesAsync(user);
			var result = await userManager.RemoveFromRolesAsync(user, roles);

			if (!result.Succeeded)
			{
				ModelState.AddModelError("", "Cannot remove user Existing Roles");
				return View(model);
			}
			result = await userManager.AddToRolesAsync(user, model.Where(x => x.IsSelected).Select(y => y.RoleName));
			if (!result.Succeeded)
			{
				ModelState.AddModelError("", "Cannot Selected roles to user");
			}
			return RedirectToAction("EditUser", new { Id = userId });


		}
		[HttpGet]
		public async Task<IActionResult> ManageUserClaims(string userId)
		{
			ViewBag.userId = userId;
			var user = await userManager.FindByIdAsync(userId);
			if (user == null)
			{
				ViewBag.ErrorMassege = $"User with {userId} is not Found!";
			 	return View("NotFound");
			}
			var existingUserClaims = await userManager.GetClaimsAsync(user);

			var model = new UserClaimsViewModel
			{
				UserId = userId

			};
			foreach (Claim claim in ClaimStore.AllClaims)
			{
				UserClaim userClaim = new UserClaim
				{
					ClaimType = claim.Type
				};

				//if the user has claim, set IsSelected property to true so the checkbox next to the claim is checked on the UI

				if (existingUserClaims.Any(c => c.Type == claim.Type && c.Value == "true"))
						
				{
					userClaim.IsSelected = true;
				}
				model.Claims.Add(userClaim);
			}
			return View(model);
		}
		[HttpPost]
		public async Task<IActionResult> ManageUserClaims(UserClaimsViewModel model)
		{
			var user = await userManager.FindByIdAsync(model.UserId);

			if (user == null)
			{
				ViewBag.ErrorMassege = $"User with {model.UserId} is not Found!";
				return View("NotFound");
			}
			var claims = await userManager.GetClaimsAsync(user);
			var result = await userManager.RemoveClaimsAsync(user, claims);

			if(!result.Succeeded)
			{
				ModelState.AddModelError("", "Cannot remove user from existing Claims");
				return View(model);

			}
			result = await userManager.AddClaimsAsync(user,
				model.Claims.Select(c => new Claim(c.ClaimType, c.IsSelected ? "true":"false")));

			if(!result.Succeeded)
			{
				ModelState.AddModelError("", "Cannot Add Selceted claims to user");
				return View(model);
			} 
			return RedirectToAction("EditUser", new { Id = model.UserId });
		}

	}
	
}


