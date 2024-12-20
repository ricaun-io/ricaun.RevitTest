﻿using EnvDTE;
using ricaun.RevitTest.Command.Utils;
using System.Diagnostics;
using System.Linq;

namespace ricaun.RevitTest.Console.Revit.Utils
{
    public static class VisualStudioDebugUtils
    {
        public static DTE DTE { get; set; } = GetDTE();

        public static System.Diagnostics.Process AttachDTE(this System.Diagnostics.Process process, DTE dte = null)
        {
            if (DebuggerUtils.IsDebuggerAttached == false) return process;
            dte = dte ?? DTE;
            try
            {
                var processDTE = process.GetProcessDTE(dte);
                processDTE?.Attach();
                Debug.WriteLine($"DTE.Debugger.Attach[{process.Id}]: {dte?.Name} {dte?.Version}");
            }
            catch { }
            return process;
        }

        public static System.Diagnostics.Process DetachDTE(this System.Diagnostics.Process process, DTE dte = null)
        {
            if (DebuggerUtils.IsDebuggerAttached == false) return process;
            dte = dte ?? DTE;
            try
            {
                var processDTE = process.GetProcessDTE(dte);
                processDTE?.Detach(false);
                Debug.WriteLine($"DTE.Debugger.Detach[{process.Id}]: {dte?.Name} {dte?.Version}");
            }
            catch { }
            return process;
        }

        internal static EnvDTE.Process GetProcessDTE(this System.Diagnostics.Process process, DTE dte)
        {
            return dte.Debugger.LocalProcesses.OfType<EnvDTE.Process>().FirstOrDefault(e => e.ProcessID == process.Id);
        }

        internal static DTE GetDTE(int versionMax = 23, int versionMin = 9)
        {
            for (int i = versionMax; i >= versionMin; i--)
            {
                try
                {
                    var objectDTE = $"VisualStudio.DTE.{i}.0";
                    var dte = GetActiveDTEObject(objectDTE);
                    if (dte is not null)
                    {
                        Debug.WriteLine($"DTE.Debugger: {objectDTE}");
                        return dte;
                    }
                }
                catch { }
            }
            return null;
        }

        private static DTE GetActiveDTEObject(string objectDTE)
        {
#if NETFRAMEWORK
            return (DTE)System.Runtime.InteropServices.Marshal.GetActiveObject(objectDTE);
#elif NETCOREAPP
            return (DTE)MarshalUtils.GetActiveObject(objectDTE);
#endif
        }

        public static string GetName()
        {
            return DTE.GetName();
        }

        public static string GetName(this DTE dte)
        {
            if (dte is null) return null;
            return string.Format("{0} {1}", dte.Name, dte.Version);
        }
    }
}