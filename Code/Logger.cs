using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MSGooroo.Deploy {

	public class LogWriter {

		private List<TextWriter> _streams;

		public LogWriter() {
			_streams = new List<TextWriter>();
        }

		public void AddStream(TextWriter watcher) {
			_streams.Add(watcher);
        }

		public void WriteMessage(string message) {
			var entry = string.Format("{0} {1}	msg	{2}",
				DateTime.Now.ToShortDateString(),
				DateTime.Now.ToShortTimeString(),
				message
			);

			foreach (var stream in _streams) {
				stream.WriteLine(entry);
            }

            Console.WriteLine(entry);

		}
		public void WriteError(string message) {
			var entry = string.Format("{0} {1}	error	{2}",
				DateTime.Now.ToShortDateString(),
				DateTime.Now.ToShortTimeString(),
				message
			);

			foreach (var stream in _streams) {
				stream.WriteLine(entry);
			}
			Console.BackgroundColor = ConsoleColor.Red;
			Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(entry);
			Console.ResetColor();

		}
	}

}
