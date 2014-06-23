var strObj = new String("bbq");
var preCheck = Object.isExtensible(strObj);
Object.preventExtensions(strObj);

strObj.exName = 2;
console.log(preCheck)
console.log(!strObj.hasOwnProperty("exName"))