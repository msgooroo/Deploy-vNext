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


		// GET: /<controller>/
		public IActionResult Index(string deployKey) {
			var path = AppDomain.CurrentDomain.BaseDirectory;
			Console.WriteLine("Running in: " + path);

			string sitesJson = File.ReadAllText(path + "\\sites.json");

			var sites = JsonConvert.DeserializeObject<List<SiteConfig>>(sitesJson);

			// Find the site with this identifier...
			var site = sites.FirstOrDefault(x => x.DeployKey == deployKey);
			if (site != null) {
				// Do the deployment....
				site.Initialize();
				var log = new LogWriter();
				log.AddStream(new StreamWriter(Context.Response.Body));

				var rev = Git.Update(site, log);

				Builder.Build(site, log);

				Deployer.Deploy(site, rev, log);
            }


			return View();
		}
	}
}
