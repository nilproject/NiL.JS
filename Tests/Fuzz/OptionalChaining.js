console.assert(null?.a == undefined);

console.assert(undefined?.a == undefined);

console.assert(null?.["a"] == undefined);

console.assert(null?.(a) == undefined);

var variable = 1;
console.assert(null?.(variable = 2) == undefined);
console.assert(undefined?.(variable = 2) == undefined);
console.assert(variable == 1);

console.assert(null?.[variable = 2] == undefined);
console.assert(undefined?.[variable = 2] == undefined);
console.assert(variable == 1);

var object = { foo: { boo: () => "hello" } };
console.assert(object?.foo?.boo() == "hello");

console.assert(false?.5:true);
