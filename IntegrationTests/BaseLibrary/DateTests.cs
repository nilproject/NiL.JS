using System;
using System.Linq;
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

        [TestMethod]
        public void ShouldCorrectHandleSwitchFromDstToStandard_SidneyTimeZone()
        {
            var timezone = TimeZoneInfo.GetSystemTimeZones()
                .First(x => x.BaseUtcOffset.Ticks == 10 * 3600 * 10000000L
                       && x.Id.Contains("AUS"));
            Date.CurrentTimeZone = timezone;

            var d1 = new Date(new Arguments { 953996400000 });
            var d2 = new Date(new Arguments { 954000000000 });
            var d3 = new Date(new Arguments { 954003600000 });

            Assert.IsTrue(d1.ToString().StartsWith("Sun Mar 26 2000 02:00:00 GMT+1100"));
            Assert.IsTrue(d2.ToString().StartsWith("Sun Mar 26 2000 02:00:00 GMT+1000"));
            Assert.IsTrue(d3.ToString().StartsWith("Sun Mar 26 2000 03:00:00 GMT+1000"));
        }
    }
}
