using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("NiL.JS")]
[assembly: AssemblyProduct("NiL.JS")]
[assembly: AssemblyDescription("JavaScript engine for .NET")]
[assembly: AssemblyCompany("NiLProject")]
[assembly: AssemblyCopyright("Copyright © NiLProject 2013-2017")]
[assembly: AssemblyTrademark("NiL.JS")]
[assembly: AssemblyVersion(InternalInfo.Version)]
[assembly: AssemblyFileVersion(InternalInfo.Version)]
[assembly: NeutralResourcesLanguage("en-US")]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]

#if !PORTABLE && !NETCORE
[assembly: Guid("a70afe5a-2b29-49fd-afbf-28794042ea21")]
#endif

internal static class InternalInfo
{
    internal const string Version = "2.5.1070";
}
