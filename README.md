
NiL.JS
======
    
Open source ECMAScript 6.0 (ES2015) (JavaScript) engine.
Licensed under BSD 3-Clause License.
    
* Fast.
* Works on Windows Runtime (Portable version)
* Support Mono.
* Compatible with ASP.NET.
* Automatic wrapping .NET objects. No changes are required.
* Support .NET namespaces (Standalon version).
* Compiler as a Service (CaaS).
* Integrated static analyser.
* Integrated debugger ("For developers" version).
* You can extend syntax with your own statements ([see examples](https://github.com/nilproject/NiL.JS/wiki/Examples#add-you-own-syntax-extension-dev-brunch-only)).

## Example
    
**C\#**

    Context.GlobalContext.DefineVariable("alert").Assign(new ExternalFunction((self, arguments) =>
    {
        MessageBox.Show(arguments[0].ToString());
        return JSObject.Undefined; // or null
    }));

**JavaScript**
    
    alert("Hello!");

## Links

[Examples](https://github.com/nilproject/NiL.JS/wiki/Examples)  
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
