"use strict";
var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (Object.prototype.hasOwnProperty.call(b, p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        if (typeof b !== "function" && b !== null)
            throw new TypeError("Class extends value " + String(b) + " is not a constructor or null");
        extendStatics(d, b);
        function __() { debugger; this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var Mother = /** @class */ (function () {
    function Mother() {
        this.sayhello = 'good day';
    }
    Mother.prototype.helloWorld = function () {
    };
    ;
    return Mother;
}());
var Daughter = /** @class */ (function (_super) {
    __extends(Daughter, _super);
    function Daughter() {
        var _this = _super !== null && _super.apply(this, arguments) || this;
        _this.sayhello = 'good day';
        return _this;
    }
    debugger;
    Daughter.prototype.howareyou = function () {
    };
    ;
    return Daughter;
}(Mother));
var daughter = new Daughter();

if (!daughter.helloWorld)
    console.log('daughter.helloWorld is undefined');