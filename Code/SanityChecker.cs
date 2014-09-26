using System;
using System.IO;
using System.Text;

namespace MSGooroo.Deploy {
	/// <summary>
	/// Summary description for SanityChecker
	/// </summary>
	public static class SanityChecker {
		public static bool Check(TextWriter output) {
			return CheckConfig(output)
				&& CheckFiles(output);


		}

		private static bool CheckConfig(TextWriter output) {
			if (string.IsNullOrEmpty(ConfigurationManager.Config["GitPath"])) {
				output.WriteLine("Error: The setting \"GitPath\" was not found in config.json, set this to the \"bin\" directory where \"git.exe\" can be found");

				return false;
			}
			if (string.IsNullOrEmpty(ConfigurationManager.Config["BuildExecutable"])) {
				output.WriteLine("Error: The setting \"BuildExecutable\" was not found in config.json, set this to the path where MSBuild.exe or xBuild.exe is found (including the executable).");

			}

			return true;
		}
		private static bool CheckFiles(TextWriter output) {

			return CheckFile(output, "GitPath", "git.exe")
				&& CheckFile(output, "BuildExecutable", null);



		}

		private static bool CheckFile(TextWriter output, string config, string testFile) {
			string expectedPath = ConfigurationManager.Config[config];
			if (testFile != null) {

				expectedPath = Path.Combine(expectedPath, testFile);
			}
			if (!File.Exists(expectedPath)) {
				output.WriteLine(
					string.Format("Error: Unable to find \"{0}\" in path \"{1}\", using \"{2}\".",
					testFile,
					expectedPath,
					config
					));
				return false;
			}

			return true;
		}
	}
}