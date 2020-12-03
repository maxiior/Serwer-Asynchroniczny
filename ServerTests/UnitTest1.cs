using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServerLibrary;
using ServerAsyn;
using System.Net;

namespace ServerTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestowanieBlednegoNumeruPortu()
        {
            try
            {
                Server<GACServerProtocol> server = new ServerTAP<GACServerProtocol>(IPAddress.Parse("127.0.0.1"), 48);
                Assert.Fail();
            }
            catch (AssertFailedException)
            {
                Assert.Fail();
            }
            catch (Exception e)
            {

            }
        }

        [TestMethod]
        public void TestowanieBlednegoNumeruIP()
        {
            try
            {
                Server<GACServerProtocol> server = new ServerTAP<GACServerProtocol>(IPAddress.Parse("127.0.0"), 2048);
                Assert.Fail();
            }
            catch (AssertFailedException)
            {
                Assert.Fail();
            }
            catch (Exception e)
            {

            }
        }

        [TestMethod]
        public void TestowanieMetodyCheckIfInDb()
        {
            try
            {
                Sqlite sql = new Sqlite();
                sql.AddWin("kr");
                Assert.Fail();
            }
            catch (AssertFailedException)
            {
                Assert.Fail();
            }
            catch (Exception e)
            {

            }
        }
    }
}
