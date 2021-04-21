(function(){
	"use strict";
	const s = 's';
	const {a,b} = {a:1,b:2}, {c,d} = {c:3,d:4};
	if (a != 1 || b != 2 || c != 3 || d != 4 || s != 's')
		throw "ACHTUNG #1!!!"
})();

(function(){
	"use strict";
	const s = 's';
	try
	{
		s = 'ы';
	}
	catch (e)
	{
		return;
	}

	throw "ACHTUNG #2!!!"
})();


(function(){
	const s = 's';
	s = 'ы';

	if (s != 's')
		throw "ACHTUNG #3!!!"
})();


(function(){
	const s = 's';

	eval('s = "ы"');

	if (s != 's')
		throw "ACHTUNG #4!!!"
})();