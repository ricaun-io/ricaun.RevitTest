﻿using Autodesk.Revit.UI;
using NUnit.Framework;
using ricaun.Revit.UI.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;

[assembly: System.Reflection.AssemblyDescription("{\"TestAsync\":\"RevitTask\",\"TimeOut\":60.0}")]

namespace ricaun.RevitTest.Tests
{
    public class RevitTaskBase
    {
        protected IRevitTask RevitTask { get; private set; }

        [OneTimeSetUp]
        public void Initialize(Func<Func<UIApplication, object>, CancellationToken, Task<object>> revitTask)
        {
            RevitTask = new RevitTaskFuncService(revitTask);
        }

        public class RevitTaskFuncService : IRevitTask
        {
            private Func<Func<UIApplication, object>, CancellationToken, Task<object>> RevitTask;
            public RevitTaskFuncService(Func<Func<UIApplication, object>, CancellationToken, Task<object>> revitTask)
            {
                RevitTask = revitTask;
            }
            public async Task<TResult> Run<TResult>(Func<UIApplication, TResult> function, CancellationToken cancellationToken)
            {
                var result = await RevitTask.Invoke((uiapp) => { return function(uiapp); }, cancellationToken);

                if (result is TResult t)
                    return t;

                return default;
            }
        }
    }

    public class TestsRevitTask : RevitTaskBase
    {
        /// <summary>
        /// Check is Revit API is in context using the ActiveAddInId property.
        /// </summary>
        /// <remarks>
        /// The ActiveAddInId property is null when the Revit API is not in context.
        /// </remarks>
        private bool InContext(UIApplication uiapp)
        {
            return !(uiapp.ActiveAddInId is null);
        }

        [Test] 
        public async Task RunContext()
        {
            var application = await RevitTask.Run((uiapp) => { return uiapp; });
            Assert.IsFalse(InContext(application));

            var inContext = await RevitTask.Run(() => { return InContext(application); });
            Assert.IsTrue(inContext);

            var inContext2 = await RevitTask.Run((uiapp) => { return InContext(uiapp); });
            Assert.IsTrue(inContext2);
        }

        [Test]
        public async Task RunAddInName(UIControlledApplication application)
        {
            Console.WriteLine($"AddInName: {application.ActiveAddInId?.GetAddInName()}");

            await RevitTask.Run(() => { Console.WriteLine($"AddInName: {application.ActiveAddInId?.GetAddInName()}"); });

            await RevitTask.Run((uiapp) => { Console.WriteLine($"AddInName: {uiapp.ActiveAddInId?.GetAddInName()}"); });

            var addInName1 = await RevitTask.Run(() => { return application.ActiveAddInId?.GetAddInName(); });
            Console.WriteLine($"AddInName: {addInName1}");

            var addInName2 = await RevitTask.Run((uiapp) => { return uiapp.ActiveAddInId?.GetAddInName(); });
            Console.WriteLine($"AddInName: {addInName2}");
        }

        [Test]
        public async Task TestPostableCommand_NewProject()
        {
            await RevitTask.Run((uiapp) =>
            {
                uiapp.DialogBoxShowing += DialogBoxShowingForceClose;
                uiapp.PostCommand(RevitCommandId.LookupPostableCommandId(PostableCommand.NewProject));
            });

            await RevitTask.Run((uiapp) =>
            {
                uiapp.DialogBoxShowing -= DialogBoxShowingForceClose;
            });
        }
        private void DialogBoxShowingForceClose(object sender, Autodesk.Revit.UI.Events.DialogBoxShowingEventArgs e)
        {
            var uiapp = sender as UIApplication;
            uiapp.DialogBoxShowing -= DialogBoxShowingForceClose;
            Console.WriteLine($"DialogBoxShowing {e.DialogId}");
            e.OverrideResult((int)TaskDialogResult.Close);
        }

