// Copyright 2009 the Sputnik authors.  All rights reserved.
/**
 * Check type of various properties
 *
 * @path annexB/B.2.2.propertyCheck.js
 * @description Checking properties of this object (unescape)
 */

if (typeof this.unescape  === "undefined")  $ERROR('#1: typeof this.unescape !== "undefined"');
if (typeof this['unescape'] === "undefined")  $ERROR('#2: typeof this["unescape"] !== "undefined"');
