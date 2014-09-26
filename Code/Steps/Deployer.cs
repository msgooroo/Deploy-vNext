using System;
using System.Threading;
using System.IO;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Web.Administration;


namespace MSGooroo.Deploy {
	/// <summary>
	/// Summary description for Git
	/// </summary>
	public static class Deployer {


		private static string[] NotErrors = new string[] {
		};

		public static bool DeployVNext(SiteConfig config, SiteRevision rev, LogWriter log) {

			ServerManager manager = new ServerManager();
			Site site = GetSite(config, log, manager);

			var outPath = Path.Combine(config.WebPath, rev.Hash);
			var projFile = Path.Combine(outPath, config.ProjectFile);


			// Allow this stuff to function without a site, so that if we dont
			// have a site we can create one and point it somewhere reasonable.
			bool success =
				RemoveOldDirectory(config, log, site, outPath)
				&& PackageApp(config, rev, log, projFile);


			if (success) {
				success = PointIis(config, rev, log, manager, site, projFile)
				&& RestartSite(config, rev, log, manager, site);
			}

			if (!success) {
				log.WriteError(string.Format("{0}: {1}> The site \"{2}\" has been Failed to deploy: {3}",
					config.Name,
					"Deploy",
					config.IisName,
					rev.Hash));
				rev.State = "Deploy failed";
				return false;
			}
			log.WriteMessage(string.Format("{0}: {1}> The site \"{2}\" has been updated to revision: {3}",
				config.Name,
				"Deploy",
				config.IisName,
				rev.Hash));

			rev.State = "Complete";

			return true;


		}

		private static bool PackageApp(SiteConfig config, SiteRevision rev, LogWriter log, string projFile) {

			try {
				var kpm = Path.Combine(ConfigurationManager.Config["KRuntime"], "kpm.cmd");
                var outDir = Path.GetDirectoryName(projFile);
				Directory.CreateDirectory(outDir);
				bool hasError = false;
				SimpleProcess.Run(
					config.SourcePath,
					kpm,
					string.Format("pack --out {0}", outDir),
					(message, isError) =>
					{
						if (isError) {
							log.WriteError(string.Format("{0}: {1}> Error: {2}", config.Name, "Package", message));
							hasError = true;
						} else {
							log.WriteMessage(string.Format("{0}: {1}> {2}", config.Name, "Package", message));
						}
					}
				);
				if (hasError) {
					log.WriteError(string.Format("{0}: {1}> Failed.", config.Name, "Package"));
					return false;
				} else {
					log.WriteMessage(string.Format("{0}: {1}> Completed successfully.", config.Name, "Package"));
					return true;
				}
			} catch (Exception ex) {
				log.WriteError(string.Format("{0}: {1}> Error 'PointIis to new directory':  {2} \r\n {3}",
					config.Name,
					"Package",
					ex.Message,
					ex.StackTrace
				));
				return false;
			}
		}

		public static bool DeployClassic(SiteConfig config, SiteRevision rev, LogWriter log) {

			ServerManager manager = new ServerManager();
			Site site = GetSite(config, log, manager);




			// Copy the files from the source directory to a directory within
			// "web" named by the hash of the commit.
			var outPath = Path.Combine(config.WebPath, rev.Hash);
			var projFile = Path.Combine(outPath, config.ProjectFile);

			// Allow this stuff to function without a site, so that if we dont
			// have a site we can create one and point it somewhere reasonable.
			bool success =
				RemoveOldDirectory(config, log, site, outPath)
				&& CreateNewDirectory(config, rev, log, outPath);

			if (success) {

				success = PointIis(config, rev, log, manager, site, projFile)
				&& RestartSite(config, rev, log, manager, site);
			}

			if (!success) {
				log.WriteError(string.Format("{0}: {1}> The site \"{2}\" has been Failed to deploy: {3}",
					config.Name,
					"Deploy",
					config.IisName,
					rev.Hash));
				rev.State = "Deploy failed";
				return false;
			}
			log.WriteMessage(string.Format("{0}: {1}> The site \"{2}\" has been updated to revision: {3}",
				config.Name,
				"Deploy",
				config.IisName,
				rev.Hash));

			rev.State = "Complete";


			return true;

		}

		private static Site GetSite(SiteConfig config, LogWriter log, ServerManager manager) {
			Site site = null;
			foreach (var s in manager.Sites) {
				if (s.Name == config.IisName) {
					site = s;
				}
			}

			if (site == null) {
				log.WriteError(string.Format("{0}: {1}> The site \"{2}\" does not exist in IIS.",
					config.Name,
					"Deploy",
					config.IisName));
				//return false;
			}

			return site;
		}

