import './../Libs/handlebars.js';

var template = Handlebars.compile('{{#* inline \'treeNodes\'}}\n' +
	    '{{#each nodes}}\n' +
	    '	<li>\n' +
	    '		{{name}}\n' +
	    '		{{#if nodes}}\n' +
	    '		<ul>\n' +
	    '		{{> treeNodes}}\n' +
	    '		<\/ul>\n' +
	    '		{{\/if}}\n' +
	    '	</li>\n' +
	    '{{\/each}}\n' +
	    '{{\/inline}}\n' +
	    '<ul>\n' +
	    '{{> treeNodes nodes=this}}\n' +
	    '<\/ul>'
    ),
	data = [
		{ name: 'A', nodes: [
			{ name: 'B' },
			{ name: 'C' }
		]}
	];