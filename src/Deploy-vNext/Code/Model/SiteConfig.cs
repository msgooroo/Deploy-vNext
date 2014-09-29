using System.Collections.Generic;
using System.IO;
using System;


namespace MSGooroo.Deploy
{

    /// <summary>
    /// Summary description for SiteConfig
    /// </summary>
    public class SiteConfig
    {
		public string Name;
		public string Branch;
		public string IisName;
		public string ConfigFile;
		public string Path;
		public string DeployKey;
		public string Repository;
		public string Remote;
		public string SshKeyPath;

		/* Build */
		public string ProjectFile;
		public string Configuration;

		public List<SiteRevision> Revisions;



		public string WebPath { get { return System.IO.Path.Combine(Path, "web"); } }
		public string SourcePath { get { return System.IO.Path.Combine(Path, "git-src"); } }

		public bool SourceIsEmpty {
			get {
				return Directory.GetFiles(SourcePath).Length==0;
			}
		}

        public void Initialize() {
			if (Revisions == null) {
				Revisions = new List<SiteRevision>();
			}

			if (!Directory.Exists(Path)) {
				Directory.CreateDirectory(Path);
			}

			if (!Directory.Exists(WebPath)) {
				Directory.CreateDirectory(WebPath);
			}

			if (!Directory.Exists(SourcePath)) {
				Directory.CreateDirectory(SourcePath);
			}


		}

	}
}