

var sjcl = { cipher: {}, hash: {}, mode: {}, misc: {}, codec: {}, exception: { corrupt: function (a) { this.toString = function () { return "CORRUPT: " + this.message }; this.message = a }, invalid: function (a) { this.toString = function () { return "INVALID: " + this.message }; this.message = a }, bug: function (a) { this.toString = function () { return "BUG: " + this.message }; this.message = a } } };
sjcl.cipher.aes = function (a) {
    this.h[0][0][0] || this.w(); var b, c, d, e, f = this.h[0][4], g = this.h[1]; b = a.length; var h = 1; if (b !== 4 && b !== 6 && b !== 8) throw new sjcl.exception.invalid("invalid aes key size"); this.a = [d = a.slice(0), e = []]; for (a = b; a < 4 * b + 28; a++) { c = d[a - 1]; if (a % b === 0 || b === 8 && a % b === 4) { c = f[c >>> 24] << 24 ^ f[c >> 16 & 255] << 16 ^ f[c >> 8 & 255] << 8 ^ f[c & 255]; if (a % b === 0) { c = c << 8 ^ c >>> 24 ^ h << 24; h = h << 1 ^ (h >> 7) * 283 } } d[a] = d[a - b] ^ c } for (b = 0; a; b++, a--) {
        c = d[b & 3 ? a : a - 4]; e[b] = a <= 4 || b < 4 ? c : g[0][f[c >>> 24]] ^ g[1][f[c >> 16 & 255]] ^ g[2][f[c >> 8 & 255]] ^
        g[3][f[c & 255]]
    }
};
sjcl.cipher.aes.prototype = {
    encrypt: function (a) { return this.H(a, 0) },
    decrypt: function (a) { return this.H(a, 1) },
    h: [[[], [], [], [], []], [[], [], [], [], []]],
    w: function () {
        var a = this.h[0], b = this.h[1], c = a[4], d = b[4], e, f, g, h = [], i = [], k, j, l, m;
        for (e = 0; e < 0x100; e++) i[(h[e] = (e << 1) ^ ((e >> 7) * 283)) ^ e] = e;
        for (f = g = 0; !c[f]; f = f ^ (k || 1), g = i[g] || 1) {
            l = g ^ (g << 1) ^ (g << 2) ^ (g << 3) ^ (g << 4);
            l = (l >> 8) ^ (l & 255) ^ 99; c[f] = l;
            d[l] = f;
            j = h[e = h[k = h[f]]];
            m = (j * 0x1010101) ^ (e * 0x10001) ^ (k * 0x101) ^ (f * 0x1010100);
            j = (h[l] * 0x101) ^ (l * 0x1010100);
            for (e = 0; e < 4; e++) {
                a[e][f] = j = (j << 24) ^ (j >>> 8);
                b[e][l] = m = (m << 24) ^ (m >>> 8)
            }
        }
        for (e = 0; e < 5; e++) {
            a[e] = a[e].slice(0);
            b[e] = b[e].slice(0);
        }
    },
    H: function (a, b) {
        if (a.length !== 4) throw new sjcl.exception.invalid("invalid aes block size");
        var c = this.a[b], d = a[0] ^ c[0], e = a[b ? 3 : 1] ^ c[1], f = a[2] ^ c[2];
        a = a[b ? 1 : 3] ^ c[3];
        var g, h, i, k = c.length / 4 - 2, j, l = 4, m = [0, 0, 0, 0];
        g = this.h[b];
        var n = g[0], o = g[1], p = g[2], q = g[3], r = g[4];
        for (j = 0; j < k; j++) {
            g = n[d >>> 24] ^ o[e >> 16 & 255] ^ p[f >> 8 & 255] ^ q[a & 255] ^ c[l];
            h = n[e >>> 24] ^ o[f >> 16 & 255] ^ p[a >> 8 & 255] ^ q[d & 255] ^ c[l + 1];
            i = n[f >>> 24] ^ o[a >> 16 & 255] ^ p[d >> 8 & 255] ^ q[e & 255] ^ c[l + 2];
            a = n[a >>> 24] ^ o[d >> 16 & 255] ^ p[e >> 8 & 255] ^ q[f & 255] ^ c[l + 3];
            l += 4;
            d = g;
            e = h;
            f = i
        }
        for (j = 0; j < 4; j++) {
            m[b ? 3 & -j : j] = r[d >>> 24] << 24 ^ r[e >> 16 & 255] << 16 ^ r[f >> 8 & 255] << 8 ^ r[a & 255] ^ c[l++];
            g = d;
            d = e;
            e = f;
            f = a;
            a = g;
        } return m
    }
};
sjcl.bitArray = {
    bitSlice: function (a, b, c) { a = sjcl.bitArray.P(a.slice(b / 32), 32 - (b & 31)).slice(1); return c === undefined ? a : sjcl.bitArray.clamp(a, c - b) }, concat: function (a, b) { if (a.length === 0 || b.length === 0) return a.concat(b); var c = a[a.length - 1], d = sjcl.bitArray.getPartial(c); return d === 32 ? a.concat(b) : sjcl.bitArray.P(b, d, c | 0, a.slice(0, a.length - 1)) }, bitLength: function (a) { var b = a.length; if (b === 0) return 0; return (b - 1) * 32 + sjcl.bitArray.getPartial(a[b - 1]) }, clamp: function (a, b) {
        if (a.length * 32 < b) return a; a = a.slice(0, Math.ceil(b /
        32)); var c = a.length; b &= 31; if (c > 0 && b) a[c - 1] = sjcl.bitArray.partial(b, a[c - 1] & 2147483648 >> b - 1, 1); return a
    }, partial: function (a, b, c) { if (a === 32) return b; return (c ? b | 0 : b << 32 - a) + a * 0x10000000000 }, getPartial: function (a) { return Math.round(a / 0x10000000000) || 32 }, equal: function (a, b) { if (sjcl.bitArray.bitLength(a) !== sjcl.bitArray.bitLength(b)) return false; var c = 0, d; for (d = 0; d < a.length; d++) c |= a[d] ^ b[d]; return c === 0 }, P: function (a, b, c, d) {
        var e; e = 0; if (d === undefined) d = []; for (; b >= 32; b -= 32) { d.push(c); c = 0 } if (b === 0) return d.concat(a);
        for (e = 0; e < a.length; e++) { d.push(c | a[e] >>> b); c = a[e] << 32 - b } e = a.length ? a[a.length - 1] : 0; a = sjcl.bitArray.getPartial(e); d.push(sjcl.bitArray.partial(b + a & 31, b + a > 32 ? c : d.pop(), 1)); return d
    }, k: function (a, b) { return [a[0] ^ b[0], a[1] ^ b[1], a[2] ^ b[2], a[3] ^ b[3]] }
};
sjcl.codec.utf8String = { fromBits: function (a) { var b = "", c = sjcl.bitArray.bitLength(a), d, e; for (d = 0; d < c / 8; d++) { if ((d & 3) === 0) e = a[d / 4]; b += String.fromCharCode(e >>> 24); e <<= 8 } return decodeURIComponent(escape(b)) }, toBits: function (a) { a = unescape(encodeURIComponent(a)); var b = [], c, d = 0; for (c = 0; c < a.length; c++) { d = d << 8 | a.charCodeAt(c); if ((c & 3) === 3) { b.push(d); d = 0 } } c & 3 && b.push(sjcl.bitArray.partial(8 * (c & 3), d)); return b } };
sjcl.codec.hex = { fromBits: function (a) { var b = "", c; for (c = 0; c < a.length; c++) b += ((a[c] | 0) + 0xf00000000000).toString(16).substr(4); return b.substr(0, sjcl.bitArray.bitLength(a) / 4) }, toBits: function (a) { var b, c = [], d; a = a.replace(/\s|0x/g, ""); d = a.length; a += "00000000"; for (b = 0; b < a.length; b += 8) c.push(parseInt(a.substr(b, 8), 16) ^ 0); return sjcl.bitArray.clamp(c, d * 4) } };
sjcl.codec.base64 = {
    D: "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/", fromBits: function (a, b) { var c = "", d, e = 0, f = sjcl.codec.base64.D, g = 0, h = sjcl.bitArray.bitLength(a); for (d = 0; c.length * 6 < h;) { c += f.charAt((g ^ a[d] >>> e) >>> 26); if (e < 6) { g = a[d] << 6 - e; e += 26; d++ } else { g <<= 6; e -= 6 } } for (; c.length & 3 && !b;) c += "="; return c }, toBits: function (a) {
        a = a.replace(/\s|=/g, ""); var b = [], c, d = 0, e = sjcl.codec.base64.D, f = 0, g; for (c = 0; c < a.length; c++) {
            g = e.indexOf(a.charAt(c)); if (g < 0) throw new sjcl.exception.invalid("this isn't base64!");
            if (d > 26) { d -= 26; b.push(f ^ g >>> d); f = g << 32 - d } else { d += 6; f ^= g << 32 - d }
        } d & 56 && b.push(sjcl.bitArray.partial(d & 56, f, 1)); return b
    }
}; sjcl.hash.sha256 = function (a) { this.a[0] || this.w(); if (a) { this.n = a.n.slice(0); this.i = a.i.slice(0); this.e = a.e } else this.reset() }; sjcl.hash.sha256.hash = function (a) { return (new sjcl.hash.sha256).update(a).finalize() };
sjcl.hash.sha256.prototype = {
    blockSize: 512, reset: function () { this.n = this.N.slice(0); this.i = []; this.e = 0; return this }, update: function (a) { if (typeof a === "string") a = sjcl.codec.utf8String.toBits(a); var b, c = this.i = sjcl.bitArray.concat(this.i, a); b = this.e; a = this.e = b + sjcl.bitArray.bitLength(a); for (b = 512 + b & -512; b <= a; b += 512) this.C(c.splice(0, 16)); return this }, finalize: function () {
        var a, b = this.i, c = this.n; b = sjcl.bitArray.concat(b, [sjcl.bitArray.partial(1, 1)]); for (a = b.length + 2; a & 15; a++) b.push(0); b.push(Math.floor(this.e /
        4294967296)); for (b.push(this.e | 0) ; b.length;) this.C(b.splice(0, 16)); this.reset(); return c
    }, N: [], a: [], w: function () { function a(e) { return (e - Math.floor(e)) * 0x100000000 | 0 } var b = 0, c = 2, d; a: for (; b < 64; c++) { for (d = 2; d * d <= c; d++) if (c % d === 0) continue a; if (b < 8) this.N[b] = a(Math.pow(c, 0.5)); this.a[b] = a(Math.pow(c, 1 / 3)); b++ } }, C: function (a) {
        var b, c, d = a.slice(0), e = this.n, f = this.a, g = e[0], h = e[1], i = e[2], k = e[3], j = e[4], l = e[5], m = e[6], n = e[7]; for (a = 0; a < 64; a++) {
            if (a < 16) b = d[a]; else {
                b = d[a + 1 & 15]; c = d[a + 14 & 15]; b = d[a & 15] = (b >>> 7 ^ b >>> 18 ^
                b >>> 3 ^ b << 25 ^ b << 14) + (c >>> 17 ^ c >>> 19 ^ c >>> 10 ^ c << 15 ^ c << 13) + d[a & 15] + d[a + 9 & 15] | 0
            } b = b + n + (j >>> 6 ^ j >>> 11 ^ j >>> 25 ^ j << 26 ^ j << 21 ^ j << 7) + (m ^ j & (l ^ m)) + f[a]; n = m; m = l; l = j; j = k + b | 0; k = i; i = h; h = g; g = b + (h & i ^ k & (h ^ i)) + (h >>> 2 ^ h >>> 13 ^ h >>> 22 ^ h << 30 ^ h << 19 ^ h << 10) | 0
        } e[0] = e[0] + g | 0; e[1] = e[1] + h | 0; e[2] = e[2] + i | 0; e[3] = e[3] + k | 0; e[4] = e[4] + j | 0; e[5] = e[5] + l | 0; e[6] = e[6] + m | 0; e[7] = e[7] + n | 0
    }
};
sjcl.mode.ccm = {
    name: "ccm", encrypt: function (a, b, c, d, e) { var f, g = b.slice(0), h = sjcl.bitArray, i = h.bitLength(c) / 8, k = h.bitLength(g) / 8; e = e || 64; d = d || []; if (i < 7) throw new sjcl.exception.invalid("ccm: iv must be at least 7 bytes"); for (f = 2; f < 4 && k >>> 8 * f; f++); if (f < 15 - i) f = 15 - i; c = h.clamp(c, 8 * (15 - f)); b = sjcl.mode.ccm.G(a, b, c, d, e, f); g = sjcl.mode.ccm.I(a, g, c, b, e, f); return h.concat(g.data, g.tag) }, decrypt: function (a, b, c, d, e) {
        e = e || 64; d = d || []; var f = sjcl.bitArray, g = f.bitLength(c) / 8, h = f.bitLength(b), i = f.clamp(b, h - e), k = f.bitSlice(b,
        h - e); h = (h - e) / 8; if (g < 7) throw new sjcl.exception.invalid("ccm: iv must be at least 7 bytes"); for (b = 2; b < 4 && h >>> 8 * b; b++); if (b < 15 - g) b = 15 - g; c = f.clamp(c, 8 * (15 - b)); i = sjcl.mode.ccm.I(a, i, c, k, e, b); a = sjcl.mode.ccm.G(a, i.data, c, d, e, b); if (!f.equal(i.tag, a)) throw new sjcl.exception.corrupt("ccm: tag doesn't match"); return i.data
    }, G: function (a, b, c, d, e, f) {
        var g = [], h = sjcl.bitArray, i = h.k; e /= 8; if (e % 2 || e < 4 || e > 16) throw new sjcl.exception.invalid("ccm: invalid tag length"); if (d.length > 0xffffffff || b.length > 0xffffffff) throw new sjcl.exception.bug("ccm: can't deal with 4GiB or more data");
        f = [h.partial(8, (d.length ? 64 : 0) | e - 2 << 2 | f - 1)]; f = h.concat(f, c); f[3] |= h.bitLength(b) / 8; f = a.encrypt(f); if (d.length) { c = h.bitLength(d) / 8; if (c <= 65279) g = [h.partial(16, c)]; else if (c <= 0xffffffff) g = h.concat([h.partial(16, 65534)], [c]); g = h.concat(g, d); for (d = 0; d < g.length; d += 4) f = a.encrypt(i(f, g.slice(d, d + 4))) } for (d = 0; d < b.length; d += 4) f = a.encrypt(i(f, b.slice(d, d + 4))); return h.clamp(f, e * 8)
    }, I: function (a, b, c, d, e, f) {
        var g, h = sjcl.bitArray; g = h.k; var i = b.length, k = h.bitLength(b); c = h.concat([h.partial(8, f - 1)], c).concat([0,
        0, 0]).slice(0, 4); d = h.bitSlice(g(d, a.encrypt(c)), 0, e); if (!i) return { tag: d, data: [] }; for (g = 0; g < i; g += 4) { c[3]++; e = a.encrypt(c); b[g] ^= e[0]; b[g + 1] ^= e[1]; b[g + 2] ^= e[2]; b[g + 3] ^= e[3] } return { tag: d, data: h.clamp(b, k) }
    }
};
sjcl.mode.ocb2 = {
    name: "ocb2", encrypt: function (a, b, c, d, e, f) {
        if (sjcl.bitArray.bitLength(c) !== 128) throw new sjcl.exception.invalid("ocb iv must be 128 bits"); var g, h = sjcl.mode.ocb2.A, i = sjcl.bitArray, k = i.k, j = [0, 0, 0, 0]; c = h(a.encrypt(c)); var l, m = []; d = d || []; e = e || 64; for (g = 0; g + 4 < b.length; g += 4) { l = b.slice(g, g + 4); j = k(j, l); m = m.concat(k(c, a.encrypt(k(c, l)))); c = h(c) } l = b.slice(g); b = i.bitLength(l); g = a.encrypt(k(c, [0, 0, 0, b])); l = i.clamp(k(l, g), b); j = k(j, k(l, g)); j = a.encrypt(k(j, k(c, h(c)))); if (d.length) j = k(j, f ? d : sjcl.mode.ocb2.pmac(a,
        d)); return m.concat(i.concat(l, i.clamp(j, e)))
    }, decrypt: function (a, b, c, d, e, f) {
        if (sjcl.bitArray.bitLength(c) !== 128) throw new sjcl.exception.invalid("ocb iv must be 128 bits"); e = e || 64; var g = sjcl.mode.ocb2.A, h = sjcl.bitArray, i = h.k, k = [0, 0, 0, 0], j = g(a.encrypt(c)), l, m, n = sjcl.bitArray.bitLength(b) - e, o = []; d = d || []; for (c = 0; c + 4 < n / 32; c += 4) { l = i(j, a.decrypt(i(j, b.slice(c, c + 4)))); k = i(k, l); o = o.concat(l); j = g(j) } m = n - c * 32; l = a.encrypt(i(j, [0, 0, 0, m])); l = i(l, h.clamp(b.slice(c), m)); k = i(k, l); k = a.encrypt(i(k, i(j, g(j)))); if (d.length) k =
        i(k, f ? d : sjcl.mode.ocb2.pmac(a, d)); if (!h.equal(h.clamp(k, e), h.bitSlice(b, n))) throw new sjcl.exception.corrupt("ocb: tag doesn't match"); return o.concat(h.clamp(l, m))
    }, pmac: function (a, b) { var c, d = sjcl.mode.ocb2.A, e = sjcl.bitArray, f = e.k, g = [0, 0, 0, 0], h = a.encrypt([0, 0, 0, 0]); h = f(h, d(d(h))); for (c = 0; c + 4 < b.length; c += 4) { h = d(h); g = f(g, a.encrypt(f(h, b.slice(c, c + 4)))) } b = b.slice(c); if (e.bitLength(b) < 128) { h = f(h, d(h)); b = e.concat(b, [2147483648 | 0]) } g = f(g, b); return a.encrypt(f(d(f(h, d(h))), g)) }, A: function (a) {
        return [a[0] <<
        1 ^ a[1] >>> 31, a[1] << 1 ^ a[2] >>> 31, a[2] << 1 ^ a[3] >>> 31, a[3] << 1 ^ (a[0] >>> 31) * 135]
    }
}; sjcl.misc.hmac = function (a, b) { this.M = b = b || sjcl.hash.sha256; var c = [[], []], d = b.prototype.blockSize / 32; this.l = [new b, new b]; if (a.length > d) a = b.hash(a); for (b = 0; b < d; b++) { c[0][b] = a[b] ^ 909522486; c[1][b] = a[b] ^ 1549556828 } this.l[0].update(c[0]); this.l[1].update(c[1]) }; sjcl.misc.hmac.prototype.encrypt = sjcl.misc.hmac.prototype.mac = function (a, b) { a = (new this.M(this.l[0])).update(a, b).finalize(); return (new this.M(this.l[1])).update(a).finalize() };
sjcl.misc.pbkdf2 = function (a, b, c, d, e) { c = c || 1E3; if (d < 0 || c < 0) throw sjcl.exception.invalid("invalid params to pbkdf2"); if (typeof a === "string") a = sjcl.codec.utf8String.toBits(a); e = e || sjcl.misc.hmac; a = new e(a); var f, g, h, i, k = [], j = sjcl.bitArray; for (i = 1; 32 * k.length < (d || 1) ; i++) { e = f = a.encrypt(j.concat(b, [i])); for (g = 1; g < c; g++) { f = a.encrypt(f); for (h = 0; h < f.length; h++) e[h] ^= f[h] } k = k.concat(e) } if (d) k = j.clamp(k, d); return k };
sjcl.random = {
    randomWords: function (a, b) { var c = []; b = this.isReady(b); var d; if (b === 0) throw new sjcl.exception.notready("generator isn't seeded"); else b & 2 && this.U(!(b & 1)); for (b = 0; b < a; b += 4) { (b + 1) % 0x10000 === 0 && this.L(); d = this.u(); c.push(d[0], d[1], d[2], d[3]) } this.L(); return c.slice(0, a) }, setDefaultParanoia: function (a) { this.t = a }, addEntropy: function (a, b, c) {
        c = c || "user"; var d, e, f = (new Date).valueOf(), g = this.q[c], h = this.isReady(); d = this.F[c]; if (d === undefined) d = this.F[c] = this.R++; if (g === undefined) g = this.q[c] = 0; this.q[c] =
        (this.q[c] + 1) % this.b.length; switch (typeof a) { case "number": break; case "object": if (b === undefined) for (c = b = 0; c < a.length; c++) for (e = a[c]; e > 0;) { b++; e >>>= 1 } this.b[g].update([d, this.J++, 2, b, f, a.length].concat(a)); break; case "string": if (b === undefined) b = a.length; this.b[g].update([d, this.J++, 3, b, f, a.length]); this.b[g].update(a); break; default: throw new sjcl.exception.bug("random: addEntropy only supports number, array or string"); } this.j[g] += b; this.f += b; if (h === 0) {
            this.isReady() !== 0 && this.K("seeded", Math.max(this.g,
            this.f)); this.K("progress", this.getProgress())
        }
    }, isReady: function (a) { a = this.B[a !== undefined ? a : this.t]; return this.g && this.g >= a ? this.j[0] > 80 && (new Date).valueOf() > this.O ? 3 : 1 : this.f >= a ? 2 : 0 }, getProgress: function (a) { a = this.B[a ? a : this.t]; return this.g >= a ? 1["0"] : this.f > a ? 1["0"] : this.f / a }, startCollectors: function () {
        if (!this.m) {
            if (window.addEventListener) { window.addEventListener("load", this.o, false); window.addEventListener("mousemove", this.p, false) } else if (document.attachEvent) {
                document.attachEvent("onload",
                this.o); document.attachEvent("onmousemove", this.p)
            } else throw new sjcl.exception.bug("can't attach event"); this.m = true
        }
    }, stopCollectors: function () { if (this.m) { if (window.removeEventListener) { window.removeEventListener("load", this.o); window.removeEventListener("mousemove", this.p) } else if (window.detachEvent) { window.detachEvent("onload", this.o); window.detachEvent("onmousemove", this.p) } this.m = false } }, addEventListener: function (a, b) { this.r[a][this.Q++] = b }, removeEventListener: function (a, b) {
        var c; a = this.r[a];
        var d = []; for (c in a) a.hasOwnProperty[c] && a[c] === b && d.push(c); for (b = 0; b < d.length; b++) { c = d[b]; delete a[c] }
    }, b: [new sjcl.hash.sha256], j: [0], z: 0, q: {}, J: 0, F: {}, R: 0, g: 0, f: 0, O: 0, a: [0, 0, 0, 0, 0, 0, 0, 0], d: [0, 0, 0, 0], s: undefined, t: 6, m: false, r: { progress: {}, seeded: {} }, Q: 0, B: [0, 48, 64, 96, 128, 192, 0x100, 384, 512, 768, 1024], u: function () { for (var a = 0; a < 4; a++) { this.d[a] = this.d[a] + 1 | 0; if (this.d[a]) break } return this.s.encrypt(this.d) }, L: function () { this.a = this.u().concat(this.u()); this.s = new sjcl.cipher.aes(this.a) }, T: function (a) {
        this.a =
        sjcl.hash.sha256.hash(this.a.concat(a)); this.s = new sjcl.cipher.aes(this.a); for (a = 0; a < 4; a++) { this.d[a] = this.d[a] + 1 | 0; if (this.d[a]) break }
    }, U: function (a) { var b = [], c = 0, d; this.O = b[0] = (new Date).valueOf() + 3E4; for (d = 0; d < 16; d++) b.push(Math.random() * 0x100000000 | 0); for (d = 0; d < this.b.length; d++) { b = b.concat(this.b[d].finalize()); c += this.j[d]; this.j[d] = 0; if (!a && this.z & 1 << d) break } if (this.z >= 1 << this.b.length) { this.b.push(new sjcl.hash.sha256); this.j.push(0) } this.f -= c; if (c > this.g) this.g = c; this.z++; this.T(b) }, p: function (a) {
        sjcl.random.addEntropy([a.x ||
        a.clientX || a.offsetX, a.y || a.clientY || a.offsetY], 2, "mouse")
    }, o: function () { sjcl.random.addEntropy(new Date, 2, "loadtime") }, K: function (a, b) { var c; a = sjcl.random.r[a]; var d = []; for (c in a) a.hasOwnProperty(c) && d.push(a[c]); for (c = 0; c < d.length; c++) d[c](b) }
};
sjcl.json = {
    defaults: { v: 1, iter: 1E3, ks: 128, ts: 64, mode: "ccm", adata: "", cipher: "aes" }, encrypt: function (a, b, c, d) {
        c = c || {}; d = d || {}; var e = sjcl.json, f = e.c({ iv: sjcl.random.randomWords(4, 0) }, e.defaults); e.c(f, c); if (typeof f.salt === "string") f.salt = sjcl.codec.base64.toBits(f.salt); if (typeof f.iv === "string") f.iv = sjcl.codec.base64.toBits(f.iv); if (!sjcl.mode[f.mode] || !sjcl.cipher[f.cipher] || typeof a === "string" && f.iter <= 100 || f.ts !== 64 && f.ts !== 96 && f.ts !== 128 || f.ks !== 128 && f.ks !== 192 && f.ks !== 0x100 || f.iv.length < 2 || f.iv.length >
        4) throw new sjcl.exception.invalid("json encrypt: invalid parameters"); if (typeof a === "string") { c = sjcl.misc.cachedPbkdf2(a, f); a = c.key.slice(0, f.ks / 32); f.salt = c.salt } if (typeof b === "string") b = sjcl.codec.utf8String.toBits(b); c = new sjcl.cipher[f.cipher](a); e.c(d, f); d.key = a; f.ct = sjcl.mode[f.mode].encrypt(c, b, f.iv, f.adata, f.tag); return e.encode(e.V(f, e.defaults))
    }, decrypt: function (a, b, c, d) {
        c = c || {}; d = d || {}; var e = sjcl.json; b = e.c(e.c(e.c({}, e.defaults), e.decode(b)), c, true); if (typeof b.salt === "string") b.salt =
        sjcl.codec.base64.toBits(b.salt); if (typeof b.iv === "string") b.iv = sjcl.codec.base64.toBits(b.iv); if (!sjcl.mode[b.mode] || !sjcl.cipher[b.cipher] || typeof a === "string" && b.iter <= 100 || b.ts !== 64 && b.ts !== 96 && b.ts !== 128 || b.ks !== 128 && b.ks !== 192 && b.ks !== 0x100 || !b.iv || b.iv.length < 2 || b.iv.length > 4) throw new sjcl.exception.invalid("json decrypt: invalid parameters"); if (typeof a === "string") { c = sjcl.misc.cachedPbkdf2(a, b); a = c.key.slice(0, b.ks / 32); b.salt = c.salt } c = new sjcl.cipher[b.cipher](a); c = sjcl.mode[b.mode].decrypt(c,
        b.ct, b.iv, b.adata, b.tag); e.c(d, b); d.key = a; return sjcl.codec.utf8String.fromBits(c)
    }, encode: function (a) {
        var b, c = "{", d = ""; for (b in a) if (a.hasOwnProperty(b)) {
            if (!b.match(/^[a-z0-9]+$/i)) throw new sjcl.exception.invalid("json encode: invalid property name"); c += d + b + ":"; d = ","; switch (typeof a[b]) {
                case "number": case "boolean": c += a[b]; break; case "string": c += '"' + escape(a[b]) + '"'; break; case "object": c += '"' + sjcl.codec.base64.fromBits(a[b], 1) + '"'; break; default: throw new sjcl.exception.bug("json encode: unsupported type");
            }
        } return c + "}"
    }, decode: function (a) { a = a.replace(/\s/g, ""); if (!a.match(/^\{.*\}$/)) throw new sjcl.exception.invalid("json decode: this isn't json!"); a = a.replace(/^\{|\}$/g, "").split(/,/); var b = {}, c, d; for (c = 0; c < a.length; c++) { if (!(d = a[c].match(/^([a-z][a-z0-9]*):(?:(\d+)|"([a-z0-9+\/%*_.@=\-]*)")$/i))) throw new sjcl.exception.invalid("json decode: this isn't json!"); b[d[1]] = d[2] ? parseInt(d[2], 10) : d[1].match(/^(ct|salt|iv)$/) ? sjcl.codec.base64.toBits(d[3]) : unescape(d[3]) } return b }, c: function (a, b, c) {
        if (a ===
        undefined) a = {}; if (b === undefined) return a; var d; for (d in b) if (b.hasOwnProperty(d)) { if (c && a[d] !== undefined && a[d] !== b[d]) throw new sjcl.exception.invalid("required parameter overridden"); a[d] = b[d] } return a
    }, V: function (a, b) { var c = {}, d; for (d in a) if (a.hasOwnProperty(d) && a[d] !== b[d]) c[d] = a[d]; return c }, W: function (a, b) { var c = {}, d; for (d = 0; d < b.length; d++) if (a[b[d]] !== undefined) c[b[d]] = a[b[d]]; return c }
}; sjcl.encrypt = sjcl.json.encrypt; sjcl.decrypt = sjcl.json.decrypt; sjcl.misc.S = {};
sjcl.misc.cachedPbkdf2 = function (a, b) { var c = sjcl.misc.S, d; b = b || {}; d = b.iter || 1E3; c = c[a] = c[a] || {}; d = c[d] = c[d] || { firstSalt: b.salt && b.salt.length ? b.salt.slice(0) : sjcl.random.randomWords(2, 0) }; c = b.salt === undefined ? d.firstSalt : b.salt; d[c] = d[c] || sjcl.misc.pbkdf2(a, c, b.iter); return { key: d[c].slice(0), salt: c.slice(0) } };


