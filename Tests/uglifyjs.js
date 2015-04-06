!function (exports, global) {
    "use strict";
    function array_to_hash(a) {
        for (var ret = Object.create(null), i = 0; i < a.length; ++i) ret[a[i]] = !0;
        return ret;
    }
    function slice(a, start) {
        return Array.prototype.slice.call(a, start || 0);
    }
    function characters(str) {
        return str.split("");
    }
    function member(name, array) {
        for (var i = array.length; --i >= 0;) if (array[i] == name) return !0;
        return !1;
    }
    function find_if(func, array) {
        for (var i = 0, n = array.length; n > i; ++i) if (func(array[i])) return array[i];
    }
    function repeat_string(str, i) {
        if (0 >= i) return "";
        if (1 == i) return str;
        var d = repeat_string(str, i >> 1);
        return d += d, 1 & i && (d += str), d;
    }
    function DefaultsError(msg, defs) {
        Error.call(this, msg), this.msg = msg, this.defs = defs;
    }
    function defaults(args, defs, croak) {
        args === !0 && (args = {});
        var ret = args || {};
        if (croak) for (var i in ret) ret.hasOwnProperty(i) && !defs.hasOwnProperty(i) && DefaultsError.croak("`" + i + "` is not a supported option", defs);
        for (var i in defs) defs.hasOwnProperty(i) && (ret[i] = args && args.hasOwnProperty(i) ? args[i] : defs[i]);
        return ret;
    }
    function merge(obj, ext) {
        var count = 0;
        for (var i in ext) ext.hasOwnProperty(i) && (obj[i] = ext[i], count++);
        return count;
    }
    function noop() { }
    function push_uniq(array, el) {
        array.indexOf(el) < 0 && array.push(el);
    }
    function string_template(text, props) {
        return text.replace(/\{(.+?)\}/g, function (str, p) {
            return props[p];
        });
    }
    function remove(array, el) {
        for (var i = array.length; --i >= 0;) array[i] === el && array.splice(i, 1);
    }
    function mergeSort(array, cmp) {
        function merge(a, b) {
            for (var r = [], ai = 0, bi = 0, i = 0; ai < a.length && bi < b.length;) r[i++] = cmp(a[ai], b[bi]) <= 0 ? a[ai++] : b[bi++];
            return ai < a.length && r.push.apply(r, a.slice(ai)), bi < b.length && r.push.apply(r, b.slice(bi)),
            r;
        }
        function _ms(a) {
            if (a.length <= 1) return a;
            var m = Math.floor(a.length / 2), left = a.slice(0, m), right = a.slice(m);
            return left = _ms(left), right = _ms(right), merge(left, right);
        }
        return array.length < 2 ? array.slice() : _ms(array);
    }
    function set_difference(a, b) {
        return a.filter(function (el) {
            return b.indexOf(el) < 0;
        });
    }
    function set_intersection(a, b) {
        return a.filter(function (el) {
            return b.indexOf(el) >= 0;
        });
    }
    function makePredicate(words) {
        function compareTo(arr) {
            if (1 == arr.length) return f += "return str === " + JSON.stringify(arr[0]) + ";";
            f += "switch(str){";
            for (var i = 0; i < arr.length; ++i) f += "case " + JSON.stringify(arr[i]) + ":";
            f += "return true}return false;";
        }
        words instanceof Array || (words = words.split(" "));
        var f = "", cats = [];
        out: for (var i = 0; i < words.length; ++i) {
            for (var j = 0; j < cats.length; ++j) if (cats[j][0].length == words[i].length) {
                cats[j].push(words[i]);
                continue out;
            }
            cats.push([words[i]]);
        }
        if (cats.length > 3) {
            cats.sort(function (a, b) {
                return b.length - a.length;
            }), f += "switch(str.length){";
            for (var i = 0; i < cats.length; ++i) {
                var cat = cats[i];
                f += "case " + cat[0].length + ":", compareTo(cat);
            }
            f += "}";
        } else compareTo(words);
        return new Function("str", f);
    }
    function all(array, predicate) {
        for (var i = array.length; --i >= 0;) if (!predicate(array[i])) return !1;
        return !0;
    }
    function Dictionary() {
        this._values = Object.create(null), this._size = 0;
    }
    function DEFNODE(type, props, methods, base) {
        arguments.length < 4 && (base = AST_Node), props = props ? props.split(/\s+/) : [];
        var self_props = props;
        base && base.PROPS && (props = props.concat(base.PROPS));
        for (var code = "return function AST_" + type + "(props){ if (props) { ", i = props.length; --i >= 0;) code += "this." + props[i] + " = props." + props[i] + ";";
        var proto = base && new base();
        (proto && proto.initialize || methods && methods.initialize) && (code += "this.initialize();"),
        code += "}}";
        var ctor = new Function(code)();
        if (proto && (ctor.prototype = proto, ctor.BASE = base), base && base.SUBCLASSES.push(ctor),
        ctor.prototype.CTOR = ctor, ctor.PROPS = props || null, ctor.SELF_PROPS = self_props,
        ctor.SUBCLASSES = [], type && (ctor.prototype.TYPE = ctor.TYPE = type), methods) for (i in methods) methods.hasOwnProperty(i) && (/^\$/.test(i) ? ctor[i.substr(1)] = methods[i] : ctor.prototype[i] = methods[i]);
        return ctor.DEFMETHOD = function (name, method) {
            this.prototype[name] = method;
        }, ctor;
    }
    function walk_body(node, visitor) {
        node.body instanceof AST_Statement ? node.body._walk(visitor) : node.body.forEach(function (stat) {
            stat._walk(visitor);
        });
    }
    function TreeWalker(callback) {
        this.visit = callback, this.stack = [];
    }
    function is_letter(code) {
        return code >= 97 && 122 >= code || code >= 65 && 90 >= code || code >= 170 && UNICODE.letter.test(String.fromCharCode(code));
    }
    function is_digit(code) {
        return code >= 48 && 57 >= code;
    }
    function is_alphanumeric_char(code) {
        return is_digit(code) || is_letter(code);
    }
    function is_unicode_digit(code) {
        return UNICODE.digit.test(String.fromCharCode(code));
    }
    function is_unicode_combining_mark(ch) {
        return UNICODE.non_spacing_mark.test(ch) || UNICODE.space_combining_mark.test(ch);
    }
    function is_unicode_connector_punctuation(ch) {
        return UNICODE.connector_punctuation.test(ch);
    }
    function is_identifier(name) {
        return !RESERVED_WORDS(name) && /^[a-z_$][a-z0-9_$]*$/i.test(name);
    }
    function is_identifier_start(code) {
        return 36 == code || 95 == code || is_letter(code);
    }
    function is_identifier_char(ch) {
        var code = ch.charCodeAt(0);
        return is_identifier_start(code) || is_digit(code) || 8204 == code || 8205 == code || is_unicode_combining_mark(ch) || is_unicode_connector_punctuation(ch) || is_unicode_digit(code);
    }
    function is_identifier_string(str) {
        return /^[a-z_$][a-z0-9_$]*$/i.test(str);
    }
    function parse_js_number(num) {
        return RE_HEX_NUMBER.test(num) ? parseInt(num.substr(2), 16) : RE_OCT_NUMBER.test(num) ? parseInt(num.substr(1), 8) : RE_DEC_NUMBER.test(num) ? parseFloat(num) : void 0;
    }
    function JS_Parse_Error(message, filename, line, col, pos) {
        this.message = message, this.filename = filename, this.line = line, this.col = col,
        this.pos = pos, this.stack = new Error().stack;
    }
    function js_error(message, filename, line, col, pos) {
        throw new JS_Parse_Error(message, filename, line, col, pos);
    }
    function is_token(token, type, val) {
        return token.type == type && (null == val || token.value == val);
    }
    function tokenizer($TEXT, filename, html5_comments) {
        function peek() {
            return S.text.charAt(S.pos);
        }
        function next(signal_eof, in_string) {
            var ch = S.text.charAt(S.pos++);
            if (signal_eof && !ch) throw EX_EOF;
            return "\r\n\u2028\u2029".indexOf(ch) >= 0 ? (S.newline_before = S.newline_before || !in_string,
            ++S.line, S.col = 0, in_string || "\r" != ch || "\n" != peek() || (++S.pos, ch = "\n")) : ++S.col,
            ch;
        }
        function forward(i) {
            for (; i-- > 0;) next();
        }
        function looking_at(str) {
            return S.text.substr(S.pos, str.length) == str;
        }
        function find(what, signal_eof) {
            var pos = S.text.indexOf(what, S.pos);
            if (signal_eof && -1 == pos) throw EX_EOF;
            return pos;
        }
        function start_token() {
            S.tokline = S.line, S.tokcol = S.col, S.tokpos = S.pos;
        }
        function token(type, value, is_comment) {
            S.regex_allowed = "operator" == type && !UNARY_POSTFIX(value) || "keyword" == type && KEYWORDS_BEFORE_EXPRESSION(value) || "punc" == type && PUNC_BEFORE_EXPRESSION(value),
            prev_was_dot = "punc" == type && "." == value;
            var ret = {
                type: type,
                value: value,
                line: S.tokline,
                col: S.tokcol,
                pos: S.tokpos,
                endline: S.line,
                endcol: S.col,
                endpos: S.pos,
                nlb: S.newline_before,
                file: filename
            };
            if (!is_comment) {
                ret.comments_before = S.comments_before, S.comments_before = [];
                for (var i = 0, len = ret.comments_before.length; len > i; i++) ret.nlb = ret.nlb || ret.comments_before[i].nlb;
            }
            return S.newline_before = !1, new AST_Token(ret);
        }
        function skip_whitespace() {
            for (var ch; WHITESPACE_CHARS(ch = peek()) || "\u2028" == ch || "\u2029" == ch;) next();
        }
        function read_while(pred) {
            for (var ch, ret = "", i = 0; (ch = peek()) && pred(ch, i++) ;) ret += next();
            return ret;
        }
        function parse_error(err) {
            js_error(err, filename, S.tokline, S.tokcol, S.tokpos);
        }
        function read_num(prefix) {
            var has_e = !1, after_e = !1, has_x = !1, has_dot = "." == prefix, num = read_while(function (ch, i) {
                var code = ch.charCodeAt(0);
                switch (code) {
                    case 120:
                    case 88:
                        return has_x ? !1 : has_x = !0;

                    case 101:
                    case 69:
                        return has_x ? !0 : has_e ? !1 : has_e = after_e = !0;

                    case 45:
                        return after_e || 0 == i && !prefix;

                    case 43:
                        return after_e;

                    case after_e = !1, 46:
                        return has_dot || has_x || has_e ? !1 : has_dot = !0;
                }
                return is_alphanumeric_char(code);
            });
            prefix && (num = prefix + num);
            var valid = parse_js_number(num);
            return isNaN(valid) ? void parse_error("Invalid syntax: " + num) : token("num", valid);
        }
        function read_escaped_char(in_string) {
            var ch = next(!0, in_string);
            switch (ch.charCodeAt(0)) {
                case 110:
                    return "\n";

                case 114:
                    return "\r";

                case 116:
                    return "	";

                case 98:
                    return "\b";

                case 118:
                    return "";

                case 102:
                    return "\f";

                case 48:
                    return "\x00";

                case 120:
                    return String.fromCharCode(hex_bytes(2));

                case 117:
                    return String.fromCharCode(hex_bytes(4));

                case 10:
                    return "";

                default:
                    return ch;
            }
        }
        function hex_bytes(n) {
            for (var num = 0; n > 0; --n) {
                var digit = parseInt(next(!0), 16);
                isNaN(digit) && parse_error("Invalid hex-character pattern in string"), num = num << 4 | digit;
            }
            return num;
        }
        function skip_line_comment(type) {
            var ret, regex_allowed = S.regex_allowed, i = find("\n");
            return -1 == i ? (ret = S.text.substr(S.pos), S.pos = S.text.length) : (ret = S.text.substring(S.pos, i),
            S.pos = i), S.col = S.tokcol + (S.pos - S.tokpos), S.comments_before.push(token(type, ret, !0)),
            S.regex_allowed = regex_allowed, next_token();
        }
        function read_name() {
            for (var ch, hex, backslash = !1, name = "", escaped = !1; null != (ch = peek()) ;) if (backslash) "u" != ch && parse_error("Expecting UnicodeEscapeSequence -- uXXXX"),
            ch = read_escaped_char(), is_identifier_char(ch) || parse_error("Unicode char: " + ch.charCodeAt(0) + " is not valid in identifier"),
            name += ch, backslash = !1; else if ("\\" == ch) escaped = backslash = !0, next(); else {
                if (!is_identifier_char(ch)) break;
                name += next();
            }
            return KEYWORDS(name) && escaped && (hex = name.charCodeAt(0).toString(16).toUpperCase(),
            name = "\\u" + "0000".substr(hex.length) + hex + name.slice(1)), name;
        }
        function read_operator(prefix) {
            function grow(op) {
                if (!peek()) return op;
                var bigger = op + peek();
                return OPERATORS(bigger) ? (next(), grow(bigger)) : op;
            }
            return token("operator", grow(prefix || next()));
        }
        function handle_slash() {
            switch (next(), peek()) {
                case "/":
                    return next(), skip_line_comment("comment1");

                case "*":
                    return next(), skip_multiline_comment();
            }
            return S.regex_allowed ? read_regexp("") : read_operator("/");
        }
        function handle_dot() {
            return next(), is_digit(peek().charCodeAt(0)) ? read_num(".") : token("punc", ".");
        }
        function read_word() {
            var word = read_name();
            return prev_was_dot ? token("name", word) : KEYWORDS_ATOM(word) ? token("atom", word) : KEYWORDS(word) ? OPERATORS(word) ? token("operator", word) : token("keyword", word) : token("name", word);
        }
        function with_eof_error(eof_error, cont) {
            return function (x) {
                try {
                    return cont(x);
                } catch (ex) {
                    if (ex !== EX_EOF) throw ex;
                    parse_error(eof_error);
                }
            };
        }
        function next_token(force_regexp) {
            if (null != force_regexp) return read_regexp(force_regexp);
            if (skip_whitespace(), start_token(), html5_comments) {
                if (looking_at("<!--")) return forward(4), skip_line_comment("comment3");
                if (looking_at("-->") && S.newline_before) return forward(3), skip_line_comment("comment4");
            }
            var ch = peek();
            if (!ch) return token("eof");
            var code = ch.charCodeAt(0);
            switch (code) {
                case 34:
                case 39:
                    return read_string(ch);

                case 46:
                    return handle_dot();

                case 47:
                    return handle_slash();
            }
            return is_digit(code) ? read_num() : PUNC_CHARS(ch) ? token("punc", next()) : OPERATOR_CHARS(ch) ? read_operator() : 92 == code || is_identifier_start(code) ? read_word() : void parse_error("Unexpected character '" + ch + "'");
        }
        var S = {
            text: $TEXT.replace(/\uFEFF/g, ""),
            filename: filename,
            pos: 0,
            tokpos: 0,
            line: 1,
            tokline: 0,
            col: 0,
            tokcol: 0,
            newline_before: !1,
            regex_allowed: !1,
            comments_before: []
        }, prev_was_dot = !1, read_string = with_eof_error("Unterminated string constant", function (quote_char) {
            for (var quote = next(), ret = ""; ;) {
                var ch = next(!0);
                if ("\\" == ch) {
                    var octal_len = 0, first = null;
                    ch = read_while(function (ch) {
                        if (ch >= "0" && "7" >= ch) {
                            if (!first) return first = ch, ++octal_len;
                            if ("3" >= first && 2 >= octal_len) return ++octal_len;
                            if (first >= "4" && 1 >= octal_len) return ++octal_len;
                        }
                        return !1;
                    }), ch = octal_len > 0 ? String.fromCharCode(parseInt(ch, 8)) : read_escaped_char(!0);
                } else if (ch == quote) break;
                ret += ch;
            }
            var tok = token("string", ret);
            return tok.quote = quote_char, tok;
        }), skip_multiline_comment = with_eof_error("Unterminated multiline comment", function () {
            var regex_allowed = S.regex_allowed, i = find("*/", !0), text = S.text.substring(S.pos, i), a = text.split("\n"), n = a.length;
            S.pos = i + 2, S.line += n - 1, n > 1 ? S.col = a[n - 1].length : S.col += a[n - 1].length,
            S.col += 2;
            var nlb = S.newline_before = S.newline_before || text.indexOf("\n") >= 0;
            return S.comments_before.push(token("comment2", text, !0)), S.regex_allowed = regex_allowed,
            S.newline_before = nlb, next_token();
        }), read_regexp = with_eof_error("Unterminated regular expression", function (regexp) {
            for (var ch, prev_backslash = !1, in_class = !1; ch = next(!0) ;) if (prev_backslash) regexp += "\\" + ch,
            prev_backslash = !1; else if ("[" == ch) in_class = !0, regexp += ch; else if ("]" == ch && in_class) in_class = !1,
            regexp += ch; else {
                if ("/" == ch && !in_class) break;
                "\\" == ch ? prev_backslash = !0 : regexp += ch;
            }
            var mods = read_name();
            return token("regexp", new RegExp(regexp, mods));
        });
        return next_token.context = function (nc) {
            return nc && (S = nc), S;
        }, next_token;
    }
    function parse($TEXT, options) {
        function is(type, value) {
            return is_token(S.token, type, value);
        }
        function peek() {
            return S.peeked || (S.peeked = S.input());
        }
        function next() {
            return S.prev = S.token, S.peeked ? (S.token = S.peeked, S.peeked = null) : S.token = S.input(),
            S.in_directives = S.in_directives && ("string" == S.token.type || is("punc", ";")),
            S.token;
        }
        function prev() {
            return S.prev;
        }
        function croak(msg, line, col, pos) {
            var ctx = S.input.context();
            js_error(msg, ctx.filename, null != line ? line : ctx.tokline, null != col ? col : ctx.tokcol, null != pos ? pos : ctx.tokpos);
        }
        function token_error(token, msg) {
            croak(msg, token.line, token.col);
        }
        function unexpected(token) {
            null == token && (token = S.token), token_error(token, "Unexpected token: " + token.type + " (" + token.value + ")");
        }
        function expect_token(type, val) {
            return is(type, val) ? next() : void token_error(S.token, "Unexpected token " + S.token.type + " «" + S.token.value + "», expected " + type + " «" + val + "»");
        }
        function expect(punc) {
            return expect_token("punc", punc);
        }
        function can_insert_semicolon() {
            return !options.strict && (S.token.nlb || is("eof") || is("punc", "}"));
        }
        function semicolon() {
            is("punc", ";") ? next() : can_insert_semicolon() || unexpected();
        }
        function parenthesised() {
            expect("(");
            var exp = expression(!0);
            return expect(")"), exp;
        }
        function embed_tokens(parser) {
            return function () {
                var start = S.token, expr = parser(), end = prev();
                return expr.start = start, expr.end = end, expr;
            };
        }
        function handle_regexp() {
            (is("operator", "/") || is("operator", "/=")) && (S.peeked = null, S.token = S.input(S.token.value.substr(1)));
        }
        function labeled_statement() {
            var label = as_symbol(AST_Label);
            find_if(function (l) {
                return l.name == label.name;
            }, S.labels) && croak("Label " + label.name + " defined twice"), expect(":"), S.labels.push(label);
            var stat = statement();
            return S.labels.pop(), stat instanceof AST_IterationStatement || label.references.forEach(function (ref) {
                ref instanceof AST_Continue && (ref = ref.label.start, croak("Continue label `" + label.name + "` refers to non-IterationStatement.", ref.line, ref.col, ref.pos));
            }), new AST_LabeledStatement({
                body: stat,
                label: label
            });
        }
        function simple_statement(tmp) {
            return new AST_SimpleStatement({
                body: (tmp = expression(!0), semicolon(), tmp)
            });
        }
        function break_cont(type) {
            var ldef, label = null;
            can_insert_semicolon() || (label = as_symbol(AST_LabelRef, !0)), null != label ? (ldef = find_if(function (l) {
                return l.name == label.name;
            }, S.labels), ldef || croak("Undefined label " + label.name), label.thedef = ldef) : 0 == S.in_loop && croak(type.TYPE + " not inside a loop or switch"),
            semicolon();
            var stat = new type({
                label: label
            });
            return ldef && ldef.references.push(stat), stat;
        }
        function for_() {
            expect("(");
            var init = null;
            return !is("punc", ";") && (init = is("keyword", "var") ? (next(), var_(!0)) : expression(!0, !0),
            is("operator", "in")) ? (init instanceof AST_Var && init.definitions.length > 1 && croak("Only one variable declaration allowed in for..in loop"),
            next(), for_in(init)) : regular_for(init);
        }
        function regular_for(init) {
            expect(";");
            var test = is("punc", ";") ? null : expression(!0);
            expect(";");
            var step = is("punc", ")") ? null : expression(!0);
            return expect(")"), new AST_For({
                init: init,
                condition: test,
                step: step,
                body: in_loop(statement)
            });
        }
        function for_in(init) {
            var lhs = init instanceof AST_Var ? init.definitions[0].name : null, obj = expression(!0);
            return expect(")"), new AST_ForIn({
                init: init,
                name: lhs,
                object: obj,
                body: in_loop(statement)
            });
        }
        function if_() {
            var cond = parenthesised(), body = statement(), belse = null;
            return is("keyword", "else") && (next(), belse = statement()), new AST_If({
                condition: cond,
                body: body,
                alternative: belse
            });
        }
        function block_() {
            expect("{");
            for (var a = []; !is("punc", "}") ;) is("eof") && unexpected(), a.push(statement());
            return next(), a;
        }
        function switch_body_() {
            expect("{");
            for (var tmp, a = [], cur = null, branch = null; !is("punc", "}") ;) is("eof") && unexpected(),
            is("keyword", "case") ? (branch && (branch.end = prev()), cur = [], branch = new AST_Case({
                start: (tmp = S.token, next(), tmp),
                expression: expression(!0),
                body: cur
            }), a.push(branch), expect(":")) : is("keyword", "default") ? (branch && (branch.end = prev()),
            cur = [], branch = new AST_Default({
                start: (tmp = S.token, next(), expect(":"), tmp),
                body: cur
            }), a.push(branch)) : (cur || unexpected(), cur.push(statement()));
            return branch && (branch.end = prev()), next(), a;
        }
        function try_() {
            var body = block_(), bcatch = null, bfinally = null;
            if (is("keyword", "catch")) {
                var start = S.token;
                next(), expect("(");
                var name = as_symbol(AST_SymbolCatch);
                expect(")"), bcatch = new AST_Catch({
                    start: start,
                    argname: name,
                    body: block_(),
                    end: prev()
                });
            }
            if (is("keyword", "finally")) {
                var start = S.token;
                next(), bfinally = new AST_Finally({
                    start: start,
                    body: block_(),
                    end: prev()
                });
            }
            return bcatch || bfinally || croak("Missing catch/finally blocks"), new AST_Try({
                body: body,
                bcatch: bcatch,
                bfinally: bfinally
            });
        }
        function vardefs(no_in, in_const) {
            for (var a = []; a.push(new AST_VarDef({
                start: S.token,
                name: as_symbol(in_const ? AST_SymbolConst : AST_SymbolVar),
                value: is("operator", "=") ? (next(), expression(!1, no_in)) : null,
                end: prev()
            })), is("punc", ",") ;) next();
            return a;
        }
        function as_atom_node() {
            var ret, tok = S.token;
            switch (tok.type) {
                case "name":
                case "keyword":
                    ret = _make_symbol(AST_SymbolRef);
                    break;

                case "num":
                    ret = new AST_Number({
                        start: tok,
                        end: tok,
                        value: tok.value
                    });
                    break;

                case "string":
                    ret = new AST_String({
                        start: tok,
                        end: tok,
                        value: tok.value,
                        quote: tok.quote
                    });
                    break;

                case "regexp":
                    ret = new AST_RegExp({
                        start: tok,
                        end: tok,
                        value: tok.value
                    });
                    break;

                case "atom":
                    switch (tok.value) {
                        case "false":
                            ret = new AST_False({
                                start: tok,
                                end: tok
                            });
                            break;

                        case "true":
                            ret = new AST_True({
                                start: tok,
                                end: tok
                            });
                            break;

                        case "null":
                            ret = new AST_Null({
                                start: tok,
                                end: tok
                            });
                    }
            }
            return next(), ret;
        }
        function expr_list(closing, allow_trailing_comma, allow_empty) {
            for (var first = !0, a = []; !is("punc", closing) && (first ? first = !1 : expect(","),
            !allow_trailing_comma || !is("punc", closing)) ;) a.push(is("punc", ",") && allow_empty ? new AST_Hole({
                start: S.token,
                end: S.token
            }) : expression(!1));
            return next(), a;
        }
        function as_property_name() {
            var tmp = S.token;
            switch (next(), tmp.type) {
                case "num":
                case "string":
                case "name":
                case "operator":
                case "keyword":
                case "atom":
                    return tmp.value;

                default:
                    unexpected();
            }
        }
        function as_name() {
            var tmp = S.token;
            switch (next(), tmp.type) {
                case "name":
                case "operator":
                case "keyword":
                case "atom":
                    return tmp.value;

                default:
                    unexpected();
            }
        }
        function _make_symbol(type) {
            var name = S.token.value;
            return new ("this" == name ? AST_This : type)({
                name: String(name),
                start: S.token,
                end: S.token
            });
        }
        function as_symbol(type, noerror) {
            if (!is("name")) return noerror || croak("Name expected"), null;
            var sym = _make_symbol(type);
            return next(), sym;
        }
        function make_unary(ctor, op, expr) {
            return "++" != op && "--" != op || is_assignable(expr) || croak("Invalid use of " + op + " operator"),
            new ctor({
                operator: op,
                expression: expr
            });
        }
        function expr_ops(no_in) {
            return expr_op(maybe_unary(!0), 0, no_in);
        }
        function is_assignable(expr) {
            return options.strict ? expr instanceof AST_This ? !1 : expr instanceof AST_PropAccess || expr instanceof AST_Symbol : !0;
        }
        function in_loop(cont) {
            ++S.in_loop;
            var ret = cont();
            return --S.in_loop, ret;
        }
        options = defaults(options, {
            strict: !1,
            filename: null,
            toplevel: null,
            expression: !1,
            html5_comments: !0,
            bare_returns: !1
        });
        var S = {
            input: "string" == typeof $TEXT ? tokenizer($TEXT, options.filename, options.html5_comments) : $TEXT,
            token: null,
            prev: null,
            peeked: null,
            in_function: 0,
            in_directives: !0,
            in_loop: 0,
            labels: []
        };
        S.token = next();
        var statement = embed_tokens(function () {
            var tmp;
            switch (handle_regexp(), S.token.type) {
                case "string":
                    var dir = S.in_directives, stat = simple_statement();
                    return dir && stat.body instanceof AST_String && !is("punc", ",") ? new AST_Directive({
                        start: stat.body.start,
                        end: stat.body.end,
                        quote: stat.body.quote,
                        value: stat.body.value
                    }) : stat;

                case "num":
                case "regexp":
                case "operator":
                case "atom":
                    return simple_statement();

                case "name":
                    return is_token(peek(), "punc", ":") ? labeled_statement() : simple_statement();

                case "punc":
                    switch (S.token.value) {
                        case "{":
                            return new AST_BlockStatement({
                                start: S.token,
                                body: block_(),
                                end: prev()
                            });

                        case "[":
                        case "(":
                            return simple_statement();

                        case ";":
                            return next(), new AST_EmptyStatement();

                        default:
                            unexpected();
                    }

                case "keyword":
                    switch (tmp = S.token.value, next(), tmp) {
                        case "break":
                            return break_cont(AST_Break);

                        case "continue":
                            return break_cont(AST_Continue);

                        case "debugger":
                            return semicolon(), new AST_Debugger();

                        case "do":
                            return new AST_Do({
                                body: in_loop(statement),
                                condition: (expect_token("keyword", "while"), tmp = parenthesised(), semicolon(),
                                tmp)
                            });

                        case "while":
                            return new AST_While({
                                condition: parenthesised(),
                                body: in_loop(statement)
                            });

                        case "for":
                            return for_();

                        case "function":
                            return function_(AST_Defun);

                        case "if":
                            return if_();

                        case "return":
                            return 0 != S.in_function || options.bare_returns || croak("'return' outside of function"),
                            new AST_Return({
                                value: is("punc", ";") ? (next(), null) : can_insert_semicolon() ? null : (tmp = expression(!0),
                                semicolon(), tmp)
                            });

                        case "switch":
                            return new AST_Switch({
                                expression: parenthesised(),
                                body: in_loop(switch_body_)
                            });

                        case "throw":
                            return S.token.nlb && croak("Illegal newline after 'throw'"), new AST_Throw({
                                value: (tmp = expression(!0), semicolon(), tmp)
                            });

                        case "try":
                            return try_();

                        case "var":
                            return tmp = var_(), semicolon(), tmp;

                        case "const":
                            return tmp = const_(), semicolon(), tmp;

                        case "with":
                            return new AST_With({
                                expression: parenthesised(),
                                body: statement()
                            });

                        default:
                            unexpected();
                    }
            }
        }), function_ = function (ctor) {
            var in_statement = ctor === AST_Defun, name = is("name") ? as_symbol(in_statement ? AST_SymbolDefun : AST_SymbolLambda) : null;
            return in_statement && !name && unexpected(), expect("("), new ctor({
                name: name,
                argnames: function (first, a) {
                    for (; !is("punc", ")") ;) first ? first = !1 : expect(","), a.push(as_symbol(AST_SymbolFunarg));
                    return next(), a;
                }(!0, []),
                body: function (loop, labels) {
                    ++S.in_function, S.in_directives = !0, S.in_loop = 0, S.labels = [];
                    var a = block_();
                    return --S.in_function, S.in_loop = loop, S.labels = labels, a;
                }(S.in_loop, S.labels)
            });
        }, var_ = function (no_in) {
            return new AST_Var({
                start: prev(),
                definitions: vardefs(no_in, !1),
                end: prev()
            });
        }, const_ = function () {
            return new AST_Const({
                start: prev(),
                definitions: vardefs(!1, !0),
                end: prev()
            });
        }, new_ = function () {
            var start = S.token;
            expect_token("operator", "new");
            var args, newexp = expr_atom(!1);
            return is("punc", "(") ? (next(), args = expr_list(")")) : args = [], subscripts(new AST_New({
                start: start,
                expression: newexp,
                args: args,
                end: prev()
            }), !0);
        }, expr_atom = function (allow_calls) {
            if (is("operator", "new")) return new_();
            var start = S.token;
            if (is("punc")) {
                switch (start.value) {
                    case "(":
                        next();
                        var ex = expression(!0);
                        return ex.start = start, ex.end = S.token, expect(")"), subscripts(ex, allow_calls);

                    case "[":
                        return subscripts(array_(), allow_calls);

                    case "{":
                        return subscripts(object_(), allow_calls);
                }
                unexpected();
            }
            if (is("keyword", "function")) {
                next();
                var func = function_(AST_Function);
                return func.start = start, func.end = prev(), subscripts(func, allow_calls);
            }
            return ATOMIC_START_TOKEN[S.token.type] ? subscripts(as_atom_node(), allow_calls) : void unexpected();
        }, array_ = embed_tokens(function () {
            return expect("["), new AST_Array({
                elements: expr_list("]", !options.strict, !0)
            });
        }), object_ = embed_tokens(function () {
            expect("{");
            for (var first = !0, a = []; !is("punc", "}") && (first ? first = !1 : expect(","),
            options.strict || !is("punc", "}")) ;) {
                var start = S.token, type = start.type, name = as_property_name();
                if ("name" == type && !is("punc", ":")) {
                    if ("get" == name) {
                        a.push(new AST_ObjectGetter({
                            start: start,
                            key: as_atom_node(),
                            value: function_(AST_Accessor),
                            end: prev()
                        }));
                        continue;
                    }
                    if ("set" == name) {
                        a.push(new AST_ObjectSetter({
                            start: start,
                            key: as_atom_node(),
                            value: function_(AST_Accessor),
                            end: prev()
                        }));
                        continue;
                    }
                }
                expect(":"), a.push(new AST_ObjectKeyVal({
                    start: start,
                    quote: start.quote,
                    key: name,
                    value: expression(!1),
                    end: prev()
                }));
            }
            return next(), new AST_Object({
                properties: a
            });
        }), subscripts = function (expr, allow_calls) {
            var start = expr.start;
            if (is("punc", ".")) return next(), subscripts(new AST_Dot({
                start: start,
                expression: expr,
                property: as_name(),
                end: prev()
            }), allow_calls);
            if (is("punc", "[")) {
                next();
                var prop = expression(!0);
                return expect("]"), subscripts(new AST_Sub({
                    start: start,
                    expression: expr,
                    property: prop,
                    end: prev()
                }), allow_calls);
            }
            return allow_calls && is("punc", "(") ? (next(), subscripts(new AST_Call({
                start: start,
                expression: expr,
                args: expr_list(")"),
                end: prev()
            }), !0)) : expr;
        }, maybe_unary = function (allow_calls) {
            var start = S.token;
            if (is("operator") && UNARY_PREFIX(start.value)) {
                next(), handle_regexp();
                var ex = make_unary(AST_UnaryPrefix, start.value, maybe_unary(allow_calls));
                return ex.start = start, ex.end = prev(), ex;
            }
            for (var val = expr_atom(allow_calls) ; is("operator") && UNARY_POSTFIX(S.token.value) && !S.token.nlb;)
                val = make_unary(AST_UnaryPostfix, S.token.value, val), val.start = start, val.end = S.token, next();
            return val;
        }, expr_op = function (left, min_prec, no_in) {
            var op = is("operator") ? S.token.value : null;
            "in" == op && no_in && (op = null);
            var prec = null != op ? PRECEDENCE[op] : null;
            if (null != prec && prec > min_prec) {
                next();
                var right = expr_op(maybe_unary(!0), prec, no_in);
                return expr_op(new AST_Binary({
                    start: left.start,
                    left: left,
                    operator: op,
                    right: right,
                    end: right.end
                }), min_prec, no_in);
            }
            return left;
        }, maybe_conditional = function (no_in) {
            var start = S.token, expr = expr_ops(no_in);
            if (is("operator", "?")) {
                next();
                var yes = expression(!1);
                return expect(":"), new AST_Conditional({
                    start: start,
                    condition: expr,
                    consequent: yes,
                    alternative: expression(!1, no_in),
                    end: prev()
                });
            }
            return expr;
        }, maybe_assign = function (no_in) {
            var start = S.token, left = maybe_conditional(no_in), val = S.token.value;
            if (is("operator") && ASSIGNMENT(val)) {
                if (is_assignable(left)) return next(), new AST_Assign({
                    start: start,
                    left: left,
                    operator: val,
                    right: maybe_assign(no_in),
                    end: prev()
                });
                croak("Invalid assignment");
            }
            return left;
        }, expression = function (commas, no_in) {
            var start = S.token, expr = maybe_assign(no_in);
            return commas && is("punc", ",") ? (next(), new AST_Seq({
                start: start,
                car: expr,
                cdr: expression(!0, no_in),
                end: peek()
            })) : expr;
        };
        return options.expression ? expression(!0) : function () {
            for (var start = S.token, body = []; !is("eof") ;) body.push(statement());
            var end = prev(), toplevel = options.toplevel;
            return toplevel ? (toplevel.body = toplevel.body.concat(body), toplevel.end = end) : toplevel = new AST_Toplevel({
                start: start,
                body: body,
                end: end
            }), toplevel;
        }();
    }
    function TreeTransformer(before, after) {
        TreeWalker.call(this), this.before = before, this.after = after;
    }
    function SymbolDef(scope, index, orig) {
        this.name = orig.name, this.orig = [orig], this.scope = scope, this.references = [],
        this.global = !1, this.mangled_name = null, this.undeclared = !1, this.constant = !1,
        this.index = index;
    }
    function OutputStream(options) {
        function to_ascii(str, identifier) {
            return str.replace(/[\u0080-\uffff]/g, function (ch) {
                var code = ch.charCodeAt(0).toString(16);
                if (code.length <= 2 && !identifier) {
                    for (; code.length < 2;) code = "0" + code;
                    return "\\x" + code;
                }
                for (; code.length < 4;) code = "0" + code;
                return "\\u" + code;
            });
        }
        function make_string(str, quote) {
            function quote_single() {
                return "'" + str.replace(/\x27/g, "\\'") + "'";
            }
            function quote_double() {
                return '"' + str.replace(/\x22/g, '\\"') + '"';
            }
            var dq = 0, sq = 0;
            switch (str = str.replace(/[\\\b\f\n\r\t\x22\x27\u2028\u2029\0\ufeff]/g, function (s) {
                switch (s) {
                  case "\\":
                    return "\\\\";

                  case "\b":
                    return "\\b";

                  case "\f":
                    return "\\f";

                  case "\n":
                    return "\\n";

                  case "\r":
                    return "\\r";

                  case "\u2028":
                    return "\\u2028";

                  case "\u2029":
                    return "\\u2029";

                  case '"':
                    return ++dq, '"';

                  case "'":
                    return ++sq, "'";

                  case "\x00":
                    return "\\x00";

                  case "\ufeff":
                    return "\\ufeff";
            }
                return s;
            }), options.ascii_only && (str = to_ascii(str)), options.quote_style) {
                case 1:
                    return quote_single();

                case 2:
                    return quote_double();

                case 3:
                    return "'" == quote ? quote_single() : quote_double();

                default:
                    return dq > sq ? quote_single() : quote_double();
            }
        }
        function encode_string(str, quote) {
            var ret = make_string(str, quote);
            return options.inline_script && (ret = ret.replace(/<\x2fscript([>\/\t\n\f\r ])/gi, "<\\/script$1")),
            ret;
        }
        function make_name(name) {
            return name = name.toString(), options.ascii_only && (name = to_ascii(name, !0)),
            name;
        }
        function make_indent(back) {
            return repeat_string(" ", options.indent_start + indentation - back * options.indent_level);
        }
        function last_char() {
            return last.charAt(last.length - 1);
        }
        function maybe_newline() {
            options.max_line_len && current_col > options.max_line_len && print("\n");
        }
        function print(str) {
            str = String(str);
            var ch = str.charAt(0);
            if (might_need_semicolon && (ch && !(";}".indexOf(ch) < 0) || /[;]$/.test(last) || (options.semicolons || requireSemicolonChars(ch) ? (OUTPUT += ";",
            current_col++, current_pos++) : (OUTPUT += "\n", current_pos++, current_line++,
            current_col = 0), options.beautify || (might_need_space = !1)), might_need_semicolon = !1,
            maybe_newline()), !options.beautify && options.preserve_line && stack[stack.length - 1]) for (var target_line = stack[stack.length - 1].start.line; target_line > current_line;) OUTPUT += "\n",
            current_pos++, current_line++, current_col = 0, might_need_space = !1;
            if (might_need_space) {
                var prev = last_char();
                (is_identifier_char(prev) && (is_identifier_char(ch) || "\\" == ch) || /^[\+\-\/]$/.test(ch) && ch == prev) && (OUTPUT += " ",
                current_col++, current_pos++), might_need_space = !1;
            }
            var a = str.split(/\r?\n/), n = a.length - 1;
            current_line += n, 0 == n ? current_col += a[n].length : current_col = a[n].length,
            current_pos += str.length, last = str, OUTPUT += str;
        }
        function force_semicolon() {
            might_need_semicolon = !1, print(";");
        }
        function next_indent() {
            return indentation + options.indent_level;
        }
        function with_block(cont) {
            var ret;
            return print("{"), newline(), with_indent(next_indent(), function () {
                ret = cont();
            }), indent(), print("}"), ret;
        }
        function with_parens(cont) {
            print("(");
            var ret = cont();
            return print(")"), ret;
        }
        function with_square(cont) {
            print("[");
            var ret = cont();
            return print("]"), ret;
        }
        function comma() {
            print(","), space();
        }
        function colon() {
            print(":"), options.space_colon && space();
        }
        function get() {
            return OUTPUT;
        }
        options = defaults(options, {
            indent_start: 0,
            indent_level: 4,
            quote_keys: !1,
            space_colon: !0,
            ascii_only: !1,
            unescape_regexps: !1,
            inline_script: !1,
            width: 80,
            max_line_len: 32e3,
            beautify: !1,
            source_map: null,
            bracketize: !1,
            semicolons: !0,
            comments: !1,
            preserve_line: !1,
            screw_ie8: !1,
            preamble: null,
            quote_style: 0
        }, !0);
        var indentation = 0, current_col = 0, current_line = 1, current_pos = 0, OUTPUT = "", might_need_space = !1, might_need_semicolon = !1, last = null, requireSemicolonChars = makePredicate("( [ + * / - , ."), space = options.beautify ? function () {
            print(" ");
        } : function () {
            might_need_space = !0;
        }, indent = options.beautify ? function (half) {
            options.beautify && print(make_indent(half ? .5 : 0));
        } : noop, with_indent = options.beautify ? function (col, cont) {
            col === !0 && (col = next_indent());
            var save_indentation = indentation;
            indentation = col;
            var ret = cont();
            return indentation = save_indentation, ret;
        } : function (col, cont) {
            return cont();
        }, newline = options.beautify ? function () {
            print("\n");
        } : maybe_newline, semicolon = options.beautify ? function () {
            print(";");
        } : function () {
            might_need_semicolon = !0;
        }, add_mapping = options.source_map ? function (token, name) {
            try {
                token && options.source_map.add(token.file || "?", current_line, current_col, token.line, token.col, name || "name" != token.type ? name : token.value);
            } catch (ex) {
                AST_Node.warn("Couldn't figure out mapping for {file}:{line},{col} → {cline},{ccol} [{name}]", {
                    file: token.file,
                    line: token.line,
                    col: token.col,
                    cline: current_line,
                    ccol: current_col,
                    name: name || ""
                });
            }
        } : noop;
        options.preamble && print(options.preamble.replace(/\r\n?|[\n\u2028\u2029]|\s*$/g, "\n"));
        var stack = [];
        return {
            get: get,
            toString: get,
            indent: indent,
            indentation: function () {
                return indentation;
            },
            current_width: function () {
                return current_col - indentation;
            },
            should_break: function () {
                return options.width && this.current_width() >= options.width;
            },
            newline: newline,
            print: print,
            space: space,
            comma: comma,
            colon: colon,
            last: function () {
                return last;
            },
            semicolon: semicolon,
            force_semicolon: force_semicolon,
            to_ascii: to_ascii,
            print_name: function (name) {
                print(make_name(name));
            },
            print_string: function (str, quote) {
                print(encode_string(str, quote));
            },
            next_indent: next_indent,
            with_indent: with_indent,
            with_block: with_block,
            with_parens: with_parens,
            with_square: with_square,
            add_mapping: add_mapping,
            option: function (opt) {
                return options[opt];
            },
            line: function () {
                return current_line;
            },
            col: function () {
                return current_col;
            },
            pos: function () {
                return current_pos;
            },
            push_node: function (node) {
                stack.push(node);
            },
            pop_node: function () {
                return stack.pop();
            },
            stack: function () {
                return stack;
            },
            parent: function (n) {
                return stack[stack.length - 2 - (n || 0)];
            }
        };
    }
    function Compressor(options, false_by_default) {
        return this instanceof Compressor ? (TreeTransformer.call(this, this.before, this.after),
        void (this.options = defaults(options, {
            sequences: !false_by_default,
            properties: !false_by_default,
            dead_code: !false_by_default,
            drop_debugger: !false_by_default,
            unsafe: !1,
            unsafe_comps: !1,
            conditionals: !false_by_default,
            comparisons: !false_by_default,
            evaluate: !false_by_default,
            booleans: !false_by_default,
            loops: !false_by_default,
            unused: !false_by_default,
            hoist_funs: !false_by_default,
            keep_fargs: !1,
            keep_fnames: !1,
            hoist_vars: !1,
            if_return: !false_by_default,
            join_vars: !false_by_default,
            cascade: !false_by_default,
            side_effects: !false_by_default,
            pure_getters: !1,
            pure_funcs: null,
            negate_iife: !false_by_default,
            screw_ie8: !1,
            drop_console: !1,
            angular: !1,
            warnings: !0,
            global_defs: {}
        }, !0))) : new Compressor(options, false_by_default);
    }
    function SourceMap(options) {
        function add(source, gen_line, gen_col, orig_line, orig_col, name) {
            if (orig_map) {
                var info = orig_map.originalPositionFor({
                    line: orig_line,
                    column: orig_col
                });
                if (null === info.source) return;
                source = info.source, orig_line = info.line, orig_col = info.column, name = info.name || name;
            }
            generator.addMapping({
                generated: {
                    line: gen_line + options.dest_line_diff,
                    column: gen_col
                },
                original: {
                    line: orig_line + options.orig_line_diff,
                    column: orig_col
                },
                source: source,
                name: name
            });
        }
        options = defaults(options, {
            file: null,
            root: null,
            orig: null,
            orig_line_diff: 0,
            dest_line_diff: 0
        });
        var generator, orig_map = options.orig && new MOZ_SourceMap.SourceMapConsumer(options.orig);
        return generator = orig_map ? MOZ_SourceMap.SourceMapGenerator.fromSourceMap(orig_map) : new MOZ_SourceMap.SourceMapGenerator({
            file: options.file,
            sourceRoot: options.root
        }), {
            add: add,
            get: function () {
                return generator;
            },
            toString: function () {
                return JSON.stringify(generator.toJSON());
            }
        };
    }
    function find_builtins() {
        function add(name) {
            push_uniq(a, name);
        }
        var a = [];
        return [Object, Array, Function, Number, String, Boolean, Error, Math, Date, RegExp].forEach(function (ctor) {
            Object.getOwnPropertyNames(ctor).map(add), ctor.prototype && Object.getOwnPropertyNames(ctor.prototype).map(add);
        }), a;
    }
    function mangle_properties(ast, options) {
        function can_mangle(name) {
            return reserved.indexOf(name) >= 0 ? !1 : /^[0-9.]+$/.test(name) ? !1 : !0;
        }
        function should_mangle(name) {
            return cache.props.has(name) || names_to_mangle.indexOf(name) >= 0;
        }
        function add(name) {
            can_mangle(name) && push_uniq(names_to_mangle, name);
        }
        function mangle(name) {
            var mangled = cache.props.get(name);
            if (!mangled) {
                do mangled = base54(++cache.cname); while (!can_mangle(mangled));
                cache.props.set(name, mangled);
            }
            return mangled;
        }
        function addStrings(node) {
            var out = {};
            try {
                !function walk(node) {
                    node.walk(new TreeWalker(function (node) {
                        if (node instanceof AST_Seq) return walk(node.cdr), !0;
                        if (node instanceof AST_String) return add(node.value), !0;
                        if (node instanceof AST_Conditional) return walk(node.consequent), walk(node.alternative),
                        !0;
                        throw out;
                    }));
                }(node);
            } catch (ex) {
                if (ex !== out) throw ex;
            }
        }
        function mangleStrings(node) {
            return node.transform(new TreeTransformer(function (node) {
                return node instanceof AST_Seq ? node.cdr = mangleStrings(node.cdr) : node instanceof AST_String ? should_mangle(node.value) && (node.value = mangle(node.value)) : node instanceof AST_Conditional && (node.consequent = mangleStrings(node.consequent),
                node.alternative = mangleStrings(node.alternative)), node;
            }));
        }
        options = defaults(options, {
            reserved: null,
            cache: null
        });
        var reserved = options.reserved;
        null == reserved && (reserved = find_builtins());
        var cache = options.cache;
        null == cache && (cache = {
            cname: -1,
            props: new Dictionary()
        });
        var names_to_mangle = [];
        return ast.walk(new TreeWalker(function (node) {
            node instanceof AST_ObjectKeyVal ? add(node.key) : node instanceof AST_ObjectProperty ? add(node.key.name) : node instanceof AST_Dot ? this.parent() instanceof AST_Assign && add(node.property) : node instanceof AST_Sub && this.parent() instanceof AST_Assign && addStrings(node.property);
        })), ast.transform(new TreeTransformer(null, function (node) {
            node instanceof AST_ObjectKeyVal ? should_mangle(node.key) && (node.key = mangle(node.key)) : node instanceof AST_ObjectProperty ? should_mangle(node.key.name) && (node.key.name = mangle(node.key.name)) : node instanceof AST_Dot ? should_mangle(node.property) && (node.property = mangle(node.property)) : node instanceof AST_Sub && (node.property = mangleStrings(node.property));
        }));
    }
    global.UglifyJS = exports, DefaultsError.prototype = Object.create(Error.prototype),
    DefaultsError.prototype.constructor = DefaultsError, DefaultsError.croak = function (msg, defs) {
        throw new DefaultsError(msg, defs);
    };
    var MAP = function () {
        function MAP(a, f, backwards) {
            function doit() {
                var val = f(a[i], i), is_last = val instanceof Last;
                return is_last && (val = val.v), val instanceof AtTop ? (val = val.v, val instanceof Splice ? top.push.apply(top, backwards ? val.v.slice().reverse() : val.v) : top.push(val)) : val !== skip && (val instanceof Splice ? ret.push.apply(ret, backwards ? val.v.slice().reverse() : val.v) : ret.push(val)),
                is_last;
            }
            var i, ret = [], top = [];
            if (a instanceof Array) if (backwards) {
                for (i = a.length; --i >= 0 && !doit() ;);
                ret.reverse(), top.reverse();
            } else for (i = 0; i < a.length && !doit() ; ++i); else for (i in a) if (a.hasOwnProperty(i) && doit()) break;
            return top.concat(ret);
        }
        function AtTop(val) {
            this.v = val;
        }
        function Splice(val) {
            this.v = val;
        }
        function Last(val) {
            this.v = val;
        }
        MAP.at_top = function (val) {
            return new AtTop(val);
        }, MAP.splice = function (val) {
            return new Splice(val);
        }, MAP.last = function (val) {
            return new Last(val);
        };
        var skip = MAP.skip = {};
        return MAP;
    }();
    Dictionary.prototype = {
        set: function (key, val) {
            return this.has(key) || ++this._size, this._values["$" + key] = val, this;
        },
        add: function (key, val) {
            return this.has(key) ? this.get(key).push(val) : this.set(key, [val]), this;
        },
        get: function (key) {
            return this._values["$" + key];
        },
        del: function (key) {
            return this.has(key) && (--this._size, delete this._values["$" + key]), this;
        },
        has: function (key) {
            return "$" + key in this._values;
        },
        each: function (f) {
            for (var i in this._values) f(this._values[i], i.substr(1));
        },
        size: function () {
            return this._size;
        },
        map: function (f) {
            var ret = [];
            for (var i in this._values) ret.push(f(this._values[i], i.substr(1)));
            return ret;
        },
        toObject: function () {
            return this._values;
        }
    }, Dictionary.fromObject = function (obj) {
        var dict = new Dictionary();
        return dict._size = merge(dict._values, obj), dict;
    };
    var AST_Token = DEFNODE("Token", "type value line col pos endline endcol endpos nlb comments_before file", {}, null), AST_Node = DEFNODE("Node", "start end", {
        clone: function () {
            return new this.CTOR(this);
        },
        $documentation: "Base class of all AST nodes",
        $propdoc: {
            start: "[AST_Token] The first token of this node",
            end: "[AST_Token] The last token of this node"
        },
        _walk: function (visitor) {
            return visitor._visit(this);
        },
        walk: function (visitor) {
            return this._walk(visitor);
        }
    }, null);
    AST_Node.warn_function = null, AST_Node.warn = function (txt, props) {
        AST_Node.warn_function && AST_Node.warn_function(string_template(txt, props));
    };
    var AST_Statement = DEFNODE("Statement", null, {
        $documentation: "Base class of all statements"
    }), AST_Debugger = DEFNODE("Debugger", null, {
        $documentation: "Represents a debugger statement"
    }, AST_Statement), AST_Directive = DEFNODE("Directive", "value scope quote", {
        $documentation: 'Represents a directive, like "use strict";',
        $propdoc: {
            value: "[string] The value of this directive as a plain string (it's not an AST_String!)",
            scope: "[AST_Scope/S] The scope that this directive affects",
            quote: "[string] the original quote character"
        }
    }, AST_Statement), AST_SimpleStatement = DEFNODE("SimpleStatement", "body", {
        $documentation: "A statement consisting of an expression, i.e. a = 1 + 2",
        $propdoc: {
            body: "[AST_Node] an expression node (should not be instanceof AST_Statement)"
        },
        _walk: function (visitor) {
            return visitor._visit(this, function () {
                this.body._walk(visitor);
            });
        }
    }, AST_Statement), AST_Block = DEFNODE("Block", "body", {
        $documentation: "A body of statements (usually bracketed)",
        $propdoc: {
            body: "[AST_Statement*] an array of statements"
        },
        _walk: function (visitor) {
            return visitor._visit(this, function () {
                walk_body(this, visitor);
            });
        }
    }, AST_Statement), AST_BlockStatement = DEFNODE("BlockStatement", null, {
        $documentation: "A block statement"
    }, AST_Block), AST_EmptyStatement = DEFNODE("EmptyStatement", null, {
        $documentation: "The empty statement (empty block or simply a semicolon)",
        _walk: function (visitor) {
            return visitor._visit(this);
        }
    }, AST_Statement), AST_StatementWithBody = DEFNODE("StatementWithBody", "body", {
        $documentation: "Base class for all statements that contain one nested body: `For`, `ForIn`, `Do`, `While`, `With`",
        $propdoc: {
            body: "[AST_Statement] the body; this should always be present, even if it's an AST_EmptyStatement"
        },
        _walk: function (visitor) {
            return visitor._visit(this, function () {
                this.body._walk(visitor);
            });
        }
    }, AST_Statement), AST_LabeledStatement = DEFNODE("LabeledStatement", "label", {
        $documentation: "Statement with a label",
        $propdoc: {
            label: "[AST_Label] a label definition"
        },
        _walk: function (visitor) {
            return visitor._visit(this, function () {
                this.label._walk(visitor), this.body._walk(visitor);
            });
        }
    }, AST_StatementWithBody), AST_IterationStatement = DEFNODE("IterationStatement", null, {
        $documentation: "Internal class.  All loops inherit from it."
    }, AST_StatementWithBody), AST_DWLoop = DEFNODE("DWLoop", "condition", {
        $documentation: "Base class for do/while statements",
        $propdoc: {
            condition: "[AST_Node] the loop condition.  Should not be instanceof AST_Statement"
        }
    }, AST_IterationStatement), AST_Do = DEFNODE("Do", null, {
        $documentation: "A `do` statement",
        _walk: function (visitor) {
            return visitor._visit(this, function () {
                this.body._walk(visitor), this.condition._walk(visitor);
            });
        }
    }, AST_DWLoop), AST_While = DEFNODE("While", null, {
        $documentation: "A `while` statement",
        _walk: function (visitor) {
            return visitor._visit(this, function () {
                this.condition._walk(visitor), this.body._walk(visitor);
            });
        }
    }, AST_DWLoop), AST_For = DEFNODE("For", "init condition step", {
        $documentation: "A `for` statement",
        $propdoc: {
            init: "[AST_Node?] the `for` initialization code, or null if empty",
            condition: "[AST_Node?] the `for` termination clause, or null if empty",
            step: "[AST_Node?] the `for` update clause, or null if empty"
        },
        _walk: function (visitor) {
            return visitor._visit(this, function () {
                this.init && this.init._walk(visitor), this.condition && this.condition._walk(visitor),
                this.step && this.step._walk(visitor), this.body._walk(visitor);
            });
        }
    }, AST_IterationStatement), AST_ForIn = DEFNODE("ForIn", "init name object", {
        $documentation: "A `for ... in` statement",
        $propdoc: {
            init: "[AST_Node] the `for/in` initialization code",
            name: "[AST_SymbolRef?] the loop variable, only if `init` is AST_Var",
            object: "[AST_Node] the object that we're looping through"
        },
        _walk: function (visitor) {
            return visitor._visit(this, function () {
                this.init._walk(visitor), this.object._walk(visitor), this.body._walk(visitor);
            });
        }
    }, AST_IterationStatement), AST_With = DEFNODE("With", "expression", {
        $documentation: "A `with` statement",
        $propdoc: {
            expression: "[AST_Node] the `with` expression"
        },
        _walk: function (visitor) {
            return visitor._visit(this, function () {
                this.expression._walk(visitor), this.body._walk(visitor);
            });
        }
    }, AST_StatementWithBody), AST_Scope = DEFNODE("Scope", "directives variables functions uses_with uses_eval parent_scope enclosed cname", {
        $documentation: "Base class for all statements introducing a lexical scope",
        $propdoc: {
            directives: "[string*/S] an array of directives declared in this scope",
            variables: "[Object/S] a map of name -> SymbolDef for all variables/functions defined in this scope",
            functions: "[Object/S] like `variables`, but only lists function declarations",
            uses_with: "[boolean/S] tells whether this scope uses the `with` statement",
            uses_eval: "[boolean/S] tells whether this scope contains a direct call to the global `eval`",
            parent_scope: "[AST_Scope?/S] link to the parent scope",
            enclosed: "[SymbolDef*/S] a list of all symbol definitions that are accessed from this scope or any subscopes",
            cname: "[integer/S] current index for mangling variables (used internally by the mangler)"
        }
    }, AST_Block), AST_Toplevel = DEFNODE("Toplevel", "globals", {
        $documentation: "The toplevel scope",
        $propdoc: {
            globals: "[Object/S] a map of name -> SymbolDef for all undeclared names"
        },
        wrap_enclose: function (arg_parameter_pairs) {
            var self = this, args = [], parameters = [];
            arg_parameter_pairs.forEach(function (pair) {
                var splitAt = pair.lastIndexOf(":");
                args.push(pair.substr(0, splitAt)), parameters.push(pair.substr(splitAt + 1));
            });
            var wrapped_tl = "(function(" + parameters.join(",") + "){ '$ORIG'; })(" + args.join(",") + ")";
            return wrapped_tl = parse(wrapped_tl), wrapped_tl = wrapped_tl.transform(new TreeTransformer(function (node) {
                return node instanceof AST_Directive && "$ORIG" == node.value ? MAP.splice(self.body) : void 0;
            }));
        },
        wrap_commonjs: function (name, export_all) {
            var self = this, to_export = [];
            export_all && (self.figure_out_scope(), self.walk(new TreeWalker(function (node) {
                node instanceof AST_SymbolDeclaration && node.definition().global && (find_if(function (n) {
                    return n.name == node.name;
                }, to_export) || to_export.push(node));
            })));
            var wrapped_tl = "(function(exports, global){ global['" + name + "'] = exports; '$ORIG'; '$EXPORTS'; }({}, (function(){return this}())))";
            return wrapped_tl = parse(wrapped_tl), wrapped_tl = wrapped_tl.transform(new TreeTransformer(function (node) {
                if (node instanceof AST_SimpleStatement && (node = node.body, node instanceof AST_String)) switch (node.getValue()) {
                    case "$ORIG":
                        return MAP.splice(self.body);

                    case "$EXPORTS":
                        var body = [];
                        return to_export.forEach(function (sym) {
                            body.push(new AST_SimpleStatement({
                                body: new AST_Assign({
                                    left: new AST_Sub({
                                        expression: new AST_SymbolRef({
                                            name: "exports"
                                        }),
                                        property: new AST_String({
                                            value: sym.name
                                        })
                                    }),
                                    operator: "=",
                                    right: new AST_SymbolRef(sym)
                                })
                            }));
                        }), MAP.splice(body);
                }
            }));
        }
    }, AST_Scope), AST_Lambda = DEFNODE("Lambda", "name argnames uses_arguments", {
        $documentation: "Base class for functions",
        $propdoc: {
            name: "[AST_SymbolDeclaration?] the name of this function",
            argnames: "[AST_SymbolFunarg*] array of function arguments",
            uses_arguments: "[boolean/S] tells whether this function accesses the arguments array"
        },
        _walk: function (visitor) {
            return visitor._visit(this, function () {
                this.name && this.name._walk(visitor), this.argnames.forEach(function (arg) {
                    arg._walk(visitor);
                }), walk_body(this, visitor);
            });
        }
    }, AST_Scope), AST_Accessor = DEFNODE("Accessor", null, {
        $documentation: "A setter/getter function.  The `name` property is always null."
    }, AST_Lambda), AST_Function = DEFNODE("Function", null, {
        $documentation: "A function expression"
    }, AST_Lambda), AST_Defun = DEFNODE("Defun", null, {
        $documentation: "A function definition"
    }, AST_Lambda), AST_Jump = DEFNODE("Jump", null, {
        $documentation: "Base class for “jumps” (for now that's `return`, `throw`, `break` and `continue`)"
    }, AST_Statement), AST_Exit = DEFNODE("Exit", "value", {
        $documentation: "Base class for “exits” (`return` and `throw`)",
        $propdoc: {
            value: "[AST_Node?] the value returned or thrown by this statement; could be null for AST_Return"
        },
        _walk: function (visitor) {
            return visitor._visit(this, this.value && function () {
                this.value._walk(visitor);
            });
        }
    }, AST_Jump), AST_Return = DEFNODE("Return", null, {
        $documentation: "A `return` statement"
    }, AST_Exit), AST_Throw = DEFNODE("Throw", null, {
        $documentation: "A `throw` statement"
    }, AST_Exit), AST_LoopControl = DEFNODE("LoopControl", "label", {
        $documentation: "Base class for loop control statements (`break` and `continue`)",
        $propdoc: {
            label: "[AST_LabelRef?] the label, or null if none"
        },
        _walk: function (visitor) {
            return visitor._visit(this, this.label && function () {
                this.label._walk(visitor);
            });
        }
    }, AST_Jump), AST_Break = DEFNODE("Break", null, {
        $documentation: "A `break` statement"
    }, AST_LoopControl), AST_Continue = DEFNODE("Continue", null, {
        $documentation: "A `continue` statement"
    }, AST_LoopControl), AST_If = DEFNODE("If", "condition alternative", {
        $documentation: "A `if` statement",
        $propdoc: {
            condition: "[AST_Node] the `if` condition",
            alternative: "[AST_Statement?] the `else` part, or null if not present"
        },
        _walk: function (visitor) {
            return visitor._visit(this, function () {
                this.condition._walk(visitor), this.body._walk(visitor), this.alternative && this.alternative._walk(visitor);
            });
        }
    }, AST_StatementWithBody), AST_Switch = DEFNODE("Switch", "expression", {
        $documentation: "A `switch` statement",
        $propdoc: {
            expression: "[AST_Node] the `switch` “discriminant”"
        },
        _walk: function (visitor) {
            return visitor._visit(this, function () {
                this.expression._walk(visitor), walk_body(this, visitor);
            });
        }
    }, AST_Block), AST_SwitchBranch = DEFNODE("SwitchBranch", null, {
        $documentation: "Base class for `switch` branches"
    }, AST_Block), AST_Default = DEFNODE("Default", null, {
        $documentation: "A `default` switch branch"
    }, AST_SwitchBranch), AST_Case = DEFNODE("Case", "expression", {
        $documentation: "A `case` switch branch",
        $propdoc: {
            expression: "[AST_Node] the `case` expression"
        },
        _walk: function (visitor) {
            return visitor._visit(this, function () {
                this.expression._walk(visitor), walk_body(this, visitor);
            });
        }
    }, AST_SwitchBranch), AST_Try = DEFNODE("Try", "bcatch bfinally", {
        $documentation: "A `try` statement",
        $propdoc: {
            bcatch: "[AST_Catch?] the catch block, or null if not present",
            bfinally: "[AST_Finally?] the finally block, or null if not present"
        },
        _walk: function (visitor) {
            return visitor._visit(this, function () {
                walk_body(this, visitor), this.bcatch && this.bcatch._walk(visitor), this.bfinally && this.bfinally._walk(visitor);
            });
        }
    }, AST_Block), AST_Catch = DEFNODE("Catch", "argname", {
        $documentation: "A `catch` node; only makes sense as part of a `try` statement",
        $propdoc: {
            argname: "[AST_SymbolCatch] symbol for the exception"
        },
        _walk: function (visitor) {
            return visitor._visit(this, function () {
                this.argname._walk(visitor), walk_body(this, visitor);
            });
        }
    }, AST_Block), AST_Finally = DEFNODE("Finally", null, {
        $documentation: "A `finally` node; only makes sense as part of a `try` statement"
    }, AST_Block), AST_Definitions = DEFNODE("Definitions", "definitions", {
        $documentation: "Base class for `var` or `const` nodes (variable declarations/initializations)",
        $propdoc: {
            definitions: "[AST_VarDef*] array of variable definitions"
        },
        _walk: function (visitor) {
            return visitor._visit(this, function () {
                this.definitions.forEach(function (def) {
                    def._walk(visitor);
                });
            });
        }
    }, AST_Statement), AST_Var = DEFNODE("Var", null, {
        $documentation: "A `var` statement"
    }, AST_Definitions), AST_Const = DEFNODE("Const", null, {
        $documentation: "A `const` statement"
    }, AST_Definitions), AST_VarDef = DEFNODE("VarDef", "name value", {
        $documentation: "A variable declaration; only appears in a AST_Definitions node",
        $propdoc: {
            name: "[AST_SymbolVar|AST_SymbolConst] name of the variable",
            value: "[AST_Node?] initializer, or null of there's no initializer"
        },
        _walk: function (visitor) {
            return visitor._visit(this, function () {
                this.name._walk(visitor), this.value && this.value._walk(visitor);
            });
        }
    }), AST_Call = DEFNODE("Call", "expression args", {
        $documentation: "A function call expression",
        $propdoc: {
            expression: "[AST_Node] expression to invoke as function",
            args: "[AST_Node*] array of arguments"
        },
        _walk: function (visitor) {
            return visitor._visit(this, function () {
                this.expression._walk(visitor), this.args.forEach(function (arg) {
                    arg._walk(visitor);
                });
            });
        }
    }), AST_New = DEFNODE("New", null, {
        $documentation: "An object instantiation.  Derives from a function call since it has exactly the same properties"
    }, AST_Call), AST_Seq = DEFNODE("Seq", "car cdr", {
        $documentation: "A sequence expression (two comma-separated expressions)",
        $propdoc: {
            car: "[AST_Node] first element in sequence",
            cdr: "[AST_Node] second element in sequence"
        },
        $cons: function (x, y) {
            var seq = new AST_Seq(x);
            return seq.car = x, seq.cdr = y, seq;
        },
        $from_array: function (array) {
            if (0 == array.length) return null;
            if (1 == array.length) return array[0].clone();
            for (var list = null, i = array.length; --i >= 0;) list = AST_Seq.cons(array[i], list);
            for (var p = list; p;) {
                if (p.cdr && !p.cdr.cdr) {
                    p.cdr = p.cdr.car;
                    break;
                }
                p = p.cdr;
            }
            return list;
        },
        to_array: function () {
            for (var p = this, a = []; p;) {
                if (a.push(p.car), p.cdr && !(p.cdr instanceof AST_Seq)) {
                    a.push(p.cdr);
                    break;
                }
                p = p.cdr;
            }
            return a;
        },
        add: function (node) {
            for (var p = this; p;) {
                if (!(p.cdr instanceof AST_Seq)) {
                    var cell = AST_Seq.cons(p.cdr, node);
                    return p.cdr = cell;
                }
                p = p.cdr;
            }
        },
        _walk: function (visitor) {
            return visitor._visit(this, function () {
                this.car._walk(visitor), this.cdr && this.cdr._walk(visitor);
            });
        }
    }), AST_PropAccess = DEFNODE("PropAccess", "expression property", {
        $documentation: 'Base class for property access expressions, i.e. `a.foo` or `a["foo"]`',
        $propdoc: {
            expression: "[AST_Node] the “container” expression",
            property: "[AST_Node|string] the property to access.  For AST_Dot this is always a plain string, while for AST_Sub it's an arbitrary AST_Node"
        }
    }), AST_Dot = DEFNODE("Dot", null, {
        $documentation: "A dotted property access expression",
        _walk: function (visitor) {
            return visitor._visit(this, function () {
                this.expression._walk(visitor);
            });
        }
    }, AST_PropAccess), AST_Sub = DEFNODE("Sub", null, {
        $documentation: 'Index-style property access, i.e. `a["foo"]`',
        _walk: function (visitor) {
            return visitor._visit(this, function () {
                this.expression._walk(visitor), this.property._walk(visitor);
            });
        }
    }, AST_PropAccess), AST_Unary = DEFNODE("Unary", "operator expression", {
        $documentation: "Base class for unary expressions",
        $propdoc: {
            operator: "[string] the operator",
            expression: "[AST_Node] expression that this unary operator applies to"
        },
        _walk: function (visitor) {
            return visitor._visit(this, function () {
                this.expression._walk(visitor);
            });
        }
    }), AST_UnaryPrefix = DEFNODE("UnaryPrefix", null, {
        $documentation: "Unary prefix expression, i.e. `typeof i` or `++i`"
    }, AST_Unary), AST_UnaryPostfix = DEFNODE("UnaryPostfix", null, {
        $documentation: "Unary postfix expression, i.e. `i++`"
    }, AST_Unary), AST_Binary = DEFNODE("Binary", "left operator right", {
        $documentation: "Binary expression, i.e. `a + b`",
        $propdoc: {
            left: "[AST_Node] left-hand side expression",
            operator: "[string] the operator",
            right: "[AST_Node] right-hand side expression"
        },
        _walk: function (visitor) {
            return visitor._visit(this, function () {
                this.left._walk(visitor), this.right._walk(visitor);
            });
        }
    }), AST_Conditional = DEFNODE("Conditional", "condition consequent alternative", {
        $documentation: "Conditional expression using the ternary operator, i.e. `a ? b : c`",
        $propdoc: {
            condition: "[AST_Node]",
            consequent: "[AST_Node]",
            alternative: "[AST_Node]"
        },
        _walk: function (visitor) {
            return visitor._visit(this, function () {
                this.condition._walk(visitor), this.consequent._walk(visitor), this.alternative._walk(visitor);
            });
        }
    }), AST_Assign = DEFNODE("Assign", null, {
        $documentation: "An assignment expression — `a = b + 5`"
    }, AST_Binary), AST_Array = DEFNODE("Array", "elements", {
        $documentation: "An array literal",
        $propdoc: {
            elements: "[AST_Node*] array of elements"
        },
        _walk: function (visitor) {
            return visitor._visit(this, function () {
                this.elements.forEach(function (el) {
                    el._walk(visitor);
                });
            });
        }
    }), AST_Object = DEFNODE("Object", "properties", {
        $documentation: "An object literal",
        $propdoc: {
            properties: "[AST_ObjectProperty*] array of properties"
        },
        _walk: function (visitor) {
            return visitor._visit(this, function () {
                this.properties.forEach(function (prop) {
                    prop._walk(visitor);
                });
            });
        }
    }), AST_ObjectProperty = DEFNODE("ObjectProperty", "key value", {
        $documentation: "Base class for literal object properties",
        $propdoc: {
            key: "[string] the property name converted to a string for ObjectKeyVal.  For setters and getters this is an arbitrary AST_Node.",
            value: "[AST_Node] property value.  For setters and getters this is an AST_Function."
        },
        _walk: function (visitor) {
            return visitor._visit(this, function () {
                this.value._walk(visitor);
            });
        }
    }), AST_ObjectKeyVal = DEFNODE("ObjectKeyVal", "quote", {
        $documentation: "A key: value object property",
        $propdoc: {
            quote: "[string] the original quote character"
        }
    }, AST_ObjectProperty), AST_ObjectSetter = DEFNODE("ObjectSetter", null, {
        $documentation: "An object setter property"
    }, AST_ObjectProperty), AST_ObjectGetter = DEFNODE("ObjectGetter", null, {
        $documentation: "An object getter property"
    }, AST_ObjectProperty), AST_Symbol = DEFNODE("Symbol", "scope name thedef", {
        $propdoc: {
            name: "[string] name of this symbol",
            scope: "[AST_Scope/S] the current scope (not necessarily the definition scope)",
            thedef: "[SymbolDef/S] the definition of this symbol"
        },
        $documentation: "Base class for all symbols"
    }), AST_SymbolAccessor = DEFNODE("SymbolAccessor", null, {
        $documentation: "The name of a property accessor (setter/getter function)"
    }, AST_Symbol), AST_SymbolDeclaration = DEFNODE("SymbolDeclaration", "init", {
        $documentation: "A declaration symbol (symbol in var/const, function name or argument, symbol in catch)",
        $propdoc: {
            init: "[AST_Node*/S] array of initializers for this declaration."
        }
    }, AST_Symbol), AST_SymbolVar = DEFNODE("SymbolVar", null, {
        $documentation: "Symbol defining a variable"
    }, AST_SymbolDeclaration), AST_SymbolConst = DEFNODE("SymbolConst", null, {
        $documentation: "A constant declaration"
    }, AST_SymbolDeclaration), AST_SymbolFunarg = DEFNODE("SymbolFunarg", null, {
        $documentation: "Symbol naming a function argument"
    }, AST_SymbolVar), AST_SymbolDefun = DEFNODE("SymbolDefun", null, {
        $documentation: "Symbol defining a function"
    }, AST_SymbolDeclaration), AST_SymbolLambda = DEFNODE("SymbolLambda", null, {
        $documentation: "Symbol naming a function expression"
    }, AST_SymbolDeclaration), AST_SymbolCatch = DEFNODE("SymbolCatch", null, {
        $documentation: "Symbol naming the exception in catch"
    }, AST_SymbolDeclaration), AST_Label = DEFNODE("Label", "references", {
        $documentation: "Symbol naming a label (declaration)",
        $propdoc: {
            references: "[AST_LoopControl*] a list of nodes referring to this label"
        },
        initialize: function () {
            this.references = [], this.thedef = this;
        }
    }, AST_Symbol), AST_SymbolRef = DEFNODE("SymbolRef", null, {
        $documentation: "Reference to some symbol (not definition/declaration)"
    }, AST_Symbol), AST_LabelRef = DEFNODE("LabelRef", null, {
        $documentation: "Reference to a label symbol"
    }, AST_Symbol), AST_This = DEFNODE("This", null, {
        $documentation: "The `this` symbol"
    }, AST_Symbol), AST_Constant = DEFNODE("Constant", null, {
        $documentation: "Base class for all constants",
        getValue: function () {
            return this.value;
        }
    }), AST_String = DEFNODE("String", "value quote", {
        $documentation: "A string literal",
        $propdoc: {
            value: "[string] the contents of this string",
            quote: "[string] the original quote character"
        }
    }, AST_Constant), AST_Number = DEFNODE("Number", "value", {
        $documentation: "A number literal",
        $propdoc: {
            value: "[number] the numeric value"
        }
    }, AST_Constant), AST_RegExp = DEFNODE("RegExp", "value", {
        $documentation: "A regexp literal",
        $propdoc: {
            value: "[RegExp] the actual regexp"
        }
    }, AST_Constant), AST_Atom = DEFNODE("Atom", null, {
        $documentation: "Base class for atoms"
    }, AST_Constant), AST_Null = DEFNODE("Null", null, {
        $documentation: "The `null` atom",
        value: null
    }, AST_Atom), AST_NaN = DEFNODE("NaN", null, {
        $documentation: "The impossible value",
        value: 0 / 0
    }, AST_Atom), AST_Undefined = DEFNODE("Undefined", null, {
        $documentation: "The `undefined` value",
        value: void 0
    }, AST_Atom), AST_Hole = DEFNODE("Hole", null, {
        $documentation: "A hole in an array",
        value: void 0
    }, AST_Atom), AST_Infinity = DEFNODE("Infinity", null, {
        $documentation: "The `Infinity` value",
        value: 1 / 0
    }, AST_Atom), AST_Boolean = DEFNODE("Boolean", null, {
        $documentation: "Base class for booleans"
    }, AST_Atom), AST_False = DEFNODE("False", null, {
        $documentation: "The `false` atom",
        value: !1
    }, AST_Boolean), AST_True = DEFNODE("True", null, {
        $documentation: "The `true` atom",
        value: !0
    }, AST_Boolean);
    TreeWalker.prototype = {
        _visit: function (node, descend) {
            this.stack.push(node);
            var ret = this.visit(node, descend ? function () {
                descend.call(node);
            } : noop);
            return !ret && descend && descend.call(node), this.stack.pop(), ret;
        },
        parent: function (n) {
            return this.stack[this.stack.length - 2 - (n || 0)];
        },
        push: function (node) {
            this.stack.push(node);
        },
        pop: function () {
            return this.stack.pop();
        },
        self: function () {
            return this.stack[this.stack.length - 1];
        },
        find_parent: function (type) {
            for (var stack = this.stack, i = stack.length; --i >= 0;) {
                var x = stack[i];
                if (x instanceof type) return x;
            }
        },
        has_directive: function (type) {
            return this.find_parent(AST_Scope).has_directive(type);
        },
        in_boolean_context: function () {
            for (var stack = this.stack, i = stack.length, self = stack[--i]; i > 0;) {
                var p = stack[--i];
                if (p instanceof AST_If && p.condition === self || p instanceof AST_Conditional && p.condition === self || p instanceof AST_DWLoop && p.condition === self || p instanceof AST_For && p.condition === self || p instanceof AST_UnaryPrefix && "!" == p.operator && p.expression === self) return !0;
                if (!(p instanceof AST_Binary) || "&&" != p.operator && "||" != p.operator) return !1;
                self = p;
            }
        },
        loopcontrol_target: function (label) {
            var stack = this.stack;
            if (label) for (var i = stack.length; --i >= 0;) {
                var x = stack[i];
                if (x instanceof AST_LabeledStatement && x.label.name == label.name) return x.body;
            } else for (var i = stack.length; --i >= 0;) {
                var x = stack[i];
                if (x instanceof AST_Switch || x instanceof AST_IterationStatement) return x;
            }
        }
    };
    var KEYWORDS = "break case catch const continue debugger default delete do else finally for function if in instanceof new return switch throw try typeof var void while with", KEYWORDS_ATOM = "false null true", RESERVED_WORDS = "abstract boolean byte char class double enum export extends final float goto implements import int interface long native package private protected public short static super synchronized this throws transient volatile yield " + KEYWORDS_ATOM + " " + KEYWORDS, KEYWORDS_BEFORE_EXPRESSION = "return new delete throw else case";
    KEYWORDS = makePredicate(KEYWORDS), RESERVED_WORDS = makePredicate(RESERVED_WORDS),
    KEYWORDS_BEFORE_EXPRESSION = makePredicate(KEYWORDS_BEFORE_EXPRESSION), KEYWORDS_ATOM = makePredicate(KEYWORDS_ATOM);
    var OPERATOR_CHARS = makePredicate(characters("+-*&%=<>!?|~^")), RE_HEX_NUMBER = /^0x[0-9a-f]+$/i, RE_OCT_NUMBER = /^0[0-7]+$/, RE_DEC_NUMBER = /^\d*\.?\d*(?:e[+-]?\d*(?:\d\.?|\.?\d)\d*)?$/i, OPERATORS = makePredicate(["in", "instanceof", "typeof", "new", "void", "delete", "++", "--", "+", "-", "!", "~", "&", "|", "^", "*", "/", "%", ">>", "<<", ">>>", "<", ">", "<=", ">=", "==", "===", "!=", "!==", "?", "=", "+=", "-=", "/=", "*=", "%=", ">>=", "<<=", ">>>=", "|=", "^=", "&=", "&&", "||"]), WHITESPACE_CHARS = makePredicate(characters("  \n\r	\f​᠎             　")), PUNC_BEFORE_EXPRESSION = makePredicate(characters("[{(,.;:")), PUNC_CHARS = makePredicate(characters("[]{}(),;:")), REGEXP_MODIFIERS = makePredicate(characters("gmsiy")), UNICODE = {
        letter: new RegExp("[\\u0041-\\u005A\\u0061-\\u007A\\u00AA\\u00B5\\u00BA\\u00C0-\\u00D6\\u00D8-\\u00F6\\u00F8-\\u02C1\\u02C6-\\u02D1\\u02E0-\\u02E4\\u02EC\\u02EE\\u0370-\\u0374\\u0376\\u0377\\u037A-\\u037D\\u037F\\u0386\\u0388-\\u038A\\u038C\\u038E-\\u03A1\\u03A3-\\u03F5\\u03F7-\\u0481\\u048A-\\u052F\\u0531-\\u0556\\u0559\\u0561-\\u0587\\u05D0-\\u05EA\\u05F0-\\u05F2\\u0620-\\u064A\\u066E\\u066F\\u0671-\\u06D3\\u06D5\\u06E5\\u06E6\\u06EE\\u06EF\\u06FA-\\u06FC\\u06FF\\u0710\\u0712-\\u072F\\u074D-\\u07A5\\u07B1\\u07CA-\\u07EA\\u07F4\\u07F5\\u07FA\\u0800-\\u0815\\u081A\\u0824\\u0828\\u0840-\\u0858\\u08A0-\\u08B2\\u0904-\\u0939\\u093D\\u0950\\u0958-\\u0961\\u0971-\\u0980\\u0985-\\u098C\\u098F\\u0990\\u0993-\\u09A8\\u09AA-\\u09B0\\u09B2\\u09B6-\\u09B9\\u09BD\\u09CE\\u09DC\\u09DD\\u09DF-\\u09E1\\u09F0\\u09F1\\u0A05-\\u0A0A\\u0A0F\\u0A10\\u0A13-\\u0A28\\u0A2A-\\u0A30\\u0A32\\u0A33\\u0A35\\u0A36\\u0A38\\u0A39\\u0A59-\\u0A5C\\u0A5E\\u0A72-\\u0A74\\u0A85-\\u0A8D\\u0A8F-\\u0A91\\u0A93-\\u0AA8\\u0AAA-\\u0AB0\\u0AB2\\u0AB3\\u0AB5-\\u0AB9\\u0ABD\\u0AD0\\u0AE0\\u0AE1\\u0B05-\\u0B0C\\u0B0F\\u0B10\\u0B13-\\u0B28\\u0B2A-\\u0B30\\u0B32\\u0B33\\u0B35-\\u0B39\\u0B3D\\u0B5C\\u0B5D\\u0B5F-\\u0B61\\u0B71\\u0B83\\u0B85-\\u0B8A\\u0B8E-\\u0B90\\u0B92-\\u0B95\\u0B99\\u0B9A\\u0B9C\\u0B9E\\u0B9F\\u0BA3\\u0BA4\\u0BA8-\\u0BAA\\u0BAE-\\u0BB9\\u0BD0\\u0C05-\\u0C0C\\u0C0E-\\u0C10\\u0C12-\\u0C28\\u0C2A-\\u0C39\\u0C3D\\u0C58\\u0C59\\u0C60\\u0C61\\u0C85-\\u0C8C\\u0C8E-\\u0C90\\u0C92-\\u0CA8\\u0CAA-\\u0CB3\\u0CB5-\\u0CB9\\u0CBD\\u0CDE\\u0CE0\\u0CE1\\u0CF1\\u0CF2\\u0D05-\\u0D0C\\u0D0E-\\u0D10\\u0D12-\\u0D3A\\u0D3D\\u0D4E\\u0D60\\u0D61\\u0D7A-\\u0D7F\\u0D85-\\u0D96\\u0D9A-\\u0DB1\\u0DB3-\\u0DBB\\u0DBD\\u0DC0-\\u0DC6\\u0E01-\\u0E30\\u0E32\\u0E33\\u0E40-\\u0E46\\u0E81\\u0E82\\u0E84\\u0E87\\u0E88\\u0E8A\\u0E8D\\u0E94-\\u0E97\\u0E99-\\u0E9F\\u0EA1-\\u0EA3\\u0EA5\\u0EA7\\u0EAA\\u0EAB\\u0EAD-\\u0EB0\\u0EB2\\u0EB3\\u0EBD\\u0EC0-\\u0EC4\\u0EC6\\u0EDC-\\u0EDF\\u0F00\\u0F40-\\u0F47\\u0F49-\\u0F6C\\u0F88-\\u0F8C\\u1000-\\u102A\\u103F\\u1050-\\u1055\\u105A-\\u105D\\u1061\\u1065\\u1066\\u106E-\\u1070\\u1075-\\u1081\\u108E\\u10A0-\\u10C5\\u10C7\\u10CD\\u10D0-\\u10FA\\u10FC-\\u1248\\u124A-\\u124D\\u1250-\\u1256\\u1258\\u125A-\\u125D\\u1260-\\u1288\\u128A-\\u128D\\u1290-\\u12B0\\u12B2-\\u12B5\\u12B8-\\u12BE\\u12C0\\u12C2-\\u12C5\\u12C8-\\u12D6\\u12D8-\\u1310\\u1312-\\u1315\\u1318-\\u135A\\u1380-\\u138F\\u13A0-\\u13F4\\u1401-\\u166C\\u166F-\\u167F\\u1681-\\u169A\\u16A0-\\u16EA\\u16EE-\\u16F8\\u1700-\\u170C\\u170E-\\u1711\\u1720-\\u1731\\u1740-\\u1751\\u1760-\\u176C\\u176E-\\u1770\\u1780-\\u17B3\\u17D7\\u17DC\\u1820-\\u1877\\u1880-\\u18A8\\u18AA\\u18B0-\\u18F5\\u1900-\\u191E\\u1950-\\u196D\\u1970-\\u1974\\u1980-\\u19AB\\u19C1-\\u19C7\\u1A00-\\u1A16\\u1A20-\\u1A54\\u1AA7\\u1B05-\\u1B33\\u1B45-\\u1B4B\\u1B83-\\u1BA0\\u1BAE\\u1BAF\\u1BBA-\\u1BE5\\u1C00-\\u1C23\\u1C4D-\\u1C4F\\u1C5A-\\u1C7D\\u1CE9-\\u1CEC\\u1CEE-\\u1CF1\\u1CF5\\u1CF6\\u1D00-\\u1DBF\\u1E00-\\u1F15\\u1F18-\\u1F1D\\u1F20-\\u1F45\\u1F48-\\u1F4D\\u1F50-\\u1F57\\u1F59\\u1F5B\\u1F5D\\u1F5F-\\u1F7D\\u1F80-\\u1FB4\\u1FB6-\\u1FBC\\u1FBE\\u1FC2-\\u1FC4\\u1FC6-\\u1FCC\\u1FD0-\\u1FD3\\u1FD6-\\u1FDB\\u1FE0-\\u1FEC\\u1FF2-\\u1FF4\\u1FF6-\\u1FFC\\u2071\\u207F\\u2090-\\u209C\\u2102\\u2107\\u210A-\\u2113\\u2115\\u2119-\\u211D\\u2124\\u2126\\u2128\\u212A-\\u212D\\u212F-\\u2139\\u213C-\\u213F\\u2145-\\u2149\\u214E\\u2160-\\u2188\\u2C00-\\u2C2E\\u2C30-\\u2C5E\\u2C60-\\u2CE4\\u2CEB-\\u2CEE\\u2CF2\\u2CF3\\u2D00-\\u2D25\\u2D27\\u2D2D\\u2D30-\\u2D67\\u2D6F\\u2D80-\\u2D96\\u2DA0-\\u2DA6\\u2DA8-\\u2DAE\\u2DB0-\\u2DB6\\u2DB8-\\u2DBE\\u2DC0-\\u2DC6\\u2DC8-\\u2DCE\\u2DD0-\\u2DD6\\u2DD8-\\u2DDE\\u2E2F\\u3005-\\u3007\\u3021-\\u3029\\u3031-\\u3035\\u3038-\\u303C\\u3041-\\u3096\\u309D-\\u309F\\u30A1-\\u30FA\\u30FC-\\u30FF\\u3105-\\u312D\\u3131-\\u318E\\u31A0-\\u31BA\\u31F0-\\u31FF\\u3400-\\u4DB5\\u4E00-\\u9FCC\\uA000-\\uA48C\\uA4D0-\\uA4FD\\uA500-\\uA60C\\uA610-\\uA61F\\uA62A\\uA62B\\uA640-\\uA66E\\uA67F-\\uA69D\\uA6A0-\\uA6EF\\uA717-\\uA71F\\uA722-\\uA788\\uA78B-\\uA78E\\uA790-\\uA7AD\\uA7B0\\uA7B1\\uA7F7-\\uA801\\uA803-\\uA805\\uA807-\\uA80A\\uA80C-\\uA822\\uA840-\\uA873\\uA882-\\uA8B3\\uA8F2-\\uA8F7\\uA8FB\\uA90A-\\uA925\\uA930-\\uA946\\uA960-\\uA97C\\uA984-\\uA9B2\\uA9CF\\uA9E0-\\uA9E4\\uA9E6-\\uA9EF\\uA9FA-\\uA9FE\\uAA00-\\uAA28\\uAA40-\\uAA42\\uAA44-\\uAA4B\\uAA60-\\uAA76\\uAA7A\\uAA7E-\\uAAAF\\uAAB1\\uAAB5\\uAAB6\\uAAB9-\\uAABD\\uAAC0\\uAAC2\\uAADB-\\uAADD\\uAAE0-\\uAAEA\\uAAF2-\\uAAF4\\uAB01-\\uAB06\\uAB09-\\uAB0E\\uAB11-\\uAB16\\uAB20-\\uAB26\\uAB28-\\uAB2E\\uAB30-\\uAB5A\\uAB5C-\\uAB5F\\uAB64\\uAB65\\uABC0-\\uABE2\\uAC00-\\uD7A3\\uD7B0-\\uD7C6\\uD7CB-\\uD7FB\\uF900-\\uFA6D\\uFA70-\\uFAD9\\uFB00-\\uFB06\\uFB13-\\uFB17\\uFB1D\\uFB1F-\\uFB28\\uFB2A-\\uFB36\\uFB38-\\uFB3C\\uFB3E\\uFB40\\uFB41\\uFB43\\uFB44\\uFB46-\\uFBB1\\uFBD3-\\uFD3D\\uFD50-\\uFD8F\\uFD92-\\uFDC7\\uFDF0-\\uFDFB\\uFE70-\\uFE74\\uFE76-\\uFEFC\\uFF21-\\uFF3A\\uFF41-\\uFF5A\\uFF66-\\uFFBE\\uFFC2-\\uFFC7\\uFFCA-\\uFFCF\\uFFD2-\\uFFD7\\uFFDA-\\uFFDC]"),
        digit: new RegExp("[\\u0030-\\u0039\\u0660-\\u0669\\u06F0-\\u06F9\\u07C0-\\u07C9\\u0966-\\u096F\\u09E6-\\u09EF\\u0A66-\\u0A6F\\u0AE6-\\u0AEF\\u0B66-\\u0B6F\\u0BE6-\\u0BEF\\u0C66-\\u0C6F\\u0CE6-\\u0CEF\\u0D66-\\u0D6F\\u0DE6-\\u0DEF\\u0E50-\\u0E59\\u0ED0-\\u0ED9\\u0F20-\\u0F29\\u1040-\\u1049\\u1090-\\u1099\\u17E0-\\u17E9\\u1810-\\u1819\\u1946-\\u194F\\u19D0-\\u19D9\\u1A80-\\u1A89\\u1A90-\\u1A99\\u1B50-\\u1B59\\u1BB0-\\u1BB9\\u1C40-\\u1C49\\u1C50-\\u1C59\\uA620-\\uA629\\uA8D0-\\uA8D9\\uA900-\\uA909\\uA9D0-\\uA9D9\\uA9F0-\\uA9F9\\uAA50-\\uAA59\\uABF0-\\uABF9\\uFF10-\\uFF19]"),
        non_spacing_mark: new RegExp("[\\u0300-\\u036F\\u0483-\\u0487\\u0591-\\u05BD\\u05BF\\u05C1\\u05C2\\u05C4\\u05C5\\u05C7\\u0610-\\u061A\\u064B-\\u065E\\u0670\\u06D6-\\u06DC\\u06DF-\\u06E4\\u06E7\\u06E8\\u06EA-\\u06ED\\u0711\\u0730-\\u074A\\u07A6-\\u07B0\\u07EB-\\u07F3\\u0816-\\u0819\\u081B-\\u0823\\u0825-\\u0827\\u0829-\\u082D\\u0900-\\u0902\\u093C\\u0941-\\u0948\\u094D\\u0951-\\u0955\\u0962\\u0963\\u0981\\u09BC\\u09C1-\\u09C4\\u09CD\\u09E2\\u09E3\\u0A01\\u0A02\\u0A3C\\u0A41\\u0A42\\u0A47\\u0A48\\u0A4B-\\u0A4D\\u0A51\\u0A70\\u0A71\\u0A75\\u0A81\\u0A82\\u0ABC\\u0AC1-\\u0AC5\\u0AC7\\u0AC8\\u0ACD\\u0AE2\\u0AE3\\u0B01\\u0B3C\\u0B3F\\u0B41-\\u0B44\\u0B4D\\u0B56\\u0B62\\u0B63\\u0B82\\u0BC0\\u0BCD\\u0C3E-\\u0C40\\u0C46-\\u0C48\\u0C4A-\\u0C4D\\u0C55\\u0C56\\u0C62\\u0C63\\u0CBC\\u0CBF\\u0CC6\\u0CCC\\u0CCD\\u0CE2\\u0CE3\\u0D41-\\u0D44\\u0D4D\\u0D62\\u0D63\\u0DCA\\u0DD2-\\u0DD4\\u0DD6\\u0E31\\u0E34-\\u0E3A\\u0E47-\\u0E4E\\u0EB1\\u0EB4-\\u0EB9\\u0EBB\\u0EBC\\u0EC8-\\u0ECD\\u0F18\\u0F19\\u0F35\\u0F37\\u0F39\\u0F71-\\u0F7E\\u0F80-\\u0F84\\u0F86\\u0F87\\u0F90-\\u0F97\\u0F99-\\u0FBC\\u0FC6\\u102D-\\u1030\\u1032-\\u1037\\u1039\\u103A\\u103D\\u103E\\u1058\\u1059\\u105E-\\u1060\\u1071-\\u1074\\u1082\\u1085\\u1086\\u108D\\u109D\\u135F\\u1712-\\u1714\\u1732-\\u1734\\u1752\\u1753\\u1772\\u1773\\u17B7-\\u17BD\\u17C6\\u17C9-\\u17D3\\u17DD\\u180B-\\u180D\\u18A9\\u1920-\\u1922\\u1927\\u1928\\u1932\\u1939-\\u193B\\u1A17\\u1A18\\u1A56\\u1A58-\\u1A5E\\u1A60\\u1A62\\u1A65-\\u1A6C\\u1A73-\\u1A7C\\u1A7F\\u1B00-\\u1B03\\u1B34\\u1B36-\\u1B3A\\u1B3C\\u1B42\\u1B6B-\\u1B73\\u1B80\\u1B81\\u1BA2-\\u1BA5\\u1BA8\\u1BA9\\u1C2C-\\u1C33\\u1C36\\u1C37\\u1CD0-\\u1CD2\\u1CD4-\\u1CE0\\u1CE2-\\u1CE8\\u1CED\\u1DC0-\\u1DE6\\u1DFD-\\u1DFF\\u20D0-\\u20DC\\u20E1\\u20E5-\\u20F0\\u2CEF-\\u2CF1\\u2DE0-\\u2DFF\\u302A-\\u302F\\u3099\\u309A\\uA66F\\uA67C\\uA67D\\uA6F0\\uA6F1\\uA802\\uA806\\uA80B\\uA825\\uA826\\uA8C4\\uA8E0-\\uA8F1\\uA926-\\uA92D\\uA947-\\uA951\\uA980-\\uA982\\uA9B3\\uA9B6-\\uA9B9\\uA9BC\\uAA29-\\uAA2E\\uAA31\\uAA32\\uAA35\\uAA36\\uAA43\\uAA4C\\uAAB0\\uAAB2-\\uAAB4\\uAAB7\\uAAB8\\uAABE\\uAABF\\uAAC1\\uABE5\\uABE8\\uABED\\uFB1E\\uFE00-\\uFE0F\\uFE20-\\uFE26]"),
        space_combining_mark: new RegExp("[\\u0903\\u093E-\\u0940\\u0949-\\u094C\\u094E\\u0982\\u0983\\u09BE-\\u09C0\\u09C7\\u09C8\\u09CB\\u09CC\\u09D7\\u0A03\\u0A3E-\\u0A40\\u0A83\\u0ABE-\\u0AC0\\u0AC9\\u0ACB\\u0ACC\\u0B02\\u0B03\\u0B3E\\u0B40\\u0B47\\u0B48\\u0B4B\\u0B4C\\u0B57\\u0BBE\\u0BBF\\u0BC1\\u0BC2\\u0BC6-\\u0BC8\\u0BCA-\\u0BCC\\u0BD7\\u0C01-\\u0C03\\u0C41-\\u0C44\\u0C82\\u0C83\\u0CBE\\u0CC0-\\u0CC4\\u0CC7\\u0CC8\\u0CCA\\u0CCB\\u0CD5\\u0CD6\\u0D02\\u0D03\\u0D3E-\\u0D40\\u0D46-\\u0D48\\u0D4A-\\u0D4C\\u0D57\\u0D82\\u0D83\\u0DCF-\\u0DD1\\u0DD8-\\u0DDF\\u0DF2\\u0DF3\\u0F3E\\u0F3F\\u0F7F\\u102B\\u102C\\u1031\\u1038\\u103B\\u103C\\u1056\\u1057\\u1062-\\u1064\\u1067-\\u106D\\u1083\\u1084\\u1087-\\u108C\\u108F\\u109A-\\u109C\\u17B6\\u17BE-\\u17C5\\u17C7\\u17C8\\u1923-\\u1926\\u1929-\\u192B\\u1930\\u1931\\u1933-\\u1938\\u19B0-\\u19C0\\u19C8\\u19C9\\u1A19-\\u1A1B\\u1A55\\u1A57\\u1A61\\u1A63\\u1A64\\u1A6D-\\u1A72\\u1B04\\u1B35\\u1B3B\\u1B3D-\\u1B41\\u1B43\\u1B44\\u1B82\\u1BA1\\u1BA6\\u1BA7\\u1BAA\\u1C24-\\u1C2B\\u1C34\\u1C35\\u1CE1\\u1CF2\\uA823\\uA824\\uA827\\uA880\\uA881\\uA8B4-\\uA8C3\\uA952\\uA953\\uA983\\uA9B4\\uA9B5\\uA9BA\\uA9BB\\uA9BD-\\uA9C0\\uAA2F\\uAA30\\uAA33\\uAA34\\uAA4D\\uAA7B\\uABE3\\uABE4\\uABE6\\uABE7\\uABE9\\uABEA\\uABEC]"),
        connector_punctuation: new RegExp("[\\u005F\\u203F\\u2040\\u2054\\uFE33\\uFE34\\uFE4D-\\uFE4F\\uFF3F]")
    };
    JS_Parse_Error.prototype.toString = function () {
        return this.message + " (line: " + this.line + ", col: " + this.col + ", pos: " + this.pos + ")\n\n" + this.stack;
    };
    var EX_EOF = {}, UNARY_PREFIX = makePredicate(["typeof", "void", "delete", "--", "++", "!", "~", "-", "+"]), UNARY_POSTFIX = makePredicate(["--", "++"]), ASSIGNMENT = makePredicate(["=", "+=", "-=", "/=", "*=", "%=", ">>=", "<<=", ">>>=", "|=", "^=", "&="]), PRECEDENCE = function (a, ret) {
        for (var i = 0; i < a.length; ++i) for (var b = a[i], j = 0; j < b.length; ++j) ret[b[j]] = i + 1;
        return ret;
    }([["||"], ["&&"], ["|"], ["^"], ["&"], ["==", "===", "!=", "!=="], ["<", ">", "<=", ">=", "in", "instanceof"], [">>", "<<", ">>>"], ["+", "-"], ["*", "/", "%"]], {}), STATEMENTS_WITH_LABELS = array_to_hash(["for", "do", "while", "switch"]), ATOMIC_START_TOKEN = array_to_hash(["atom", "num", "string", "regexp", "name"]);
    TreeTransformer.prototype = new TreeWalker(), function (undefined) {
        function _(node, descend) {
            node.DEFMETHOD("transform", function (tw, in_list) {
                var x, y;
                return tw.push(this), tw.before && (x = tw.before(this, descend, in_list)), x === undefined && (tw.after ? (tw.stack[tw.stack.length - 1] = x = this.clone(),
                descend(x, tw), y = tw.after(x, in_list), y !== undefined && (x = y)) : (x = this,
                descend(x, tw))), tw.pop(), x;
            });
        }
        function do_list(list, tw) {
            return MAP(list, function (node) {
                return node.transform(tw, !0);
            });
        }
        _(AST_Node, noop), _(AST_LabeledStatement, function (self, tw) {
            self.label = self.label.transform(tw), self.body = self.body.transform(tw);
        }), _(AST_SimpleStatement, function (self, tw) {
            self.body = self.body.transform(tw);
        }), _(AST_Block, function (self, tw) {
            self.body = do_list(self.body, tw);
        }), _(AST_DWLoop, function (self, tw) {
            self.condition = self.condition.transform(tw), self.body = self.body.transform(tw);
        }), _(AST_For, function (self, tw) {
            self.init && (self.init = self.init.transform(tw)), self.condition && (self.condition = self.condition.transform(tw)),
            self.step && (self.step = self.step.transform(tw)), self.body = self.body.transform(tw);
        }), _(AST_ForIn, function (self, tw) {
            self.init = self.init.transform(tw), self.object = self.object.transform(tw), self.body = self.body.transform(tw);
        }), _(AST_With, function (self, tw) {
            self.expression = self.expression.transform(tw), self.body = self.body.transform(tw);
        }), _(AST_Exit, function (self, tw) {
            self.value && (self.value = self.value.transform(tw));
        }), _(AST_LoopControl, function (self, tw) {
            self.label && (self.label = self.label.transform(tw));
        }), _(AST_If, function (self, tw) {
            self.condition = self.condition.transform(tw), self.body = self.body.transform(tw),
            self.alternative && (self.alternative = self.alternative.transform(tw));
        }), _(AST_Switch, function (self, tw) {
            self.expression = self.expression.transform(tw), self.body = do_list(self.body, tw);
        }), _(AST_Case, function (self, tw) {
            self.expression = self.expression.transform(tw), self.body = do_list(self.body, tw);
        }), _(AST_Try, function (self, tw) {
            self.body = do_list(self.body, tw), self.bcatch && (self.bcatch = self.bcatch.transform(tw)),
            self.bfinally && (self.bfinally = self.bfinally.transform(tw));
        }), _(AST_Catch, function (self, tw) {
            self.argname = self.argname.transform(tw), self.body = do_list(self.body, tw);
        }), _(AST_Definitions, function (self, tw) {
            self.definitions = do_list(self.definitions, tw);
        }), _(AST_VarDef, function (self, tw) {
            self.name = self.name.transform(tw), self.value && (self.value = self.value.transform(tw));
        }), _(AST_Lambda, function (self, tw) {
            self.name && (self.name = self.name.transform(tw)), self.argnames = do_list(self.argnames, tw),
            self.body = do_list(self.body, tw);
        }), _(AST_Call, function (self, tw) {
            self.expression = self.expression.transform(tw), self.args = do_list(self.args, tw);
        }), _(AST_Seq, function (self, tw) {
            self.car = self.car.transform(tw), self.cdr = self.cdr.transform(tw);
        }), _(AST_Dot, function (self, tw) {
            self.expression = self.expression.transform(tw);
        }), _(AST_Sub, function (self, tw) {
            self.expression = self.expression.transform(tw), self.property = self.property.transform(tw);
        }), _(AST_Unary, function (self, tw) {
            self.expression = self.expression.transform(tw);
        }), _(AST_Binary, function (self, tw) {
            self.left = self.left.transform(tw), self.right = self.right.transform(tw);
        }), _(AST_Conditional, function (self, tw) {
            self.condition = self.condition.transform(tw), self.consequent = self.consequent.transform(tw),
            self.alternative = self.alternative.transform(tw);
        }), _(AST_Array, function (self, tw) {
            self.elements = do_list(self.elements, tw);
        }), _(AST_Object, function (self, tw) {
            self.properties = do_list(self.properties, tw);
        }), _(AST_ObjectProperty, function (self, tw) {
            self.value = self.value.transform(tw);
        });
    }(), SymbolDef.prototype = {
        unmangleable: function (options) {
            return options || (options = {}), this.global && !options.toplevel || this.undeclared || !options.eval && (this.scope.uses_eval || this.scope.uses_with) || options.keep_fnames && (this.orig[0] instanceof AST_SymbolLambda || this.orig[0] instanceof AST_SymbolDefun);
        },
        mangle: function (options) {
            var cache = options.cache && options.cache.props;
            if (this.global && cache && cache.has(this.name)) this.mangled_name = cache.get(this.name); else if (!this.mangled_name && !this.unmangleable(options)) {
                var s = this.scope;
                !options.screw_ie8 && this.orig[0] instanceof AST_SymbolLambda && (s = s.parent_scope),
                this.mangled_name = s.next_mangled(options, this), this.global && cache && cache.set(this.name, this.mangled_name);
            }
        }
    }, AST_Toplevel.DEFMETHOD("figure_out_scope", function (options) {
        options = defaults(options, {
            screw_ie8: !1,
            cache: null
        });
        var self = this, scope = self.parent_scope = null, defun = null, nesting = 0, tw = new TreeWalker(function (node, descend) {
            if (options.screw_ie8 && node instanceof AST_Catch) {
                var save_scope = scope;
                return scope = new AST_Scope(node), scope.init_scope_vars(nesting), scope.parent_scope = save_scope,
                descend(), scope = save_scope, !0;
            }
            if (node instanceof AST_Scope) {
                node.init_scope_vars(nesting);
                var save_scope = node.parent_scope = scope, save_defun = defun;
                return defun = scope = node, ++nesting, descend(), --nesting, scope = save_scope,
                defun = save_defun, !0;
            }
            if (node instanceof AST_Directive) return node.scope = scope, push_uniq(scope.directives, node.value),
            !0;
            if (node instanceof AST_With) for (var s = scope; s; s = s.parent_scope) s.uses_with = !0; else if (node instanceof AST_Symbol && (node.scope = scope),
            node instanceof AST_SymbolLambda) defun.def_function(node); else if (node instanceof AST_SymbolDefun) (node.scope = defun.parent_scope).def_function(node); else if (node instanceof AST_SymbolVar || node instanceof AST_SymbolConst) {
                var def = defun.def_variable(node);
                def.constant = node instanceof AST_SymbolConst, def.init = tw.parent().value;
            } else node instanceof AST_SymbolCatch && (options.screw_ie8 ? scope : defun).def_variable(node);
        });
        self.walk(tw);
        var func = null, globals = self.globals = new Dictionary(), tw = new TreeWalker(function (node, descend) {
            if (node instanceof AST_Lambda) {
                var prev_func = func;
                return func = node, descend(), func = prev_func, !0;
            }
            if (node instanceof AST_SymbolRef) {
                var name = node.name, sym = node.scope.find_variable(name);
                if (sym) node.thedef = sym; else {
                    var g;
                    if (globals.has(name) ? g = globals.get(name) : (g = new SymbolDef(self, globals.size(), node),
                    g.undeclared = !0, g.global = !0, globals.set(name, g)), node.thedef = g, "eval" == name && tw.parent() instanceof AST_Call) for (var s = node.scope; s && !s.uses_eval; s = s.parent_scope) s.uses_eval = !0;
                    func && "arguments" == name && (func.uses_arguments = !0);
                }
                return node.reference(), !0;
            }
        });
        self.walk(tw), options.cache && (this.cname = options.cache.cname);
    }), AST_Scope.DEFMETHOD("init_scope_vars", function (nesting) {
        this.directives = [], this.variables = new Dictionary(), this.functions = new Dictionary(),
        this.uses_with = !1, this.uses_eval = !1, this.parent_scope = null, this.enclosed = [],
        this.cname = -1, this.nesting = nesting;
    }), AST_Scope.DEFMETHOD("strict", function () {
        return this.has_directive("use strict");
    }), AST_Lambda.DEFMETHOD("init_scope_vars", function () {
        AST_Scope.prototype.init_scope_vars.apply(this, arguments), this.uses_arguments = !1;
    }), AST_SymbolRef.DEFMETHOD("reference", function () {
        var def = this.definition();
        def.references.push(this);
        for (var s = this.scope; s && (push_uniq(s.enclosed, def), s !== def.scope) ;) s = s.parent_scope;
        this.frame = this.scope.nesting - def.scope.nesting;
    }), AST_Scope.DEFMETHOD("find_variable", function (name) {
        return name instanceof AST_Symbol && (name = name.name), this.variables.get(name) || this.parent_scope && this.parent_scope.find_variable(name);
    }), AST_Scope.DEFMETHOD("has_directive", function (value) {
        return this.parent_scope && this.parent_scope.has_directive(value) || (this.directives.indexOf(value) >= 0 ? this : null);
    }), AST_Scope.DEFMETHOD("def_function", function (symbol) {
        this.functions.set(symbol.name, this.def_variable(symbol));
    }), AST_Scope.DEFMETHOD("def_variable", function (symbol) {
        var def;
        return this.variables.has(symbol.name) ? (def = this.variables.get(symbol.name),
        def.orig.push(symbol)) : (def = new SymbolDef(this, this.variables.size(), symbol),
        this.variables.set(symbol.name, def), def.global = !this.parent_scope), symbol.thedef = def;
    }), AST_Scope.DEFMETHOD("next_mangled", function (options) {
        var ext = this.enclosed;
        out: for (; ;) {
            var m = base54(++this.cname);
            if (is_identifier(m) && !(options.except.indexOf(m) >= 0)) {
                for (var i = ext.length; --i >= 0;) {
                    var sym = ext[i], name = sym.mangled_name || sym.unmangleable(options) && sym.name;
                    if (m == name) continue out;
                }
                return m;
            }
        }
    }), AST_Function.DEFMETHOD("next_mangled", function (options, def) {
        for (var tricky_def = def.orig[0] instanceof AST_SymbolFunarg && this.name && this.name.definition() ; ;) {
            var name = AST_Lambda.prototype.next_mangled.call(this, options, def);
            if (!tricky_def || tricky_def.mangled_name != name) return name;
        }
    }), AST_Scope.DEFMETHOD("references", function (sym) {
        return sym instanceof AST_Symbol && (sym = sym.definition()), this.enclosed.indexOf(sym) < 0 ? null : sym;
    }), AST_Symbol.DEFMETHOD("unmangleable", function (options) {
        return this.definition().unmangleable(options);
    }), AST_SymbolAccessor.DEFMETHOD("unmangleable", function () {
        return !0;
    }), AST_Label.DEFMETHOD("unmangleable", function () {
        return !1;
    }), AST_Symbol.DEFMETHOD("unreferenced", function () {
        return 0 == this.definition().references.length && !(this.scope.uses_eval || this.scope.uses_with);
    }), AST_Symbol.DEFMETHOD("undeclared", function () {
        return this.definition().undeclared;
    }), AST_LabelRef.DEFMETHOD("undeclared", function () {
        return !1;
    }), AST_Label.DEFMETHOD("undeclared", function () {
        return !1;
    }), AST_Symbol.DEFMETHOD("definition", function () {
        return this.thedef;
    }), AST_Symbol.DEFMETHOD("global", function () {
        return this.definition().global;
    }), AST_Toplevel.DEFMETHOD("_default_mangler_options", function (options) {
        return defaults(options, {
            except: [],
            eval: !1,
            sort: !1,
            toplevel: !1,
            screw_ie8: !1,
            keep_fnames: !1
        });
    }), AST_Toplevel.DEFMETHOD("mangle_names", function (options) {
        options = this._default_mangler_options(options);
        var lname = -1, to_mangle = [];
        options.cache && this.globals.each(function (symbol) {
            options.except.indexOf(symbol.name) < 0 && to_mangle.push(symbol);
        });
        var tw = new TreeWalker(function (node, descend) {
            if (node instanceof AST_LabeledStatement) {
                var save_nesting = lname;
                return descend(), lname = save_nesting, !0;
            }
            if (node instanceof AST_Scope) {
                var a = (tw.parent(), []);
                return node.variables.each(function (symbol) {
                    options.except.indexOf(symbol.name) < 0 && a.push(symbol);
                }), options.sort && a.sort(function (a, b) {
                    return b.references.length - a.references.length;
                }), void to_mangle.push.apply(to_mangle, a);
            }
            if (node instanceof AST_Label) {
                var name;
                do name = base54(++lname); while (!is_identifier(name));
                return node.mangled_name = name, !0;
            }
            return options.screw_ie8 && node instanceof AST_SymbolCatch ? void to_mangle.push(node.definition()) : void 0;
        });
        this.walk(tw), to_mangle.forEach(function (def) {
            def.mangle(options);
        }), options.cache && (options.cache.cname = this.cname);
    }), AST_Toplevel.DEFMETHOD("compute_char_frequency", function (options) {
        options = this._default_mangler_options(options);
        var tw = new TreeWalker(function (node) {
            node instanceof AST_Constant ? base54.consider(node.print_to_string()) : node instanceof AST_Return ? base54.consider("return") : node instanceof AST_Throw ? base54.consider("throw") : node instanceof AST_Continue ? base54.consider("continue") : node instanceof AST_Break ? base54.consider("break") : node instanceof AST_Debugger ? base54.consider("debugger") : node instanceof AST_Directive ? base54.consider(node.value) : node instanceof AST_While ? base54.consider("while") : node instanceof AST_Do ? base54.consider("do while") : node instanceof AST_If ? (base54.consider("if"),
            node.alternative && base54.consider("else")) : node instanceof AST_Var ? base54.consider("var") : node instanceof AST_Const ? base54.consider("const") : node instanceof AST_Lambda ? base54.consider("function") : node instanceof AST_For ? base54.consider("for") : node instanceof AST_ForIn ? base54.consider("for in") : node instanceof AST_Switch ? base54.consider("switch") : node instanceof AST_Case ? base54.consider("case") : node instanceof AST_Default ? base54.consider("default") : node instanceof AST_With ? base54.consider("with") : node instanceof AST_ObjectSetter ? base54.consider("set" + node.key) : node instanceof AST_ObjectGetter ? base54.consider("get" + node.key) : node instanceof AST_ObjectKeyVal ? base54.consider(node.key) : node instanceof AST_New ? base54.consider("new") : node instanceof AST_This ? base54.consider("this") : node instanceof AST_Try ? base54.consider("try") : node instanceof AST_Catch ? base54.consider("catch") : node instanceof AST_Finally ? base54.consider("finally") : node instanceof AST_Symbol && node.unmangleable(options) ? base54.consider(node.name) : node instanceof AST_Unary || node instanceof AST_Binary ? base54.consider(node.operator) : node instanceof AST_Dot && base54.consider(node.property);
        });
        this.walk(tw), base54.sort();
    });
    var base54 = function () {
        function reset() {
            frequency = Object.create(null), chars = string.split("").map(function (ch) {
                return ch.charCodeAt(0);
            }), chars.forEach(function (ch) {
                frequency[ch] = 0;
            });
        }
        function base54(num) {
            var ret = "", base = 54;
            num++;
            do num--, ret += String.fromCharCode(chars[num % base]), num = Math.floor(num / base),
            base = 64; while (num > 0);
            return ret;
        }
        var chars, frequency, string = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ$_0123456789";
        return base54.consider = function (str) {
            for (var i = str.length; --i >= 0;) {
                var code = str.charCodeAt(i);
                code in frequency && ++frequency[code];
            }
        }, base54.sort = function () {
            chars = mergeSort(chars, function (a, b) {
                return is_digit(a) && !is_digit(b) ? 1 : is_digit(b) && !is_digit(a) ? -1 : frequency[b] - frequency[a];
            });
        }, base54.reset = reset, reset(), base54.get = function () {
            return chars;
        }, base54.freq = function () {
            return frequency;
        }, base54;
    }();
    AST_Toplevel.DEFMETHOD("scope_warnings", function (options) {
        options = defaults(options, {
            undeclared: !1,
            unreferenced: !0,
            assign_to_global: !0,
            func_arguments: !0,
            nested_defuns: !0,
            eval: !0
        });
        var tw = new TreeWalker(function (node) {
            if (options.undeclared && node instanceof AST_SymbolRef && node.undeclared() && AST_Node.warn("Undeclared symbol: {name} [{file}:{line},{col}]", {
                name: node.name,
                file: node.start.file,
                line: node.start.line,
                col: node.start.col
            }), options.assign_to_global) {
                var sym = null;
                node instanceof AST_Assign && node.left instanceof AST_SymbolRef ? sym = node.left : node instanceof AST_ForIn && node.init instanceof AST_SymbolRef && (sym = node.init),
                sym && (sym.undeclared() || sym.global() && sym.scope !== sym.definition().scope) && AST_Node.warn("{msg}: {name} [{file}:{line},{col}]", {
                    msg: sym.undeclared() ? "Accidental global?" : "Assignment to global",
                    name: sym.name,
                    file: sym.start.file,
                    line: sym.start.line,
                    col: sym.start.col
                });
            }
            options.eval && node instanceof AST_SymbolRef && node.undeclared() && "eval" == node.name && AST_Node.warn("Eval is used [{file}:{line},{col}]", node.start),
            options.unreferenced && (node instanceof AST_SymbolDeclaration || node instanceof AST_Label) && !(node instanceof AST_SymbolCatch) && node.unreferenced() && AST_Node.warn("{type} {name} is declared but not referenced [{file}:{line},{col}]", {
                type: node instanceof AST_Label ? "Label" : "Symbol",
                name: node.name,
                file: node.start.file,
                line: node.start.line,
                col: node.start.col
            }), options.func_arguments && node instanceof AST_Lambda && node.uses_arguments && AST_Node.warn("arguments used in function {name} [{file}:{line},{col}]", {
                name: node.name ? node.name.name : "anonymous",
                file: node.start.file,
                line: node.start.line,
                col: node.start.col
            }), options.nested_defuns && node instanceof AST_Defun && !(tw.parent() instanceof AST_Scope) && AST_Node.warn('Function {name} declared in nested statement "{type}" [{file}:{line},{col}]', {
                name: node.name.name,
                type: tw.parent().TYPE,
                file: node.start.file,
                line: node.start.line,
                col: node.start.col
            });
        });
        this.walk(tw);
    }), function () {
        function DEFPRINT(nodetype, generator) {
            nodetype.DEFMETHOD("_codegen", generator);
        }
        function PARENS(nodetype, func) {
            Array.isArray(nodetype) ? nodetype.forEach(function (nodetype) {
                PARENS(nodetype, func);
            }) : nodetype.DEFMETHOD("needs_parens", func);
        }
        function display_body(body, is_toplevel, output) {
            var last = body.length - 1;
            body.forEach(function (stmt, i) {
                stmt instanceof AST_EmptyStatement || (output.indent(), stmt.print(output), i == last && is_toplevel || (output.newline(),
                is_toplevel && output.newline()));
            });
        }
        function print_bracketed(body, output) {
            body.length > 0 ? output.with_block(function () {
                display_body(body, !1, output);
            }) : output.print("{}");
        }
        function make_then(self, output) {
            if (output.option("bracketize")) return void make_block(self.body, output);
            if (!self.body) return output.force_semicolon();
            if (self.body instanceof AST_Do && !output.option("screw_ie8")) return void make_block(self.body, output);
            for (var b = self.body; ;) if (b instanceof AST_If) {
                if (!b.alternative) return void make_block(self.body, output);
                b = b.alternative;
            } else {
                if (!(b instanceof AST_StatementWithBody)) break;
                b = b.body;
            }
            force_statement(self.body, output);
        }
        function parenthesize_for_noin(node, output, noin) {
            if (noin) try {
                node.walk(new TreeWalker(function (node) {
                    if (node instanceof AST_Binary && "in" == node.operator) throw output;
                })), node.print(output);
            } catch (ex) {
                if (ex !== output) throw ex;
                node.print(output, !0);
            } else node.print(output);
        }
        function regexp_safe_literal(code) {
            return [92, 47, 46, 43, 42, 63, 40, 41, 91, 93, 123, 125, 36, 94, 58, 124, 33, 10, 13, 0, 65279, 8232, 8233].indexOf(code) < 0;
        }
        function force_statement(stat, output) {
            output.option("bracketize") ? !stat || stat instanceof AST_EmptyStatement ? output.print("{}") : stat instanceof AST_BlockStatement ? stat.print(output) : output.with_block(function () {
                output.indent(), stat.print(output), output.newline();
            }) : !stat || stat instanceof AST_EmptyStatement ? output.force_semicolon() : stat.print(output);
        }
        function first_in_statement(output) {
            for (var a = output.stack(), i = a.length, node = a[--i], p = a[--i]; i > 0;) {
                if (p instanceof AST_Statement && p.body === node) return !0;
                if (!(p instanceof AST_Seq && p.car === node || p instanceof AST_Call && p.expression === node && !(p instanceof AST_New) || p instanceof AST_Dot && p.expression === node || p instanceof AST_Sub && p.expression === node || p instanceof AST_Conditional && p.condition === node || p instanceof AST_Binary && p.left === node || p instanceof AST_UnaryPostfix && p.expression === node)) return !1;
                node = p, p = a[--i];
            }
        }
        function no_constructor_parens(self, output) {
            return 0 == self.args.length && !output.option("beautify");
        }
        function best_of(a) {
            for (var best = a[0], len = best.length, i = 1; i < a.length; ++i) a[i].length < len && (best = a[i],
            len = best.length);
            return best;
        }
        function make_num(num) {
            var m, str = num.toString(10), a = [str.replace(/^0\./, ".").replace("e+", "e")];
            return Math.floor(num) === num ? (num >= 0 ? a.push("0x" + num.toString(16).toLowerCase(), "0" + num.toString(8)) : a.push("-0x" + (-num).toString(16).toLowerCase(), "-0" + (-num).toString(8)),
            (m = /^(.*?)(0+)$/.exec(num)) && a.push(m[1] + "e" + m[2].length)) : (m = /^0?\.(0+)(.*)$/.exec(num)) && a.push(m[2] + "e-" + (m[1].length + m[2].length), str.substr(str.indexOf("."))),
            best_of(a);
        }
        function make_block(stmt, output) {
            return stmt instanceof AST_BlockStatement ? void stmt.print(output) : void output.with_block(function () {
                output.indent(), stmt.print(output), output.newline();
            });
        }
        function DEFMAP(nodetype, generator) {
            nodetype.DEFMETHOD("add_source_map", function (stream) {
                generator(this, stream);
            });
        }
        function basic_sourcemap_gen(self, output) {
            output.add_mapping(self.start);
        }
        AST_Node.DEFMETHOD("print", function (stream, force_parens) {
            function doit() {
                self.add_comments(stream), self.add_source_map(stream), generator(self, stream);
            }
            var self = this, generator = self._codegen;
            stream.push_node(self), force_parens || self.needs_parens(stream) ? stream.with_parens(doit) : doit(),
            stream.pop_node();
        }), AST_Node.DEFMETHOD("print_to_string", function (options) {
            var s = OutputStream(options);
            return this.print(s), s.get();
        }), AST_Node.DEFMETHOD("add_comments", function (output) {
            var c = output.option("comments"), self = this;
            if (c) {
                var start = self.start;
                if (start && !start._comments_dumped) {
                    start._comments_dumped = !0;
                    var comments = start.comments_before || [];
                    self instanceof AST_Exit && self.value && self.value.walk(new TreeWalker(function (node) {
                        return node.start && node.start.comments_before && (comments = comments.concat(node.start.comments_before),
                        node.start.comments_before = []), node instanceof AST_Function || node instanceof AST_Array || node instanceof AST_Object ? !0 : void 0;
                    })), c.test ? comments = comments.filter(function (comment) {
                        return c.test(comment.value);
                    }) : "function" == typeof c && (comments = comments.filter(function (comment) {
                        return c(self, comment);
                    })), !output.option("beautify") && comments.length > 0 && /comment[134]/.test(comments[0].type) && 0 !== output.col() && comments[0].nlb && output.print("\n"),
                    comments.forEach(function (c) {
                        /comment[134]/.test(c.type) ? (output.print("//" + c.value + "\n"), output.indent()) : "comment2" == c.type && (output.print("/*" + c.value + "*/"),
                        start.nlb ? (output.print("\n"), output.indent()) : output.space());
                    });
                }
            }
        }), PARENS(AST_Node, function () {
            return !1;
        }), PARENS(AST_Function, function (output) {
            return first_in_statement(output);
        }), PARENS(AST_Object, function (output) {
            return first_in_statement(output);
        }), PARENS([AST_Unary, AST_Undefined], function (output) {
            var p = output.parent();
            return p instanceof AST_PropAccess && p.expression === this;
        }), PARENS(AST_Seq, function (output) {
            var p = output.parent();
            return p instanceof AST_Call || p instanceof AST_Unary || p instanceof AST_Binary || p instanceof AST_VarDef || p instanceof AST_PropAccess || p instanceof AST_Array || p instanceof AST_ObjectProperty || p instanceof AST_Conditional;
        }), PARENS(AST_Binary, function (output) {
            var p = output.parent();
            if (p instanceof AST_Call && p.expression === this) return !0;
            if (p instanceof AST_Unary) return !0;
            if (p instanceof AST_PropAccess && p.expression === this) return !0;
            if (p instanceof AST_Binary) {
                var po = p.operator, pp = PRECEDENCE[po], so = this.operator, sp = PRECEDENCE[so];
                if (pp > sp || pp == sp && this === p.right) return !0;
            }
        }), PARENS(AST_PropAccess, function (output) {
            var p = output.parent();
            if (p instanceof AST_New && p.expression === this) try {
                this.walk(new TreeWalker(function (node) {
                    if (node instanceof AST_Call) throw p;
                }));
            } catch (ex) {
                if (ex !== p) throw ex;
                return !0;
            }
        }), PARENS(AST_Call, function (output) {
            var p1, p = output.parent();
            return p instanceof AST_New && p.expression === this ? !0 : this.expression instanceof AST_Function && p instanceof AST_PropAccess && p.expression === this && (p1 = output.parent(1)) instanceof AST_Assign && p1.left === p;
        }), PARENS(AST_New, function (output) {
            var p = output.parent();
            return no_constructor_parens(this, output) && (p instanceof AST_PropAccess || p instanceof AST_Call && p.expression === this) ? !0 : void 0;
        }), PARENS(AST_Number, function (output) {
            var p = output.parent();
            return this.getValue() < 0 && p instanceof AST_PropAccess && p.expression === this ? !0 : void 0;
        }), PARENS([AST_Assign, AST_Conditional], function (output) {
            var p = output.parent();
            return p instanceof AST_Unary ? !0 : p instanceof AST_Binary && !(p instanceof AST_Assign) ? !0 : p instanceof AST_Call && p.expression === this ? !0 : p instanceof AST_Conditional && p.condition === this ? !0 : p instanceof AST_PropAccess && p.expression === this ? !0 : void 0;
        }), DEFPRINT(AST_Directive, function (self, output) {
            output.print_string(self.value, self.quote), output.semicolon();
        }), DEFPRINT(AST_Debugger, function (self, output) {
            output.print("debugger"), output.semicolon();
        }), AST_StatementWithBody.DEFMETHOD("_do_print_body", function (output) {
            force_statement(this.body, output);
        }), DEFPRINT(AST_Statement, function (self, output) {
            self.body.print(output), output.semicolon();
        }), DEFPRINT(AST_Toplevel, function (self, output) {
            display_body(self.body, !0, output), output.print("");
        }), DEFPRINT(AST_LabeledStatement, function (self, output) {
            self.label.print(output), output.colon(), self.body.print(output);
        }), DEFPRINT(AST_SimpleStatement, function (self, output) {
            self.body.print(output), output.semicolon();
        }), DEFPRINT(AST_BlockStatement, function (self, output) {
            print_bracketed(self.body, output);
        }), DEFPRINT(AST_EmptyStatement, function (self, output) {
            output.semicolon();
        }), DEFPRINT(AST_Do, function (self, output) {
            output.print("do"), output.space(), self._do_print_body(output), output.space(),
            output.print("while"), output.space(), output.with_parens(function () {
                self.condition.print(output);
            }), output.semicolon();
        }), DEFPRINT(AST_While, function (self, output) {
            output.print("while"), output.space(), output.with_parens(function () {
                self.condition.print(output);
            }), output.space(), self._do_print_body(output);
        }), DEFPRINT(AST_For, function (self, output) {
            output.print("for"), output.space(), output.with_parens(function () {
                !self.init || self.init instanceof AST_EmptyStatement ? output.print(";") : (self.init instanceof AST_Definitions ? self.init.print(output) : parenthesize_for_noin(self.init, output, !0),
                output.print(";"), output.space()), self.condition ? (self.condition.print(output),
                output.print(";"), output.space()) : output.print(";"), self.step && self.step.print(output);
            }), output.space(), self._do_print_body(output);
        }), DEFPRINT(AST_ForIn, function (self, output) {
            output.print("for"), output.space(), output.with_parens(function () {
                self.init.print(output), output.space(), output.print("in"), output.space(), self.object.print(output);
            }), output.space(), self._do_print_body(output);
        }), DEFPRINT(AST_With, function (self, output) {
            output.print("with"), output.space(), output.with_parens(function () {
                self.expression.print(output);
            }), output.space(), self._do_print_body(output);
        }), AST_Lambda.DEFMETHOD("_do_print", function (output, nokeyword) {
            var self = this;
            nokeyword || output.print("function"), self.name && (output.space(), self.name.print(output)),
            output.with_parens(function () {
                self.argnames.forEach(function (arg, i) {
                    i && output.comma(), arg.print(output);
                });
            }), output.space(), print_bracketed(self.body, output);
        }), DEFPRINT(AST_Lambda, function (self, output) {
            self._do_print(output);
        }), AST_Exit.DEFMETHOD("_do_print", function (output, kind) {
            output.print(kind), this.value && (output.space(), this.value.print(output)), output.semicolon();
        }), DEFPRINT(AST_Return, function (self, output) {
            self._do_print(output, "return");
        }), DEFPRINT(AST_Throw, function (self, output) {
            self._do_print(output, "throw");
        }), AST_LoopControl.DEFMETHOD("_do_print", function (output, kind) {
            output.print(kind), this.label && (output.space(), this.label.print(output)), output.semicolon();
        }), DEFPRINT(AST_Break, function (self, output) {
            self._do_print(output, "break");
        }), DEFPRINT(AST_Continue, function (self, output) {
            self._do_print(output, "continue");
        }), DEFPRINT(AST_If, function (self, output) {
            output.print("if"), output.space(), output.with_parens(function () {
                self.condition.print(output);
            }), output.space(), self.alternative ? (make_then(self, output), output.space(),
            output.print("else"), output.space(), force_statement(self.alternative, output)) : self._do_print_body(output);
        }), DEFPRINT(AST_Switch, function (self, output) {
            output.print("switch"), output.space(), output.with_parens(function () {
                self.expression.print(output);
            }), output.space(), self.body.length > 0 ? output.with_block(function () {
                self.body.forEach(function (stmt, i) {
                    i && output.newline(), output.indent(!0), stmt.print(output);
                });
            }) : output.print("{}");
        }), AST_SwitchBranch.DEFMETHOD("_do_print_body", function (output) {
            this.body.length > 0 && (output.newline(), this.body.forEach(function (stmt) {
                output.indent(), stmt.print(output), output.newline();
            }));
        }), DEFPRINT(AST_Default, function (self, output) {
            output.print("default:"), self._do_print_body(output);
        }), DEFPRINT(AST_Case, function (self, output) {
            output.print("case"), output.space(), self.expression.print(output), output.print(":"),
            self._do_print_body(output);
        }), DEFPRINT(AST_Try, function (self, output) {
            output.print("try"), output.space(), print_bracketed(self.body, output), self.bcatch && (output.space(),
            self.bcatch.print(output)), self.bfinally && (output.space(), self.bfinally.print(output));
        }), DEFPRINT(AST_Catch, function (self, output) {
            output.print("catch"), output.space(), output.with_parens(function () {
                self.argname.print(output);
            }), output.space(), print_bracketed(self.body, output);
        }), DEFPRINT(AST_Finally, function (self, output) {
            output.print("finally"), output.space(), print_bracketed(self.body, output);
        }), AST_Definitions.DEFMETHOD("_do_print", function (output, kind) {
            output.print(kind), output.space(), this.definitions.forEach(function (def, i) {
                i && output.comma(), def.print(output);
            });
            var p = output.parent(), in_for = p instanceof AST_For || p instanceof AST_ForIn, avoid_semicolon = in_for && p.init === this;
            avoid_semicolon || output.semicolon();
        }), DEFPRINT(AST_Var, function (self, output) {
            self._do_print(output, "var");
        }), DEFPRINT(AST_Const, function (self, output) {
            self._do_print(output, "const");
        }), DEFPRINT(AST_VarDef, function (self, output) {
            if (self.name.print(output), self.value) {
                output.space(), output.print("="), output.space();
                var p = output.parent(1), noin = p instanceof AST_For || p instanceof AST_ForIn;
                parenthesize_for_noin(self.value, output, noin);
            }
        }), DEFPRINT(AST_Call, function (self, output) {
            self.expression.print(output), self instanceof AST_New && no_constructor_parens(self, output) || output.with_parens(function () {
                self.args.forEach(function (expr, i) {
                    i && output.comma(), expr.print(output);
                });
            });
        }), DEFPRINT(AST_New, function (self, output) {
            output.print("new"), output.space(), AST_Call.prototype._codegen(self, output);
        }), AST_Seq.DEFMETHOD("_do_print", function (output) {
            this.car.print(output), this.cdr && (output.comma(), output.should_break() && (output.newline(),
            output.indent()), this.cdr.print(output));
        }), DEFPRINT(AST_Seq, function (self, output) {
            self._do_print(output);
        }), DEFPRINT(AST_Dot, function (self, output) {
            var expr = self.expression;
            expr.print(output), expr instanceof AST_Number && expr.getValue() >= 0 && (/[xa-f.]/i.test(output.last()) || output.print(".")),
            output.print("."), output.add_mapping(self.end), output.print_name(self.property);
        }), DEFPRINT(AST_Sub, function (self, output) {
            self.expression.print(output), output.print("["), self.property.print(output), output.print("]");
        }), DEFPRINT(AST_UnaryPrefix, function (self, output) {
            var op = self.operator;
            output.print(op), (/^[a-z]/i.test(op) || /[+-]$/.test(op) && self.expression instanceof AST_UnaryPrefix && /^[+-]/.test(self.expression.operator)) && output.space(),
            self.expression.print(output);
        }), DEFPRINT(AST_UnaryPostfix, function (self, output) {
            self.expression.print(output), output.print(self.operator);
        }), DEFPRINT(AST_Binary, function (self, output) {
            self.left.print(output), output.space(), output.print(self.operator), "<" == self.operator && self.right instanceof AST_UnaryPrefix && "!" == self.right.operator && self.right.expression instanceof AST_UnaryPrefix && "--" == self.right.expression.operator ? output.print(" ") : output.space(),
            self.right.print(output);
        }), DEFPRINT(AST_Conditional, function (self, output) {
            self.condition.print(output), output.space(), output.print("?"), output.space(),
            self.consequent.print(output), output.space(), output.colon(), self.alternative.print(output);
        }), DEFPRINT(AST_Array, function (self, output) {
            output.with_square(function () {
                var a = self.elements, len = a.length;
                len > 0 && output.space(), a.forEach(function (exp, i) {
                    i && output.comma(), exp.print(output), i === len - 1 && exp instanceof AST_Hole && output.comma();
                }), len > 0 && output.space();
            });
        }), DEFPRINT(AST_Object, function (self, output) {
            self.properties.length > 0 ? output.with_block(function () {
                self.properties.forEach(function (prop, i) {
                    i && (output.print(","), output.newline()), output.indent(), prop.print(output);
                }), output.newline();
            }) : output.print("{}");
        }), DEFPRINT(AST_ObjectKeyVal, function (self, output) {
            var key = self.key, quote = self.quote;
            output.option("quote_keys") ? output.print_string(key + "") : ("number" == typeof key || !output.option("beautify") && +key + "" == key) && parseFloat(key) >= 0 ? output.print(make_num(key)) : (RESERVED_WORDS(key) ? output.option("screw_ie8") : is_identifier_string(key)) ? output.print_name(key) : output.print_string(key, quote),
            output.colon(), self.value.print(output);
        }), DEFPRINT(AST_ObjectSetter, function (self, output) {
            output.print("set"), output.space(), self.key.print(output), self.value._do_print(output, !0);
        }), DEFPRINT(AST_ObjectGetter, function (self, output) {
            output.print("get"), output.space(), self.key.print(output), self.value._do_print(output, !0);
        }), DEFPRINT(AST_Symbol, function (self, output) {
            var def = self.definition();
            output.print_name(def ? def.mangled_name || def.name : self.name);
        }), DEFPRINT(AST_Undefined, function (self, output) {
            output.print("void 0");
        }), DEFPRINT(AST_Hole, noop), DEFPRINT(AST_Infinity, function (self, output) {
            output.print("Infinity");
        }), DEFPRINT(AST_NaN, function (self, output) {
            output.print("NaN");
        }), DEFPRINT(AST_This, function (self, output) {
            output.print("this");
        }), DEFPRINT(AST_Constant, function (self, output) {
            output.print(self.getValue());
        }), DEFPRINT(AST_String, function (self, output) {
            output.print_string(self.getValue(), self.quote);
        }), DEFPRINT(AST_Number, function (self, output) {
            output.print(make_num(self.getValue()));
        }), DEFPRINT(AST_RegExp, function (self, output) {
            var str = self.getValue().toString();
            output.option("ascii_only") ? str = output.to_ascii(str) : output.option("unescape_regexps") && (str = str.split("\\\\").map(function (str) {
                return str.replace(/\\u[0-9a-fA-F]{4}|\\x[0-9a-fA-F]{2}/g, function (s) {
                    var code = parseInt(s.substr(2), 16);
                    return regexp_safe_literal(code) ? String.fromCharCode(code) : s;
                });
            }).join("\\\\")), output.print(str);
            var p = output.parent();
            p instanceof AST_Binary && /^in/.test(p.operator) && p.left === self && output.print(" ");
        }), DEFMAP(AST_Node, noop), DEFMAP(AST_Directive, basic_sourcemap_gen), DEFMAP(AST_Debugger, basic_sourcemap_gen),
        DEFMAP(AST_Symbol, basic_sourcemap_gen), DEFMAP(AST_Jump, basic_sourcemap_gen),
        DEFMAP(AST_StatementWithBody, basic_sourcemap_gen), DEFMAP(AST_LabeledStatement, noop),
        DEFMAP(AST_Lambda, basic_sourcemap_gen), DEFMAP(AST_Switch, basic_sourcemap_gen),
        DEFMAP(AST_SwitchBranch, basic_sourcemap_gen), DEFMAP(AST_BlockStatement, basic_sourcemap_gen),
        DEFMAP(AST_Toplevel, noop), DEFMAP(AST_New, basic_sourcemap_gen), DEFMAP(AST_Try, basic_sourcemap_gen),
        DEFMAP(AST_Catch, basic_sourcemap_gen), DEFMAP(AST_Finally, basic_sourcemap_gen),
        DEFMAP(AST_Definitions, basic_sourcemap_gen), DEFMAP(AST_Constant, basic_sourcemap_gen),
        DEFMAP(AST_ObjectProperty, function (self, output) {
            output.add_mapping(self.start, self.key);
        });
    }(), Compressor.prototype = new TreeTransformer(), merge(Compressor.prototype, {
        option: function (key) {
            return this.options[key];
        },
        warn: function () {
            this.options.warnings && AST_Node.warn.apply(AST_Node, arguments);
        },
        before: function (node, descend, in_list) {
            if (node._squeezed) return node;
            var was_scope = !1;
            return node instanceof AST_Scope && (node = node.hoist_declarations(this), was_scope = !0),
            descend(node, this), node = node.optimize(this), was_scope && node instanceof AST_Scope && (node.drop_unused(this),
            descend(node, this)), node._squeezed = !0, node;
        }
    }), function () {
        function OPT(node, optimizer) {
            node.DEFMETHOD("optimize", function (compressor) {
                var self = this;
                if (self._optimized) return self;
                var opt = optimizer(self, compressor);
                return opt._optimized = !0, opt === self ? opt : opt.transform(compressor);
            });
        }
        function make_node(ctor, orig, props) {
            return props || (props = {}), orig && (props.start || (props.start = orig.start),
            props.end || (props.end = orig.end)), new ctor(props);
        }
        function make_node_from_constant(compressor, val, orig) {
            if (val instanceof AST_Node) return val.transform(compressor);
            switch (typeof val) {
                case "string":
                    return make_node(AST_String, orig, {
                        value: val
                    }).optimize(compressor);

                case "number":
                    return make_node(isNaN(val) ? AST_NaN : AST_Number, orig, {
                        value: val
                    }).optimize(compressor);

                case "boolean":
                    return make_node(val ? AST_True : AST_False, orig).optimize(compressor);

                case "undefined":
                    return make_node(AST_Undefined, orig).optimize(compressor);

                default:
                    if (null === val) return make_node(AST_Null, orig, {
                        value: null
                    }).optimize(compressor);
                    if (val instanceof RegExp) return make_node(AST_RegExp, orig, {
                        value: val
                    }).optimize(compressor);
                    throw new Error(string_template("Can't handle constant of type: {type}", {
                        type: typeof val
                    }));
            }
        }
        function as_statement_array(thing) {
            if (null === thing) return [];
            if (thing instanceof AST_BlockStatement) return thing.body;
            if (thing instanceof AST_EmptyStatement) return [];
            if (thing instanceof AST_Statement) return [thing];
            throw new Error("Can't convert thing to statement array");
        }
        function is_empty(thing) {
            return null === thing ? !0 : thing instanceof AST_EmptyStatement ? !0 : thing instanceof AST_BlockStatement ? 0 == thing.body.length : !1;
        }
        function loop_body(x) {
            return x instanceof AST_Switch ? x : (x instanceof AST_For || x instanceof AST_ForIn || x instanceof AST_DWLoop) && x.body instanceof AST_BlockStatement ? x.body : x;
        }
        function tighten_body(statements, compressor) {
            function process_for_angular(statements) {
                function has_inject(comment) {
                    return /@ngInject/.test(comment.value);
                }
                function make_arguments_names_list(func) {
                    return func.argnames.map(function (sym) {
                        return make_node(AST_String, sym, {
                            value: sym.name
                        });
                    });
                }
                function make_array(orig, elements) {
                    return make_node(AST_Array, orig, {
                        elements: elements
                    });
                }
                function make_injector(func, name) {
                    return make_node(AST_SimpleStatement, func, {
                        body: make_node(AST_Assign, func, {
                            operator: "=",
                            left: make_node(AST_Dot, name, {
                                expression: make_node(AST_SymbolRef, name, name),
                                property: "$inject"
                            }),
                            right: make_array(func, make_arguments_names_list(func))
                        })
                    });
                }
                function check_expression(body) {
                    body && body.args && (body.args.forEach(function (argument, index, array) {
                        var comments = argument.start.comments_before;
                        argument instanceof AST_Lambda && comments.length && has_inject(comments[0]) && (array[index] = make_array(argument, make_arguments_names_list(argument).concat(argument)));
                    }), body.expression && body.expression.expression && check_expression(body.expression.expression));
                }
                return statements.reduce(function (a, stat) {
                    if (a.push(stat), stat.body && stat.body.args) check_expression(stat.body); else {
                        var token = stat.start, comments = token.comments_before;
                        if (comments && comments.length > 0) {
                            var last = comments.pop();
                            has_inject(last) && (stat instanceof AST_Defun ? a.push(make_injector(stat, stat.name)) : stat instanceof AST_Definitions ? stat.definitions.forEach(function (def) {
                                def.value && def.value instanceof AST_Lambda && a.push(make_injector(def.value, def.name));
                            }) : compressor.warn("Unknown statement marked with @ngInject [{file}:{line},{col}]", token));
                        }
                    }
                    return a;
                }, []);
            }
            function eliminate_spurious_blocks(statements) {
                var seen_dirs = [];
                return statements.reduce(function (a, stat) {
                    return stat instanceof AST_BlockStatement ? (CHANGED = !0, a.push.apply(a, eliminate_spurious_blocks(stat.body))) : stat instanceof AST_EmptyStatement ? CHANGED = !0 : stat instanceof AST_Directive ? seen_dirs.indexOf(stat.value) < 0 ? (a.push(stat),
                    seen_dirs.push(stat.value)) : CHANGED = !0 : a.push(stat), a;
                }, []);
            }
            function handle_if_return(statements, compressor) {
                var self = compressor.self(), in_lambda = self instanceof AST_Lambda, ret = [];
                loop: for (var i = statements.length; --i >= 0;) {
                    var stat = statements[i];
                    switch (!0) {
                        case in_lambda && stat instanceof AST_Return && !stat.value && 0 == ret.length:
                            CHANGED = !0;
                            continue loop;

                        case stat instanceof AST_If:
                            if (stat.body instanceof AST_Return) {
                                if ((in_lambda && 0 == ret.length || ret[0] instanceof AST_Return && !ret[0].value) && !stat.body.value && !stat.alternative) {
                                    CHANGED = !0;
                                    var cond = make_node(AST_SimpleStatement, stat.condition, {
                                        body: stat.condition
                                    });
                                    ret.unshift(cond);
                                    continue loop;
                                }
                                if (ret[0] instanceof AST_Return && stat.body.value && ret[0].value && !stat.alternative) {
                                    CHANGED = !0, stat = stat.clone(), stat.alternative = ret[0], ret[0] = stat.transform(compressor);
                                    continue loop;
                                }
                                if ((0 == ret.length || ret[0] instanceof AST_Return) && stat.body.value && !stat.alternative && in_lambda) {
                                    CHANGED = !0, stat = stat.clone(), stat.alternative = ret[0] || make_node(AST_Return, stat, {
                                        value: make_node(AST_Undefined, stat)
                                    }), ret[0] = stat.transform(compressor);
                                    continue loop;
                                }
                                if (!stat.body.value && in_lambda) {
                                    CHANGED = !0, stat = stat.clone(), stat.condition = stat.condition.negate(compressor),
                                    stat.body = make_node(AST_BlockStatement, stat, {
                                        body: as_statement_array(stat.alternative).concat(ret)
                                    }), stat.alternative = null, ret = [stat.transform(compressor)];
                                    continue loop;
                                }
                                if (1 == ret.length && in_lambda && ret[0] instanceof AST_SimpleStatement && (!stat.alternative || stat.alternative instanceof AST_SimpleStatement)) {
                                    CHANGED = !0, ret.push(make_node(AST_Return, ret[0], {
                                        value: make_node(AST_Undefined, ret[0])
                                    }).transform(compressor)), ret = as_statement_array(stat.alternative).concat(ret),
                                    ret.unshift(stat);
                                    continue loop;
                                }
                            }
                            var ab = aborts(stat.body), lct = ab instanceof AST_LoopControl ? compressor.loopcontrol_target(ab.label) : null;
                            if (ab && (ab instanceof AST_Return && !ab.value && in_lambda || ab instanceof AST_Continue && self === loop_body(lct) || ab instanceof AST_Break && lct instanceof AST_BlockStatement && self === lct)) {
                                ab.label && remove(ab.label.thedef.references, ab), CHANGED = !0;
                                var body = as_statement_array(stat.body).slice(0, -1);
                                stat = stat.clone(), stat.condition = stat.condition.negate(compressor), stat.body = make_node(AST_BlockStatement, stat, {
                                    body: as_statement_array(stat.alternative).concat(ret)
                                }), stat.alternative = make_node(AST_BlockStatement, stat, {
                                    body: body
                                }), ret = [stat.transform(compressor)];
                                continue loop;
                            }
                            var ab = aborts(stat.alternative), lct = ab instanceof AST_LoopControl ? compressor.loopcontrol_target(ab.label) : null;
                            if (ab && (ab instanceof AST_Return && !ab.value && in_lambda || ab instanceof AST_Continue && self === loop_body(lct) || ab instanceof AST_Break && lct instanceof AST_BlockStatement && self === lct)) {
                                ab.label && remove(ab.label.thedef.references, ab), CHANGED = !0, stat = stat.clone(),
                                stat.body = make_node(AST_BlockStatement, stat.body, {
                                    body: as_statement_array(stat.body).concat(ret)
                                }), stat.alternative = make_node(AST_BlockStatement, stat.alternative, {
                                    body: as_statement_array(stat.alternative).slice(0, -1)
                                }), ret = [stat.transform(compressor)];
                                continue loop;
                            }
                            ret.unshift(stat);
                            break;

                        default:
                            ret.unshift(stat);
                    }
                }
                return ret;
            }
            function eliminate_dead_code(statements, compressor) {
                var has_quit = !1, orig = statements.length, self = compressor.self();
                return statements = statements.reduce(function (a, stat) {
                    if (has_quit) extract_declarations_from_unreachable_code(compressor, stat, a); else {
                        if (stat instanceof AST_LoopControl) {
                            var lct = compressor.loopcontrol_target(stat.label);
                            stat instanceof AST_Break && lct instanceof AST_BlockStatement && loop_body(lct) === self || stat instanceof AST_Continue && loop_body(lct) === self ? stat.label && remove(stat.label.thedef.references, stat) : a.push(stat);
                        } else a.push(stat);
                        aborts(stat) && (has_quit = !0);
                    }
                    return a;
                }, []), CHANGED = statements.length != orig, statements;
            }
            function sequencesize(statements, compressor) {
                function push_seq() {
                    seq = AST_Seq.from_array(seq), seq && ret.push(make_node(AST_SimpleStatement, seq, {
                        body: seq
                    })), seq = [];
                }
                if (statements.length < 2) return statements;
                var seq = [], ret = [];
                return statements.forEach(function (stat) {
                    stat instanceof AST_SimpleStatement && seq.length < 2e3 ? seq.push(stat.body) : (push_seq(),
                    ret.push(stat));
                }), push_seq(), ret = sequencesize_2(ret, compressor), CHANGED = ret.length != statements.length,
                ret;
            }
            function sequencesize_2(statements, compressor) {
                function cons_seq(right) {
                    ret.pop();
                    var left = prev.body;
                    return left instanceof AST_Seq ? left.add(right) : left = AST_Seq.cons(left, right),
                    left.transform(compressor);
                }
                var ret = [], prev = null;
                return statements.forEach(function (stat) {
                    if (prev) if (stat instanceof AST_For) {
                        var opera = {};
                        try {
                            prev.body.walk(new TreeWalker(function (node) {
                                if (node instanceof AST_Binary && "in" == node.operator) throw opera;
                            })), !stat.init || stat.init instanceof AST_Definitions ? stat.init || (stat.init = prev.body,
                            ret.pop()) : stat.init = cons_seq(stat.init);
                        } catch (ex) {
                            if (ex !== opera) throw ex;
                        }
                    } else stat instanceof AST_If ? stat.condition = cons_seq(stat.condition) : stat instanceof AST_With ? stat.expression = cons_seq(stat.expression) : stat instanceof AST_Exit && stat.value ? stat.value = cons_seq(stat.value) : stat instanceof AST_Exit ? stat.value = cons_seq(make_node(AST_Undefined, stat)) : stat instanceof AST_Switch && (stat.expression = cons_seq(stat.expression));
                    ret.push(stat), prev = stat instanceof AST_SimpleStatement ? stat : null;
                }), ret;
            }
            function join_consecutive_vars(statements, compressor) {
                var prev = null;
                return statements.reduce(function (a, stat) {
                    return stat instanceof AST_Definitions && prev && prev.TYPE == stat.TYPE ? (prev.definitions = prev.definitions.concat(stat.definitions),
                    CHANGED = !0) : stat instanceof AST_For && prev instanceof AST_Definitions && (!stat.init || stat.init.TYPE == prev.TYPE) ? (CHANGED = !0,
                    a.pop(), stat.init ? stat.init.definitions = prev.definitions.concat(stat.init.definitions) : stat.init = prev,
                    a.push(stat), prev = stat) : (prev = stat, a.push(stat)), a;
                }, []);
            }
            function negate_iifes(statements, compressor) {
                statements.forEach(function (stat) {
                    stat instanceof AST_SimpleStatement && (stat.body = function transform(thing) {
                        return thing.transform(new TreeTransformer(function (node) {
                            if (node instanceof AST_Call && node.expression instanceof AST_Function) return make_node(AST_UnaryPrefix, node, {
                                operator: "!",
                                expression: node
                            });
                            if (node instanceof AST_Call) node.expression = transform(node.expression); else if (node instanceof AST_Seq) node.car = transform(node.car); else if (node instanceof AST_Conditional) {
                                var expr = transform(node.condition);
                                if (expr !== node.condition) {
                                    node.condition = expr;
                                    var tmp = node.consequent;
                                    node.consequent = node.alternative, node.alternative = tmp;
                                }
                            }
                            return node;
                        }));
                    }(stat.body));
                });
            }
            var CHANGED;
            do CHANGED = !1, compressor.option("angular") && (statements = process_for_angular(statements)),
            statements = eliminate_spurious_blocks(statements), compressor.option("dead_code") && (statements = eliminate_dead_code(statements, compressor)),
            compressor.option("if_return") && (statements = handle_if_return(statements, compressor)),
            compressor.option("sequences") && (statements = sequencesize(statements, compressor)),
            compressor.option("join_vars") && (statements = join_consecutive_vars(statements, compressor)); while (CHANGED);
            return compressor.option("negate_iife") && negate_iifes(statements, compressor),
            statements;
        }
        function extract_declarations_from_unreachable_code(compressor, stat, target) {
            compressor.warn("Dropping unreachable code [{file}:{line},{col}]", stat.start),
            stat.walk(new TreeWalker(function (node) {
                return node instanceof AST_Definitions ? (compressor.warn("Declarations in unreachable code! [{file}:{line},{col}]", node.start),
                node.remove_initializers(), target.push(node), !0) : node instanceof AST_Defun ? (target.push(node),
                !0) : node instanceof AST_Scope ? !0 : void 0;
            }));
        }
        function best_of(ast1, ast2) {
            return ast1.print_to_string().length > ast2.print_to_string().length ? ast2 : ast1;
        }
        function aborts(thing) {
            return thing && thing.aborts();
        }
        function if_break_in_loop(self, compressor) {
            function drop_it(rest) {
                rest = as_statement_array(rest), self.body instanceof AST_BlockStatement ? (self.body = self.body.clone(),
                self.body.body = rest.concat(self.body.body.slice(1)), self.body = self.body.transform(compressor)) : self.body = make_node(AST_BlockStatement, self.body, {
                    body: rest
                }).transform(compressor), if_break_in_loop(self, compressor);
            }
            var first = self.body instanceof AST_BlockStatement ? self.body.body[0] : self.body;
            first instanceof AST_If && (first.body instanceof AST_Break && compressor.loopcontrol_target(first.body.label) === self ? (self.condition = self.condition ? make_node(AST_Binary, self.condition, {
                left: self.condition,
                operator: "&&",
                right: first.condition.negate(compressor)
            }) : first.condition.negate(compressor), drop_it(first.alternative)) : first.alternative instanceof AST_Break && compressor.loopcontrol_target(first.alternative.label) === self && (self.condition = self.condition ? make_node(AST_Binary, self.condition, {
                left: self.condition,
                operator: "&&",
                right: first.condition
            }) : first.condition, drop_it(first.body)));
        }
        function has_side_effects_or_prop_access(node, compressor) {
            var save_pure_getters = compressor.option("pure_getters");
            compressor.options.pure_getters = !1;
            var ret = node.has_side_effects(compressor);
            return compressor.options.pure_getters = save_pure_getters, ret;
        }
        function literals_in_boolean_context(self, compressor) {
            return compressor.option("booleans") && compressor.in_boolean_context() && !self.has_side_effects(compressor) ? make_node(AST_True, self) : self;
        }
        OPT(AST_Node, function (self, compressor) {
            return self;
        }), AST_Node.DEFMETHOD("equivalent_to", function (node) {
            return this.print_to_string() == node.print_to_string();
        }), function (def) {
            var unary_bool = ["!", "delete"], binary_bool = ["in", "instanceof", "==", "!=", "===", "!==", "<", "<=", ">=", ">"];
            def(AST_Node, function () {
                return !1;
            }), def(AST_UnaryPrefix, function () {
                return member(this.operator, unary_bool);
            }), def(AST_Binary, function () {
                return member(this.operator, binary_bool) || ("&&" == this.operator || "||" == this.operator) && this.left.is_boolean() && this.right.is_boolean();
            }), def(AST_Conditional, function () {
                return this.consequent.is_boolean() && this.alternative.is_boolean();
            }), def(AST_Assign, function () {
                return "=" == this.operator && this.right.is_boolean();
            }), def(AST_Seq, function () {
                return this.cdr.is_boolean();
            }), def(AST_True, function () {
                return !0;
            }), def(AST_False, function () {
                return !0;
            });
        }(function (node, func) {
            node.DEFMETHOD("is_boolean", func);
        }), function (def) {
            def(AST_Node, function () {
                return !1;
            }), def(AST_String, function () {
                return !0;
            }), def(AST_UnaryPrefix, function () {
                return "typeof" == this.operator;
            }), def(AST_Binary, function (compressor) {
                return "+" == this.operator && (this.left.is_string(compressor) || this.right.is_string(compressor));
            }), def(AST_Assign, function (compressor) {
                return ("=" == this.operator || "+=" == this.operator) && this.right.is_string(compressor);
            }), def(AST_Seq, function (compressor) {
                return this.cdr.is_string(compressor);
            }), def(AST_Conditional, function (compressor) {
                return this.consequent.is_string(compressor) && this.alternative.is_string(compressor);
            }), def(AST_Call, function (compressor) {
                return compressor.option("unsafe") && this.expression instanceof AST_SymbolRef && "String" == this.expression.name && this.expression.undeclared();
            });
        }(function (node, func) {
            node.DEFMETHOD("is_string", func);
        }), function (def) {
            function ev(node, compressor) {
                if (!compressor) throw new Error("Compressor must be passed");
                return node._eval(compressor);
            }
            AST_Node.DEFMETHOD("evaluate", function (compressor) {
                if (!compressor.option("evaluate")) return [this];
                try {
                    var val = this._eval(compressor);
                    return [best_of(make_node_from_constant(compressor, val, this), this), val];
                } catch (ex) {
                    if (ex !== def) throw ex;
                    return [this];
                }
            }), def(AST_Statement, function () {
                throw new Error(string_template("Cannot evaluate a statement [{file}:{line},{col}]", this.start));
            }), def(AST_Function, function () {
                throw def;
            }), def(AST_Node, function () {
                throw def;
            }), def(AST_Constant, function () {
                return this.getValue();
            }), def(AST_UnaryPrefix, function (compressor) {
                var e = this.expression;
                switch (this.operator) {
                    case "!":
                        return !ev(e, compressor);

                    case "typeof":
                        if (e instanceof AST_Function) return "function";
                        if (e = ev(e, compressor), e instanceof RegExp) throw def;
                        return typeof e;

                    case "void":
                        return void ev(e, compressor);

                    case "~":
                        return ~ev(e, compressor);

                    case "-":
                        if (e = ev(e, compressor), 0 === e) throw def;
                        return -e;

                    case "+":
                        return +ev(e, compressor);
                }
                throw def;
            }), def(AST_Binary, function (c) {
                var left = this.left, right = this.right;
                switch (this.operator) {
                    case "&&":
                        return ev(left, c) && ev(right, c);

                    case "||":
                        return ev(left, c) || ev(right, c);

                    case "|":
                        return ev(left, c) | ev(right, c);

                    case "&":
                        return ev(left, c) & ev(right, c);

                    case "^":
                        return ev(left, c) ^ ev(right, c);

                    case "+":
                        return ev(left, c) + ev(right, c);

                    case "*":
                        return ev(left, c) * ev(right, c);

                    case "/":
                        return ev(left, c) / ev(right, c);

                    case "%":
                        return ev(left, c) % ev(right, c);

                    case "-":
                        return ev(left, c) - ev(right, c);

                    case "<<":
                        return ev(left, c) << ev(right, c);

                    case ">>":
                        return ev(left, c) >> ev(right, c);

                    case ">>>":
                        return ev(left, c) >>> ev(right, c);

                    case "==":
                        return ev(left, c) == ev(right, c);

                    case "===":
                        return ev(left, c) === ev(right, c);

                    case "!=":
                        return ev(left, c) != ev(right, c);

                    case "!==":
                        return ev(left, c) !== ev(right, c);

                    case "<":
                        return ev(left, c) < ev(right, c);

                    case "<=":
                        return ev(left, c) <= ev(right, c);

                    case ">":
                        return ev(left, c) > ev(right, c);

                    case ">=":
                        return ev(left, c) >= ev(right, c);

                    case "in":
                        return ev(left, c) in ev(right, c);

                    case "instanceof":
                        return ev(left, c) instanceof ev(right, c);
                }
                throw def;
            }), def(AST_Conditional, function (compressor) {
                return ev(this.condition, compressor) ? ev(this.consequent, compressor) : ev(this.alternative, compressor);
            }), def(AST_SymbolRef, function (compressor) {
                var d = this.definition();
                if (d && d.constant && d.init) return ev(d.init, compressor);
                throw def;
            }), def(AST_Dot, function (compressor) {
                if (compressor.option("unsafe") && "length" == this.property) {
                    var str = ev(this.expression, compressor);
                    if ("string" == typeof str) return str.length;
                }
                throw def;
            });
        }(function (node, func) {
            node.DEFMETHOD("_eval", func);
        }), function (def) {
            function basic_negation(exp) {
                return make_node(AST_UnaryPrefix, exp, {
                    operator: "!",
                    expression: exp
                });
            }
            def(AST_Node, function () {
                return basic_negation(this);
            }), def(AST_Statement, function () {
                throw new Error("Cannot negate a statement");
            }), def(AST_Function, function () {
                return basic_negation(this);
            }), def(AST_UnaryPrefix, function () {
                return "!" == this.operator ? this.expression : basic_negation(this);
            }), def(AST_Seq, function (compressor) {
                var self = this.clone();
                return self.cdr = self.cdr.negate(compressor), self;
            }), def(AST_Conditional, function (compressor) {
                var self = this.clone();
                return self.consequent = self.consequent.negate(compressor), self.alternative = self.alternative.negate(compressor),
                best_of(basic_negation(this), self);
            }), def(AST_Binary, function (compressor) {
                var self = this.clone(), op = this.operator;
                if (compressor.option("unsafe_comps")) switch (op) {
                    case "<=":
                        return self.operator = ">", self;

                    case "<":
                        return self.operator = ">=", self;

                    case ">=":
                        return self.operator = "<", self;

                    case ">":
                        return self.operator = "<=", self;
                }
                switch (op) {
                    case "==":
                        return self.operator = "!=", self;

                    case "!=":
                        return self.operator = "==", self;

                    case "===":
                        return self.operator = "!==", self;

                    case "!==":
                        return self.operator = "===", self;

                    case "&&":
                        return self.operator = "||", self.left = self.left.negate(compressor), self.right = self.right.negate(compressor),
                        best_of(basic_negation(this), self);

                    case "||":
                        return self.operator = "&&", self.left = self.left.negate(compressor), self.right = self.right.negate(compressor),
                        best_of(basic_negation(this), self);
                }
                return basic_negation(this);
            });
        }(function (node, func) {
            node.DEFMETHOD("negate", function (compressor) {
                return func.call(this, compressor);
            });
        }), function (def) {
            def(AST_Node, function (compressor) {
                return !0;
            }), def(AST_EmptyStatement, function (compressor) {
                return !1;
            }), def(AST_Constant, function (compressor) {
                return !1;
            }), def(AST_This, function (compressor) {
                return !1;
            }), def(AST_Call, function (compressor) {
                var pure = compressor.option("pure_funcs");
                return pure ? pure.indexOf(this.expression.print_to_string()) < 0 : !0;
            }), def(AST_Block, function (compressor) {
                for (var i = this.body.length; --i >= 0;) if (this.body[i].has_side_effects(compressor)) return !0;
                return !1;
            }), def(AST_SimpleStatement, function (compressor) {
                return this.body.has_side_effects(compressor);
            }), def(AST_Defun, function (compressor) {
                return !0;
            }), def(AST_Function, function (compressor) {
                return !1;
            }), def(AST_Binary, function (compressor) {
                return this.left.has_side_effects(compressor) || this.right.has_side_effects(compressor);
            }), def(AST_Assign, function (compressor) {
                return !0;
            }), def(AST_Conditional, function (compressor) {
                return this.condition.has_side_effects(compressor) || this.consequent.has_side_effects(compressor) || this.alternative.has_side_effects(compressor);
            }), def(AST_Unary, function (compressor) {
                return "delete" == this.operator || "++" == this.operator || "--" == this.operator || this.expression.has_side_effects(compressor);
            }), def(AST_SymbolRef, function (compressor) {
                return this.global() && this.undeclared();
            }), def(AST_Object, function (compressor) {
                for (var i = this.properties.length; --i >= 0;) if (this.properties[i].has_side_effects(compressor)) return !0;
                return !1;
            }), def(AST_ObjectProperty, function (compressor) {
                return this.value.has_side_effects(compressor);
            }), def(AST_Array, function (compressor) {
                for (var i = this.elements.length; --i >= 0;) if (this.elements[i].has_side_effects(compressor)) return !0;
                return !1;
            }), def(AST_Dot, function (compressor) {
                return compressor.option("pure_getters") ? this.expression.has_side_effects(compressor) : !0;
            }), def(AST_Sub, function (compressor) {
                return compressor.option("pure_getters") ? this.expression.has_side_effects(compressor) || this.property.has_side_effects(compressor) : !0;
            }), def(AST_PropAccess, function (compressor) {
                return !compressor.option("pure_getters");
            }), def(AST_Seq, function (compressor) {
                return this.car.has_side_effects(compressor) || this.cdr.has_side_effects(compressor);
            });
        }(function (node, func) {
            node.DEFMETHOD("has_side_effects", func);
        }), function (def) {
            function block_aborts() {
                var n = this.body.length;
                return n > 0 && aborts(this.body[n - 1]);
            }
            def(AST_Statement, function () {
                return null;
            }), def(AST_Jump, function () {
                return this;
            }), def(AST_BlockStatement, block_aborts), def(AST_SwitchBranch, block_aborts),
            def(AST_If, function () {
                return this.alternative && aborts(this.body) && aborts(this.alternative) && this;
            });
        }(function (node, func) {
            node.DEFMETHOD("aborts", func);
        }), OPT(AST_Directive, function (self, compressor) {
            return self.scope.has_directive(self.value) !== self.scope ? make_node(AST_EmptyStatement, self) : self;
        }), OPT(AST_Debugger, function (self, compressor) {
            return compressor.option("drop_debugger") ? make_node(AST_EmptyStatement, self) : self;
        }), OPT(AST_LabeledStatement, function (self, compressor) {
            return self.body instanceof AST_Break && compressor.loopcontrol_target(self.body.label) === self.body ? make_node(AST_EmptyStatement, self) : 0 == self.label.references.length ? self.body : self;
        }), OPT(AST_Block, function (self, compressor) {
            return self.body = tighten_body(self.body, compressor), self;
        }), OPT(AST_BlockStatement, function (self, compressor) {
            switch (self.body = tighten_body(self.body, compressor), self.body.length) {
                case 1:
                    return self.body[0];

                case 0:
                    return make_node(AST_EmptyStatement, self);
            }
            return self;
        }), AST_Scope.DEFMETHOD("drop_unused", function (compressor) {
            var self = this;
            if (compressor.option("unused") && !(self instanceof AST_Toplevel) && !self.uses_eval) {
                var in_use = [], initializations = new Dictionary(), scope = this, tw = new TreeWalker(function (node, descend) {
                    if (node !== self) {
                        if (node instanceof AST_Defun) return initializations.add(node.name.name, node),
                        !0;
                        if (node instanceof AST_Definitions && scope === self) return node.definitions.forEach(function (def) {
                            def.value && (initializations.add(def.name.name, def.value), def.value.has_side_effects(compressor) && def.value.walk(tw));
                        }), !0;
                        if (node instanceof AST_SymbolRef) return push_uniq(in_use, node.definition()),
                        !0;
                        if (node instanceof AST_Scope) {
                            var save_scope = scope;
                            return scope = node, descend(), scope = save_scope, !0;
                        }
                    }
                });
                self.walk(tw);
                for (var i = 0; i < in_use.length; ++i) in_use[i].orig.forEach(function (decl) {
                    var init = initializations.get(decl.name);
                    init && init.forEach(function (init) {
                        var tw = new TreeWalker(function (node) {
                            node instanceof AST_SymbolRef && push_uniq(in_use, node.definition());
                        });
                        init.walk(tw);
                    });
                });
                var tt = new TreeTransformer(function (node, descend, in_list) {
                    if (node instanceof AST_Lambda && !(node instanceof AST_Accessor) && compressor.option("unsafe") && !compressor.option("keep_fargs")) for (var a = node.argnames, i = a.length; --i >= 0;) {
                        var sym = a[i];
                        if (!sym.unreferenced()) break;
                        a.pop(), compressor.warn("Dropping unused function argument {name} [{file}:{line},{col}]", {
                            name: sym.name,
                            file: sym.start.file,
                            line: sym.start.line,
                            col: sym.start.col
                        });
                    }
                    if (node instanceof AST_Defun && node !== self) return member(node.name.definition(), in_use) ? node : (compressor.warn("Dropping unused function {name} [{file}:{line},{col}]", {
                        name: node.name.name,
                        file: node.name.start.file,
                        line: node.name.start.line,
                        col: node.name.start.col
                    }), make_node(AST_EmptyStatement, node));
                    if (node instanceof AST_Definitions && !(tt.parent() instanceof AST_ForIn)) {
                        var def = node.definitions.filter(function (def) {
                            if (member(def.name.definition(), in_use)) return !0;
                            var w = {
                                name: def.name.name,
                                file: def.name.start.file,
                                line: def.name.start.line,
                                col: def.name.start.col
                            };
                            return def.value && def.value.has_side_effects(compressor) ? (def._unused_side_effects = !0,
                            compressor.warn("Side effects in initialization of unused variable {name} [{file}:{line},{col}]", w),
                            !0) : (compressor.warn("Dropping unused variable {name} [{file}:{line},{col}]", w),
                            !1);
                        });
                        def = mergeSort(def, function (a, b) {
                            return !a.value && b.value ? -1 : !b.value && a.value ? 1 : 0;
                        });
                        for (var side_effects = [], i = 0; i < def.length;) {
                            var x = def[i];
                            x._unused_side_effects ? (side_effects.push(x.value), def.splice(i, 1)) : (side_effects.length > 0 && (side_effects.push(x.value),
                            x.value = AST_Seq.from_array(side_effects), side_effects = []), ++i);
                        }
                        return side_effects = side_effects.length > 0 ? make_node(AST_BlockStatement, node, {
                            body: [make_node(AST_SimpleStatement, node, {
                                body: AST_Seq.from_array(side_effects)
                            })]
                        }) : null, 0 != def.length || side_effects ? 0 == def.length ? side_effects : (node.definitions = def,
                        side_effects && (side_effects.body.unshift(node), node = side_effects), node) : make_node(AST_EmptyStatement, node);
                    }
                    if (node instanceof AST_For && (descend(node, this), node.init instanceof AST_BlockStatement)) {
                        var body = node.init.body.slice(0, -1);
                        return node.init = node.init.body.slice(-1)[0].body, body.push(node), in_list ? MAP.splice(body) : make_node(AST_BlockStatement, node, {
                            body: body
                        });
                    }
                    return node instanceof AST_Scope && node !== self ? node : void 0;
                });
                self.transform(tt);
            }
        }), AST_Scope.DEFMETHOD("hoist_declarations", function (compressor) {
            var hoist_funs = compressor.option("hoist_funs"), hoist_vars = compressor.option("hoist_vars"), self = this;
            if (hoist_funs || hoist_vars) {
                var dirs = [], hoisted = [], vars = new Dictionary(), vars_found = 0, var_decl = 0;
                self.walk(new TreeWalker(function (node) {
                    return node instanceof AST_Scope && node !== self ? !0 : node instanceof AST_Var ? (++var_decl,
                    !0) : void 0;
                })), hoist_vars = hoist_vars && var_decl > 1;
                var tt = new TreeTransformer(function (node) {
                    if (node !== self) {
                        if (node instanceof AST_Directive) return dirs.push(node), make_node(AST_EmptyStatement, node);
                        if (node instanceof AST_Defun && hoist_funs) return hoisted.push(node), make_node(AST_EmptyStatement, node);
                        if (node instanceof AST_Var && hoist_vars) {
                            node.definitions.forEach(function (def) {
                                vars.set(def.name.name, def), ++vars_found;
                            });
                            var seq = node.to_assignments(), p = tt.parent();
                            return p instanceof AST_ForIn && p.init === node ? null == seq ? node.definitions[0].name : seq : p instanceof AST_For && p.init === node ? seq : seq ? make_node(AST_SimpleStatement, node, {
                                body: seq
                            }) : make_node(AST_EmptyStatement, node);
                        }
                        if (node instanceof AST_Scope) return node;
                    }
                });
                if (self = self.transform(tt), vars_found > 0) {
                    var defs = [];
                    if (vars.each(function (def, name) {
                        self instanceof AST_Lambda && find_if(function (x) {
                            return x.name == def.name.name;
                    }, self.argnames) ? vars.del(name) : (def = def.clone(), def.value = null, defs.push(def),
                        vars.set(name, def));
                    }), defs.length > 0) {
                        for (var i = 0; i < self.body.length;) {
                            if (self.body[i] instanceof AST_SimpleStatement) {
                                var sym, assign, expr = self.body[i].body;
                                if (expr instanceof AST_Assign && "=" == expr.operator && (sym = expr.left) instanceof AST_Symbol && vars.has(sym.name)) {
                                    var def = vars.get(sym.name);
                                    if (def.value) break;
                                    def.value = expr.right, remove(defs, def), defs.push(def), self.body.splice(i, 1);
                                    continue;
                                }
                                if (expr instanceof AST_Seq && (assign = expr.car) instanceof AST_Assign && "=" == assign.operator && (sym = assign.left) instanceof AST_Symbol && vars.has(sym.name)) {
                                    var def = vars.get(sym.name);
                                    if (def.value) break;
                                    def.value = assign.right, remove(defs, def), defs.push(def), self.body[i].body = expr.cdr;
                                    continue;
                                }
                            }
                            if (self.body[i] instanceof AST_EmptyStatement) self.body.splice(i, 1); else {
                                if (!(self.body[i] instanceof AST_BlockStatement)) break;
                                var tmp = [i, 1].concat(self.body[i].body);
                                self.body.splice.apply(self.body, tmp);
                            }
                        }
                        defs = make_node(AST_Var, self, {
                            definitions: defs
                        }), hoisted.push(defs);
                    }
                }
                self.body = dirs.concat(hoisted, self.body);
            }
            return self;
        }), OPT(AST_SimpleStatement, function (self, compressor) {
            return compressor.option("side_effects") && !self.body.has_side_effects(compressor) ? (compressor.warn("Dropping side-effect-free statement [{file}:{line},{col}]", self.start),
            make_node(AST_EmptyStatement, self)) : self;
        }), OPT(AST_DWLoop, function (self, compressor) {
            var cond = self.condition.evaluate(compressor);
            if (self.condition = cond[0], !compressor.option("loops")) return self;
            if (cond.length > 1) {
                if (cond[1]) return make_node(AST_For, self, {
                    body: self.body
                });
                if (self instanceof AST_While && compressor.option("dead_code")) {
                    var a = [];
                    return extract_declarations_from_unreachable_code(compressor, self.body, a), make_node(AST_BlockStatement, self, {
                        body: a
                    });
                }
            }
            return self;
        }), OPT(AST_While, function (self, compressor) {
            return compressor.option("loops") ? (self = AST_DWLoop.prototype.optimize.call(self, compressor),
            self instanceof AST_While && (if_break_in_loop(self, compressor), self = make_node(AST_For, self, self).transform(compressor)),
            self) : self;
        }), OPT(AST_For, function (self, compressor) {
            var cond = self.condition;
            if (cond && (cond = cond.evaluate(compressor), self.condition = cond[0]), !compressor.option("loops")) return self;
            if (cond && cond.length > 1 && !cond[1] && compressor.option("dead_code")) {
                var a = [];
                return self.init instanceof AST_Statement ? a.push(self.init) : self.init && a.push(make_node(AST_SimpleStatement, self.init, {
                    body: self.init
                })), extract_declarations_from_unreachable_code(compressor, self.body, a), make_node(AST_BlockStatement, self, {
                    body: a
                });
            }
            return if_break_in_loop(self, compressor), self;
        }), OPT(AST_If, function (self, compressor) {
            if (!compressor.option("conditionals")) return self;
            var cond = self.condition.evaluate(compressor);
            if (self.condition = cond[0], cond.length > 1) if (cond[1]) {
                if (compressor.warn("Condition always true [{file}:{line},{col}]", self.condition.start),
                compressor.option("dead_code")) {
                    var a = [];
                    return self.alternative && extract_declarations_from_unreachable_code(compressor, self.alternative, a),
                    a.push(self.body), make_node(AST_BlockStatement, self, {
                        body: a
                    }).transform(compressor);
                }
            } else if (compressor.warn("Condition always false [{file}:{line},{col}]", self.condition.start),
            compressor.option("dead_code")) {
                var a = [];
                return extract_declarations_from_unreachable_code(compressor, self.body, a), self.alternative && a.push(self.alternative),
                make_node(AST_BlockStatement, self, {
                    body: a
                }).transform(compressor);
            }
            is_empty(self.alternative) && (self.alternative = null);
            var negated = self.condition.negate(compressor), negated_is_best = best_of(self.condition, negated) === negated;
            if (self.alternative && negated_is_best) {
                negated_is_best = !1, self.condition = negated;
                var tmp = self.body;
                self.body = self.alternative || make_node(AST_EmptyStatement), self.alternative = tmp;
            }
            if (is_empty(self.body) && is_empty(self.alternative)) return make_node(AST_SimpleStatement, self.condition, {
                body: self.condition
            }).transform(compressor);
            if (self.body instanceof AST_SimpleStatement && self.alternative instanceof AST_SimpleStatement) return make_node(AST_SimpleStatement, self, {
                body: make_node(AST_Conditional, self, {
                    condition: self.condition,
                    consequent: self.body.body,
                    alternative: self.alternative.body
                })
            }).transform(compressor);
            if (is_empty(self.alternative) && self.body instanceof AST_SimpleStatement) return negated_is_best ? make_node(AST_SimpleStatement, self, {
                body: make_node(AST_Binary, self, {
                    operator: "||",
                    left: negated,
                    right: self.body.body
                })
            }).transform(compressor) : make_node(AST_SimpleStatement, self, {
                body: make_node(AST_Binary, self, {
                    operator: "&&",
                    left: self.condition,
                    right: self.body.body
                })
            }).transform(compressor);
            if (self.body instanceof AST_EmptyStatement && self.alternative && self.alternative instanceof AST_SimpleStatement) return make_node(AST_SimpleStatement, self, {
                body: make_node(AST_Binary, self, {
                    operator: "||",
                    left: self.condition,
                    right: self.alternative.body
                })
            }).transform(compressor);
            if (self.body instanceof AST_Exit && self.alternative instanceof AST_Exit && self.body.TYPE == self.alternative.TYPE) return make_node(self.body.CTOR, self, {
                value: make_node(AST_Conditional, self, {
                    condition: self.condition,
                    consequent: self.body.value || make_node(AST_Undefined, self.body).optimize(compressor),
                    alternative: self.alternative.value || make_node(AST_Undefined, self.alternative).optimize(compressor)
                })
            }).transform(compressor);
            if (self.body instanceof AST_If && !self.body.alternative && !self.alternative && (self.condition = make_node(AST_Binary, self.condition, {
                operator: "&&",
                left: self.condition,
                right: self.body.condition
            }).transform(compressor), self.body = self.body.body), aborts(self.body) && self.alternative) {
                var alt = self.alternative;
                return self.alternative = null, make_node(AST_BlockStatement, self, {
                    body: [self, alt]
                }).transform(compressor);
            }
            if (aborts(self.alternative)) {
                var body = self.body;
                return self.body = self.alternative, self.condition = negated_is_best ? negated : self.condition.negate(compressor),
                self.alternative = null, make_node(AST_BlockStatement, self, {
                    body: [self, body]
                }).transform(compressor);
            }
            return self;
        }), OPT(AST_Switch, function (self, compressor) {
            if (0 == self.body.length && compressor.option("conditionals")) return make_node(AST_SimpleStatement, self, {
                body: self.expression
            }).transform(compressor);
            for (; ;) {
                var last_branch = self.body[self.body.length - 1];
                if (last_branch) {
                    var stat = last_branch.body[last_branch.body.length - 1];
                    if (stat instanceof AST_Break && loop_body(compressor.loopcontrol_target(stat.label)) === self && last_branch.body.pop(),
                    last_branch instanceof AST_Default && 0 == last_branch.body.length) {
                        self.body.pop();
                        continue;
                    }
                }
                break;
            }
            var exp = self.expression.evaluate(compressor);
            out: if (2 == exp.length) try {
                if (self.expression = exp[0], !compressor.option("dead_code")) break out;
                var value = exp[1], in_if = !1, in_block = !1, started = !1, stopped = !1, ruined = !1, tt = new TreeTransformer(function (node, descend, in_list) {
                    if (node instanceof AST_Lambda || node instanceof AST_SimpleStatement) return node;
                    if (node instanceof AST_Switch && node === self) return node = node.clone(), descend(node, this),
                    ruined ? node : make_node(AST_BlockStatement, node, {
                        body: node.body.reduce(function (a, branch) {
                            return a.concat(branch.body);
                        }, [])
                    }).transform(compressor);
                    if (node instanceof AST_If || node instanceof AST_Try) {
                        var save = in_if;
                        return in_if = !in_block, descend(node, this), in_if = save, node;
                    }
                    if (node instanceof AST_StatementWithBody || node instanceof AST_Switch) {
                        var save = in_block;
                        return in_block = !0, descend(node, this), in_block = save, node;
                    }
                    if (node instanceof AST_Break && this.loopcontrol_target(node.label) === self) return in_if ? (ruined = !0,
                    node) : in_block ? node : (stopped = !0, in_list ? MAP.skip : make_node(AST_EmptyStatement, node));
                    if (node instanceof AST_SwitchBranch && this.parent() === self) {
                        if (stopped) return MAP.skip;
                        if (node instanceof AST_Case) {
                            var exp = node.expression.evaluate(compressor);
                            if (exp.length < 2) throw self;
                            return exp[1] === value || started ? (started = !0, aborts(node) && (stopped = !0),
                            descend(node, this), node) : MAP.skip;
                        }
                        return descend(node, this), node;
                    }
                });
                tt.stack = compressor.stack.slice(), self = self.transform(tt);
            } catch (ex) {
                if (ex !== self) throw ex;
            }
            return self;
        }), OPT(AST_Case, function (self, compressor) {
            return self.body = tighten_body(self.body, compressor), self;
        }), OPT(AST_Try, function (self, compressor) {
            return self.body = tighten_body(self.body, compressor), self;
        }), AST_Definitions.DEFMETHOD("remove_initializers", function () {
            this.definitions.forEach(function (def) {
                def.value = null;
            });
        }), AST_Definitions.DEFMETHOD("to_assignments", function () {
            var assignments = this.definitions.reduce(function (a, def) {
                if (def.value) {
                    var name = make_node(AST_SymbolRef, def.name, def.name);
                    a.push(make_node(AST_Assign, def, {
                        operator: "=",
                        left: name,
                        right: def.value
                    }));
                }
                return a;
            }, []);
            return 0 == assignments.length ? null : AST_Seq.from_array(assignments);
        }), OPT(AST_Definitions, function (self, compressor) {
            return 0 == self.definitions.length ? make_node(AST_EmptyStatement, self) : self;
        }), OPT(AST_Function, function (self, compressor) {
            return self = AST_Lambda.prototype.optimize.call(self, compressor), compressor.option("unused") && !compressor.option("keep_fnames") && self.name && self.name.unreferenced() && (self.name = null),
            self;
        }), OPT(AST_Call, function (self, compressor) {
            if (compressor.option("unsafe")) {
                var exp = self.expression;
                if (exp instanceof AST_SymbolRef && exp.undeclared()) switch (exp.name) {
                    case "Array":
                        if (1 != self.args.length) return make_node(AST_Array, self, {
                            elements: self.args
                        }).transform(compressor);
                        break;

                    case "Object":
                        if (0 == self.args.length) return make_node(AST_Object, self, {
                            properties: []
                        });
                        break;

                    case "String":
                        if (0 == self.args.length) return make_node(AST_String, self, {
                            value: ""
                        });
                        if (self.args.length <= 1) return make_node(AST_Binary, self, {
                            left: self.args[0],
                            operator: "+",
                            right: make_node(AST_String, self, {
                                value: ""
                            })
                        }).transform(compressor);
                        break;

                    case "Number":
                        if (0 == self.args.length) return make_node(AST_Number, self, {
                            value: 0
                        });
                        if (1 == self.args.length) return make_node(AST_UnaryPrefix, self, {
                            expression: self.args[0],
                            operator: "+"
                        }).transform(compressor);

                    case "Boolean":
                        if (0 == self.args.length) return make_node(AST_False, self);
                        if (1 == self.args.length) return make_node(AST_UnaryPrefix, self, {
                            expression: make_node(AST_UnaryPrefix, null, {
                                expression: self.args[0],
                                operator: "!"
                            }),
                            operator: "!"
                        }).transform(compressor);
                        break;

                    case "Function":
                        if (0 == self.args.length) return make_node(AST_Function, self, {
                            argnames: [],
                            body: []
                        });
                        if (all(self.args, function (x) {
                            return x instanceof AST_String;
                        })) try {
                            var code = "(function(" + self.args.slice(0, -1).map(function (arg) {
                                return arg.value;
                            }).join(",") + "){" + self.args[self.args.length - 1].value + "})()", ast = parse(code);
                            ast.figure_out_scope({
                                screw_ie8: compressor.option("screw_ie8")
                            });
                            var comp = new Compressor(compressor.options);
                            ast = ast.transform(comp), ast.figure_out_scope({
                                screw_ie8: compressor.option("screw_ie8")
                            }), ast.mangle_names();
                            var fun;
                            try {
                                ast.walk(new TreeWalker(function (node) {
                                    if (node instanceof AST_Lambda) throw fun = node, ast;
                                }));
                            } catch (ex) {
                                if (ex !== ast) throw ex;
                            }
                            if (!fun) return self;
                            var args = fun.argnames.map(function (arg, i) {
                                return make_node(AST_String, self.args[i], {
                                    value: arg.print_to_string()
                                });
                            }), code = OutputStream();
                            return AST_BlockStatement.prototype._codegen.call(fun, fun, code), code = code.toString().replace(/^\{|\}$/g, ""),
                            args.push(make_node(AST_String, self.args[self.args.length - 1], {
                                value: code
                            })), self.args = args, self;
                        } catch (ex) {
                            if (!(ex instanceof JS_Parse_Error)) throw console.log(ex), ex;
                            compressor.warn("Error parsing code passed to new Function [{file}:{line},{col}]", self.args[self.args.length - 1].start),
                            compressor.warn(ex.toString());
                        }
                } else {
                    if (exp instanceof AST_Dot && "toString" == exp.property && 0 == self.args.length) return make_node(AST_Binary, self, {
                        left: make_node(AST_String, self, {
                            value: ""
                        }),
                        operator: "+",
                        right: exp.expression
                    }).transform(compressor);
                    if (exp instanceof AST_Dot && exp.expression instanceof AST_Array && "join" == exp.property) {
                        var separator = 0 == self.args.length ? "," : self.args[0].evaluate(compressor)[1];
                        if (null != separator) {
                            var elements = exp.expression.elements.reduce(function (a, el) {
                                if (el = el.evaluate(compressor), 0 == a.length || 1 == el.length) a.push(el); else {
                                    var last = a[a.length - 1];
                                    if (2 == last.length) {
                                        var val = "" + last[1] + separator + el[1];
                                        a[a.length - 1] = [make_node_from_constant(compressor, val, last[0]), val];
                                    } else a.push(el);
                                }
                                return a;
                            }, []);
                            if (0 == elements.length) return make_node(AST_String, self, {
                                value: ""
                            });
                            if (1 == elements.length) return elements[0][0];
                            if ("" == separator) {
                                var first;
                                return first = elements[0][0] instanceof AST_String || elements[1][0] instanceof AST_String ? elements.shift()[0] : make_node(AST_String, self, {
                                    value: ""
                                }), elements.reduce(function (prev, el) {
                                    return make_node(AST_Binary, el[0], {
                                        operator: "+",
                                        left: prev,
                                        right: el[0]
                                    });
                                }, first).transform(compressor);
                            }
                            var node = self.clone();
                            return node.expression = node.expression.clone(), node.expression.expression = node.expression.expression.clone(),
                            node.expression.expression.elements = elements.map(function (el) {
                                return el[0];
                            }), best_of(self, node);
                        }
                    }
                }
            }
            if (compressor.option("side_effects") && self.expression instanceof AST_Function && 0 == self.args.length && !AST_Block.prototype.has_side_effects.call(self.expression, compressor)) return make_node(AST_Undefined, self).transform(compressor);
            if (compressor.option("drop_console") && self.expression instanceof AST_PropAccess) {
                for (var name = self.expression.expression; name.expression;) name = name.expression;
                if (name instanceof AST_SymbolRef && "console" == name.name && name.undeclared()) return make_node(AST_Undefined, self).transform(compressor);
            }
            return self.evaluate(compressor)[0];
        }), OPT(AST_New, function (self, compressor) {
            if (compressor.option("unsafe")) {
                var exp = self.expression;
                if (exp instanceof AST_SymbolRef && exp.undeclared()) switch (exp.name) {
                    case "Object":
                    case "RegExp":
                    case "Function":
                    case "Error":
                    case "Array":
                        return make_node(AST_Call, self, self).transform(compressor);
                }
            }
            return self;
        }), OPT(AST_Seq, function (self, compressor) {
            if (!compressor.option("side_effects")) return self;
            if (!self.car.has_side_effects(compressor)) {
                var p;
                if (!(self.cdr instanceof AST_SymbolRef && "eval" == self.cdr.name && self.cdr.undeclared() && (p = compressor.parent()) instanceof AST_Call && p.expression === self)) return self.cdr;
            }
            if (compressor.option("cascade")) {
                if (self.car instanceof AST_Assign && !self.car.left.has_side_effects(compressor)) {
                    if (self.car.left.equivalent_to(self.cdr)) return self.car;
                    if (self.cdr instanceof AST_Call && self.cdr.expression.equivalent_to(self.car.left)) return self.cdr.expression = self.car,
                    self.cdr;
                }
                if (!self.car.has_side_effects(compressor) && !self.cdr.has_side_effects(compressor) && self.car.equivalent_to(self.cdr)) return self.car;
            }
            return self.cdr instanceof AST_UnaryPrefix && "void" == self.cdr.operator && !self.cdr.expression.has_side_effects(compressor) ? (self.cdr.expression = self.car,
            self.cdr) : self.cdr instanceof AST_Undefined ? make_node(AST_UnaryPrefix, self, {
                operator: "void",
                expression: self.car
            }) : self;
        }), AST_Unary.DEFMETHOD("lift_sequences", function (compressor) {
            if (compressor.option("sequences") && this.expression instanceof AST_Seq) {
                var seq = this.expression, x = seq.to_array();
                return this.expression = x.pop(), x.push(this), seq = AST_Seq.from_array(x).transform(compressor);
            }
            return this;
        }), OPT(AST_UnaryPostfix, function (self, compressor) {
            return self.lift_sequences(compressor);
        }), OPT(AST_UnaryPrefix, function (self, compressor) {
            self = self.lift_sequences(compressor);
            var e = self.expression;
            if (compressor.option("booleans") && compressor.in_boolean_context()) {
                switch (self.operator) {
                    case "!":
                        if (e instanceof AST_UnaryPrefix && "!" == e.operator) return e.expression;
                        break;

                    case "typeof":
                        return compressor.warn("Boolean expression always true [{file}:{line},{col}]", self.start),
                        make_node(AST_True, self);
                }
                e instanceof AST_Binary && "!" == self.operator && (self = best_of(self, e.negate(compressor)));
            }
            return self.evaluate(compressor)[0];
        }), AST_Binary.DEFMETHOD("lift_sequences", function (compressor) {
            if (compressor.option("sequences")) {
                if (this.left instanceof AST_Seq) {
                    var seq = this.left, x = seq.to_array();
                    return this.left = x.pop(), x.push(this), seq = AST_Seq.from_array(x).transform(compressor);
                }
                if (this.right instanceof AST_Seq && this instanceof AST_Assign && !has_side_effects_or_prop_access(this.left, compressor)) {
                    var seq = this.right, x = seq.to_array();
                    return this.right = x.pop(), x.push(this), seq = AST_Seq.from_array(x).transform(compressor);
                }
            }
            return this;
        });
        var commutativeOperators = makePredicate("== === != !== * & | ^");
        OPT(AST_Binary, function (self, compressor) {
            var reverse = compressor.has_directive("use asm") ? noop : function (op, force) {
                if (force || !self.left.has_side_effects(compressor) && !self.right.has_side_effects(compressor)) {
                    op && (self.operator = op);
                    var tmp = self.left;
                    self.left = self.right, self.right = tmp;
                }
            };
            if (commutativeOperators(self.operator) && (self.right instanceof AST_Constant && !(self.left instanceof AST_Constant) && (self.left instanceof AST_Binary && PRECEDENCE[self.left.operator] >= PRECEDENCE[self.operator] || reverse(null, !0)),
            /^[!=]==?$/.test(self.operator))) {
                if (self.left instanceof AST_SymbolRef && self.right instanceof AST_Conditional) {
                    if (self.right.consequent instanceof AST_SymbolRef && self.right.consequent.definition() === self.left.definition()) {
                        if (/^==/.test(self.operator)) return self.right.condition;
                        if (/^!=/.test(self.operator)) return self.right.condition.negate(compressor);
                    }
                    if (self.right.alternative instanceof AST_SymbolRef && self.right.alternative.definition() === self.left.definition()) {
                        if (/^==/.test(self.operator)) return self.right.condition.negate(compressor);
                        if (/^!=/.test(self.operator)) return self.right.condition;
                    }
                }
                if (self.right instanceof AST_SymbolRef && self.left instanceof AST_Conditional) {
                    if (self.left.consequent instanceof AST_SymbolRef && self.left.consequent.definition() === self.right.definition()) {
                        if (/^==/.test(self.operator)) return self.left.condition;
                        if (/^!=/.test(self.operator)) return self.left.condition.negate(compressor);
                    }
                    if (self.left.alternative instanceof AST_SymbolRef && self.left.alternative.definition() === self.right.definition()) {
                        if (/^==/.test(self.operator)) return self.left.condition.negate(compressor);
                        if (/^!=/.test(self.operator)) return self.left.condition;
                    }
                }
            }
            if (self = self.lift_sequences(compressor), compressor.option("comparisons")) switch (self.operator) {
                case "===":
                case "!==":
                    (self.left.is_string(compressor) && self.right.is_string(compressor) || self.left.is_boolean() && self.right.is_boolean()) && (self.operator = self.operator.substr(0, 2));

                case "==":
                case "!=":
                    self.left instanceof AST_String && "undefined" == self.left.value && self.right instanceof AST_UnaryPrefix && "typeof" == self.right.operator && compressor.option("unsafe") && (self.right.expression instanceof AST_SymbolRef && self.right.expression.undeclared() || (self.right = self.right.expression,
                    self.left = make_node(AST_Undefined, self.left).optimize(compressor), 2 == self.operator.length && (self.operator += "=")));
            }
            if (compressor.option("booleans") && compressor.in_boolean_context()) switch (self.operator) {
                case "&&":
                    var ll = self.left.evaluate(compressor), rr = self.right.evaluate(compressor);
                    if (ll.length > 1 && !ll[1] || rr.length > 1 && !rr[1]) return compressor.warn("Boolean && always false [{file}:{line},{col}]", self.start),
                    self.left.has_side_effects(compressor) ? make_node(AST_Seq, self, {
                        car: self.left,
                        cdr: make_node(AST_False)
                    }).optimize(compressor) : make_node(AST_False, self);
                    if (ll.length > 1 && ll[1]) return rr[0];
                    if (rr.length > 1 && rr[1]) return ll[0];
                    break;

                case "||":
                    var ll = self.left.evaluate(compressor), rr = self.right.evaluate(compressor);
                    if (ll.length > 1 && ll[1] || rr.length > 1 && rr[1]) return compressor.warn("Boolean || always true [{file}:{line},{col}]", self.start),
                    self.left.has_side_effects(compressor) ? make_node(AST_Seq, self, {
                        car: self.left,
                        cdr: make_node(AST_True)
                    }).optimize(compressor) : make_node(AST_True, self);
                    if (ll.length > 1 && !ll[1]) return rr[0];
                    if (rr.length > 1 && !rr[1]) return ll[0];
                    break;

                case "+":
                    var ll = self.left.evaluate(compressor), rr = self.right.evaluate(compressor);
                    if (ll.length > 1 && ll[0] instanceof AST_String && ll[1] || rr.length > 1 && rr[0] instanceof AST_String && rr[1]) return compressor.warn("+ in boolean context always true [{file}:{line},{col}]", self.start),
                    make_node(AST_True, self);
            }
            if (compressor.option("comparisons")) {
                if (!(compressor.parent() instanceof AST_Binary) || compressor.parent() instanceof AST_Assign) {
                    var negated = make_node(AST_UnaryPrefix, self, {
                        operator: "!",
                        expression: self.negate(compressor)
                    });
                    self = best_of(self, negated);
                }
                switch (self.operator) {
                    case "<":
                        reverse(">");
                        break;

                    case "<=":
                        reverse(">=");
                }
            }
            return "+" == self.operator && self.right instanceof AST_String && "" === self.right.getValue() && self.left instanceof AST_Binary && "+" == self.left.operator && self.left.is_string(compressor) ? self.left : (compressor.option("evaluate") && "+" == self.operator && (self.left instanceof AST_Constant && self.right instanceof AST_Binary && "+" == self.right.operator && self.right.left instanceof AST_Constant && self.right.is_string(compressor) && (self = make_node(AST_Binary, self, {
                operator: "+",
                left: make_node(AST_String, null, {
                    value: "" + self.left.getValue() + self.right.left.getValue(),
                    start: self.left.start,
                    end: self.right.left.end
                }),
                right: self.right.right
            })), self.right instanceof AST_Constant && self.left instanceof AST_Binary && "+" == self.left.operator && self.left.right instanceof AST_Constant && self.left.is_string(compressor) && (self = make_node(AST_Binary, self, {
                operator: "+",
                left: self.left.left,
                right: make_node(AST_String, null, {
                    value: "" + self.left.right.getValue() + self.right.getValue(),
                    start: self.left.right.start,
                    end: self.right.end
                })
            })), self.left instanceof AST_Binary && "+" == self.left.operator && self.left.is_string(compressor) && self.left.right instanceof AST_Constant && self.right instanceof AST_Binary && "+" == self.right.operator && self.right.left instanceof AST_Constant && self.right.is_string(compressor) && (self = make_node(AST_Binary, self, {
                operator: "+",
                left: make_node(AST_Binary, self.left, {
                    operator: "+",
                    left: self.left.left,
                    right: make_node(AST_String, null, {
                        value: "" + self.left.right.getValue() + self.right.left.getValue(),
                        start: self.left.right.start,
                        end: self.right.left.end
                    })
                }),
                right: self.right.right
            }))), self.right instanceof AST_Binary && self.right.operator == self.operator && ("*" == self.operator || "&&" == self.operator || "||" == self.operator) ? (self.left = make_node(AST_Binary, self.left, {
                operator: self.operator,
                left: self.left,
                right: self.right.left
            }), self.right = self.right.right, self.transform(compressor)) : self.evaluate(compressor)[0]);
        }), OPT(AST_SymbolRef, function (self, compressor) {
            if (self.undeclared()) {
                var defines = compressor.option("global_defs");
                if (defines && defines.hasOwnProperty(self.name)) return make_node_from_constant(compressor, defines[self.name], self);
                switch (self.name) {
                    case "undefined":
                        return make_node(AST_Undefined, self);

                    case "NaN":
                        return make_node(AST_NaN, self).transform(compressor);

                    case "Infinity":
                        return make_node(AST_Infinity, self).transform(compressor);
                }
            }
            return self;
        }), OPT(AST_Infinity, function (self, compressor) {
            return make_node(AST_Binary, self, {
                operator: "/",
                left: make_node(AST_Number, self, {
                    value: 1
                }),
                right: make_node(AST_Number, self, {
                    value: 0
                })
            });
        }), OPT(AST_NaN, function (self, compressor) {
            return make_node(AST_Binary, self, {
                operator: "/",
                left: make_node(AST_Number, self, {
                    value: 0
                }),
                right: make_node(AST_Number, self, {
                    value: 0
                })
            });
        }), OPT(AST_Undefined, function (self, compressor) {
            if (compressor.option("unsafe")) {
                var scope = compressor.find_parent(AST_Scope), undef = scope.find_variable("undefined");
                if (undef) {
                    var ref = make_node(AST_SymbolRef, self, {
                        name: "undefined",
                        scope: scope,
                        thedef: undef
                    });
                    return ref.reference(), ref;
                }
            }
            return self;
        });
        var ASSIGN_OPS = ["+", "-", "/", "*", "%", ">>", "<<", ">>>", "|", "^", "&"];
        OPT(AST_Assign, function (self, compressor) {
            return self = self.lift_sequences(compressor), "=" == self.operator && self.left instanceof AST_SymbolRef && self.right instanceof AST_Binary && self.right.left instanceof AST_SymbolRef && self.right.left.name == self.left.name && member(self.right.operator, ASSIGN_OPS) && (self.operator = self.right.operator + "=",
            self.right = self.right.right), self;
        }), OPT(AST_Conditional, function (self, compressor) {
            if (!compressor.option("conditionals")) return self;
            if (self.condition instanceof AST_Seq) {
                var car = self.condition.car;
                return self.condition = self.condition.cdr, AST_Seq.cons(car, self);
            }
            var cond = self.condition.evaluate(compressor);
            if (cond.length > 1) return cond[1] ? (compressor.warn("Condition always true [{file}:{line},{col}]", self.start),
            self.consequent) : (compressor.warn("Condition always false [{file}:{line},{col}]", self.start),
            self.alternative);
            var negated = cond[0].negate(compressor);
            best_of(cond[0], negated) === negated && (self = make_node(AST_Conditional, self, {
                condition: negated,
                consequent: self.alternative,
                alternative: self.consequent
            }));
            var consequent = self.consequent, alternative = self.alternative;
            if (consequent instanceof AST_Assign && alternative instanceof AST_Assign && consequent.operator == alternative.operator && consequent.left.equivalent_to(alternative.left)) return make_node(AST_Assign, self, {
                operator: consequent.operator,
                left: consequent.left,
                right: make_node(AST_Conditional, self, {
                    condition: self.condition,
                    consequent: consequent.right,
                    alternative: alternative.right
                })
            });
            if (consequent instanceof AST_Call && alternative.TYPE === consequent.TYPE && consequent.args.length == alternative.args.length && consequent.expression.equivalent_to(alternative.expression)) {
                if (0 == consequent.args.length) return make_node(AST_Seq, self, {
                    car: self.condition,
                    cdr: consequent
                });
                if (1 == consequent.args.length) return consequent.args[0] = make_node(AST_Conditional, self, {
                    condition: self.condition,
                    consequent: consequent.args[0],
                    alternative: alternative.args[0]
                }), consequent;
            }
            return consequent instanceof AST_Conditional && consequent.alternative.equivalent_to(alternative) ? make_node(AST_Conditional, self, {
                condition: make_node(AST_Binary, self, {
                    left: self.condition,
                    operator: "&&",
                    right: consequent.condition
                }),
                consequent: consequent.consequent,
                alternative: alternative
            }) : consequent instanceof AST_Constant && alternative instanceof AST_Constant && consequent.equivalent_to(alternative) ? self.condition.has_side_effects(compressor) ? AST_Seq.from_array([self.condition, make_node_from_constant(compressor, consequent.value, self)]) : make_node_from_constant(compressor, consequent.value, self) : consequent instanceof AST_True && alternative instanceof AST_False ? (self.condition = self.condition.negate(compressor),
            make_node(AST_UnaryPrefix, self.condition, {
                operator: "!",
                expression: self.condition
            })) : consequent instanceof AST_False && alternative instanceof AST_True ? self.condition.negate(compressor) : self;
        }), OPT(AST_Boolean, function (self, compressor) {
            if (compressor.option("booleans")) {
                var p = compressor.parent();
                return p instanceof AST_Binary && ("==" == p.operator || "!=" == p.operator) ? (compressor.warn("Non-strict equality against boolean: {operator} {value} [{file}:{line},{col}]", {
                    operator: p.operator,
                    value: self.value,
                    file: p.start.file,
                    line: p.start.line,
                    col: p.start.col
                }), make_node(AST_Number, self, {
                    value: +self.value
                })) : make_node(AST_UnaryPrefix, self, {
                    operator: "!",
                    expression: make_node(AST_Number, self, {
                        value: 1 - self.value
                    })
                });
            }
            return self;
        }), OPT(AST_Sub, function (self, compressor) {
            var prop = self.property;
            if (prop instanceof AST_String && compressor.option("properties")) {
                if (prop = prop.getValue(), RESERVED_WORDS(prop) ? compressor.option("screw_ie8") : is_identifier_string(prop)) return make_node(AST_Dot, self, {
                    expression: self.expression,
                    property: prop
                }).optimize(compressor);
                var v = parseFloat(prop);
                isNaN(v) || v.toString() != prop || (self.property = make_node(AST_Number, self.property, {
                    value: v
                }));
            }
            return self;
        }), OPT(AST_Dot, function (self, compressor) {
            var prop = self.property;
            return RESERVED_WORDS(prop) && !compressor.option("screw_ie8") ? make_node(AST_Sub, self, {
                expression: self.expression,
                property: make_node(AST_String, self, {
                    value: prop
                })
            }).optimize(compressor) : self.evaluate(compressor)[0];
        }), OPT(AST_Array, literals_in_boolean_context), OPT(AST_Object, literals_in_boolean_context),
        OPT(AST_RegExp, literals_in_boolean_context);
    }(), function () {
        function my_start_token(moznode) {
            var loc = moznode.loc, start = loc && loc.start, range = moznode.range;
            return new AST_Token({
                file: loc && loc.source,
                line: start && start.line,
                col: start && start.column,
                pos: range ? range[0] : moznode.start,
                endline: start && start.line,
                endcol: start && start.column,
                endpos: range ? range[0] : moznode.start
            });
        }
        function my_end_token(moznode) {
            var loc = moznode.loc, end = loc && loc.end, range = moznode.range;
            return new AST_Token({
                file: loc && loc.source,
                line: end && end.line,
                col: end && end.column,
                pos: range ? range[1] : moznode.end,
                endline: end && end.line,
                endcol: end && end.column,
                endpos: range ? range[1] : moznode.end
            });
        }
        function map(moztype, mytype, propmap) {
            var moz_to_me = "function From_Moz_" + moztype + "(M){\n";
            moz_to_me += "return new " + mytype.name + "({\nstart: my_start_token(M),\nend: my_end_token(M)";
            var me_to_moz = "function To_Moz_" + moztype + "(M){\n";
            me_to_moz += "return {\ntype: " + JSON.stringify(moztype), propmap && propmap.split(/\s*,\s*/).forEach(function (prop) {
                var m = /([a-z0-9$_]+)(=|@|>|%)([a-z0-9$_]+)/i.exec(prop);
                if (!m) throw new Error("Can't understand property map: " + prop);
                var moz = m[1], how = m[2], my = m[3];
                switch (moz_to_me += ",\n" + my + ": ", me_to_moz += ",\n" + moz + ": ", how) {
                    case "@":
                        moz_to_me += "M." + moz + ".map(from_moz)", me_to_moz += "M." + my + ".map(to_moz)";
                        break;

                    case ">":
                        moz_to_me += "from_moz(M." + moz + ")", me_to_moz += "to_moz(M." + my + ")";
                        break;

                    case "=":
                        moz_to_me += "M." + moz, me_to_moz += "M." + my;
                        break;

                    case "%":
                        moz_to_me += "from_moz(M." + moz + ").body", me_to_moz += "to_moz_block(M)";
                        break;

                    default:
                        throw new Error("Can't understand operator in propmap: " + prop);
                }
            }), moz_to_me += "\n})\n}", me_to_moz += "\n}\n}", moz_to_me = new Function("my_start_token", "my_end_token", "from_moz", "return(" + moz_to_me + ")")(my_start_token, my_end_token, from_moz),
            me_to_moz = new Function("to_moz", "to_moz_block", "return(" + me_to_moz + ")")(to_moz, to_moz_block),
            MOZ_TO_ME[moztype] = moz_to_me, def_to_moz(mytype, me_to_moz);
        }
        function from_moz(node) {
            FROM_MOZ_STACK.push(node);
            var ret = null != node ? MOZ_TO_ME[node.type](node) : null;
            return FROM_MOZ_STACK.pop(), ret;
        }
        function set_moz_loc(mynode, moznode, myparent) {
            var start = mynode.start, end = mynode.end;
            return null != start.pos && null != end.endpos && (moznode.range = [start.pos, end.endpos]),
            start.line && (moznode.loc = {
                start: {
                    line: start.line,
                    column: start.col
                },
                end: end.endline ? {
                    line: end.endline,
                    column: end.endcol
                } : null
            }, start.file && (moznode.loc.source = start.file)), moznode;
        }
        function def_to_moz(mytype, handler) {
            mytype.DEFMETHOD("to_mozilla_ast", function () {
                return set_moz_loc(this, handler(this));
            });
        }
        function to_moz(node) {
            return null != node ? node.to_mozilla_ast() : null;
        }
        function to_moz_block(node) {
            return {
                type: "BlockStatement",
                body: node.body.map(to_moz)
            };
        }
        var MOZ_TO_ME = {
            ExpressionStatement: function (M) {
                var expr = M.expression;
                return "Literal" === expr.type && "string" == typeof expr.value ? new AST_Directive({
                    start: my_start_token(M),
                    end: my_end_token(M),
                    value: expr.value
                }) : new AST_SimpleStatement({
                    start: my_start_token(M),
                    end: my_end_token(M),
                    body: from_moz(expr)
                });
            },
            TryStatement: function (M) {
                var handlers = M.handlers || [M.handler];
                if (handlers.length > 1 || M.guardedHandlers && M.guardedHandlers.length) throw new Error("Multiple catch clauses are not supported.");
                return new AST_Try({
                    start: my_start_token(M),
                    end: my_end_token(M),
                    body: from_moz(M.block).body,
                    bcatch: from_moz(handlers[0]),
                    bfinally: M.finalizer ? new AST_Finally(from_moz(M.finalizer)) : null
                });
            },
            Property: function (M) {
                var key = M.key, name = "Identifier" == key.type ? key.name : key.value, args = {
                    start: my_start_token(key),
                    end: my_end_token(M.value),
                    key: name,
                    value: from_moz(M.value)
                };
                switch (M.kind) {
                    case "init":
                        return new AST_ObjectKeyVal(args);

                    case "set":
                        return args.value.name = from_moz(key), new AST_ObjectSetter(args);

                    case "get":
                        return args.value.name = from_moz(key), new AST_ObjectGetter(args);
                }
            },
            ObjectExpression: function (M) {
                return new AST_Object({
                    start: my_start_token(M),
                    end: my_end_token(M),
                    properties: M.properties.map(function (prop) {
                        return prop.type = "Property", from_moz(prop);
                    })
                });
            },
            SequenceExpression: function (M) {
                return AST_Seq.from_array(M.expressions.map(from_moz));
            },
            MemberExpression: function (M) {
                return new (M.computed ? AST_Sub : AST_Dot)({
                    start: my_start_token(M),
                    end: my_end_token(M),
                    property: M.computed ? from_moz(M.property) : M.property.name,
                    expression: from_moz(M.object)
                });
            },
            SwitchCase: function (M) {
                return new (M.test ? AST_Case : AST_Default)({
                    start: my_start_token(M),
                    end: my_end_token(M),
                    expression: from_moz(M.test),
                    body: M.consequent.map(from_moz)
                });
            },
            VariableDeclaration: function (M) {
                return new ("const" === M.kind ? AST_Const : AST_Var)({
                    start: my_start_token(M),
                    end: my_end_token(M),
                    definitions: M.declarations.map(from_moz)
                });
            },
            Literal: function (M) {
                var val = M.value, args = {
                    start: my_start_token(M),
                    end: my_end_token(M)
                };
                if (null === val) return new AST_Null(args);
                switch (typeof val) {
                    case "string":
                        return args.value = val, new AST_String(args);

                    case "number":
                        return args.value = val, new AST_Number(args);

                    case "boolean":
                        return new (val ? AST_True : AST_False)(args);

                    default:
                        return args.value = val, new AST_RegExp(args);
                }
            },
            Identifier: function (M) {
                var p = FROM_MOZ_STACK[FROM_MOZ_STACK.length - 2];
                return new ("LabeledStatement" == p.type ? AST_Label : "VariableDeclarator" == p.type && p.id === M ? "const" == p.kind ? AST_SymbolConst : AST_SymbolVar : "FunctionExpression" == p.type ? p.id === M ? AST_SymbolLambda : AST_SymbolFunarg : "FunctionDeclaration" == p.type ? p.id === M ? AST_SymbolDefun : AST_SymbolFunarg : "CatchClause" == p.type ? AST_SymbolCatch : "BreakStatement" == p.type || "ContinueStatement" == p.type ? AST_LabelRef : AST_SymbolRef)({
                    start: my_start_token(M),
                    end: my_end_token(M),
                    name: M.name
                });
            }
        };
        MOZ_TO_ME.UpdateExpression = MOZ_TO_ME.UnaryExpression = function (M) {
            var prefix = "prefix" in M ? M.prefix : "UnaryExpression" == M.type ? !0 : !1;
            return new (prefix ? AST_UnaryPrefix : AST_UnaryPostfix)({
                start: my_start_token(M),
                end: my_end_token(M),
                operator: M.operator,
                expression: from_moz(M.argument)
            });
        }, map("Program", AST_Toplevel, "body@body"), map("EmptyStatement", AST_EmptyStatement),
        map("BlockStatement", AST_BlockStatement, "body@body"), map("IfStatement", AST_If, "test>condition, consequent>body, alternate>alternative"),
        map("LabeledStatement", AST_LabeledStatement, "label>label, body>body"), map("BreakStatement", AST_Break, "label>label"),
        map("ContinueStatement", AST_Continue, "label>label"), map("WithStatement", AST_With, "object>expression, body>body"),
        map("SwitchStatement", AST_Switch, "discriminant>expression, cases@body"), map("ReturnStatement", AST_Return, "argument>value"),
        map("ThrowStatement", AST_Throw, "argument>value"), map("WhileStatement", AST_While, "test>condition, body>body"),
        map("DoWhileStatement", AST_Do, "test>condition, body>body"), map("ForStatement", AST_For, "init>init, test>condition, update>step, body>body"),
        map("ForInStatement", AST_ForIn, "left>init, right>object, body>body"), map("DebuggerStatement", AST_Debugger),
        map("FunctionDeclaration", AST_Defun, "id>name, params@argnames, body%body"), map("VariableDeclarator", AST_VarDef, "id>name, init>value"),
        map("CatchClause", AST_Catch, "param>argname, body%body"), map("ThisExpression", AST_This),
        map("ArrayExpression", AST_Array, "elements@elements"), map("FunctionExpression", AST_Function, "id>name, params@argnames, body%body"),
        map("BinaryExpression", AST_Binary, "operator=operator, left>left, right>right"),
        map("LogicalExpression", AST_Binary, "operator=operator, left>left, right>right"),
        map("AssignmentExpression", AST_Assign, "operator=operator, left>left, right>right"),
        map("ConditionalExpression", AST_Conditional, "test>condition, consequent>consequent, alternate>alternative"),
        map("NewExpression", AST_New, "callee>expression, arguments@args"), map("CallExpression", AST_Call, "callee>expression, arguments@args"),
        def_to_moz(AST_Directive, function (M) {
            return {
                type: "ExpressionStatement",
                expression: {
                    type: "Literal",
                    value: M.value
                }
            };
        }), def_to_moz(AST_SimpleStatement, function (M) {
            return {
                type: "ExpressionStatement",
                expression: to_moz(M.body)
            };
        }), def_to_moz(AST_SwitchBranch, function (M) {
            return {
                type: "SwitchCase",
                test: to_moz(M.expression),
                consequent: M.body.map(to_moz)
            };
        }), def_to_moz(AST_Try, function (M) {
            return {
                type: "TryStatement",
                block: to_moz_block(M),
                handler: to_moz(M.bcatch),
                guardedHandlers: [],
                finalizer: to_moz(M.bfinally)
            };
        }), def_to_moz(AST_Catch, function (M) {
            return {
                type: "CatchClause",
                param: to_moz(M.argname),
                guard: null,
                body: to_moz_block(M)
            };
        }), def_to_moz(AST_Definitions, function (M) {
            return {
                type: "VariableDeclaration",
                kind: M instanceof AST_Const ? "const" : "var",
                declarations: M.definitions.map(to_moz)
            };
        }), def_to_moz(AST_Seq, function (M) {
            return {
                type: "SequenceExpression",
                expressions: M.to_array().map(to_moz)
            };
        }), def_to_moz(AST_PropAccess, function (M) {
            var isComputed = M instanceof AST_Sub;
            return {
                type: "MemberExpression",
                object: to_moz(M.expression),
                computed: isComputed,
                property: isComputed ? to_moz(M.property) : {
                    type: "Identifier",
                    name: M.property
                }
            };
        }), def_to_moz(AST_Unary, function (M) {
            return {
                type: "++" == M.operator || "--" == M.operator ? "UpdateExpression" : "UnaryExpression",
                operator: M.operator,
                prefix: M instanceof AST_UnaryPrefix,
                argument: to_moz(M.expression)
            };
        }), def_to_moz(AST_Binary, function (M) {
            return {
                type: "&&" == M.operator || "||" == M.operator ? "LogicalExpression" : "BinaryExpression",
                left: to_moz(M.left),
                operator: M.operator,
                right: to_moz(M.right)
            };
        }), def_to_moz(AST_Object, function (M) {
            return {
                type: "ObjectExpression",
                properties: M.properties.map(to_moz)
            };
        }), def_to_moz(AST_ObjectProperty, function (M) {
            var kind, key = is_identifier(M.key) ? {
                type: "Identifier",
                name: M.key
            } : {
                type: "Literal",
                value: M.key
            };
            return M instanceof AST_ObjectKeyVal ? kind = "init" : M instanceof AST_ObjectGetter ? kind = "get" : M instanceof AST_ObjectSetter && (kind = "set"),
            {
                type: "Property",
                kind: kind,
                key: key,
                value: to_moz(M.value)
            };
        }), def_to_moz(AST_Symbol, function (M) {
            var def = M.definition();
            return {
                type: "Identifier",
                name: def ? def.mangled_name || def.name : M.name
            };
        }), def_to_moz(AST_Constant, function (M) {
            var value = M.value;
            return "number" == typeof value && (0 > value || 0 === value && 0 > 1 / value) ? {
                type: "UnaryExpression",
                operator: "-",
                prefix: !0,
                argument: {
                    type: "Literal",
                    value: -value
                }
            } : {
                type: "Literal",
                value: value
            };
        }), def_to_moz(AST_Atom, function (M) {
            return {
                type: "Identifier",
                name: String(M.value)
            };
        }), AST_Boolean.DEFMETHOD("to_mozilla_ast", AST_Constant.prototype.to_mozilla_ast),
        AST_Null.DEFMETHOD("to_mozilla_ast", AST_Constant.prototype.to_mozilla_ast), AST_Hole.DEFMETHOD("to_mozilla_ast", function () {
            return null;
        }), AST_Block.DEFMETHOD("to_mozilla_ast", AST_BlockStatement.prototype.to_mozilla_ast),
        AST_Lambda.DEFMETHOD("to_mozilla_ast", AST_Function.prototype.to_mozilla_ast);
        var FROM_MOZ_STACK = null;
        AST_Node.from_mozilla_ast = function (node) {
            var save_stack = FROM_MOZ_STACK;
            FROM_MOZ_STACK = [];
            var ast = from_moz(node);
            return FROM_MOZ_STACK = save_stack, ast;
        };
    }(), exports.array_to_hash = array_to_hash, exports.slice = slice, exports.characters = characters,
    exports.member = member, exports.find_if = find_if, exports.repeat_string = repeat_string,
    exports.DefaultsError = DefaultsError, exports.defaults = defaults, exports.merge = merge,
    exports.noop = noop, exports.MAP = MAP, exports.push_uniq = push_uniq, exports.string_template = string_template,
    exports.remove = remove, exports.mergeSort = mergeSort, exports.set_difference = set_difference,
    exports.set_intersection = set_intersection, exports.makePredicate = makePredicate,
    exports.all = all, exports.Dictionary = Dictionary, exports.DEFNODE = DEFNODE, exports.AST_Token = AST_Token,
    exports.AST_Node = AST_Node, exports.AST_Statement = AST_Statement, exports.AST_Debugger = AST_Debugger,
    exports.AST_Directive = AST_Directive, exports.AST_SimpleStatement = AST_SimpleStatement,
    exports.walk_body = walk_body, exports.AST_Block = AST_Block, exports.AST_BlockStatement = AST_BlockStatement,
    exports.AST_EmptyStatement = AST_EmptyStatement, exports.AST_StatementWithBody = AST_StatementWithBody,
    exports.AST_LabeledStatement = AST_LabeledStatement, exports.AST_IterationStatement = AST_IterationStatement,
    exports.AST_DWLoop = AST_DWLoop, exports.AST_Do = AST_Do, exports.AST_While = AST_While,
    exports.AST_For = AST_For, exports.AST_ForIn = AST_ForIn, exports.AST_With = AST_With,
    exports.AST_Scope = AST_Scope, exports.AST_Toplevel = AST_Toplevel, exports.AST_Lambda = AST_Lambda,
    exports.AST_Accessor = AST_Accessor, exports.AST_Function = AST_Function, exports.AST_Defun = AST_Defun,
    exports.AST_Jump = AST_Jump, exports.AST_Exit = AST_Exit, exports.AST_Return = AST_Return,
    exports.AST_Throw = AST_Throw, exports.AST_LoopControl = AST_LoopControl, exports.AST_Break = AST_Break,
    exports.AST_Continue = AST_Continue, exports.AST_If = AST_If, exports.AST_Switch = AST_Switch,
    exports.AST_SwitchBranch = AST_SwitchBranch, exports.AST_Default = AST_Default,
    exports.AST_Case = AST_Case, exports.AST_Try = AST_Try, exports.AST_Catch = AST_Catch,
    exports.AST_Finally = AST_Finally, exports.AST_Definitions = AST_Definitions, exports.AST_Var = AST_Var,
    exports.AST_Const = AST_Const, exports.AST_VarDef = AST_VarDef, exports.AST_Call = AST_Call,
    exports.AST_New = AST_New, exports.AST_Seq = AST_Seq, exports.AST_PropAccess = AST_PropAccess,
    exports.AST_Dot = AST_Dot, exports.AST_Sub = AST_Sub, exports.AST_Unary = AST_Unary,
    exports.AST_UnaryPrefix = AST_UnaryPrefix, exports.AST_UnaryPostfix = AST_UnaryPostfix,
    exports.AST_Binary = AST_Binary, exports.AST_Conditional = AST_Conditional, exports.AST_Assign = AST_Assign,
    exports.AST_Array = AST_Array, exports.AST_Object = AST_Object, exports.AST_ObjectProperty = AST_ObjectProperty,
    exports.AST_ObjectKeyVal = AST_ObjectKeyVal, exports.AST_ObjectSetter = AST_ObjectSetter,
    exports.AST_ObjectGetter = AST_ObjectGetter, exports.AST_Symbol = AST_Symbol, exports.AST_SymbolAccessor = AST_SymbolAccessor,
    exports.AST_SymbolDeclaration = AST_SymbolDeclaration, exports.AST_SymbolVar = AST_SymbolVar,
    exports.AST_SymbolConst = AST_SymbolConst, exports.AST_SymbolFunarg = AST_SymbolFunarg,
    exports.AST_SymbolDefun = AST_SymbolDefun, exports.AST_SymbolLambda = AST_SymbolLambda,
    exports.AST_SymbolCatch = AST_SymbolCatch, exports.AST_Label = AST_Label, exports.AST_SymbolRef = AST_SymbolRef,
    exports.AST_LabelRef = AST_LabelRef, exports.AST_This = AST_This, exports.AST_Constant = AST_Constant,
    exports.AST_String = AST_String, exports.AST_Number = AST_Number, exports.AST_RegExp = AST_RegExp,
    exports.AST_Atom = AST_Atom, exports.AST_Null = AST_Null, exports.AST_NaN = AST_NaN,
    exports.AST_Undefined = AST_Undefined, exports.AST_Hole = AST_Hole, exports.AST_Infinity = AST_Infinity,
    exports.AST_Boolean = AST_Boolean, exports.AST_False = AST_False, exports.AST_True = AST_True,
    exports.TreeWalker = TreeWalker, exports.KEYWORDS = KEYWORDS, exports.KEYWORDS_ATOM = KEYWORDS_ATOM,
    exports.RESERVED_WORDS = RESERVED_WORDS, exports.KEYWORDS_BEFORE_EXPRESSION = KEYWORDS_BEFORE_EXPRESSION,
    exports.OPERATOR_CHARS = OPERATOR_CHARS, exports.RE_HEX_NUMBER = RE_HEX_NUMBER,
    exports.RE_OCT_NUMBER = RE_OCT_NUMBER, exports.RE_DEC_NUMBER = RE_DEC_NUMBER, exports.OPERATORS = OPERATORS,
    exports.WHITESPACE_CHARS = WHITESPACE_CHARS, exports.PUNC_BEFORE_EXPRESSION = PUNC_BEFORE_EXPRESSION,
    exports.PUNC_CHARS = PUNC_CHARS, exports.REGEXP_MODIFIERS = REGEXP_MODIFIERS, exports.UNICODE = UNICODE,
    exports.is_letter = is_letter, exports.is_digit = is_digit, exports.is_alphanumeric_char = is_alphanumeric_char,
    exports.is_unicode_digit = is_unicode_digit, exports.is_unicode_combining_mark = is_unicode_combining_mark,
    exports.is_unicode_connector_punctuation = is_unicode_connector_punctuation, exports.is_identifier = is_identifier,
    exports.is_identifier_start = is_identifier_start, exports.is_identifier_char = is_identifier_char,
    exports.is_identifier_string = is_identifier_string, exports.parse_js_number = parse_js_number,
    exports.JS_Parse_Error = JS_Parse_Error, exports.js_error = js_error, exports.is_token = is_token,
    exports.EX_EOF = EX_EOF, exports.tokenizer = tokenizer, exports.UNARY_PREFIX = UNARY_PREFIX,
    exports.UNARY_POSTFIX = UNARY_POSTFIX, exports.ASSIGNMENT = ASSIGNMENT, exports.PRECEDENCE = PRECEDENCE,
    exports.STATEMENTS_WITH_LABELS = STATEMENTS_WITH_LABELS, exports.ATOMIC_START_TOKEN = ATOMIC_START_TOKEN,
    exports.parse = parse, exports.TreeTransformer = TreeTransformer, exports.SymbolDef = SymbolDef,
    exports.base54 = base54, exports.OutputStream = OutputStream, exports.Compressor = Compressor,
    exports.SourceMap = SourceMap, exports.find_builtins = find_builtins, exports.mangle_properties = mangle_properties;
}({}, function () {
    return this;
}());