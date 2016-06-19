console.asserta(() => Object.is(undefined, undefined));
console.asserta(() => Object.is(null, null));
console.asserta(() => Object.is(NaN, NaN));
console.asserta(() => !Object.is(-0, +0));

console.asserta(() => Object.getOwnPropertySymbols({ [Symbol.for("hello")] : 1 }).join(), "Symbol(hello)");