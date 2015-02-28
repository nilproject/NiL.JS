
NiL.JS
======
    
Open source ECMAScript 5.1 (JavaScript) engine.<br/>
Licensed under BSD 3-Clause License.
    
* Work on Windows Runtime (Portable version)
* No native dependence.
* One assembly.
* Automatically wrapping .NET objects. No changes are required.
* Access to AST and result code analysis.
* Compatible with ASP.NET.
* High performance.
* Integrated debugger (In "For developers" version).
* 99% of Sputnik tests passed.

## Sample
    
**C\#**

    Context.GlobalContext.DefineVariable("alert").Assign(new ExternalFunction((self, arguments) =>
    {
        MessageBox.Show(arguments[0].ToString());
        return JSObject.Undefined; // or null
    }));

**JavaScript**
    
    alert("Hello!");

## Links

[Samples](https://github.com/nilproject/NiL.JS/wiki/Samples)  
[NuGet](https://www.nuget.org/packages/NiL.JS)  
[Code analyzer on this engine](http://nilproject.net/linter.html)  

## Known bugs

**Async**
Execution of function in two or more threads gives incorrect result. To fix it, need to write "eval();" in any part of method body (e.g. after "return;"). But, better way to avoid this is to create functions dynamically for each thread using eval or Function("some code").

## If you found bug

... then you can choose one of three paths:  
  **1.** Ignore it. Maybe, sometime, somebody finds it too and tell me about it.  
  **2.** Create [bug report](https://github.com/nilproject/NiL.JS/issues). It's will take some time, but reduce time for fixing it.  
  **3.** Fork project and fix bug by yourself, after that create pull request.  

I hope that you will not choose the first path.
