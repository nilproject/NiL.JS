import * as consts from "./Consts.mjs"

//var consts = 1;

(function () {
	let a = 1;
	{
		var a = 2;
		console.log(a);
	}
	console.log(a);

	console.log(consts.Pi);
})();