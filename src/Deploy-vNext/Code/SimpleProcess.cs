using System;
using System.Diagnostics;

namespace MSGooroo.Deploy {
	public class SimpleProcess {
		public delegate void OutputReceived(string message, bool isError);

		private OutputReceived _func;
		private Process _process;

		public static void Run(string workingDir, string fileName, string arguments, OutputReceived func) {
			var process = new SimpleProcess(workingDir, fileName, arguments, func);
			process.RunBlocking();
        }

		private SimpleProcess(string workingDir, string fileName, string arguments, OutputReceived func) {
			_process = new Process();
			_process.StartInfo.UseShellExecute = false;
			_process.StartInfo.CreateNoWindow = true;
			_process.StartInfo.RedirectStandardOutput = true;
			_process.StartInfo.RedirectStandardError = true;

			//_process.StartInfo.LoadUserProfile = true;
			//_process.StartInfo.UserName = "System";

			if (workingDir != null) {
				_process.StartInfo.WorkingDirectory = workingDir;
			}
			_process.StartInfo.FileName = fileName;
			_process.StartInfo.Arguments = arguments;

			_process.OutputDataReceived += new DataReceivedEventHandler(outputDataReceived);
			_process.ErrorDataReceived += new DataReceivedEventHandler(errorDataReceived);
			_func = func;
        }


		private bool RunBlocking() {
			try {
				_process.Start();
				_process.BeginOutputReadLine();
				_process.BeginErrorReadLine();
				_process.WaitForExit();
				return _process.ExitCode == 0;
			} catch (Exception ex) {
				_func(ex.Message + "\r\n" + ex.StackTrace, true);

                return false;
			}

		}

		void outputDataReceived(object sender, DataReceivedEventArgs e) {
			if (!string.IsNullOrEmpty(e.Data)) {
				_func(e.Data, false);
			}
		}

		void errorDataReceived(object sender, DataReceivedEventArgs e) {
			if (!string.IsNullOrEmpty(e.Data)) {
				_func(e.Data, true);
			}
		}
	}
}
