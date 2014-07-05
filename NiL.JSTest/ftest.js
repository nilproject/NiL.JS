console.log(function () {
    return new Date("1970").toISOString();
}());
console.log(function () {
    var timeZoneMinutes = new Date().getTimezoneOffset() * (-1);
    var date, dateStr;

    if (timeZoneMinutes > 0) {
        date = new Date(1970, 0, -99999999, 0, 0, 0, 0);

        try {
            date.toISOString();
            return false;
        } catch (e) {
            return e instanceof RangeError;
        }
    } else {
        date = new Date(1970, 0, -99999999, 0, 0 + timeZoneMinutes + 60, 0, 0);

        dateStr = date.toISOString();

        return dateStr[dateStr.length - 1] === "Z";
    }
}());