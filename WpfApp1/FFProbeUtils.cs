using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public static class FFProbeUtils
    {
		public static double GetDurationInSeconds(string fileName)
		{
			string ffprobeFindDur = $"/c ffprobe.exe -i \"{fileName}\" -show_format | find \"duration\" ";
			Process process = new Process()
			{
				StartInfo = new ProcessStartInfo()
				{
					FileName = Environment.ExpandEnvironmentVariables("%SystemRoot%") + @"\System32\cmd.exe",
					Arguments = ffprobeFindDur,
					UseShellExecute = false,
					WindowStyle = ProcessWindowStyle.Hidden,
					CreateNoWindow = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true
				}
			};
			process.Start();
			string output = process.StandardOutput.ReadToEnd();
			string[] split = output.Trim().Split('=');
			Double.TryParse(split[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double result);
			process.WaitForExit();
			return result;
		}

		public static string FormatDuration(double durationInS)
		{
			TimeSpan duration = TimeSpan.FromSeconds(durationInS);
			return duration.ToString(@"hh\:mm\:ss\.fff");
		}

		public static string GetFormattedDuration(string filename)
		{
			return FormatDuration(GetDurationInSeconds(filename));
		}

		public static double FromFormattedString(string formatted)
		{
			TimeSpan.TryParse(formatted, out TimeSpan res);
			return res.TotalSeconds;
		}
	}
}
