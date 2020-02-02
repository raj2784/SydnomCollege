using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SydnomCollege.Models;
using SydnomCollege.Security;

namespace SydnomCollege
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.Configure<CookiePolicyOptions>(options =>
			{
				// This lambda determines whether user consent for non-essential cookies is needed for a given request.
				options.CheckConsentNeeded = context => true;
				options.MinimumSameSitePolicy = SameSiteMode.None;
			});


			//services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
			services.AddMvc(options =>
			{
				var policy = new AuthorizationPolicyBuilder()
								  .RequireAuthenticatedUser()
								  .Build();
				options.Filters.Add(new AuthorizeFilter(policy));
			}).AddXmlSerializerFormatters();


			// To change Access Denied Path 
			//services.ConfigureApplicationCookie(options =>
			//{
			//	options.AccessDeniedPath = new PathString("/Administration/AccessDenied");
			//});

			services.AddAuthorization(options =>
			{
				//Claims Policy
				options.AddPolicy("ClaimsPolicy", policy => policy.RequireClaim("Delete Role", "true")
																  .RequireClaim("Create Role", "true")
																  .RequireClaim("Edit Role", "true"));
				//Roles Policy
				options.AddPolicy("RolesPolicy", policy => policy.RequireRole("Admin", "SuperAdmin"));

			});

			services.AddAuthentication()
			.AddGoogle(options =>
			{
				options.ClientId = "765330581278-ag7jmp5hg7v2utf1mqki4kq1qa5e8bqe.apps.googleusercontent.com";
				options.ClientSecret = "PvTODNku78MhG2i7FgXw36gJ";
			})
			.AddFacebook(options =>
			{
				options.AppId = "488616388677493";
				options.AppSecret = "41279fd984cb7fae19149a9e55a50f84";
			})
			.AddTwitter(options =>
			{

				options.ConsumerKey = "seQFdz1ZLAXcUSmkOxfePyP7R";
				options.ConsumerSecret = "JEFZ9qhw2r9F1VIMDuImlrjZqNZoid5cgWjLdiYBnBo5RXqRLT";
							
			});

			//For MSSQL Databse
			services.AddDbContextPool<StudentContext>(options => options.UseSqlServer(Configuration.GetConnectionString("EntConncetion")));

			//For Oracle Database
			//services.AddDbContextPool<StudentContext>(options => options.UseOracle(Configuration.GetConnectionString("EntConncetion")));

			//For MySQl Database
			//services.AddDbContextPool<StudentContext>(options => options.UseMySQL(Configuration.GetConnectionString("EntConncetion")));

			services.AddIdentity<IdentityUser, IdentityRole>(options =>
			{
				// this code is make custom password complexity
				//options.Password.RequiredLength = 7;
				//options.Password.RequiredUniqueChars = 3;
				//options.Password.RequireDigit = true;
				//options.Password.RequireLowercase = true;
				//options.Password.RequireNonAlphanumeric = true;
				//options.Password.RequireUppercase = true;


				options.SignIn.RequireConfirmedEmail = true;
				//options.SignIn.RequireConfirmedEmail = false;

				options.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation";
				//to Lockout the account
				options.Lockout.MaxFailedAccessAttempts = 5;
				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

			})
				.AddEntityFrameworkStores<StudentContext>()
				.AddDefaultTokenProviders()

				//Custom Token provider for change the lifespan just for Email
				.AddTokenProvider<CustomEmailConfirmationTokenProvider<IdentityUser>>("CustomEmailConfirmation");

			// this code change lifespan for all tokens type (Defaultokenprovider tokens have 1 day timespan by default)
			//services.Configure<DataProtectionTokenProviderOptions>(o => o.TokenLifespan = TimeSpan.FromHours(5));

			services.Configure<CustomEmailConfirmationTokenProviderOptions>(o => o.TokenLifespan = TimeSpan.FromDays(3));

		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();  
			}
			else
			{
				app.UseExceptionHandler("/Error");
				app.UseStatusCodePagesWithReExecute("/Error/{0}");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseCookiePolicy();
			app.UseAuthentication();
			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
