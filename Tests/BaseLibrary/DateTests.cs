﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Extensions;

namespace Tests.BaseLibrary;

[TestClass]
public class DateTests
{
    private static readonly TimeZoneInfo TestMoscowTimezone = TimeZoneInfo.CreateCustomTimeZone(
        "Test Moscow timezone", 
        new TimeSpan(3 * 3600 * 10000000L), 
        "Test Moscow timezone",
        "Test Moscow timezone",
        "Test Moscow daylight timezone",
        new TimeZoneInfo.AdjustmentRule[]
        {
            TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                default,
                new DateTime(2010, 12, 31),
                TimeSpan.FromHours(1),
                TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1,1,1,2,0,0), 3, 5, DayOfWeek.Sunday),
                TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1,1,1,3,0,0), 10, 5, DayOfWeek.Sunday)),
        });
    private static readonly TimeZoneInfo TestAustraliaTimezone = TimeZoneInfo.CreateCustomTimeZone(
        "Test Australia timezone",
        new TimeSpan(10 * 3600 * 10000000L),
        "Test Australia timezone",
        "Test Australia timezone",
        "Test Australia daylight timezone",
        new TimeZoneInfo.AdjustmentRule[]
        {
            TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                default,
                new DateTime(2007, 12, 31),
                TimeSpan.FromHours(1),
                TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1,1,1,2,0,0), 10, 5, DayOfWeek.Sunday),
                TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1,1,1,3,0,0), 3, 5, DayOfWeek.Sunday)),

            TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                new DateTime(2008, 1, 1),
                new DateTime(9999, 12, 31),
                TimeSpan.FromHours(1),
                TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1,1,1,2,0,0), 10, 5, DayOfWeek.Sunday),
                TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1,1,1,3,0,0), 4, 5, DayOfWeek.Sunday)),
        });

    [TestMethod]
    public void ShouldParseUSformat()
    {
        Context.CurrentGlobalContext.CurrentTimeZone = TimeZoneInfo.Local;
        var date = new Date(new Arguments { "10/31/2010 08:00" });
        Assert.AreEqual(DateTime.Parse("2010-10-31 08:00").ToUniversalTime(), date.ToDateTime().ToUniversalTime());
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
        Assert.AreEqual(date.getYear(), dateTime.Year - 1900);

        Assert.AreEqual(date.getHours(), dateTime.Hour);
        Assert.AreEqual(date.getMinutes(), dateTime.Minute);
        Assert.AreEqual(date.getSeconds(), dateTime.Second);
    }

    [TestMethod]
    public void ShouldCorrectHandleSwitchFromDstToStandard_SidneyTimeZone()
    {
        var timezone = TestAustraliaTimezone;
        Context.CurrentGlobalContext.CurrentTimeZone = timezone;

        var firstDate = new Date(new Arguments { 953996400000 });
        var secondDate = new Date(new Arguments { 954000000000 }); // the thing which geek will never have
        var thirdDate = new Date(new Arguments { 954003600000 });

        Assert.IsTrue(firstDate.ToString().StartsWith("Sun Mar 26 2000 02:00:00 GMT+1100"));
        Assert.IsTrue(secondDate.ToString().StartsWith("Sun Mar 26 2000 02:00:00 GMT+1000"));
        Assert.IsTrue(thirdDate.ToString().StartsWith("Sun Mar 26 2000 03:00:00 GMT+1000"));
    }

    [TestMethod]
    public void ShouldCorrectHandleSwitchFromDstToStandardBySetDate_MoscowTime()
    {
        var timezone = TestMoscowTimezone;
        Context.CurrentGlobalContext.CurrentTimeZone = timezone;

        var d = new Date(new Arguments { "3/27/2010 08:00" });
        Assert.AreEqual(-180, d.getTimezoneOffset().As<int>());
        d.setDate(28);
        Assert.AreEqual(-240, d.getTimezoneOffset().As<int>());
    }

    private static bool rtzDelegate(TimeZoneInfo x)
    {
        return x.StandardName.IndexOf("RTZ 2", StringComparison.OrdinalIgnoreCase) != -1 || x.Id.IndexOf("MSK", StringComparison.OrdinalIgnoreCase) != -1 || x.Id.IndexOf("Moscow", StringComparison.OrdinalIgnoreCase) != -1;
    }

    [TestMethod]
    public void ShouldCorrectHandleSwitchFromDstToStandardBySetTime_MoscowTime()
    {
        var timezone = TestMoscowTimezone;
        Context.CurrentGlobalContext.CurrentTimeZone = timezone;

        var d = new Date(new Arguments { "3/27/2010 08:00" });
        Assert.AreEqual(-180, d.getTimezoneOffset().As<int>());
        d.setTime((long)d.getTime() + 86400 * 1000);
        Assert.AreEqual(-240, d.getTimezoneOffset().As<int>());
    }

    [TestMethod]
    public void ShouldCorrectParseIsoTimeInCurrentTimezone_SidneyTimeZone()
    {
        var timezone = TestAustraliaTimezone;
        Context.CurrentGlobalContext.CurrentTimeZone = timezone;

        var d = new Date(new Arguments { "2010-08-30T00:00:00" });

        Assert.AreEqual(1283090400000, (long)d.valueOf());
    }

    [TestMethod]
    public void ShouldReturnsCorrectUtcDateForParsedIsoTime_SidneyTimeZone()
    {
        var timezone = TestAustraliaTimezone;
        Context.CurrentGlobalContext.CurrentTimeZone = timezone;

        var d = new Date(new Arguments { "2010-08-30T00:00:00" });

        Assert.AreEqual(29, (int)d.getUTCDate());
    }

    [TestMethod]
    public void ShouldReturnsCorrectUtcDayForParsedIsoTime_SidneyTimeZone()
    {
        var timezone = TestAustraliaTimezone;
        Context.CurrentGlobalContext.CurrentTimeZone = timezone;

        var d = new Date(new Arguments { "2010-08-30T00:00:00" });

        Assert.AreEqual(0, (int)d.getUTCDay());
    }

    [TestMethod]
    public void ShouldReturnsCorrectUtcHoursForParsedIsoTime_SidneyTimeZone()
    {
        var timezone = TestAustraliaTimezone;
        Context.CurrentGlobalContext.CurrentTimeZone = timezone;

        var d = new Date(new Arguments { "2010-08-30T00:00:00" });

        Assert.AreEqual(14, (int)d.getUTCHours());
    }

    [TestMethod]
    public void ShouldReturnsCorrectUtcYearForParsedIsoTime_SidneyTimeZone()
    {
        var timezone = TestAustraliaTimezone;
        Context.CurrentGlobalContext.CurrentTimeZone = timezone;

        var d = new Date(new Arguments { "2010-08-30T00:00:00" });

        Assert.AreEqual(110, (int)d.getYear());
    }

    [TestMethod]
    public void ShouldReturnsCorrectIsoStringForParsedIsoTime_SidneyTimeZone()
    {
        var timezone = TestAustraliaTimezone;
        Context.CurrentGlobalContext.CurrentTimeZone = timezone;

        var d = new Date(new Arguments { "2010-08-30T00:00:00" });

        Assert.AreEqual("2010-08-29T14:00:00.000Z", d.toISOString().ToString());
    }

    [TestMethod]
    public void ShouldReturnsCorrectJsonForParsedIsoTime_SidneyTimeZone()
    {
        var timezone = TestAustraliaTimezone;
        Context.CurrentGlobalContext.CurrentTimeZone = timezone;

        var d = new Date(new Arguments { "2010-08-30T00:00:00" });

        Assert.AreEqual("2010-08-29T14:00:00.000Z", d.toJSON().ToString());
    }

    [TestMethod]
    public void SetMonthShouldWorkCorrectlyAndReturnTickValue_SidneyTimeZone()
    {
        var timezone = TestAustraliaTimezone;
        Context.CurrentGlobalContext.CurrentTimeZone = timezone;

        var result = new Date(new Arguments { "2010-08-30T00:00:00" }).setMonth(5, null);

        Assert.AreEqual(1277820000000, (long)result);
    }

    [TestMethod]
    public void SetUtcMonthShouldWorkCorrectlyAndReturnTickValue_SidneyTimeZone()
    {
        var timezone = TestAustraliaTimezone;
        Context.CurrentGlobalContext.CurrentTimeZone = timezone;

        var result = new Date(new Arguments { "2010-08-30T00:00:00" }).setUTCMonth(5, null);

        Assert.AreEqual(1277820000000, (long)result);
    }

    [TestMethod]
    public void SetUtcMillisecondsShouldWorkCorrectlyAndReturnTickValue_SidneyTimeZone()
    {
        var timezone = TestAustraliaTimezone;
        Context.CurrentGlobalContext.CurrentTimeZone = timezone;

        var result = new Date(new Arguments { "2010-08-30T00:00:00" }).setUTCMilliseconds(555);

        Assert.AreEqual(1283090400555, (long)result);
    }

    [TestMethod]
    public void SetUtcHoursShouldWorkCorrectlyAndReturnTickValue_SidneyTimeZone()
    {
        var timezone = TestAustraliaTimezone;
        Context.CurrentGlobalContext.CurrentTimeZone = timezone;

        var result = new Date(new Arguments { "2010-08-30T00:00:00" }).setUTCHours(4, null, null, null);

        Assert.AreEqual(1283054400000, (long)result);
    }

    [TestMethod]
    public void SetUtcDateShouldWorkCorrectlyAndReturnTickValue_SidneyTimeZone()
    {
        var timezone = TestAustraliaTimezone;
        Context.CurrentGlobalContext.CurrentTimeZone = timezone;

        var result = new Date(new Arguments { "2010-08-30T00:00:00" }).setUTCDate(15);

        Assert.AreEqual(1281880800000, (long)result);
    }

    [TestMethod]
    public void SetUtcMinutesShouldWorkCorrectlyAndReturnTickValue_SidneyTimeZone()
    {
        var timezone = TestAustraliaTimezone;
        Context.CurrentGlobalContext.CurrentTimeZone = timezone;

        var result = new Date(new Arguments { "2010-08-30T00:00:00" }).setUTCMinutes(34, null, null);

        Assert.AreEqual(1283092440000, (long)result);
    }

    [TestMethod]
    public void SetMillisecondsShouldWorkCorrectlyAndReturnTickValue_SidneyTimeZone()
    {
        var timezone = TestAustraliaTimezone;
        Context.CurrentGlobalContext.CurrentTimeZone = timezone;

        var result = new Date(new Arguments { "2010-08-30T00:00:00" }).setMilliseconds(555);

        Assert.AreEqual(1283090400555, (long)result);
    }

    [TestMethod]
    public void SetFullYearShouldWorkCorrectlyAndReturnTickValue_SidneyTimeZone()
    {
        var timezone = TestAustraliaTimezone;
        Context.CurrentGlobalContext.CurrentTimeZone = timezone;

        var result = new Date(new Arguments { "2010-08-30T00:00:00" }).setFullYear(2005, null, null);

        Assert.AreEqual(1125324000000, (long)result);
    }

    [TestMethod]
    public void SetUtcFullYearShouldWorkCorrectlyAndReturnTickValue_SidneyTimeZone()
    {
        var timezone = TestAustraliaTimezone;
        Context.CurrentGlobalContext.CurrentTimeZone = timezone;

        var result = new Date(new Arguments { "2010-08-30T00:00:00" }).setUTCFullYear(2004, null, null);

        Assert.AreEqual(1093788000000, (long)result);
    }

    [TestMethod]
    public void SetMinutesShouldWorkCorrectlyAndReturnTickValue_SidneyTimeZone()
    {
        var timezone = TestAustraliaTimezone;
        Context.CurrentGlobalContext.CurrentTimeZone = timezone;

        var result = new Date(new Arguments { "2010-08-30T00:00:00" }).setMinutes(34, null, null);

        Assert.AreEqual(1283092440000, (long)result);
    }

    [TestMethod]
    public void SetTimeShouldWorkCorrectlyAndReturnTickValue_SidneyTimeZone()
    {
        var timezone = TestAustraliaTimezone;
        Context.CurrentGlobalContext.CurrentTimeZone = timezone;

        var result = new Date(new Arguments { "2010-08-30T00:00:00" }).setTime(0);

        Assert.AreEqual(0, (long)result);
    }

    [TestMethod]
    public void SetTimeShouldRecalcTimezoneOffset_SidneyTimeZone()
    {
        var timezone = TestAustraliaTimezone;
        Context.CurrentGlobalContext.CurrentTimeZone = timezone;

        var d = new Date(new Arguments { "2010-08-30T00:00:00" });

        Assert.AreEqual(-600, (long)d.getTimezoneOffset());
        d.setTime((long)d.valueOf() - 180 * 86400 * 1000L);
        Assert.AreEqual(-660, (long)d.getTimezoneOffset());
    }

    [TestMethod]
    public void SetTimeShouldNotOffsetValueWithTimezone_SidneyTimeZone()
    {
        var timezone = TestAustraliaTimezone;
        Context.CurrentGlobalContext.CurrentTimeZone = timezone;

        var result = new Date(new Arguments { "2010-08-30T00:00:00" }).setTime(1525898414436);

        Assert.AreEqual(1525898414436, (long)result);
    }

    [TestMethod]
    public void ToLocaleDateStringShouldCorrectFormatValue_SidneyTimeZone()
    {
        var timezone = TestAustraliaTimezone;
        Context.CurrentGlobalContext.CurrentTimeZone = timezone;

        var result = new Date(new Arguments { "2010-08-30T00:00:00" }).toLocaleDateString();

        Assert.AreEqual("Mon Aug 30 2010", result.ToString());
    }

    [TestMethod]
    public void ToLocaleTimeStringShouldCorrectFormatValue_SidneyTimeZone()
    {
        var timezone = TestAustraliaTimezone;
        Context.CurrentGlobalContext.CurrentTimeZone = timezone;

        var result = new Date(new Arguments { "2010-08-30T00:00:00" }).toLocaleTimeString();

        var shouldBe = "00:00:00 GMT+1000 (";
        Assert.IsTrue(result.ToString().StartsWith(shouldBe), result.ToString() + " != " + shouldBe);
    }

    [TestMethod]
    public void ToStringShouldCorrectFormatValue_SidneyTimeZone()
    {
        var timezone = TestAustraliaTimezone;
        Context.CurrentGlobalContext.CurrentTimeZone = timezone;

        var result = new Date(new Arguments { "2010-08-30T00:00:00" }).toString();

        var shouldBe = "Mon Aug 30 2010 00:00:00 GMT+1000 (";
        Assert.IsTrue(result.ToString().StartsWith(shouldBe), result.ToString() + " != " + shouldBe);
    }

    [TestMethod]
    public void ToTimeStringShouldCorrectFormatValue_SidneyTimeZone()
    {
        var timezone = TestAustraliaTimezone;
        Context.CurrentGlobalContext.CurrentTimeZone = timezone;

        var result = new Date(new Arguments { "2010-08-30T00:00:00" }).toTimeString();

        var shouldBe = "00:00:00 GMT+1000 (";
        Assert.IsTrue(result.ToString().StartsWith(shouldBe), result.ToString() + " != " + shouldBe);
    }
}
