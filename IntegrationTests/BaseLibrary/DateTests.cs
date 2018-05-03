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
        public void ShouldParseUSformat()
        {
            var date = new Date(new Arguments { "10/31/2010 08:00" });
            Assert.AreEqual(DateTime.Parse("2010-10-31 08:00"), date.ToDateTime());
        }

        [TestMethod]
        public void ShouldGiveISOString()
        {
            var expected = "1970-01-01T00:00:00.000Z";
            var date = new Date(new Arguments { "1970" });
            Assert.AreEqual(date.toISOString(), expected);
        }

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
            var timezones = TimeZoneInfo.GetSystemTimeZones().Where(x => x.BaseUtcOffset.Ticks == 10 * 3600 * 10000000L).ToArray();
            var timezone = timezones.First(x => x.Id.IndexOf("AUS", StringComparison.OrdinalIgnoreCase) != -1);
            Date.CurrentTimeZone = timezone;

            var firstDate = new Date(new Arguments { 953996400000 });
            var secondDate = new Date(new Arguments { 954000000000 }); // the thing which geek will never have
            var thirdDate = new Date(new Arguments { 954003600000 });

            Assert.IsTrue(firstDate.ToString().StartsWith("Sun Mar 26 2000 02:00:00 GMT+1100"));
            Assert.IsTrue(secondDate.ToString().StartsWith("Sun Mar 26 2000 02:00:00 GMT+1000"));
            Assert.IsTrue(thirdDate.ToString().StartsWith("Sun Mar 26 2000 03:00:00 GMT+1000"));
        }
    }
}