		private static bool RestartSite(SiteConfig config, SiteRevision rev, LogWriter log, ServerManager manager, Site site) {
			try {
				if (site.State == ObjectState.Stopped) {
					log.WriteMessage(string.Format("{0}: {1}> The site \"{2}\" is being Started.",
						config.Name,
						"Deploy",
						config.IisName,
						rev.Hash));

					site.Start();

					var count = 0;
					while (site.State != ObjectState.Stopped && count < 10) {
						Thread.Sleep(1000);
						count++;
					}

					if (site.State != ObjectState.Started) {
						log.WriteError(string.Format("{0}: {1}> The site \"{2}\" could not be started. Aborting after restart attempt.",
							config.Name,
							"Deploy",
							config.IisName
						));

					}
					manager.CommitChanges();
				}
				return true;
			} catch (Exception ex) {
				log.WriteError(string.Format("{0}: {1}> Error 'Restart IIS': {2} \r\n {3}",
					config.Name,
					"Deploy",
					ex.Message,
					ex.StackTrace
				));
				return false;

			}
		}

		private static bool PointIis(SiteConfig config, SiteRevision rev, LogWriter log, ServerManager manager, Site site, string projFile) {
			try {
				if (site.Applications.Count != 1) {
					log.WriteError(string.Format("{0}: {1}> The site \"{2}\" contains multiple applications.  I'm confused as to which path to update.", config.Name, "Deploy", config.IisName));
					return false;
				}

				if (site.Applications[0].VirtualDirectories.Count != 1) {
					log.WriteError(string.Format("{0}: {1}> The site \"{2}\" contains one application, but multiple VirtualDirectories.  I'm confused as to which path to update.", config.Name, "Deploy", config.IisName));
					return false;
				}

				log.WriteMessage(string.Format("{0}: {1}> Setting site \"{2}",
					config.Name,
					"Deploy",
					config.IisName,
					rev.Hash
				));
				site.Applications[0].VirtualDirectories[0].PhysicalPath = Path.GetDirectoryName(projFile);
				manager.CommitChanges();
				return true;
			} catch (Exception ex) {
				log.WriteError(string.Format("{0}: {1}> Error 'PointIis to new directory':  {2} \r\n {3}",
					config.Name,
					"Deploy",
					ex.Message,
					ex.StackTrace
				));
				return false;
			}
		}

		private static bool CreateNewDirectory(SiteConfig config, SiteRevision rev, LogWriter log, string outPath) {
			try {
				log.WriteMessage(string.Format("{0}: {1}> Copying compiled code to {2}...",
				config.Name,
				"Deploy",
				config.IisName,
				rev.Hash
			));
				DirectoryCopy(config.SourcePath, outPath, true);
				return true;

			} catch (Exception ex) {
				log.WriteError(string.Format("{0}: {1}> Error 'RemoveOldDirectory':  {2} \r\n {3}",
					config.Name,
					"Deploy",
					ex.Message,
					ex.StackTrace
				));
				return false;
			}
		}

		private static bool RemoveOldDirectory(SiteConfig config, LogWriter log, Site site, string outPath) {
			try {
				if (Directory.Exists(outPath)) {

					if (site != null && site.State == ObjectState.Started) {
						log.WriteMessage(string.Format("{0}: {1}> The site \"{2}\" is being stopped...",
							config.Name,
							"Deploy",
							config.IisName
						));
						site.Stop();

						var count = 0;
						while (site.State != ObjectState.Stopped && count < 10) {
							Thread.Sleep(1000);
							count++;
						}
						if (site.State != ObjectState.Stopped) {
							log.WriteError(string.Format("{0}: {1}> The site \"{2}\" could not be stopped. Aborting after restart attempt.",
								config.Name,
								"Deploy",
								config.IisName
							));
							site.Start();

						}
					}
					Directory.Delete(outPath, true);

				}
				return true;
			} catch (Exception ex) {
				log.WriteError(string.Format("{0}: {1}> Error 'RemoveOldDirectory':  {2} \r\n {3}",
					config.Name,
					"Deploy",
					ex.Message,
					ex.StackTrace
				));
				return false;


			}
		}



		// From: http://msdn.microsoft.com/en-us/library/bb762914.aspx
		private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs) {
			DirectoryInfo dir = new DirectoryInfo(sourceDirName);
			DirectoryInfo[] dirs = dir.GetDirectories();

			if (!dir.Exists) {
				throw new DirectoryNotFoundException(
					"Source directory does not exist or could not be found: "
					+ sourceDirName);
			}

			if (!Directory.Exists(destDirName)) {
				Directory.CreateDirectory(destDirName);
			}

			FileInfo[] files = dir.GetFiles();
			foreach (FileInfo file in files) {
				string temppath = Path.Combine(destDirName, file.Name);
				file.CopyTo(temppath, false);
			}

			if (copySubDirs) {
				foreach (DirectoryInfo subdir in dirs) {
					// Dont copy .git
					if (subdir.Name != ".git") {
						string temppath = Path.Combine(destDirName, subdir.Name);
						DirectoryCopy(subdir.FullName, temppath, copySubDirs);
					}
				}
			}
		}
	}
}