using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XbUtilProcessTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var result = Xb.App.Process.GetConsoleResult("echo Hello!");
            Assert.AreEqual("Hello!\r\n", result);


            var task = Xb.App.Process.GetConsoleResultAsync("explorer.exe \"C:\\dev\"");
            task.ConfigureAwait(false);
            task.Wait();
            Assert.AreEqual(string.Empty, task.Result);
        }
    }
}