var browserUtil = {
    isRhino: true,

    pauseAndThen: function (cb) { cb(); },

    cpsIterate: function (f, start, end, pause, callback) {
        function go() {
            var called = false;
            if (start >= end) {
                callback && callback();
            } else {
                f(start, function () {
                    if (!called) { called = true; start++; go(); }
                });
            }
        }
        go(start);
    },

    cpsMap: function (map, list, pause, callback) {
        browserUtil.cpsIterate(function (i, cb) { map(list[i], i, list.length, cb); },
                               0, list.length, pause, callback);
    },

    loadScripts: function (scriptNames, callback) {
        for (i = 0; i < scriptNames.length; i++) {
            load(scriptNames[i]);
            callback && callback();
        }
    },

    write: function (type, message) {
        print(message);
        return {
            update: function (type2, message2) {
                if (type2 === 'pass') { print("  + " + message2); }
                else if (type2 === 'unimplemented') { print("  ? " + message2); }
                else { print("  - " + message2); }
            }
        };
    },

    writeNewline: function () { print(""); },

    status: function (message) { }
};

sjcl.test = { vector: {}, all: {} };

/* A bit of a hack.  Because sjcl.test will be reloaded several times
 * for different variants of sjcl, but browserUtils will not, this
 * variable keeps a permanent record of whether anything has failed.
 */
