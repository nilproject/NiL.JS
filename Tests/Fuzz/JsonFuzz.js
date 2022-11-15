var json = JSON.stringify({ o: { v: 1, s: 'str' }, a: 'a', b: 1 }, undefined, '  ');
var expected = `{
  "o": {
    "v": 1,
    "s": "str"
  },
  "a": "a",
  "b": 1
}`;
if (json != expected)
    console.log(json);