var int8Array = new Int8Array(10);
console.log(int8Array.length);
int8Array.length = 11;
console.log(int8Array.length);
for (var i = 0; i < int8Array.length; i++)
    int8Array[i] = i * 17;
for (var i = 0; i < int8Array.length; i++)
    console.log(int8Array[i]);