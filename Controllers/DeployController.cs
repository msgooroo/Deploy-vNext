using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System;
using Microsoft.AspNet.Mvc;
using System.Threading;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace MSGooroo.Deploy.Controllers {
	public class DeployController : Controller {


		public IActionResult Test() {
			return Json(new { Message = "Working!", Now = DateTime.UtcNow });

		}

		// GET: /<controller>/
		public IActionResult Index(string deployKey) {

			var path = AppDomain.CurrentDomain.BaseDirectory;
			Console.WriteLine("Running in: " + path);

			var log = new LogWriter();
			log.AddStream(new StreamWriter(Context.Response.Body));

			List<SiteConfig> sites = null;
			string sitesJson = File.ReadAllText(path + "\\sites.json");
			try {
				sites = JsonConvert.DeserializeObject<List<SiteConfig>>(sitesJson);
			} catch (Exception ex) {
				log.WriteError(string.Format("Unable to read sites, json invald: {0}", ex.Message));
				return null;
			}

			// Find the site with this identifier...
			var site = sites.FirstOrDefault(x => x.DeployKey == deployKey);
			if (site != null) {


				// Do the deployment....
				site.Initialize();

				var rev = Git.Update(site, log);

				if (site.ProjectFile.EndsWith("project.json")) {
					Deployer.DeployVNext(site, rev, log);
				} else {
					if (Builder.Build(site, log)) {
						Deployer.DeployClassic(site, rev, log);
					}

				}



			} else {
				log.WriteError(string.Format("Unable to find site with DeployKey: {0}", deployKey));
			}


			return View();
		}
	}
}
