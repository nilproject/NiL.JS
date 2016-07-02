using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if DEV
[assembly: AssemblyTitle("NiL.JS for Developers")]
[assembly: AssemblyProduct("NiL.JS Dev")]
#else
[assembly: AssemblyTitle("NiL.JS")]
[assembly: AssemblyProduct("NiL.JS")]
#endif
[assembly: AssemblyDescription("JavaScript engine for .NET")]
[assembly: AssemblyCompany("NiLProject")]
[assembly: AssemblyCopyright("Copyright © NiLProject 2015")]
[assembly: AssemblyTrademark("NiL.JS")]
[assembly: AssemblyVersion("2.2.914")]
[assembly: AssemblyFileVersion("2.2.914")]
[assembly: NeutralResourcesLanguage("en-US")]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]

#if !PORTABLE
[assembly: Guid("a70afe5a-2b29-49fd-afbf-28794042ea21")]
#endif
