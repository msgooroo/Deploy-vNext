using Microsoft.Web.Administration;

namespace MSGooroo.Deploy {
	public static class SwapSiteIIS {

		public static bool ChangeSitePath(string siteName, string newPath) {


			ServerManager manager = new ServerManager();
			bool found = false;
			foreach (var site in manager.Sites) {
				if (site.Name == siteName) {
					site.Applications[0].VirtualDirectories[0].PhysicalPath =newPath;
					manager.CommitChanges();
					found = true;
				}
			}

			return found;
		}
	}
}
