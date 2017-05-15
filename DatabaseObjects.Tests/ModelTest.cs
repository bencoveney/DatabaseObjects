using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DatabaseObjects.Tests
{
    [TestClass]
    public class ModelTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            Model.LoadFromDatabase("Test");
        }
    }
}
