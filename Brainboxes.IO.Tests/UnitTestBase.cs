using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brainboxes.IO.Tests
{
    public class UnitTestBase
    {
        public ConnectionPool cp;

        [TestInitialize]
        public void TestInit()
        {
            cp = new ConnectionPool();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            cp.CloseAll();
        }

    }
}
