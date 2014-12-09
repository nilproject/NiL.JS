<html>
<head>
    <title></title>
    <link href='http://fonts.googleapis.com/css?family=Duru+Sans' rel='stylesheet' type='text/css'>
</head>
<body style="font-family: 'Duru Sans', sans-serif;">
    <div style="font-weight: 500; font-size: 100px; position: relative; top: -10px;">NiL.JS</div>
    Open source ECMAScript 5.1 (JavaScript) engine.<br/>
    Licensed under BSD 3-Clause License.
    <ul>
        <li><span style="font-weight:bold">No</span> native dependence.</li>
        <li><span style="font-weight:bold">One</span> assembly.</li>
        <li><span style="font-weight:bold">Automatically</span> wrapping .NET objects. No changes are required.</li>
        <li><span style="font-weight:bold">Access to AST and result code analysis.</span></li>
        <li>Compatible with <span style="font-weight:bold">ASP.NET</span>.</li>
        <li>High performance.</li>
        <li>Integrated debugger (In "For developers" version).</li>
        <li>Support for..of.</li>
        <li>Support consts.</li>
        <li>Support generators.</li>
        <li>99% of Sputnik tests passed.</li>
    </ul>
    <p><strong>C#</strong></p>
    <blockquote>
Context.GlobalContext.DefineVariable("alert").Assign(new ExternalFunction((self, arguments) =&gt; <br/>
{<br/>
&nbsp;&nbsp;&nbsp;&nbsp;MessageBox.Show(arguments[0].ToString());<br/>
&nbsp;&nbsp;&nbsp;&nbsp;return JSObject.Undefined; // or null<br/>
}));<br/>
</blockquote>
    <p><strong>JavaScript</strong></p>
    <blockquote>
<p>alert("Hello!");</p>
</blockquote>
    <a href="https://github.com/nilproject/NiL.JS/wiki/Samples">Samples</a>
    <br/><br/>
    <a href="https://www.nuget.org/packages/NiL.JS">Available in NuGet</a>
</body>
</html>
