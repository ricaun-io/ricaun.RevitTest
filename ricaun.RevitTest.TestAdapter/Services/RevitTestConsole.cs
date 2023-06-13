﻿using ricaun.RevitTest.TestAdapter.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace ricaun.RevitTest.TestAdapter.Services
{
    public class RevitTestConsole : IDisposable
    {
        private readonly string applicationPath;

        private string ValidadeApplication(string applicationPath)
        {
            if (string.IsNullOrWhiteSpace(applicationPath))
                return null;

            if (ApplicationUtils.Download(applicationPath, out string directory))
            {
                AdapterLogger.Logger.Info($"Application Download: {applicationPath}");

                var applicationName = Path.ChangeExtension(Path.GetFileName(applicationPath), "exe");
                var applicationNewPath = Path.Combine(directory, applicationName);

                if (File.Exists(applicationNewPath))
                {
                    var fileVersionInfo = FileVersionInfo.GetVersionInfo(applicationNewPath);
                    var productVersion = $"{fileVersionInfo.ProductVersion}";
                    AdapterLogger.Logger.Warning($"Application Process: {Path.GetFileName(applicationNewPath)} {productVersion}");
                    return applicationNewPath;
                }
            }

            return null;
        }

        public RevitTestConsole(string application = null)
        {
            applicationPath = ValidadeApplication(application);
            if (applicationPath is null)
            {
                var directory = ApplicationUtils.CreateTemporaryDirectory(Properties.Resources.ricaun_RevitTest_Console_Name);
                var file = Path.Combine(directory, Properties.Resources.ricaun_RevitTest_Console_Name);
                applicationPath = Properties.Resources.ricaun_RevitTest_Console.CopyToFile(file);
            }
            AdapterLogger.Logger.Info($"Application: {Path.GetFileName(applicationPath)}");
        }

        public async Task RunTestAction(
            string file,
            int version = 0,
            bool revitOpen = false,
            bool revitClose = false,
            Action<string> consoleAction = null,
            params string[] filter)
        {
            await new RevitTestProcessStart(applicationPath)
                .SetFile(file)
                .SetRevitVersion(version)
                .SetOutputConsole()
                .SetOpen(revitOpen)
                .SetClose(revitClose)
                .SetLog()
                .SetTestFilter(filter)
                .SetDebugger(System.Diagnostics.Debugger.IsAttached)
                .Run(consoleAction);
        }

        public async Task<string[]> RunTestRead(string file)
        {
            var read = await new RevitTestProcessStart(applicationPath)
                .SetFile(file)
                .SetOutputConsole()
                .SetRead()
                .Run();

            var testNames = read.Deserialize<string[]>();
            return testNames;
        }

        public void Dispose()
        {
            try
            {
                AdapterLogger.Logger.Debug($"Dispose: {Path.GetFileName(applicationPath)}");
                if (File.Exists(applicationPath))
                {
                    File.Delete(applicationPath);
                    Directory.Delete(Path.GetDirectoryName(applicationPath), true);
                }
            }
            catch { }
        }
    }
}