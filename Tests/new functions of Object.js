Debug.asserta(() => Object.is(undefined, undefined));
Debug.asserta(() => Object.is(null, null));
Debug.asserta(() => Object.is(NaN, NaN));
Debug.asserta(() => !Object.is(-0, +0));

Debug.asserta(() => Object.getOwnPropertySymbols({ [Symbol.for("hello")] : 1 }).join(), "Symbol(hello)");