if (typeof browserUtil.allPassed === 'undefined') {
    browserUtil.allPassed = true;
}

sjcl.test.TestCase = function (name, doRun) {
    this.doRun = doRun;
    this.name = name;
    this.passes = 0;
    this.failures = 0;
    this.isUnimplemented = false;
    sjcl.test.all[name] = this;
};

sjcl.test.TestCase.prototype = {
    /** Pass some subtest of this test */
    pass: function () { this.passes++; },

    /** Fail some subtest of this test */
    fail: function (message) {
        if (message !== undefined) {
            this.log("fail", "*** FAIL *** " + this.name + ": " + message);
        } else {
            this.log("fail", "*** FAIL *** " + this.name);
        }
        this.failures++;
        browserUtil.allPassed = false;
    },

    unimplemented: function () {
        this.isUnimplemented = true;
    },

    /** Log a message to the console */
    log: browserUtil.write,

    /** Require that the first argument is true; otherwise fail with the given message */
    require: function (bool, message) {
        if (bool) {
            this.pass();
        } else if (message !== undefined) {
            this.fail(message);
        } else {
            this.fail("requirement failed");
        }
    },

    /** Pause and then take the specified action. */
    pauseAndThen: browserUtil.pauseAndThen,

    /** Continuation-passing-style iteration */
    cpsIterate: browserUtil.cpsIterate,

    /** Continuation-passing-style iteration */
    cpsMap: browserUtil.cpsMap,

    /** Report the results of this test. */
    report: function (repo) {
        var t = (new Date()).valueOf() - this.startTime;
        if (this.failures !== 0) {
            repo.update("fail", "failed " + this.failures + " / " +
                        (this.passes + this.failures) + " tests. (" + t + " ms)");
        } else if (this.passes === 1) {
            repo.update("pass", "passed. (" + t + " ms)");
        } else if (this.isUnimplemented) {
            repo.update("unimplemented", "unimplemented");
        } else {
            repo.update("pass", "passed all " + this.passes + " tests. (" + t + " ms)");
        }
        browserUtil.writeNewline();
    },


    /** Run the test. */
    run: function (ntests, i, cb) {
        var thiz = this;
        this.startTime = (new Date()).valueOf();
        this.pauseAndThen(function () {
            thiz.doRun(function () {
                cb && cb();
            })
        });
    }
};

