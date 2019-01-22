using System;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Xb.App
{
    public class Process : IDisposable
    {
        #region "Static"

        private static readonly bool _isWindows
            = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        private static readonly bool _isMac
            = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        private static readonly bool _isLinux
            = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        private static Encoding _consoleEncoding = null;

        private static bool _isEncodingInited = false;

        private static void EnsureEncoding()
        {
            if (!Xb.App.Process._isEncodingInited)
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Xb.App.Process._isEncodingInited = true;
            }
        }

        /// <summary>
        /// Encoding of console output.
        /// </summary>
        public static Encoding ConsoleEncoding
        {
            get
            {
                if (Xb.App.Process._consoleEncoding == null)
                {
                    Xb.App.Process.EnsureEncoding();

                    if (Xb.App.Process._isWindows)
                    {
                        Xb.App.Process._consoleEncoding
                            = Encoding.GetEncoding("shift_jis");
                    }
                    else if (Xb.App.Process._isMac)
                    {
                        Xb.App.Process._consoleEncoding = new UTF8Encoding(false);
                    }
                    else if (Xb.App.Process._isLinux)
                    {
                        Xb.App.Process._consoleEncoding = new UTF8Encoding(false);
                    }
                    else
                    {
                        Xb.App.Process._consoleEncoding = new UTF8Encoding(false);
                    }
                }

                return Xb.App.Process._consoleEncoding;
            }
            set
            {
                Xb.App.Process._consoleEncoding = value;
            }
        }

        /// <summary>
        /// Get instance.
        /// </summary>
        /// <returns></returns>
        public static Xb.App.Process GetNew(string fileName, string arguments = null)
        {
            var result = new Xb.App.Process();
            result.FileName = fileName;
            result.Arguments = string.IsNullOrEmpty(arguments)
                ? string.Empty
                : arguments;

            return result;
        }

        /// <summary>
        /// Start process and get instance.
        /// </summary>
        /// <returns></returns>
        public static Xb.App.Process Create(
            string fileName, 
            string arguments = null, 
            bool isShowWindow = true,
            string workingDirectory = null
        )
        {
            var result = Xb.App.Process.GetNew(fileName, arguments);

            if (!string.IsNullOrEmpty(workingDirectory))
                result.WorkingDirectory = workingDirectory;

            result.Start(isShowWindow);

            return result;
        }

        /// <summary>
        /// Get Xb-Process objects those name matches.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Xb.App.Process[] FindAllProcesses(string name = null)
        {
            var processes = System.Diagnostics.Process.GetProcesses();
            var list = new List<Xb.App.Process>();

            foreach (var process in processes)
            {
                var processName = string.Empty;
                var moduleName = string.Empty;
                var fileName = string.Empty;

                try
                {
                    processName = process.ProcessName;
                }
                catch (Exception) { }

                try
                {
                    moduleName = process.MainModule.ModuleName;
                }
                catch (Exception) { }

                try
                {
                    fileName = process.MainModule.FileName;
                }
                catch (Exception) { }

                if (
                    !string.IsNullOrEmpty(name)
                    && processName.IndexOf(name) == -1
                    && moduleName.IndexOf(name) == -1
                    && fileName.IndexOf(name) == -1
                )
                    continue;

                var xProc = Xb.App.Process.GetNew(fileName);
                xProc.DotnetProcess = process;
                list.Add(xProc);
            }

            return list.ToArray();
        }

        /// <summary>
        /// Get the first Xb-Process object whose name matched.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Xb.App.Process FindProcess(string name)
        {
            var processes = System.Diagnostics.Process.GetProcesses();

            foreach (var process in processes)
            {
                var processName = string.Empty;
                var moduleName = string.Empty;
                var fileName = string.Empty;

                try
                {
                    processName = process.ProcessName;
                }
                catch (Exception) { }

                try
                {
                    moduleName = process.MainModule.ModuleName;
                }
                catch (Exception) { }

                try
                {
                    fileName = process.MainModule.FileName;
                }
                catch (Exception) { }

                if (
                    !string.IsNullOrEmpty(name)
                    && processName.IndexOf(name) == -1
                    && moduleName.IndexOf(name) == -1
                    && fileName.IndexOf(name) == -1
                )
                    continue;

                var result = Xb.App.Process.GetNew(fileName);
                result.DotnetProcess = process;

                return result;
            }

            return null;
        }

        /// <summary>
        /// Execute binary, and get text-result.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static Result GetProcessResult(
            string fileName, 
            string arguments = null,
            string workingDirectory = null)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException();

            using (var instance = Xb.App.Process.Create(fileName, arguments, false, workingDirectory))
            {
                return instance.GetResult();
            }
        }

        /// <summary>
        /// Execute binary on async, and get text-result.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static async Task<Result> GetProcessResultAsync(
            string fileName,
            string arguments = null,
            string workingDirectory = null,
            int timeoutSec = 10)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException();

            using (var instance = Xb.App.Process.Create(fileName, arguments, false, workingDirectory))
            {
                return await instance.GetResultAsync(timeoutSec);
            }
        }

        /// <summary>
        /// Execute console command, and get text-result.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Result GetConsoleResult(
            string command,
            string workingDirectory = null
        )
        {
            if (string.IsNullOrEmpty(command))
                throw new ArgumentNullException();

            using (var instance = Xb.App.Process.GetNewConsoleProcess(command, workingDirectory))
            {
                instance.Start(false);

                return instance.GetResult();
            }
        }

        /// <summary>
        /// Execute console command on async, and get text-result.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static async Task<Result> GetConsoleResultAsync(
            string command,
            string workingDirectory = null,
            int timeoutSec = 10
        )
        {
            if (string.IsNullOrEmpty(command))
                throw new ArgumentNullException();

            using (var instance = Xb.App.Process.GetNewConsoleProcess(command, workingDirectory))
            {
                instance.Start(false);

                return await instance.GetResultAsync(timeoutSec);
            }
        }

        private static Xb.App.Process GetNewConsoleProcess(
            string command,
            string workingDirectory = null
        )
        {
            var instance = Xb.App.Process.GetNew(null);

            var fileName = string.Empty;
            var arguments = string.Empty;

            if (Xb.App.Process._isWindows)
            {
                // windows
                // 何もしないのが最も感覚的にすんなり行く

                // cmd.exeの特殊文字
                // https://thinca.hatenablog.com/entry/20100210/1265813598
                //var escapedArgs = command
                //    .Replace("&", "^&")
                //    .Replace("<", "^<")
                //    .Replace(">", "^>")
                //    .Replace("(", "^(")
                //    .Replace(")", "^)")
                //    .Replace("%", "^%")
                //    .Replace("\"", "^\"");

                fileName = "cmd.exe";
                arguments = $"/c {command}";
            }
            else if (Xb.App.Process._isMac)
            {
                // mac
                var escapedArgs = command.Replace("\"", "\\\"");
                fileName = "/bin/bash";
                arguments = $"-c \"{escapedArgs}\"";
            }
            else if (Xb.App.Process._isLinux)
            {
                // linux
                var escapedArgs = command.Replace("\"", "\\\"");
                fileName = "/bin/bash";
                arguments = $"-c \"{escapedArgs}\"";
            }
            else
            {
                throw new NotSupportedException("Unknown OS Platform");
            }

            instance.FileName = fileName;
            instance.Arguments = string.IsNullOrEmpty(arguments)
                ? string.Empty
                : arguments;
            instance.Encoding = Xb.App.Process.ConsoleEncoding;

            if (!string.IsNullOrEmpty(workingDirectory))
                instance.WorkingDirectory = workingDirectory;

            return instance;
        }

        #endregion


        public class Result
        {
            public static Result CreateSucceeded(string message)
            {
                return new Result(true, message);
            }
            public static Result CreateError(string message)
            {
                return new Result(false, message);
            }

            public readonly string Message;
            public readonly bool Succeeded;

            private Result(bool succeeded, string message)
            {
                this.Succeeded = succeeded;
                this.Message = message;
            }
        }

        /// <summary>
        /// Working directory
        /// </summary>
        public string WorkingDirectory { get; set; } = string.Empty;

        /// <summary>
        /// FileName(full-path)
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Arguments
        /// </summary>
        public string Arguments { get; set; } = string.Empty;

        /// <summary>
        /// Path, Args, Result text encoding
        /// </summary>
        public Encoding Encoding { get; set; } = new UTF8Encoding(false);

        /// <summary>
        /// Process ID
        /// </summary>
        public int Id
        {
            get
            {
                if (this.DotnetProcess == null)
                    return -1;

                return this.DotnetProcess.Id;
            }
        }

        /// <summary>
        /// Process name
        /// </summary>
        public string ProcessName
        {
            get
            {
                if (this.DotnetProcess == null)
                    return string.Empty;

                return this.DotnetProcess.ProcessName;
            }
        }

        /// <summary>
        /// Running time
        /// </summary>
        public TimeSpan TotalProcessorTime
        {
            get
            {
                if (this.DotnetProcess == null)
                    return TimeSpan.Parse("0");

                try
                {
                    return this.DotnetProcess.TotalProcessorTime;
                }
                catch (Exception) { }

                return TimeSpan.Parse("0");
            }
        }

        /// <summary>
        /// Memory usage
        /// </summary>
        public long WorkingSet64
        {
            get
            {
                if (this.DotnetProcess == null)
                    return -1;

                return this.DotnetProcess.WorkingSet64;
            }
        }


        /// <summary>
        /// .Net process object
        /// </summary>
        private System.Diagnostics.Process DotnetProcess = null;


        /// <summary>
        /// Constructor
        /// </summary>
        private Process()
        {
        }

        /// <summary>
        /// Execute process
        /// </summary>
        /// <param name="isShowWindow"></param>
        public void Start(bool isShowWindow = true)
        {
            if (this.DotnetProcess != null)
                throw new InvalidOperationException("Already Started.");

            Xb.App.Process.EnsureEncoding();

            this.DotnetProcess = new System.Diagnostics.Process();

            this.DotnetProcess.StartInfo.UseShellExecute = false;
            this.DotnetProcess.StartInfo.RedirectStandardOutput = true;
            this.DotnetProcess.StartInfo.RedirectStandardError = true;
            this.DotnetProcess.StartInfo.RedirectStandardInput = false;
            this.DotnetProcess.StartInfo.StandardOutputEncoding = this.Encoding;

            this.DotnetProcess.StartInfo.CreateNoWindow = !isShowWindow;
            this.DotnetProcess.StartInfo.WorkingDirectory = this.WorkingDirectory;
            this.DotnetProcess.StartInfo.FileName = this.FileName;
            this.DotnetProcess.StartInfo.Arguments = this.Arguments;

            var result = this.DotnetProcess.Start();
        }

        /// <summary>
        /// Get text-result.
        /// </summary>
        /// <returns></returns>
        public Result GetResult()
        {
            if (this.DotnetProcess == null)
                throw new InvalidOperationException("Process Not Started.");

            var output = this.DotnetProcess.StandardOutput.ReadToEnd();
            var error = this.DotnetProcess.StandardError.ReadToEnd();

            this.DotnetProcess.WaitForExit();
            this.DotnetProcess.Close();

            var result = (string.IsNullOrEmpty(output) && !string.IsNullOrEmpty(error))
                ? Result.CreateError(error)
                : Result.CreateSucceeded(output);

            return result;
        }

        /// <summary>
        /// Get text-result on async.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task<Result> GetResultAsync(int timeout = 10)
        {
            if (this.DotnetProcess == null)
                throw new InvalidOperationException("Process Not Started.");

            if (timeout < 0)
                timeout = 0;

            var tasks = new List<Task>();
            Result result = null;

            var timeoutTask = Task.Delay(timeout * 1000);
            timeoutTask.ConfigureAwait(false);
            tasks.Add(timeoutTask);

            var mainTask = Task.Run(() =>
            {
                try
                {
                    result = this.GetResult();
                }
                catch (Exception)
                {
                }
            });
            mainTask.ConfigureAwait(false);

            tasks.Add(mainTask);

            await Task.WhenAny(tasks);

            if (result == null)
                result = Result.CreateError("No Response");

            return result;
        }

        #region IDisposable Support
        private bool IsDisposed = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    try
                    {
                        //if (this._process != null && !this._process.HasExited)
                        //    this._process.Kill();

                        if (this.DotnetProcess != null)
                        {
                            try
                            {
                                this.DotnetProcess.Kill();
                            }
                            catch (Exception) { }

                            try
                            {
                                this.DotnetProcess.Dispose();
                            }
                            catch (Exception) { }
                        }

                        this.DotnetProcess = null;
                    }
                    catch (Exception){ }


                }

                this.IsDisposed = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }
        #endregion
    }
}
