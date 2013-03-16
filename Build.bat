@echo off

set fdir=%WINDIR%\Microsoft.NET\Framework64

if not exist %fdir% (
	set fdir=%WINDIR%\Microsoft.NET\Framework
)

set msbuild=%fdir%\v4.0.30319\msbuild.exe

set outDir=bin\net40\Debug

%msbuild% WebSocket4Net\WebSocket4Net.csproj /p:Configuration=Debug;OutDir=..\%outDir% /t:Clean;Rebuild
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

"Tools\ILMerge" /keyfile:"websocket4net.snk" /targetplatform:v4 /ndebug /out:%outDir%\WebSocket4Net.dll %outDir%\WebSocket4Net.dll %outDir%\SuperSocket.ClientEngine.Common.dll %outDir%\SuperSocket.ClientEngine.Core.dll %outDir%\SuperSocket.ClientEngine.Protocol.dll
del %outDir%\SuperSocket.ClientEngine.Common.dll
del %outDir%\SuperSocket.ClientEngine.Core.dll
del %outDir%\SuperSocket.ClientEngine.Protocol.dll
del %outDir%\*.pdb


set outDir=bin\net40\Release

%msbuild% WebSocket4Net\WebSocket4Net.csproj /p:Configuration=Release;OutDir=..\%outDir% /t:Clean;Rebuild
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

"Tools\ILMerge" /keyfile:"websocket4net.snk" /targetplatform:v4 /ndebug /out:%outDir%\WebSocket4Net.dll %outDir%\WebSocket4Net.dll %outDir%\SuperSocket.ClientEngine.Common.dll %outDir%\SuperSocket.ClientEngine.Core.dll %outDir%\SuperSocket.ClientEngine.Protocol.dll
del %outDir%\SuperSocket.ClientEngine.Common.dll
del %outDir%\SuperSocket.ClientEngine.Core.dll
del %outDir%\SuperSocket.ClientEngine.Protocol.dll
del %outDir%\*.pdb

set outDir=bin\net35\Debug

%msbuild% WebSocket4Net\WebSocket4Net.Net35.csproj /p:Configuration=Debug;OutDir=..\%outDir% /t:Clean;Rebuild
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

"Tools\ILMerge" /keyfile:"websocket4net.snk" /targetplatform:v2 /ndebug /out:%outDir%\WebSocket4Net.dll %outDir%\WebSocket4Net.dll %outDir%\SuperSocket.ClientEngine.Common.dll %outDir%\SuperSocket.ClientEngine.Core.dll %outDir%\SuperSocket.ClientEngine.Protocol.dll
del %outDir%\SuperSocket.ClientEngine.Common.dll
del %outDir%\SuperSocket.ClientEngine.Core.dll
del %outDir%\SuperSocket.ClientEngine.Protocol.dll
del %outDir%\*.pdb
sdxxdddddddddds

set outDir=bin\net35\Release

%msbuild% WebSocket4Net\WebSocket4Net.Net35.csproj /p:Configuration=Release;OutDir=..\%outDir% /t:Clean;Rebuild
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

"Tools\ILMerge" /keyfile:"websocket4net.snk" /targetplatform:v2 /ndebug /out:%outDir%\WebSocket4Net.dll %outDir%\WebSocket4Net.dll %outDir%\SuperSocket.ClientEngine.Common.dll %outDir%\SuperSocket.ClientEngine.Core.dll %outDir%\SuperSocket.ClientEngine.Protocol.dll
del %outDir%\SuperSocket.ClientEngine.Common.dll
del %outDir%\SuperSocket.ClientEngine.Core.dll
del %outDir%\SuperSocket.ClientEngine.Protocol.dll
del %outDir%\*.pdb

set outDir=bin\net20\Debug

%msbuild% WebSocket4Net\WebSocket4Net.Net20.csproj /p:Configuration=Debug;OutDir=..\%outDir% /t:Clean;Rebuild
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

"Tools\ILMerge" /keyfile:"websocket4net.snk" /targetplatform:v2 /ndebug /allowDup /out:%outDir%\WebSocket4Net.dll %outDir%\WebSocket4Net.dll %outDir%\SuperSocket.ClientEngine.Common.dll %outDir%\SuperSocket.ClientEngine.Core.dll %outDir%\SuperSocket.ClientEngine.Protocol.dll %outDir%\Newtonsoft.Json.dll
del %outDir%\SuperSocket.ClientEngine.Common.dll
del %outDir%\SuperSocket.ClientEngine.Core.dll
del %outDir%\SuperSocket.ClientEngine.Protocol.dll
del %outDir%\Newtonsoft.Json.dll
del %outDir%\*.pdb


set outDir=bin\net20\Release

%msbuild% WebSocket4Net\WebSocket4Net.Net20.csproj /p:Configuration=Release;OutDir=..\%outDir% /t:Clean;Rebuild
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

"Tools\ILMerge" /keyfile:"websocket4net.snk" /targetplatform:v2 /ndebug /allowDup /out:%outDir%\WebSocket4Net.dll %outDir%\WebSocket4Net.dll %outDir%\SuperSocket.ClientEngine.Common.dll %outDir%\SuperSocket.ClientEngine.Core.dll %outDir%\SuperSocket.ClientEngine.Protocol.dll %outDir%\Newtonsoft.Json.dll
del %outDir%\SuperSocket.ClientEngine.Common.dll
del %outDir%\SuperSocket.ClientEngine.Core.dll
del %outDir%\SuperSocket.ClientEngine.Protocol.dll
del %outDir%\Newtonsoft.Json.dll
del %outDir%\*.pdb

pause