// pass a list of tests to run, or pass nothing and it will run them all
sjcl.test.run = function (tests, callback) {
    var t;

    if (tests === undefined || tests.length == 0) {
        tests = [];
        for (t in sjcl.test.all) {
            if (sjcl.test.all.hasOwnProperty(t)) {
                tests.push(t);
            }
        }
    }

    browserUtil.cpsMap(function (t, i, n, cb) {
        sjcl.test.all[tests[i]].run(n, i + 1, cb);
    }, tests, true, callback);
};

/* Several test scripts rely on sjcl.codec.hex to parse their test
 * vectors, but we are not guaranteed that sjcl.codec.hex is
 * implemented.
 */
sjcl.codec = sjcl.codec || {};
sjcl.codec.hex = sjcl.codec.hex ||
{
    fromBits: function (arr) {
        var out = "", i, x;
        for (i = 0; i < arr.length; i++) {
            out += ((arr[i] | 0) + 0xF00000000000).toString(16).substr(4);
        }
        return out.substr(0, sjcl.bitArray.bitLength(arr) / 4);//.replace(/(.{8})/g, "$1 ");
    },
    toBits: function (str) {
        var i, out = [], len;
        str = str.replace(/\s|0x/g, "");
        len = str.length;
        str = str + "00000000";
        for (i = 0; i < str.length; i += 8) {
            out.push(parseInt(str.substr(i, 8), 16) ^ 0);
        }
        return sjcl.bitArray.clamp(out, len * 4);
    }
};

