__evaluated = eval("do {__in__do__before__break=1; if(true) break; __in__do__after__break=2;} while(0)");
console.log(__evaluated);
console.log(__in__do__before__break);