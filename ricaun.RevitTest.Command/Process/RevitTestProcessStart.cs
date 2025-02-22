﻿using ricaun.NUnit.Models;
using ricaun.RevitTest.Command.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ricaun.RevitTest.Command.Process
{
    public class RevitTestProcessStart : ProcessStart
    {
        public RevitTestProcessStart(string processPath) : base(processPath)
        {
        }

        private RevitTestProcessStart SetRevitArgument(string name, object value = null)
        {
            SetArgument(name, value);
            return this;
        }
        public RevitTestProcessStart SetFile(string file) => SetRevitArgument("file", file);
        public RevitTestProcessStart SetRead(bool read = true)
        {
            if (!read) return this;
            return SetRevitArgument("read");
        }
        public RevitTestProcessStart SetLog(bool log = true)
        {
            if (!log) return this;
            return SetRevitArgument("log");
        }
        public RevitTestProcessStart SetRevitVersion(string revitVersion)
        {
            if (string.IsNullOrWhiteSpace(revitVersion) == false)
                SetRevitArgument("version", revitVersion);

            return this;
        }
        public RevitTestProcessStart SetRevitLanguage(string revitLanguage)
        {
            if (string.IsNullOrWhiteSpace(revitLanguage) == false)
                SetRevitArgument("language", revitLanguage);

            return this;
        }
        public RevitTestProcessStart SetOutput(string output) => SetRevitArgument("output", output);
        public RevitTestProcessStart SetOutputConsole() => SetOutput("console");
        public RevitTestProcessStart SetOpen(bool open = true)
        {
            if (!open) return this;
            return SetRevitArgument("open");
        }
        public RevitTestProcessStart SetClose(bool close = true)
        {
            if (!close) return this;
            return SetRevitArgument("close");
        }
        public RevitTestProcessStart SetTimeout(double timeoutMinutes) => SetRevitArgument("timeout", timeoutMinutes);
        public RevitTestProcessStart SetTestFilter(string[] testFilters)
        {
            if (testFilters.Length == 0)
                return this;

            // Convert filter to file to fix the limit size of arguments (Fix: #65)
            if (testFilters.Length > 32)
            {
                var tempFileTestFilters = Path.GetTempFileName();
                File.WriteAllLines(tempFileTestFilters, testFilters);
                return SetRevitArgument("test", tempFileTestFilters);
            }

            return SetRevitArgument("test", testFilters);
        }
        public RevitTestProcessStart SetTestFilter(string testFilter)
        {
            if (string.IsNullOrEmpty(testFilter))
                return this;
            return SetRevitArgument("test", testFilter);
        }
        public RevitTestProcessStart SetDebugger(bool debugger = true)
        {
            if (!debugger) return this;
            return SetRevitArgument("debugger");
        }

        public Task RunExecuteTests(Action<TestModel> actionTest,
            Action<string> consoleAction = null,
            Action<string> debugAction = null,
            Action<string> errorAction = null)
        {
            bool testAssemblyEnabled = true;
            Action<string> outputConsole = (item) =>
            {
                if (string.IsNullOrEmpty(item)) return;

                if (item.StartsWith($"{{\"{nameof(TestAssemblyModel.FileName)}"))
                {
                    debugAction?.Invoke($"OutputConsole: DEBUG: {item.Trim()}");

                    var testAssembly = item.Deserialize<TestAssemblyModel>();

                    if (testAssemblyEnabled == false) return;

                    consoleAction?.Invoke($"TestAssembly: {testAssembly}");
                    foreach (var testModel in testAssembly.Tests.SelectMany(e => e.Tests))
                    {
                        actionTest?.Invoke(testModel);
                    }
                }
                else if (item.StartsWith($"{{\"{nameof(TestModel.Name)}"))
                {
                    debugAction?.Invoke($"OutputConsole: DEBUG: {item.Trim()}");

                    if (item.Deserialize<TestModel>() is TestModel testModel)
                    {
                        actionTest?.Invoke(testModel);
                        testAssemblyEnabled = false;
                    }
                }
                else if (item.StartsWith(" "))
                {
                    consoleAction?.Invoke($"OutputConsole: {item}");
                }
                else
                {
                    debugAction?.Invoke($"OutputConsole: DEBUG: {item}");
                }
            };
            Action<string> outputError = (item) =>
            {
                if (string.IsNullOrEmpty(item)) return;
                errorAction?.Invoke($"OutputConsole: ERROR: {item}");
            };
            return Run(outputConsole, outputError);
        }

        public Task RunReadTests(Action<string[]> actionTests,
            Action<string> consoleAction = null,
            Action<string> debugAction = null,
            Action<string> errorAction = null)
        {
            Action<string> outputError = (item) =>
            {
                if (string.IsNullOrEmpty(item)) return;
                errorAction?.Invoke($"OutputConsole: ERROR: {item}");
            };

            Action<string> outputConsole = (item) =>
            {
                if (string.IsNullOrEmpty(item)) return;

                debugAction?.Invoke($"OutputConsole: {item.Trim()}");

                if (item.StartsWith("["))
                {
                    if (item.Deserialize<string[]>() is string[] tests)
                    {
                        actionTests?.Invoke(tests);
                        debugAction?.Invoke($"OutputConsole: Deserialize {tests.Length} TestNames");
                    }
                }
            };
            return Run(outputConsole, outputError);
        }
    }
}