        [Test]
        public async Task TestAsync_Idling()
        {
            await RevitTask.Run((uiapp) =>
            {
                uiapp.Idling += Uiapp_Idling;
            });

            await RevitTask.Run((uiapp) =>
            {
                uiapp.Idling -= Uiapp_Idling;
            });

            Console.WriteLine(Index);
            Assert.GreaterOrEqual(Index, 1);
        }

        /// <summary>
        /// This test gonna check if the `IsTestRunning` is working
        /// </summary>
        [Explicit]
        [TestCase(2)]
        public async Task TestAsync_Idling_Timeout(int length)
        {
            var timeout = 0;
            for (int i = 0; i < length; i++)
            {
                try
                {
                    var source = new System.Threading.CancellationTokenSource(200);
                    var cancellationToken = source.Token;
                    await Task.Delay(500);
                    await RevitTask.Run((uiapp) =>
                    {
                        // This never execute
                        TaskDialog.Show("Show", "Close me");
                    }, cancellationToken);
                }
                catch (Exception) { timeout++; }
            }
            Console.WriteLine(timeout);
            Assert.AreEqual(length, timeout);
        }

        private int Index;
        private void Uiapp_Idling(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs e)
        {
            Index++;
        }

        [Test]
        public async Task TestAsync()
        {
            await Task.Delay(1100);
            Console.WriteLine("Delay");
        }

        [Test]
        public async Task TestAsync(UIApplication uiapp)
        {
            await Task.Delay(100);
            var inContextShouldBeFalse = InContext(uiapp);
            Console.WriteLine(inContextShouldBeFalse);
            Assert.IsFalse(inContextShouldBeFalse);
            var inContext = await RevitTask.Run((app) => { return InContext(app); });
            Console.WriteLine(inContext);
            Assert.IsTrue(inContext);

        }

    }

    public class TestsFuncTaskParameters
    {
        [Test]
        public async Task RevitTaskNone0(Func<Action, Task> revitTask)
        {
            var result = false;
            await revitTask.Invoke(() => { result = true; });
            Assert.IsTrue(result);
        }
        [Test]
        public async Task RevitTaskNone1(Func<Action<UIApplication>, Task> revitTask)
        {
            var result = false;
            await revitTask.Invoke((uiapp) => { result = true; });
            Assert.IsTrue(result);
        }
        [Test]
        public async Task RevitTaskNone2(Func<Func<object>, Task<object>> revitTask)
        {
            var result = (bool)await revitTask.Invoke(() => { return true; });
            Assert.IsTrue(result);
        }
        [Test]
        public async Task RevitTaskNone3(Func<Func<UIApplication, object>, Task<object>> revitTask)
        {
            var result = (bool)await revitTask.Invoke((uiapp) => { return true; });
            Assert.IsTrue(result);
        }

        [Test]
        public async Task RevitTaskToken0(Func<Action, CancellationToken, Task> revitTask)
        {
            var result = false;
            await revitTask.Invoke(() => { result = true; }, CancellationToken.None);
            Assert.IsTrue(result);
        }
        [Test]
        public async Task RevitTaskToken1(Func<Action<UIApplication>, CancellationToken, Task> revitTask)
        {
            var result = false;
            await revitTask.Invoke((uiapp) => { result = true; }, CancellationToken.None);
            Assert.IsTrue(result);
        }
        [Test]
        public async Task RevitTaskToken2(Func<Func<object>, CancellationToken, Task<object>> revitTask)
        {
            var result = (bool) await revitTask.Invoke(() => { return true; }, CancellationToken.None);
            Assert.IsTrue(result);
        }
        [Test]
        public async Task RevitTaskToken3(Func<Func<UIApplication, object>, CancellationToken, Task<object>> revitTask)
        {
            var result = (bool) await revitTask.Invoke((uiapp) => { return true; }, CancellationToken.None);
            Assert.IsTrue(result);
        }
    }
}
