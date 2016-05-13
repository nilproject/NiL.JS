"%ProgramFiles(x86)%\MSBuild\14.0\bin\msbuild" nil.js.csproj -p:VisualStudioVersion=10.0 -p:OutputPath=net40\
"%ProgramFiles(x86)%\MSBuild\14.0\bin\msbuild" "%~dp0..\NiL.JS.Portable\nil.js.portable.csproj" /p:Configuration=Release
nuget pack NiL.JS.csproj -Build -OutputDirectory nuget
rd net40 /s /q