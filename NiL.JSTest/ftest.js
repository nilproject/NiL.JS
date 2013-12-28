var a = { set f(v) { console.log(v) }, get f() { return 'world' } };
a.f = 'hello';
console.log(a.f);