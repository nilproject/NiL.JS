# git stash
rd /s /q nil.js\bin
rd /s /q nil.js\obj
rd /s /q nil.js.NetCore\bin
rd /s /q nil.js.NetCore\obj
rd /s /q NiL.JS.Portable\bin
rd /s /q NiL.JS.Portable\obj
"%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Preview\MSBuild\Current\Bin\msbuild" nil.js\nil.js.csproj -p:VisualStudioVersion=15.0
"%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Preview\MSBuild\Current\Bin\msbuild" nil.js\nil.js.csproj -p:VisualStudioVersion=15.0 -p:OutputPath=..\net461\
"%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Preview\MSBuild\Current\Bin\msbuild" nil.js\nil.js.csproj -p:VisualStudioVersion=11.0 -p:OutputPath=..\net45\
"%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Preview\MSBuild\Current\Bin\msbuild" nil.js\nil.js.csproj -p:VisualStudioVersion=10.0 -p:OutputPath=..\net40\
"%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Preview\MSBuild\Current\Bin\msbuild" NiL.JS.Portable\nil.js.portable.csproj /p:Configuration=Release
mkdir nuget
cd nil.js
nuget pack -OutputDirectory ../nuget
cd ..
rd /s /q net40
cd NiL.JS.NetCore
dotnet build -c Release -property:VersionPrefix=2.5.$(git rev-list --count develop)
dotnet pack -c Release
cd ..
# git stash pop
rd /s /q net461
rd /s /q net45