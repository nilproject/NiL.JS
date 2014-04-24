function base64encode(str) {
    // Символы для base64-преобразования
    var b64chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=';
    var b64encoded = '';
    var chr1, chr2, chr3;
    var enc1, enc2, enc3, enc4;

    for (var i = 0; i < str.length;) {
        chr1 = str.charCodeAt(i++);
        chr2 = str.charCodeAt(i++);
        chr3 = str.charCodeAt(i++);

        enc1 = chr1 >> 2;
        enc2 = ((chr1 & 3) << 4) | (chr2 >> 4);

        enc3 = isNaN(chr2) ? 64 : (((chr2 & 15) << 2) | (chr3 >> 6));
        enc4 = isNaN(chr3) ? 64 : (chr3 & 63);

        b64encoded += b64chars[enc1] + b64chars[enc2] + b64chars[enc3] + b64chars[enc4];
    }
    return b64encoded;
}

function base64decode(str) {
    // Символы для base64-преобразования
    var b64chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=';
    var b64decoded = '';
    var chr1, chr2, chr3;
    var enc1, enc2, enc3, enc4;

    var chToindex = [];
    for (var i = 0; i < 64; i++)
        chToindex[b64chars.charCodeAt(i)] = i;

    for (var i = 0; i < str.length;) {
        enc1 = chToindex[str.charCodeAt(i++)];
        enc2 = chToindex[str.charCodeAt(i++)];
        enc3 = chToindex[str.charCodeAt(i++)];
        enc4 = chToindex[str.charCodeAt(i++)];

        chr1 = (enc1 << 2) | (enc2 >> 4);
        chr2 = ((enc2 & 15) << 4) | (enc3 >> 2);
        chr3 = ((enc3 & 3) << 6) | enc4;

        b64decoded = b64decoded + String.fromCharCode(chr1);

        if (enc3 < 64) {
            b64decoded += String.fromCharCode(chr2);
        }
        if (enc4 < 64) {
            b64decoded += String.fromCharCode(chr3);
        }
    }
    return b64decoded;
}

function extend(str)
{
    var res = "";
    for (var i = 0; i < str.length; i++)
    {
        var chc = str.charCodeAt(i);
        res += String.fromCharCode((chc >> 8) & 0xff) + String.fromCharCode(chc & 0xff);
    }
    return res;
}

function collapse(str)
{
    var res = "";
    for (var i = 0; i < str.length; i++) {
        var chc0 = str.charCodeAt(i++);
        var chc1 = str.charCodeAt(i);
        res += String.fromCharCode((chc0 << 8) + chc1);
    }
    return res;
}

var text = "Привет";
console.log(collapse(base64decode(base64encode(extend(text)))));