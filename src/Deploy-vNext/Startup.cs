using System;
using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Routing;

using Microsoft.Framework.ConfigurationModel;

namespace MSGooroo.Deploy {
	public class Startup {
		public void Configure(IApplicationBuilder app) {
			// Setup configuration sources
			var configuration = new Configuration();
			configuration.AddJsonFile("config.json");
			configuration.AddEnvironmentVariables();

			if (!SanityChecker.Check(Console.Out)) {
				Console.WriteLine("My world does not appear sane, quitting.");
			}


			// Set up application services
			app.UseServices(services =>
			{
				// Add MVC services to the services container
				services.AddMvc();
			});

			// Add static files to the request pipeline
			app.UseStaticFiles();

			// Add MVC to the request pipeline
			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller}/{action}/{id?}",
					defaults: new { controller = "Home", action = "Index" });

				routes.MapRoute(
					name: "api",
					template: "{controller}/{id?}");
			});
		}
	}
}
