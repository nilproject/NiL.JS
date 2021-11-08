echo $(
rd nil.js\bin -Force -Recurse -erroraction 'silentlycontinue'
rd nil.js\obj -Force -Recurse -erroraction 'silentlycontinue'
mkdir nuget -erroraction 'silentlycontinue'
) > $null
$REVISION=$(git rev-list --count origin/develop)
[System.IO.File]::WriteAllText("$(get-location)\\NiL.JS\\Properties\\InternalInfo.cs","internal static class InternalInfo
{
    internal const string Version = ""2.5.$($REVISION)"";
    internal const string Year = ""$(get-date -Format yyyy)"";
}")
cd NiL.JS
dotnet build -c Release -property:VersionPrefix=2.5.$($REVISION) -property:SignAssembly=true
dotnet pack -c Release -property:VersionPrefix=2.5.$($REVISION) -property:SignAssembly=true
mv -Force bin/release/NiL.JS.2.5.$($REVISION).nupkg ../nuget/NiL.JS.2.5.$($REVISION).nupkg
cd ..