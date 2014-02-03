var obj = new Date(1978, 3);

var n_obj = new Object(obj);

//CHECK#2
if ((n_obj.getYear() !== 78) || (n_obj.getMonth() !== 3)) {
    $ERROR('#2: When the Object constructor is called and if the value is an Object simply value returns.');
}