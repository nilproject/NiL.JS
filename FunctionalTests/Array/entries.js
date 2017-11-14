//var arr = ['a', , 'c', ];

//var eArr = arr.entries();

//var index = 0;
//for (var item of eArr)
//{
//    if (index !== item[0])
//        console.log(`[Flat array] Expected ${index}. Actual ${item[0]}`);

//    index++;
//}

////////////////////////////////////////

Array.prototype[2] = 'h';

var arr = [];
arr[10] = 1;
var eArr = arr.entries();

var index = 0;
for (var item of eArr)
{
    if (index !== item[0])
        console.log(`[Sparse array] Expected ${index}. Actual ${item[0]}`);

    if (index === 2) {
        if (item[1] !== 'h')
            console.log(`[Sparse array] Expected 'h'. Actual ${item[1]}`);
    }

    if (index === 10) {
        if (item[1] !== 1)
            console.log(`[Sparse array] Expected ${arr[10]}. Actual ${item[1]}`);
    }

    index++;
}

if (index !== 11)
    console.log(`[Sparse array] Expected count 11. Actual ${item[1]}`);

////////////////////////////////////////

var e = [].entries.call({ length: 10 });

var index = 0;
for (var item of e)
{
    if (index !== item[0])
        console.log(`[Object] Expected ${index}. Actual ${item[0]}`);

    index++;
}

if (index !== 10)
    console.log(`[Object] Expected count 11. Actual ${index}`);