﻿using ricaun.RevitTest.Command;

namespace ricaun.RevitTest.Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var result = RunTest.ParseArguments<Revit.RevitRunTestService>(args);

#if DEBUG
            if (!result)
            {
                Revit.Utils.RevitDebugUtils.ProcessServerSelect();
            }
#endif
        }
    }
}