using System;
using System.IO;
using Microsoft.Framework.ConfigurationModel;

namespace MSGooroo.Deploy {
	/// <summary>
	/// Summary description for Git
	/// </summary>
	public static class Builder {


		private static string[] NotErrors = new string[] {
		};



		public static bool Build(SiteConfig config, LogWriter log) {
			var hasError = false;

			string buildExecutable = ConfigurationManager.Config["BuildExecutable"];


			log.WriteMessage(string.Format("{0}: Build> Start", config.Name));

			SimpleProcess.Run(
				config.SourcePath,
				buildExecutable,
				string.Format("{0} /p:Configuration={1}", config.ProjectFile, config.Configuration),
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
					if (isError) {
						log.WriteError(string.Format("{0}: Build> Error: {1}", config.Name, message));
						hasError = true;
					} else {
						log.WriteMessage(string.Format("{0}: Build> {1}", config.Name, message));
					}
				}
			);
			if (hasError) {
				log.WriteError(string.Format("{0}: Build> Failed.", config.Name));
			} else {
				log.WriteMessage(string.Format("{0}: Build> Completed successfully.", config.Name));
			}
			return hasError;
		}


	}
}