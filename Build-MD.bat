@echo off

set solutionDir=%cd%\

set fdir=%WINDIR%\Microsoft.NET\Framework
set msbuild=%fdir%\v4.0.30319\msbuild.exe

set mddir="%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\MonoAndroid\v1.0"
if not exist %mddir% (
    set mddir="%ProgramFiles%\Reference Assemblies\Microsoft\Framework\MonoAndroid\v1.0"
)

set outDir=bin\monoandroid23\Debug
%msbuild% WebSocket4Net.MonoDroid\WebSocket4Net.MonoDroid.csproj /p:OutDir=..\%outDir%;Configuration=Debug /t:Clean;Rebuild
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

"Tools\ILMerge" /keyfile:"websocket4net.snk" /targetplatform:v4,%mddir% /ndebug /out:%outDir%\WebSocket4Net.dll %outDir%\WebSocket4Net.dll %outDir%\SuperSocket.ClientEngine.Common.dll %outDir%\SuperSocket.ClientEngine.Core.dll %outDir%\SuperSocket.ClientEngine.Protocol.dll
del %outDir%\SuperSocket.ClientEngine.Common.dll
del %outDir%\SuperSocket.ClientEngine.Core.dll
del %outDir%\SuperSocket.ClientEngine.Protocol.dll
del %outDir%\*.pdb

set outDir=bin\monoandroid23\Release
%msbuild% WebSocket4Net.MonoDroid\WebSocket4Net.MonoDroid.csproj /p:OutDir=..\%outDir%;Configuration=Release /t:Clean;Rebuild
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

"Tools\ILMerge" /keyfile:"websocket4net.snk" /targetplatform:v4,%mddir% /ndebug /out:%outDir%\WebSocket4Net.dll %outDir%\WebSocket4Net.dll %outDir%\SuperSocket.ClientEngine.Common.dll %outDir%\SuperSocket.ClientEngine.Core.dll %outDir%\SuperSocket.ClientEngine.Protocol.dll
del %outDir%\SuperSocket.ClientEngine.Common.dll
del %outDir%\SuperSocket.ClientEngine.Core.dll
del %outDir%\SuperSocket.ClientEngine.Protocol.dll
del %outDir%\*.pdb

pause
