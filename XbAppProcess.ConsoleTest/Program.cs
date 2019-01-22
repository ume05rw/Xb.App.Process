using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XbUtilProcess.ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            GetConsoleResultTest();

            GetConsoleResultAsyncTest().GetAwaiter().GetResult();

            GetProcessResultTest1();

            GetProcessResultTest2();

            GetProcessResultAsyncTest().GetAwaiter().GetResult();

            //MultipleExecFailureTest();

            //GetAllProcesses();

            //GetAllProcesses2();

            //DisposeTest1();

            //FindAndDisposeTest();

            var a = 1;
        }

        private static void GetConsoleResultTest()
        {
            var result = Xb.App.Process.GetConsoleResult("echo Hello!");
            Console.WriteLine($"task1.Result: {result.Message}");
        }

        private static async Task<bool> GetConsoleResultAsyncTest()
        {
            var result = await Xb.App.Process.GetConsoleResultAsync("echo Hello!", null, 0);
            Console.WriteLine($"task1.Result: {result.Message}");

            var result2 = await Xb.App.Process.GetConsoleResultAsync("echo Hello!", null, 100);
            Console.WriteLine($"task1.Result: {result2.Message}");

            return true;
        }

        private static void GetProcessResultTest1()
        {
            var protocolCode = "NEC1";
            var deviceId = "64";
            var subDeviceId = "-1";
            var functionCode = "0";

            var baseDir = @"C:\Program Files (x86)\IrScrutinizer\";
            var fileName = @"C:\Program Files (x86)\Java\jdk1.8.0_172\bin\java.exe";
            var arguments = ""
                + $"-splash: \"-Djava.library.path={baseDir}Windows-x86\" "
                + $"-jar \"{baseDir}IrScrutinizer.jar\" "
                + $"--irpmaster -c \"{baseDir}IrpProtocols.ini\" "
                + $"-p -n {protocolCode} D={deviceId} "
                + $"S={subDeviceId} F={functionCode}";

            var consoleResult = Xb.App.Process.GetProcessResult(fileName, arguments);
            Console.WriteLine(consoleResult.Message);
        }

        private static void GetProcessResultTest2()
        {
            var protocolCode = "NEC";
            var deviceId = "64";
            var subDeviceId = "-1";
            var functionCode = "0";

            var baseDir = @"C:\Program Files (x86)\IrScrutinizer\";
            var fileName = @"C:\Program Files (x86)\Java\jdk1.8.0_172\bin\java.exe";
            var arguments = ""
                + $"-splash: \"-Djava.library.path={baseDir}Windows-x86\" "
                + $"-jar \"{baseDir}IrScrutinizer.jar\" "
                + $"--irpmaster -c \"{baseDir}IrpProtocols.ini\" "
                + $"-p -n {protocolCode} D={deviceId} "
                + $"S={subDeviceId} F={functionCode}";

            var consoleResult = Xb.App.Process.GetProcessResult(fileName, arguments);

            // ouput error message
            Console.WriteLine(consoleResult.Message);
        }

        private static async Task<bool> GetProcessResultAsyncTest()
        {
            var protocolCode = "NEC1";
            var deviceId = "64";
            var subDeviceId = "-1";
            var functionCode = "0";

            var baseDir = @"C:\Program Files (x86)\IrScrutinizer\";
            var fileName = @"C:\Program Files (x86)\Java\jdk1.8.0_172\bin\java.exe";
            var arguments = ""
                + $"-splash: \"-Djava.library.path={baseDir}Windows-x86\" "
                + $"-jar \"{baseDir}IrScrutinizer.jar\" "
                + $"--irpmaster -c \"{baseDir}IrpProtocols.ini\" "
                + $"-p -n {protocolCode} D={deviceId} "
                + $"S={subDeviceId} F={functionCode}";

            var consoleResult = await Xb.App.Process.GetProcessResultAsync(fileName, arguments, null, 0);
            Console.WriteLine(consoleResult.Message);

            var consoleResult2 = await Xb.App.Process.GetProcessResultAsync(fileName, arguments, null, 4);
            Console.WriteLine(consoleResult2.Message);

            return true;
        }

        private static void MultipleExecFailureTest()
        {
            var tasks = new List<Task>();
            for (var i = 0; i < 100; i++)
            {
                var task = Task.Run(() =>
                {
                    var result = Xb.App.Process.GetConsoleResult($"echo Hello{i}");
                    Console.WriteLine($"result: {result.Message}");
                });
                tasks.Add(task);
            }

            Task.WhenAll(tasks)
                .GetAwaiter()
                .GetResult();
        }



        private static void GetAllProcesses()
        {
            var procs = Xb.App.Process.FindAllProcesses();
            foreach (var proc in procs)
            {
                Console.WriteLine($"-----");
                Console.WriteLine($"{proc.Id}");
                Console.WriteLine($"{proc.ProcessName}");
                Console.WriteLine($"{proc.FileName}");
                Console.WriteLine($"{proc.TotalProcessorTime}");
                Console.WriteLine($"{proc.WorkingSet64}");
            }
        }

        private static void GetAllProcesses2()
        {
            var procs = Xb.App.Process.FindAllProcesses("svchost");
            foreach (var proc in procs)
            {
                Console.WriteLine($"-----");
                Console.WriteLine($"{proc.Id}");
                Console.WriteLine($"{proc.ProcessName}");
                Console.WriteLine($"{proc.FileName}");
                Console.WriteLine($"{proc.TotalProcessorTime}");
                Console.WriteLine($"{proc.WorkingSet64}");
            }
        }

        private static void DisposeTest1()
        {
            using (var proc = Xb.App.Process.Create("notepad.exe"))
            {
                Console.WriteLine("How?");
            }

            var a = 1;
        }

        private static void FindAndDisposeTest()
        {
            var proc1 = Xb.App.Process.Create("notepad.exe");

            var proc2 = Xb.App.Process.FindProcess("notepad");

            Console.WriteLine($"-----");
            Console.WriteLine($"{proc2.Id}");
            Console.WriteLine($"{proc2.ProcessName}");
            Console.WriteLine($"{proc2.FileName}");
            Console.WriteLine($"{proc2.TotalProcessorTime}");
            Console.WriteLine($"{proc2.WorkingSet64}");

            Console.WriteLine($"How?: {(proc1.Id == proc2.Id)}");

            proc2.Dispose();

            var a = 1;

        }
    }
}
