
NiL.JS
======
    
Open source ECMAScript 5.1 (JavaScript) engine.<br/>
Licensed under BSD 3-Clause License.
    
* No native dependence.
* One assembly.
* Automatically wrapping .NET objects. No changes are required.
* Access to AST and result code analysis.
* Compatible with ASP.NET.
* High performance.
* Integrated debugger (In "For developers" version).
* 99% of Sputnik tests passed.

Sample
======
    
**C\#**

    Context.GlobalContext.DefineVariable("alert").Assign(new ExternalFunction((self, arguments) =>
    {
        MessageBox.Show(arguments[0].ToString());
        return JSObject.Undefined; // or null
    }));

**JavaScript**
    
    alert("Hello!");

[Samples](https://github.com/nilproject/NiL.JS/wiki/Samples)  
[Available in NuGet](https://www.nuget.org/packages/NiL.JS)  
[Linter with this engine](http://nilproject.net/linter.html)  
