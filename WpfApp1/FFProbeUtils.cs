using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WpfApp1
{
    public static class FFProbeUtils
    {
		public static JObject GetFileInfo(string fileName)
		{
			//string Cmd = $"/C ffprobe -v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{fileName}\" 2>&1 ";
			string Cmd = $"/C ffprobe -v quiet -print_format json=compact=1 -show_format \"{fileName}\" 2>&1 ";
			Process process = new Process()
			{
				StartInfo = new ProcessStartInfo()
				{
					FileName = "cmd.exe",
					Arguments = Cmd,
					UseShellExecute = false,
					WindowStyle = ProcessWindowStyle.Hidden,
					CreateNoWindow = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true
				}
			};
			process.Start();
			JObject fileinfo = JObject.Parse(process.StandardOutput.ReadToEnd());
			process.WaitForExit();
			return fileinfo;
		}

		public static double FromFormattedString(string formatted)
		{
			return TimeSpan.ParseExact(formatted, @"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture).TotalSeconds;
		}
	}
}
