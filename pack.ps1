# git stash
echo $(
rd nil.js\bin -Force -Recurse -erroraction 'silentlycontinue'
rd nil.js\obj -Force -Recurse -erroraction 'silentlycontinue'
mkdir nuget -erroraction 'silentlycontinue'
) > $null
[System.IO.File]::WriteAllText("$(get-location)\\NiL.JS\\Properties\\InternalInfo.cs","internal static class InternalInfo
{
    internal const string Version = ""2.5.$(git rev-list --count develop)"";
}")
cd NiL.JS
dotnet build -c Release -property:VersionPrefix=2.5.$(git rev-list --count develop)
dotnet pack -c Release -property:VersionPrefix=2.5.$(git rev-list --count develop)
cd ..
# git stash pop
pause