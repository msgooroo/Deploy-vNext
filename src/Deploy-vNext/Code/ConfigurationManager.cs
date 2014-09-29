using System.Collections.Generic;
using System.IO;
using Microsoft.Framework.ConfigurationModel;
using System;

namespace MSGooroo.Deploy {

	/// <summary>
	/// Static object for easy access to configuration and
	/// application settings
	/// </summary>
	public static class ConfigurationManager {
		public static string WorkingDirectory {
            get {
				return AppDomain.CurrentDomain.BaseDirectory;
			}
		}

		public static Configuration Config {
			get {
				// Setup configuration sources
				var configuration = new Configuration();
				configuration.AddJsonFile("config.json");
				configuration.AddEnvironmentVariables();

				return configuration;
			}
		}

	}
}