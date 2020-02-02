﻿using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SydnomCollege.ModelView;

namespace SydnomCollege.Controllers
{
	[Authorize]
	public class AccountController : Controller
	{
		private readonly UserManager<IdentityUser> userManager;
		private readonly SignInManager<IdentityUser> signInManager;
		private readonly ILogger<AccountController> logger;

		public AccountController(UserManager<IdentityUser> userManager,
								SignInManager<IdentityUser> signInManager,
								ILogger<AccountController> logger)
		{
			this.userManager = userManager;
			this.signInManager = signInManager;
			this.logger = logger;
		}
		[HttpGet]
		[AllowAnonymous]
		public IActionResult Register()
		{
			return View();
		}
		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = new IdentityUser
				{
					UserName = model.Email,
					Email = model.Email

				};
				var result = await userManager.CreateAsync(user, model.Password);

				if (result.Succeeded)
				{
					// for Email Confrimation 
					var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

					var confirmationLink = Url.Action("ConfirmEmail", "Account",
									new { userId = user.Id, token = token }, Request.Scheme);

					logger.Log(LogLevel.Warning, confirmationLink);

					if (signInManager.IsSignedIn(User) && User.IsInRole("Admin"))
					{
						return RedirectToAction("ListUsers", "Administration");
					}
					using (MailMessage mm = new MailMessage(new MailAddress("Dotnet2784@gmail.com", 
						                               "Sydnom College Admin"), new MailAddress(model.Email)))
					//customise name here
					{
						try
						{
							//customise subject here
							mm.Subject = "Sydnom/Email ConfirmationLink do not reply!";
							//customise mail body here
							mm.Body = confirmationLink;
							mm.IsBodyHtml = true;
							SmtpClient smtp = new SmtpClient();
							smtp.Host = /*"smtp-mail.outlook.com";*/"smtp.gmail.com";
							smtp.EnableSsl = true;
							NetworkCredential NetworkCred = new NetworkCredential("Dotnet2784@gmail.com", "dotnetmvc");
							smtp.UseDefaultCredentials = false;                   //email & password
							smtp.Credentials = NetworkCred;
							smtp.Port = 587;
							smtp.Send(mm);
						}
						catch (Exception ex)
						{
							throw ex;
						}
					}

					ViewBag.ErrorTitle = "Registration Succsessfull";
					ViewBag.ErrorMessage = "Befor you login please comfirm your" +
						"email, by clicking on the confrimationlink wihich we have emailed you!!!";
					return View("Error");
					//await signInManager.SignInAsync(user, isPersistent: false);
					//return RedirectToAction("Login", "Account");
				}

				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}

			}
			return View(model);
		}

		[HttpPost, HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> IsEmailInUse(string email)
		{
			var newuser = await userManager.FindByEmailAsync(email);
			if (newuser == null)
			{
				return Json(true);
			}
			else
			{
				return Json($"Email {email} is alredy in Use, Please try with another Email");
			}
		}
		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> Logout()
		{
			await signInManager.SignOutAsync();
			return RedirectToAction("Index", "Home");
		}
		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> Login(string retunUrl)
		{
			LoginViewModel model = new LoginViewModel
			{
				ReturnUrl = retunUrl,
				ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
			};
			return View(model);
		}

		//if want to use Email confrimation use this below Login action

		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> Login(LoginViewModel model, string returnurl)
		{
			model.ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

			if (ModelState.IsValid)
			{
				var user = await userManager.FindByEmailAsync(model.Email);

				if (user != null && !user.EmailConfirmed && (await userManager.CheckPasswordAsync(user, model.Password)))
				{
					ModelState.AddModelError(string.Empty, "Email not confirmed yet");
					return View(model);
				}

				var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.Rememberme, true);

				if (result.Succeeded)
				{
					if (!string.IsNullOrEmpty(returnurl) && Url.IsLocalUrl(returnurl))
					{
						//return LocalRedirect(returnurl);
						return Redirect(returnurl);
					}
					else
					{
						return RedirectToAction("index", "Students");
					}
				}
				if(result.IsLockedOut)
				{
					return View("AccountLocked");
				}
				ModelState.AddModelError(string.Empty, "Invalid Login Attemp");

			}
			return View(model);
		}

		//[HttpPost]
		//[AllowAnonymous]
		//public async Task<IActionResult> Login(LoginViewModel model, string returnurl)
		//{
		//	model.ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

		//	if (ModelState.IsValid)
		//	{
		//		var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.Rememberme, false);

		//		if (result.Succeeded)
		//		{
		//			if (!string.IsNullOrEmpty(returnurl) && Url.IsLocalUrl(returnurl))
		//			{
		//				//return LocalRedirect(returnurl);
		//				return Redirect(returnurl);
		//			}
		//			else
		//			{
		//				return RedirectToAction("index", "Students");
		//			}
		//		}
		//		ModelState.AddModelError(string.Empty, "Invalid Login Attemp");

		//	}
		//	return View(model);
		//}

		[AllowAnonymous]
		[HttpPost]
		public IActionResult ExternalLogin(string provider, string returnUrl)
		{
			var redirectUrl = Url.Action("ExternalLoginCallback", "Account",
											              new { ReturnUrl = returnUrl });

			var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
			return new ChallengeResult(provider, properties);

		}
		[AllowAnonymous]
		public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
		{
			returnUrl = returnUrl ?? Url.Content("~/");

			LoginViewModel loginViewModel = new LoginViewModel
			{
				ReturnUrl = returnUrl,
				ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
			};
			if (remoteError != null)
			{
				ModelState.AddModelError(string.Empty, $"Error from External Login Provider:{remoteError}");
				return View("Login", loginViewModel);
			}
			var info = await signInManager.GetExternalLoginInfoAsync();
			if (info == null)
			{
				ModelState.AddModelError(string.Empty, "Error from External Login Information.");
				return View("Login", loginViewModel);
			}

			// this code for confrim external login email.

			var email = info.Principal.FindFirstValue(ClaimTypes.Email);

			IdentityUser user = null;

			if (email != null)
			{
				user = await userManager.FindByEmailAsync(email);

				if (user != null && !user.EmailConfirmed)
				{
					ModelState.AddModelError(string.Empty, "Email not confirmed yet!");
					return View("Login", loginViewModel);
				}
			}
			// External email confrimation code ends here.

			var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey,
																	                 isPersistent: false, bypassTwoFactor: true);

			if (signInResult.Succeeded)
			{
				return LocalRedirect(returnUrl);
			}
			else
			{
				// comment this line when want to use External email confrirmation
				//var email = info.Principal.FindFirstValue(ClaimTypes.Email);

				if (email != null)
				{
					// comment this line when want to use External email confrirmation
					//var user = await userManager.FindByEmailAsync(email);

					if (user == null)
					{
						user = new IdentityUser
						{
							UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
							Email = info.Principal.FindFirstValue(ClaimTypes.Email)
						};
						await userManager.CreateAsync(user);

						var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

						var confirmationLink = Url.Action("ConfirmEmail", "Account",
															 new { userId = user.Id, token = token }, Request.Scheme);
						logger.Log(LogLevel.Warning, confirmationLink);

						ViewBag.ErrorTitle = "Registration Successful";
						ViewBag.ErrorMessage = "Before you can login, please confrim your email " +
							"by clicking on the confrirmation link which we have emailed you";
						return View("Error");

					}

					await userManager.AddLoginAsync(user, info);
					await signInManager.SignInAsync(user, isPersistent: false);

					return LocalRedirect(returnUrl);
				}
				ViewBag.ErrorTitle = $"Email Claim not receive from {info.LoginProvider}";
				ViewBag.ErrorMessage = $"Please contact support on dotnet2784@gmail.com";

				return View("Error");

			}

		}
		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> ConfirmEmail(string userId, string token)
		{
			if (userId == null || token == null)
			{
				return RedirectToAction("Index", "Students");
			}
			var user = await userManager.FindByIdAsync(userId);
			if (user == null)
			{
				ViewBag.ErrorMessage = $"The User Id  {userId} is invalid";
				return View("NotFound");
			}
			var result = await userManager.ConfirmEmailAsync(user, token);
			if (result.Succeeded)
			{
				return View();
			}
			ViewBag.ErrorTitle = "Email cannot confrimed";
			return View("Error");
		}
		[HttpGet]
		[AllowAnonymous]
		public IActionResult ForgotPassword()
		{
			return View();
		}
		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await userManager.FindByEmailAsync(model.Email);
				if (user != null && await userManager.IsEmailConfirmedAsync(user))
				{
					var token = await userManager.GeneratePasswordResetTokenAsync(user);

					var passwordResetLink = Url.Action("ResetPassword", "Account",
									new { email = model.Email, token = token }, Request.Scheme);

					logger.Log(LogLevel.Warning, passwordResetLink);

					return View("ForgotPasswordConfirmation");

				}
				return View("ForgotPasswordConfirmation");

			}
			return View(model);
		}
		[HttpGet]
		[AllowAnonymous]
		public IActionResult ResetPassword(string token, string Email)
		{
			if (token == null || Email == null)
			{
				ModelState.AddModelError("", "Invalid password reset token");
			}
			return View();
		}
		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await userManager.FindByEmailAsync(model.Email);
				if (user != null)
				{
					var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
					if (result.Succeeded)
					{
						if(await userManager.IsLockedOutAsync(user))
						{
							await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
						}
						return View("ResetPasswordConfirmation");
					}
					foreach (var error in result.Errors)
					{
						ModelState.AddModelError("", error.Description);
					}
					return View(model);
				}
				return View("ResetPasswordConfirmation");
			}
			return View(model);
		}
		[HttpGet]
		public async Task<IActionResult> ChangePassword()
		{
			var user = await userManager.GetUserAsync(User);
			var userHasPassword = await userManager.HasPasswordAsync(user);
			if (!userHasPassword)
			{
				return RedirectToAction("AddPassword");
			}
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await userManager.GetUserAsync(User);
				if (user == null)
				{
					return RedirectToAction("Login", "Account");
				}
				var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
				if (!result.Succeeded)
				{
					foreach (var error in result.Errors)
					{
						ModelState.AddModelError("", error.Description);
					}
					return View();
				}
				await signInManager.RefreshSignInAsync(user);
				return View("ChangePasswordConfirmation");
			}

			return View(model);
		}
		[HttpGet]
		public async Task<IActionResult> AddPassword()
		{
			var user = await userManager.GetUserAsync(User);
			var userHasPassword = await userManager.HasPasswordAsync(user);
			if (userHasPassword)
			{
				return RedirectToAction("ChangePassword");
			}
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> AddPassword(AddPasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await userManager.GetUserAsync(User);
				var result = await userManager.AddPasswordAsync(user, model.NewPassword);

				if (!result.Succeeded)
				{
					foreach (var error in result.Errors)
					{
						ModelState.AddModelError("", error.Description);
					}
					return View();
				}
				await signInManager.RefreshSignInAsync(user);
				return RedirectToAction("AddPasswordConfirmation");

			}
			return View(model);
		}

		[AllowAnonymous]
		public IActionResult AccessDenied()
		{
			return View();
		}
	}

}
