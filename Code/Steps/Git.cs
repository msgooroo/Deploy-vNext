using System;
using System.IO;
using Microsoft.Framework.ConfigurationModel;

namespace MSGooroo.Deploy {
	/// <summary>
	/// Summary description for Git
	/// </summary>
	public class Git {

		public Git() {
		}

		private static string[] NotErrors = new string[] {
			"Identity added:",
			"From",
			"Checking out",
			"Switched to a new branch",
			" *"
		};


		public static SiteRevision Update(SiteConfig config, LogWriter log) {

			var rev = new SiteRevision() { Time = DateTime.UtcNow, State = "Updating..." };

			if (config.SourceIsEmpty) {
				rev.Hash = Clone(config, log);

			} else {
				rev.Hash = Fetch(config, log);
			}

			if (rev.Hash != null) {
				rev.State = "Updated";
			} else {
				rev.State = "Error: Update failed";
			}
			return rev;
		}

		public static string Fetch(SiteConfig config, LogWriter log) {
			return RunGit(config, "git-pull", log);

		}

		public static string Clone(SiteConfig config, LogWriter log) {
			return RunGit(config, "git-clone", log);

		}

		public static string ProcessTemplate(string template, SiteConfig config) {
			string batchPath = Path.Combine(ConfigurationManager.WorkingDirectory, "BatchTemplates", template + ".bat");
			if (!File.Exists(batchPath)) {
				throw new FileNotFoundException("Unable to find file: " + batchPath);
			}

			var batchText = File.ReadAllText(batchPath);

			batchText = batchText.Replace("<{GitPath}>", ConfigurationManager.Config["GitPath"]);
			batchText = batchText.Replace("<{Path}>", config.Path);
			batchText = batchText.Replace("<{SshKeyPath}>", config.SshKeyPath);
			batchText = batchText.Replace("<{Repository}>", config.Repository);
			batchText = batchText.Replace("<{Path}>", config.Path);
			batchText = batchText.Replace("<{Remote}>", config.Remote);
			batchText = batchText.Replace("<{Branch}>", config.Branch);

			return batchText;


		}

		/// <summary>
		/// Runs the git command batch file and extracts the current commit revision hash
		/// </summary>
		/// <param name="config"></param>
		/// <param name="command"></param>
		/// <param name="log"></param>
		/// <returns></returns>
		public static string RunGit(SiteConfig config, string command, LogWriter log) {
			var hasError = false;

			string batch = ProcessTemplate(command, config);
			string batchFileName = Guid.NewGuid().ToString() + ".bat";
			string batchFilePath = Path.Combine(config.Path, batchFileName);

			File.WriteAllText(batchFilePath, batch);
			log.WriteMessage(string.Format("{0}> Running {1}...", command, batchFilePath));

			string revision = null;
			SimpleProcess.Run(
				config.Path,
				batchFilePath,
				"",
				(message, isError) =>
				{
					if (isError) {
						foreach (var prefix in NotErrors) {
							if (message.StartsWith(prefix)) {
								isError = false;
								break;
							}
						}
					}

					if (message.StartsWith("commit")){
						// Get the revision
						var parts = message.Split(' ');
						revision = parts[1];
                    }

					if (isError) {
						log.WriteError(string.Format("{0}: {1}> Error: {2}", config.Name, command, message));
						hasError = true;
					} else {
						log.WriteMessage(string.Format("{0}: {1}> {2}", config.Name, command, message));
					}
				}
			);
			if (hasError) {
				log.WriteError(string.Format("{0}: {1}> Failed.", config.Name, command));
			} else {
				log.WriteMessage(string.Format("{0}: {1}> Completed successfully.", config.Name, command));
			}
			return revision;
		}


	}
}