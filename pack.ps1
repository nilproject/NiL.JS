# git stash
echo $(
rd nil.js\bin -Force -Recurse -erroraction 'silentlycontinue'
rd nil.js\obj -Force -Recurse -erroraction 'silentlycontinue'
rd nil.js.NetCore\bin -Force -Recurse -erroraction 'silentlycontinue'
rd nil.js.NetCore\obj -Force -Recurse -erroraction 'silentlycontinue'
rd NiL.JS.Portable\bin -Force -Recurse -erroraction 'silentlycontinue'
rd NiL.JS.Portable\obj -Force -Recurse -erroraction 'silentlycontinue'
mkdir nuget -erroraction 'silentlycontinue'
) > $null
[System.IO.File]::WriteAllText("$(get-location)\\NiL.JS\\Properties\\InternalInfo.cs","internal static class InternalInfo
{
    internal const string Version = ""2.5.$(git rev-list --count develop)"";
}")
& "\Program Files (x86)\Microsoft Visual Studio\2019\Preview\MSBuild\Current\Bin\msbuild" nil.js\nil.js.csproj -p:VisualStudioVersion=15.0
& "\Program Files (x86)\Microsoft Visual Studio\2019\Preview\MSBuild\Current\Bin\msbuild" nil.js\nil.js.csproj -p:VisualStudioVersion=15.0 -p:OutputPath=..\net461\
& "\Program Files (x86)\Microsoft Visual Studio\2019\Preview\MSBuild\Current\Bin\msbuild" nil.js\nil.js.csproj -p:VisualStudioVersion=11.0 -p:OutputPath=..\net45\
& "\Program Files (x86)\Microsoft Visual Studio\2019\Preview\MSBuild\Current\Bin\msbuild" nil.js\nil.js.csproj -p:VisualStudioVersion=10.0 -p:OutputPath=..\net40\
& "\Program Files (x86)\Microsoft Visual Studio\2019\Preview\MSBuild\Current\Bin\msbuild" NiL.JS.Portable\nil.js.portable.csproj /p:Configuration=Release
cd nil.js
./nuget pack -OutputDirectory ../nuget
cd ..
rd net40 -Recurse -Force
cd NiL.JS.NetCore
dotnet build -c Release -property:VersionPrefix=2.5.$(git rev-list --count develop)
dotnet pack -c Release
cd ..
# git stash pop
rd net461 -Recurse -Force
rd net45 -Recurse -Force