sjcl.test.vector.aes = [
  {
      key: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0xf34481ec, 0x3cc627ba, 0xcd5dc3fb, 0x08f273e6],
      ct: [0x0336763e, 0x966d9259, 0x5a567cc9, 0xce537f5e]
  },
  {
      key: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x9798c464, 0x0bad75c7, 0xc3227db9, 0x10174e72],
      ct: [0xa9a1631b, 0xf4996954, 0xebc09395, 0x7b234589]
  },
  {
      key: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x96ab5c2f, 0xf612d9df, 0xaae8c31f, 0x30c42168],
      ct: [0xff4f8391, 0xa6a40ca5, 0xb25d23be, 0xdd44a597]
  },
  {
      key: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x6a118a87, 0x4519e64e, 0x9963798a, 0x503f1d35],
      ct: [0xdc43be40, 0xbe0e5371, 0x2f7e2bf5, 0xca707209]
  },
  {
      key: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0xcb9fceec, 0x81286ca3, 0xe989bd97, 0x9b0cb284],
      ct: [0x92beedab, 0x1895a94f, 0xaa69b632, 0xe5cc47ce]
  },
  {
      key: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0xb26aeb18, 0x74e47ca8, 0x358ff223, 0x78f09144],
      ct: [0x459264f4, 0x798f6a78, 0xbacb89c1, 0x5ed3d601]
  },
  {
      key: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x58c8e00b, 0x2631686d, 0x54eab84b, 0x91f0aca1],
      ct: [0x08a4e2ef, 0xec8a8e33, 0x12ca7460, 0xb9040bbf]
  },
  {
      key: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0xf34481ec, 0x3cc627ba, 0xcd5dc3fb, 0x08f273e6],
      ct: [0x0336763e, 0x966d9259, 0x5a567cc9, 0xce537f5e]
  },
  {
      key: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x9798c464, 0x0bad75c7, 0xc3227db9, 0x10174e72],
      ct: [0xa9a1631b, 0xf4996954, 0xebc09395, 0x7b234589]
  },
  {
      key: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x96ab5c2f, 0xf612d9df, 0xaae8c31f, 0x30c42168],
      ct: [0xff4f8391, 0xa6a40ca5, 0xb25d23be, 0xdd44a597]
  },
  {
      key: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x6a118a87, 0x4519e64e, 0x9963798a, 0x503f1d35],
      ct: [0xdc43be40, 0xbe0e5371, 0x2f7e2bf5, 0xca707209]
  },
  {
      key: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0xcb9fceec, 0x81286ca3, 0xe989bd97, 0x9b0cb284],
      ct: [0x92beedab, 0x1895a94f, 0xaa69b632, 0xe5cc47ce]
  },
  {
      key: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0xb26aeb18, 0x74e47ca8, 0x358ff223, 0x78f09144],
      ct: [0x459264f4, 0x798f6a78, 0xbacb89c1, 0x5ed3d601]
  },
  {
      key: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x58c8e00b, 0x2631686d, 0x54eab84b, 0x91f0aca1],
      ct: [0x08a4e2ef, 0xec8a8e33, 0x12ca7460, 0xb9040bbf]
  },
  {
      key: [0x10a58869, 0xd74be5a3, 0x74cf867c, 0xfb473859],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x6d251e69, 0x44b051e0, 0x4eaa6fb4, 0xdbf78465]
  },
  {
      key: [0xcaea65cd, 0xbb75e916, 0x9ecd22eb, 0xe6e54675],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x6e292011, 0x90152df4, 0xee058139, 0xdef610bb]
  },
  {
      key: [0xa2e2fa9b, 0xaf7d2082, 0x2ca9f054, 0x2f764a41],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xc3b44b95, 0xd9d2f256, 0x70eee9a0, 0xde099fa3]
  },
  {
      key: [0xb6364ac4, 0xe1de1e28, 0x5eaf144a, 0x2415f7a0],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x5d9b0557, 0x8fc944b3, 0xcf1ccf0e, 0x746cd581]
  },
  {
      key: [0x64cf9c7a, 0xbc50b888, 0xaf65f49d, 0x521944b2],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xf7efc89d, 0x5dba5781, 0x04016ce5, 0xad659c05]
  },
  {
      key: [0x47d6742e, 0xefcc0465, 0xdc96355e, 0x851b64d9],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x0306194f, 0x666d1836, 0x24aa230a, 0x8b264ae7]
  },
  {
      key: [0x3eb39790, 0x678c56be, 0xe34bbcde, 0xccf6cdb5],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x858075d5, 0x36d79cce, 0xe571f7d7, 0x204b1f67]
  },
  {
      key: [0x64110a92, 0x4f0743d5, 0x00ccadae, 0x72c13427],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x35870c6a, 0x57e9e923, 0x14bcb808, 0x7cde72ce]
  },
  {
      key: [0x18d81265, 0x16f8a12a, 0xb1a36d9f, 0x04d68e51],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x6c68e9be, 0x5ec41e22, 0xc825b7c7, 0xaffb4363]
  },
  {
      key: [0xf5303579, 0x68578480, 0xb398a3c2, 0x51cd1093],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xf5df3999, 0x0fc688f1, 0xb07224cc, 0x03e86cea]
  },
  {
      key: [0xda84367f, 0x325d42d6, 0x01b43269, 0x64802e8e],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xbba071bc, 0xb470f8f6, 0x586e5d3a, 0xdd18bc66]
  },
  {
      key: [0xe37b1c6a, 0xa2846f6f, 0xdb413f23, 0x8b089f23],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x43c9f7e6, 0x2f5d288b, 0xb27aa40e, 0xf8fe1ea8]
  },
  {
      key: [0x6c002b68, 0x2483e0ca, 0xbcc731c2, 0x53be5674],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x3580d19c, 0xff44f101, 0x4a7c966a, 0x69059de5]
  },
  {
      key: [0x143ae8ed, 0x6555aba9, 0x6110ab58, 0x893a8ae1],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x806da864, 0xdd29d48d, 0xeafbe764, 0xf8202aef]
  },
  {
      key: [0xb69418a8, 0x5332240d, 0xc8249235, 0x3956ae0c],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xa303d940, 0xded8f0ba, 0xff6f7541, 0x4cac5243]
  },
  {
      key: [0x71b5c08a, 0x1993e136, 0x2e4d0ce9, 0xb22b78d5],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xc2dabd11, 0x7f8a3eca, 0xbfbb11d1, 0x2194d9d0]
  },
  {
      key: [0xe234cdca, 0x2606b81f, 0x29408d5f, 0x6da21206],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xfff60a47, 0x40086b3b, 0x9c56195b, 0x98d91a7b]
  },
  {
      key: [0x13237c49, 0x074a3da0, 0x78dc1d82, 0x8bb78c6f],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x8146a08e, 0x2357f0ca, 0xa30ca8c9, 0x4d1a0544]
  },
  {
      key: [0x3071a2a4, 0x8fe6cbd0, 0x4f1a1290, 0x98e308f8],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x4b98e06d, 0x356deb07, 0xebb824e5, 0x713f7be3]
  },
  {
      key: [0x90f42ec0, 0xf68385f2, 0xffc5dfc0, 0x3a654dce],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x7a20a53d, 0x460fc9ce, 0x0423a7a0, 0x764c6cf2]
  },
  {
      key: [0xfebd9a24, 0xd8b65c1c, 0x787d50a4, 0xed3619a9],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xf4a70d8a, 0xf877f9b0, 0x2b4c40df, 0x57d45b17]
  },
  {
      key: [0x10a58869, 0xd74be5a3, 0x74cf867c, 0xfb473859],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x6d251e69, 0x44b051e0, 0x4eaa6fb4, 0xdbf78465]
  },
  {
      key: [0xcaea65cd, 0xbb75e916, 0x9ecd22eb, 0xe6e54675],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x6e292011, 0x90152df4, 0xee058139, 0xdef610bb]
  },
  {
      key: [0xa2e2fa9b, 0xaf7d2082, 0x2ca9f054, 0x2f764a41],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xc3b44b95, 0xd9d2f256, 0x70eee9a0, 0xde099fa3]
  },
  {
      key: [0xb6364ac4, 0xe1de1e28, 0x5eaf144a, 0x2415f7a0],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x5d9b0557, 0x8fc944b3, 0xcf1ccf0e, 0x746cd581]
  },
  {
      key: [0x64cf9c7a, 0xbc50b888, 0xaf65f49d, 0x521944b2],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xf7efc89d, 0x5dba5781, 0x04016ce5, 0xad659c05]
  },
  {
      key: [0x47d6742e, 0xefcc0465, 0xdc96355e, 0x851b64d9],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x0306194f, 0x666d1836, 0x24aa230a, 0x8b264ae7]
  },
  {
      key: [0x3eb39790, 0x678c56be, 0xe34bbcde, 0xccf6cdb5],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x858075d5, 0x36d79cce, 0xe571f7d7, 0x204b1f67]
  },
  {
      key: [0x64110a92, 0x4f0743d5, 0x00ccadae, 0x72c13427],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x35870c6a, 0x57e9e923, 0x14bcb808, 0x7cde72ce]
  },
  {
      key: [0x18d81265, 0x16f8a12a, 0xb1a36d9f, 0x04d68e51],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x6c68e9be, 0x5ec41e22, 0xc825b7c7, 0xaffb4363]
  },
  {
      key: [0xf5303579, 0x68578480, 0xb398a3c2, 0x51cd1093],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xf5df3999, 0x0fc688f1, 0xb07224cc, 0x03e86cea]
  },
  {
      key: [0xda84367f, 0x325d42d6, 0x01b43269, 0x64802e8e],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xbba071bc, 0xb470f8f6, 0x586e5d3a, 0xdd18bc66]
  },
  {
      key: [0xe37b1c6a, 0xa2846f6f, 0xdb413f23, 0x8b089f23],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x43c9f7e6, 0x2f5d288b, 0xb27aa40e, 0xf8fe1ea8]
  },
  {
      key: [0x6c002b68, 0x2483e0ca, 0xbcc731c2, 0x53be5674],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x3580d19c, 0xff44f101, 0x4a7c966a, 0x69059de5]
  },
  {
      key: [0x143ae8ed, 0x6555aba9, 0x6110ab58, 0x893a8ae1],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x806da864, 0xdd29d48d, 0xeafbe764, 0xf8202aef]
  },
  {
      key: [0xb69418a8, 0x5332240d, 0xc8249235, 0x3956ae0c],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xa303d940, 0xded8f0ba, 0xff6f7541, 0x4cac5243]
  },
  {
      key: [0x71b5c08a, 0x1993e136, 0x2e4d0ce9, 0xb22b78d5],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xc2dabd11, 0x7f8a3eca, 0xbfbb11d1, 0x2194d9d0]
  },
  {
      key: [0xe234cdca, 0x2606b81f, 0x29408d5f, 0x6da21206],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xfff60a47, 0x40086b3b, 0x9c56195b, 0x98d91a7b]
  },
  {
      key: [0x13237c49, 0x074a3da0, 0x78dc1d82, 0x8bb78c6f],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x8146a08e, 0x2357f0ca, 0xa30ca8c9, 0x4d1a0544]
  },
  {
      key: [0x3071a2a4, 0x8fe6cbd0, 0x4f1a1290, 0x98e308f8],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x4b98e06d, 0x356deb07, 0xebb824e5, 0x713f7be3]
  },
  {
      key: [0x90f42ec0, 0xf68385f2, 0xffc5dfc0, 0x3a654dce],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x7a20a53d, 0x460fc9ce, 0x0423a7a0, 0x764c6cf2]
  },
  {
      key: [0xfebd9a24, 0xd8b65c1c, 0x787d50a4, 0xed3619a9],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xf4a70d8a, 0xf877f9b0, 0x2b4c40df, 0x57d45b17]
  },
  {
      key: [0x80000000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x0edd33d3, 0xc621e546, 0x455bd8ba, 0x1418bec8]
  },
  {
      key: [0xc0000000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x4bc3f883, 0x450c113c, 0x64ca42e1, 0x112a9e87]
  },
  {
      key: [0xe0000000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x72a1da77, 0x0f5d7ac4, 0xc9ef94d8, 0x22affd97]
  },
  {
      key: [0xf0000000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x970014d6, 0x34e2b765, 0x0777e8e8, 0x4d03ccd8]
  },
  {
      key: [0xf8000000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xf17e79ae, 0xd0db7e27, 0x9e955b5f, 0x493875a7]
  },
  {
      key: [0xfc000000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x9ed5a751, 0x36a940d0, 0x963da379, 0xdb4af26a]
  },
  {
      key: [0xfe000000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xc4295f83, 0x465c7755, 0xe8fa364b, 0xac6a7ea5]
  },
  {
      key: [0xff000000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xb1d75825, 0x6b28fd85, 0x0ad49442, 0x08cf1155]
  },
  {
      key: [0xff800000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x42ffb34c, 0x743de4d8, 0x8ca38011, 0xc990890b]
  },
  {
      key: [0xffc00000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x9958f0ec, 0xea8b2172, 0xc0c1995f, 0x9182c0f3]
  },
  {
      key: [0xffe00000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x956d7798, 0xfac20f82, 0xa8823f98, 0x4d06f7f5]
  },
  {
      key: [0xfff00000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xa01bf44f, 0x2d16be92, 0x8ca44aaf, 0x7b9b106b]
  },
  {
      key: [0xfff80000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xb5f1a33e, 0x50d40d10, 0x3764c76b, 0xd4c6b6f8]
  },
  {
      key: [0xfffc0000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x2637050c, 0x9fc0d481, 0x7e2d69de, 0x878aee8d]
  },
  {
      key: [0xfffe0000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x113ecbe4, 0xa453269a, 0x0dd26069, 0x467fb5b5]
  },
  {
      key: [0xffff0000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x97d0754f, 0xe68f11b9, 0xe375d070, 0xa608c884]
  },
  {
      key: [0xffff8000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xc6a0b3e9, 0x98d05068, 0xa5399778, 0x405200b4]
  },
  {
      key: [0xffffc000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xdf556a33, 0x438db87b, 0xc41b1752, 0xc55e5e49]
  },
  {
      key: [0xffffe000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x90fb128d, 0x3a1af6e5, 0x48521bb9, 0x62bf1f05]
  },
  {
      key: [0xfffff000, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x26298e9c, 0x1db517c2, 0x15fadfb7, 0xd2a8d691]
  },
  {
      key: [0xfffff800, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xa6cb761d, 0x61f8292d, 0x0df393a2, 0x79ad0380]
  },
  {
      key: [0xfffffc00, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x12acd89b, 0x13cd5f87, 0x26e34d44, 0xfd486108]
  },
  {
      key: [0xfffffe00, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x95b1703f, 0xc57ba09f, 0xe0c3580f, 0xebdd7ed4]
  },
  {
      key: [0xffffff00, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xde11722d, 0x893e9f91, 0x21c381be, 0xcc1da59a]
  },
  {
      key: [0xffffff80, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x6d114ccb, 0x27bf3910, 0x12e8974c, 0x546d9bf2]
  },
  {
      key: [0xffffffc0, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x5ce37e17, 0xeb4646ec, 0xfac29b9c, 0xc38d9340]
  },
  {
      key: [0xffffffe0, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x18c1b6e2, 0x15712205, 0x6d0243d8, 0xa165cddb]
  },
  {
      key: [0xfffffff0, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x99693e6a, 0x59d1366c, 0x74d82356, 0x2d7e1431]
  },
  {
      key: [0xfffffff8, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x6c7c64dc, 0x84a8bba7, 0x58ed17eb, 0x025a57e3]
  },
  {
      key: [0xfffffffc, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xe17bc79f, 0x30eaab2f, 0xac2cbbe3, 0x458d687a]
  },
  {
      key: [0xfffffffe, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x1114bc20, 0x28009b92, 0x3f0b0191, 0x5ce5e7c4]
  },
  {
      key: [0xffffffff, 0x00000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x9c28524a, 0x16a1e1c1, 0x452971ca, 0xa8d13476]
  },
  {
      key: [0xffffffff, 0x80000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xed62e163, 0x63638360, 0xfdd6ad62, 0x112794f0]
  },
  {
      key: [0xffffffff, 0xc0000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x5a8688f0, 0xb2a2c162, 0x24c16165, 0x8ffd4044]
  },
  {
      key: [0xffffffff, 0xe0000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x23f71084, 0x2b9bb9c3, 0x2f26648c, 0x786807ca]
  },
  {
      key: [0xffffffff, 0xf0000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x44a98bf1, 0x1e163f63, 0x2c47ec6a, 0x49683a89]
  },
  {
      key: [0xffffffff, 0xf8000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x0f18aff9, 0x4274696d, 0x9b61848b, 0xd50ac5e5]
  },
  {
      key: [0xffffffff, 0xfc000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x82408571, 0xc3e24245, 0x40207f83, 0x3b6dda69]
  },
  {
      key: [0xffffffff, 0xfe000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x303ff996, 0x947f0c7d, 0x1f43c8f3, 0x027b9b75]
  },
  {
      key: [0xffffffff, 0xff000000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x7df4daf4, 0xad29a361, 0x5a9b6ece, 0x5c99518a]
  },
  {
      key: [0xffffffff, 0xff800000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xc72954a4, 0x8d0774db, 0x0b4971c5, 0x26260415]
  },
  {
      key: [0xffffffff, 0xffc00000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x1df9b761, 0x12dc6531, 0xe07d2cfd, 0xa04411f0]
  },
  {
      key: [0xffffffff, 0xffe00000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x8e4d8e69, 0x9119e1fc, 0x87545a64, 0x7fb1d34f]
  },
  {
      key: [0xffffffff, 0xfff00000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xe6c4807a, 0xe11f36f0, 0x91c57d9f, 0xb68548d1]
  },
  {
      key: [0xffffffff, 0xfff80000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x8ebf73aa, 0xd49c8200, 0x7f77a5c1, 0xccec6ab4]
  },
  {
      key: [0xffffffff, 0xfffc0000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x4fb288cc, 0x20400490, 0x01d2c758, 0x5ad123fc]
  },
  {
      key: [0xffffffff, 0xfffe0000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x04497110, 0xefb9dceb, 0x13e2b13f, 0xb4465564]
  },
  {
      key: [0xffffffff, 0xffff0000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x75550e6c, 0xb5a88e49, 0x634c9ab6, 0x9eda0430]
  },
  {
      key: [0xffffffff, 0xffff8000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xb6768473, 0xce9843ea, 0x66a81405, 0xdd50b345]
  },
  {
      key: [0xffffffff, 0xffffc000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xcb2f4303, 0x83f9084e, 0x03a65357, 0x1e065de6]
  },
  {
      key: [0xffffffff, 0xffffe000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xff4e66c0, 0x7bae3e79, 0xfb7d2108, 0x47a3b0ba]
  },
  {
      key: [0xffffffff, 0xfffff000, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x7b907851, 0x25505fad, 0x59b13c18, 0x6dd66ce3]
  },
  {
      key: [0xffffffff, 0xfffff800, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x8b527a6a, 0xebdaec9e, 0xaef8eda2, 0xcb7783e5]
  },
  {
      key: [0xffffffff, 0xfffffc00, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x43fdaf53, 0xebbc9880, 0xc228617d, 0x6a9b548b]
  },
  {
      key: [0xffffffff, 0xfffffe00, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x53786104, 0xb9744b98, 0xf052c46f, 0x1c850d0b]
  },
  {
      key: [0xffffffff, 0xffffff00, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xb5ab3013, 0xdd1e61df, 0x06cbaf34, 0xca2aee78]
  },
  {
      key: [0xffffffff, 0xffffff80, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x7470469b, 0xe9723030, 0xfdcc73a8, 0xcd4fbb10]
  },
  {
      key: [0xffffffff, 0xffffffc0, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xa35a63f5, 0x343ebe9e, 0xf8167bcb, 0x48ad122e]
  },
  {
      key: [0xffffffff, 0xffffffe0, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xfd8687f0, 0x757a210e, 0x9fdf1812, 0x04c30863]
  },
  {
      key: [0xffffffff, 0xfffffff0, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x7a181e84, 0xbd5457d2, 0x6a88fbae, 0x96018fb0]
  },
  {
      key: [0xffffffff, 0xfffffff8, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x653317b9, 0x362b6f9b, 0x9e1a580e, 0x68d494b5]
  },
  {
      key: [0xffffffff, 0xfffffffc, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x995c9dc0, 0xb689f03c, 0x45867b5f, 0xaa5c18d1]
  },
  {
      key: [0xffffffff, 0xfffffffe, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x77a4d96d, 0x56dda398, 0xb9aabecf, 0xc75729fd]
  },
  {
      key: [0xffffffff, 0xffffffff, 0x00000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x84be19e0, 0x53635f09, 0xf2665e7b, 0xae85b42d]
  },
  {
      key: [0xffffffff, 0xffffffff, 0x80000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x32cd6528, 0x42926aea, 0x4aa6137b, 0xb2be2b5e]
  },
  {
      key: [0xffffffff, 0xffffffff, 0xc0000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x493d4a4f, 0x38ebb337, 0xd10aa84e, 0x9171a554]
  },
  {
      key: [0xffffffff, 0xffffffff, 0xe0000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xd9bff7ff, 0x454b0ec5, 0xa4a2a695, 0x66e2cb84]
  },
  {
      key: [0xffffffff, 0xffffffff, 0xf0000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x3535d565, 0xace3f31e, 0xb249ba2c, 0xc6765d7a]
  },
  {
      key: [0xffffffff, 0xffffffff, 0xf8000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xf60e91fc, 0x3269eecf, 0x3231c6e9, 0x945697c6]
  },
  {
      key: [0xffffffff, 0xffffffff, 0xfc000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xab69cfad, 0xf51f8e60, 0x4d9cc371, 0x82f6635a]
  },
  {
      key: [0xffffffff, 0xffffffff, 0xfe000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x7866373f, 0x24a0b6ed, 0x56e0d96f, 0xcdafb877]
  },
  {
      key: [0xffffffff, 0xffffffff, 0xff000000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x1ea448c2, 0xaac954f5, 0xd812e9d7, 0x8494446a]
  },
  {
      key: [0xffffffff, 0xffffffff, 0xff800000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xacc5599d, 0xd8ac0223, 0x9a0fef4a, 0x36dd1668]
  },
  {
      key: [0xffffffff, 0xffffffff, 0xffc00000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xd8764468, 0xbb103828, 0xcf7e1473, 0xce895073]
  },
  {
      key: [0xffffffff, 0xffffffff, 0xffe00000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x1b0d0289, 0x3683b9f1, 0x80458e4a, 0xa6b73982]
  },
  {
      key: [0xffffffff, 0xffffffff, 0xfff00000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x96d9b017, 0xd302df41, 0x0a937dcd, 0xb8bb6e43]
  },
  {
      key: [0xffffffff, 0xffffffff, 0xfff80000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xef1623cc, 0x44313cff, 0x440b1594, 0xa7e21cc6]
  },
  {
      key: [0xffffffff, 0xffffffff, 0xfffc0000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x284ca2fa, 0x35807b8b, 0x0ae4d19e, 0x11d7dbd7]
  },
  {
      key: [0xffffffff, 0xffffffff, 0xfffe0000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xf2e97687, 0x5755f940, 0x1d54f36e, 0x2a23a594]
  },
  {
      key: [0xffffffff, 0xffffffff, 0xffff0000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xec198a18, 0xe10e5324, 0x03b7e208, 0x87c8dd80]
  },
  {
      key: [0xffffffff, 0xffffffff, 0xffff8000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x545d50eb, 0xd919e4a6, 0x949d96ad, 0x47e46a80]
  },
  {
      key: [0xffffffff, 0xffffffff, 0xffffc000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xdbdfb527, 0x060e0a71, 0x009c7bb0, 0xc68f1d44]
  },
  {
      key: [0xffffffff, 0xffffffff, 0xffffe000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x9cfa1322, 0xea33da21, 0x73a024f2, 0xff0d896d]
  },
  {
      key: [0xffffffff, 0xffffffff, 0xfffff000, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x8785b1a7, 0x5b0f3bd9, 0x58dcd0e2, 0x9318c521]
  },
  {
      key: [0xffffffff, 0xffffffff, 0xfffff800, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x38f67b9e, 0x98e4a97b, 0x6df030a9, 0xfcdd0104]
  },
  {
      key: [0xffffffff, 0xffffffff, 0xfffffc00, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x192afffb, 0x2c880e82, 0xb05926d0, 0xfc6c448b]
  },
  {
      key: [0xffffffff, 0xffffffff, 0xfffffe00, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x6a7980ce, 0x7b105cf5, 0x30952d74, 0xdaaf798c]
  },
  {
      key: [0xffffffff, 0xffffffff, 0xffffff00, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xea3695e1, 0x351b9d68, 0x58bd958c, 0xf513ef6c]
  },
  {
      key: [0xffffffff, 0xffffffff, 0xffffff80, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x6da0490b, 0xa0ba0343, 0xb935681d, 0x2cce5ba1]
  },
  {
      key: [0xffffffff, 0xffffffff, 0xffffffc0, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xf0ea23af, 0x08534011, 0xc60009ab, 0x29ada2f1]
  },
  {
      key: [0xffffffff, 0xffffffff, 0xffffffe0, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xff13806c, 0xf19cc387, 0x21554d7c, 0x0fcdcd4b]
  },
  {
      key: [0xffffffff, 0xffffffff, 0xfffffff0, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x6838af1f, 0x4f69bae9, 0xd85dd188, 0xdcdf0688]
  },
  {
      key: [0xffffffff, 0xffffffff, 0xfffffff8, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0x36cf44c9, 0x2d550bfb, 0x1ed28ef5, 0x83ddf5d7]
  },
  {
      key: [0xffffffff, 0xffffffff, 0xfffffffc, 0x00000000],
      pt: [0x00000000, 0x00000000, 0x00000000, 0x00000000],
      ct: [0xd06e3195, 0xb5376f10, 0x9d5c4ec6, 0xc5d62ced]
  }
];

new sjcl.test.TestCase("AES official known-answer tests", function (cb) {
    if (!sjcl.cipher.aes) {
        this.unimplemented();
        cb && cb();
        return;
    }

    var i, kat = sjcl.test.vector.aes, tv, len, aes;

    //XXX add more vectors instead of looping
    for (var index = 0; index < 8; index++) {
        for (i = 0; i < kat.length; i++) {
            tv = kat[i];
            len = 32 * tv.key.length;
            aes = new sjcl.cipher.aes(tv.key);
            var ct = aes.encrypt(tv.pt);
            var pt = aes.decrypt(tv.ct);
            this.require(sjcl.bitArray.equal(ct, tv.ct), "encrypt " + len + " #" + i);
            this.require(sjcl.bitArray.equal(pt, tv.pt), "decrypt " + len + " #" + i);
        }
    }
    cb && cb();
});

sjcl.test.run();
