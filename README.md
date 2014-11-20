<html>
<head>
    <title></title>
    <link href='http://fonts.googleapis.com/css?family=Duru+Sans' rel='stylesheet' type='text/css'>
</head>
<body style="font-family: 'Duru Sans', sans-serif;">
    <div style="font-weight: 500; font-size: 100px; position: relative; top: -10px;">NiL.JS</div>
    Open source ECMAScript 5.1 (JavaScript) engine.
    <ul>
        <li><span style="font-weight:bold">No</span> native dependence.</li>
        <li>Optional generation of <span style="font-weight:bold">IL</span> (Experimental).</li>
        <li><span style="font-weight:bold">One</span> assembly.</li>
        <li><span style="font-weight:bold">Automatically</span> wrapping .NET objects. No changes are required.</li>
        <li>Runs on <span style="font-weight:bold">all</span> platforms supported .NET.</li>
        <li><span style="font-weight:bold">Translator as a service.</span></li>
        <li>Compatible with <span style="font-weight:bold">ASP.NET</span>.</li>
        <li>High performance.</li>
        <li>Integrated debugger (In "For developers" version).</li>
        <li>Support for..of.</li>
        <li>Support consts.</li>
        <li>Support generators (Experimental. With some issues).</li>
        <li>99% of Sputnik tests passed.</li>
    </ul>
    <p><strong>C#</strong></p>
    <blockquote>
<p>NiL.JS.Core.Context.GlobalContext.DefineVariable("alert")
<br/>           .Assign(new ExternalFunction((thisBind, arguments) =&gt; {</p>
<p>System.Windows.Forms.MessageBox.Show(arguments[0].ToString()); </p>
<p>return JSObject.Undefined; // or null</p>
<p>}));</p>
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
