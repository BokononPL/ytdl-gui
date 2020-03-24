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
			Process process = new Process();
			process.StartInfo.FileName = Environment.ExpandEnvironmentVariables("%SystemRoot%") + @"\System32\cmd.exe";
			process.StartInfo.Arguments = ffprobeFindDur;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
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

	}
}
