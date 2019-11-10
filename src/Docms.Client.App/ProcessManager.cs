using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;

namespace Docms.Client.App
{
    public class ProcessManager
    {
        private static readonly string processName = Process.GetCurrentProcess().MainModule.FileName;

        public static Process Execute(string arguments)
        {
            var filename = Path.GetFullPath(processName);
            var startInfo = new ProcessStartInfo
            {
                Arguments = arguments,
                FileName = filename,
                UseShellExecute = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(filename)
            };
            if (Environment.GetEnvironmentVariable("_DOCMS_DEBUG") != null)
            {
                startInfo.WindowStyle = ProcessWindowStyle.Normal;
            }
            else
            {
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            }
            return Process.Start(startInfo);
        }

        public static Process[] FindProcess(string argument)
        {
            var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(processName));
            var list = new List<Process>();
            foreach (var process in processes)
            {
                using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
                {
                    foreach (ManagementBaseObject managementBaseObject in managementObjectSearcher.Get())
                    {
                        ManagementObject managementObject = (ManagementObject)managementBaseObject;
                        if (managementObject["CommandLine"] is string commandText)
                        {
                            int num = commandText.IndexOf(processName, StringComparison.OrdinalIgnoreCase) + processName.Length;
                            var filename = commandText.Substring(0, num).Trim(new[] { '\\' });
                            var arguments = commandText.Substring(num + 1).Trim();
                            if (arguments.Equals(argument, StringComparison.InvariantCultureIgnoreCase))
                            {
                                list.Add(process);
                            }
                        }
                    }
                }
            }

            return list.ToArray();
        }
    }
}
