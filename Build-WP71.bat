@echo off

set fdir=%WINDIR%\Microsoft.NET\Framework
set msbuild=%fdir%\v4.0.30319\msbuild.exe

set sldir="%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\Silverlight\v4.0\Profile\WindowsPhone71"
if not exist %sldir% (
    set sldir="%ProgramFiles%\Reference Assemblies\Microsoft\Framework\Silverlight\v4.0\Profile\WindowsPhone71"
)

set outDir=bin\sl40-windowsphone71\Debug
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"
%msbuild% WebSocket4Net.WP71\WebSocket4Net.WP71.csproj /p:Configuration=Debug;OutDir=..\%outDir% /t:Clean;Rebuild

"Tools\ILMerge" /keyfile:"websocket4net.snk" /targetplatform:v4,%sldir% /ndebug /out:%outDir%\WebSocket4Net.dll %outDir%\WebSocket4Net.dll %outDir%\SuperSocket.ClientEngine.Common.dll %outDir%\SuperSocket.ClientEngine.Core.dll %outDir%\SuperSocket.ClientEngine.Protocol.dll %outDir%\BouncyCastle.Crypto.dll
del %outDir%\SuperSocket.ClientEngine.Common.dll
del %outDir%\SuperSocket.ClientEngine.Core.dll
del %outDir%\SuperSocket.ClientEngine.Protocol.dll
del %outDir%\BouncyCastle.Crypto.dll
del %outDir%\*.pdb

set outDir=bin\sl40-windowsphone71\Release

FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"
%msbuild% WebSocket4Net.WP71\WebSocket4Net.WP71.csproj /p:Configuration=Release;OutDir=..\%outDir% /t:Clean;Rebuild

"Tools\ILMerge" /keyfile:"websocket4net.snk" /targetplatform:v4,%sldir% /ndebug /out:%outDir%\WebSocket4Net.dll %outDir%\WebSocket4Net.dll %outDir%\SuperSocket.ClientEngine.Common.dll %outDir%\SuperSocket.ClientEngine.Core.dll %outDir%\SuperSocket.ClientEngine.Protocol.dll %outDir%\BouncyCastle.Crypto.dll
del %outDir%\SuperSocket.ClientEngine.Common.dll
del %outDir%\SuperSocket.ClientEngine.Core.dll
del %outDir%\SuperSocket.ClientEngine.Protocol.dll
del %outDir%\BouncyCastle.Crypto.dll
del %outDir%\*.pdb

pause
