@echo off

set fdir=%WINDIR%\Microsoft.NET\Framework64

if not exist %fdir% (
	set fdir=%WINDIR%\Microsoft.NET\Framework
)

set msbuild=%fdir%\v4.0.30319\msbuild.exe

set outDir=bin\net45\Debug

%msbuild% WebSocket4Net\WebSocket4Net.Net45.csproj /p:Configuration=Debug;OutDir=..\%outDir% /t:Clean;Rebuild
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

"Tools\ILMerge" /keyfile:"websocket4net.snk" /targetplatform:v4 /ndebug /out:%outDir%\WebSocket4Net.dll %outDir%\WebSocket4Net.dll %outDir%\SuperSocket.ClientEngine.Common.dll %outDir%\SuperSocket.ClientEngine.Core.dll %outDir%\SuperSocket.ClientEngine.Protocol.dll
del %outDir%\SuperSocket.ClientEngine.Common.dll
del %outDir%\SuperSocket.ClientEngine.Core.dll
del %outDir%\SuperSocket.ClientEngine.Protocol.dll
del %outDir%\*.pdb

set outDir=bin\net45\Release

%msbuild% WebSocket4Net\WebSocket4Net.Net45.csproj /p:Configuration=Release;OutDir=..\%outDir% /t:Clean;Rebuild
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G

"Tools\ILMerge" /keyfile:"websocket4net.snk" /targetplatform:v4 /ndebug /out:%outDir%\WebSocket4Net.dll %outDir%\WebSocket4Net.dll %outDir%\SuperSocket.ClientEngine.Common.dll %outDir%\SuperSocket.ClientEngine.Core.dll %outDir%\SuperSocket.ClientEngine.Protocol.dll
del %outDir%\SuperSocket.ClientEngine.Common.dll
del %outDir%\SuperSocket.ClientEngine.Core.dll
del %outDir%\SuperSocket.ClientEngine.Protocol.dll
del %outDir%\*.pdb

pause
