echo $(
rd nil.js\bin -Force -Recurse -erroraction 'silentlycontinue'
rd nil.js\obj -Force -Recurse -erroraction 'silentlycontinue'
mkdir nuget -erroraction 'silentlycontinue'
) > $null
$REVISION=$(git rev-list --count origin/develop)
[System.IO.File]::WriteAllText("$(get-location)\\NiL.JS\\Properties\\InternalInfo.cs","internal static class InternalInfo
{
    internal const string Version = ""$VERSION.$($REVISION)"";
    internal const string Year = ""$(get-date -Format yyyy)"";
}")
cd NiL.JS
dotnet build -c Release -property:VersionPrefix=$VERSION.$($REVISION) -property:SignAssembly=true
dotnet pack -c Release -property:VersionPrefix=$VERSION.$($REVISION) -property:SignAssembly=true
mv -Force bin/release/NiL.JS.$VERSION.$($REVISION).nupkg ../nuget/NiL.JS.$VERSION.$($REVISION).nupkg
cd ..