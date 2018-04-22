using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;

namespace IntegrationTests.BaseLibrary
{
    [TestClass]
    public class DateTests
    {
        [TestMethod]
        public void NewDateShouldContainCurrentTime()
        {
            var dateTime = DateTime.Now;
            var date = new Date();

            Assert.AreEqual(date.getDate(), dateTime.Day);
            Assert.AreEqual((int)date.getMonth().Value + 1, dateTime.Month);
            Assert.AreEqual(date.getYear(), dateTime.Year);

            Assert.AreEqual(date.getHours(), dateTime.Hour);
            Assert.AreEqual(date.getMinutes(), dateTime.Minute);
            Assert.AreEqual(date.getSeconds(), dateTime.Second);
        }
    }
}
