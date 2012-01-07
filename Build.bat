@echo off

set fdir=%WINDIR%\Microsoft.NET\Framework64

if not exist %fdir% (
	set fdir=%WINDIR%\Microsoft.NET\Framework
)

set msbuild=%fdir%\v4.0.30319\msbuild.exe

%msbuild% WebSocket4Net\WebSocket4Net.csproj /p:Configuration=Release /t:Clean;Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\websocket4net.snk
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

set mgdir=Bin\Net40\Release
set ver=v4
set ref=
call :Merge

%msbuild% WebSocket4Net\WebSocket4Net.Mono.csproj /p:Configuration=Release /t:Clean;Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\websocket4net.snk
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

set mgdir=Bin\Mono\Release
set ver=v4
set ref=
call :Merge

%msbuild% WebSocket4Net\WebSocket4Net.Net35.csproj /p:Configuration=Release /t:Clean;Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\websocket4net.snk
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

set mgdir=Bin\Net35\Release
set ver=v2
set ref=
call :Merge

set fdir=%WINDIR%\Microsoft.NET\Framework
set msbuild=%fdir%\v4.0.30319\msbuild.exe

%msbuild% WebSocket4Net.Silverlight\WebSocket4Net.Silverlight.csproj  /p:Configuration=Release /t:Clean;Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\websocket4net.snk
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

set mgdir=Bin\Silverlight\Release
set ver=v4
set ref=,"C:\Program Files (x86)\Microsoft Silverlight\4.0.60531.0"
call :Merge

pause
goto:eof

:Merge
set mgbkdir=%mgdir%\Merge
if not exist %mgbkdir% (
	mkdir %mgbkdir%
)
move %mgdir%\* %mgbkdir%
Tools\ILMerge /keyfile:websocket4net.snk /targetplatform:%ver%%ref% /ndebug /log /out:%mgdir%\WebSocket4Net.dll %mgbkdir%\WebSocket4Net.dll %mgbkdir%\SuperSocket.ClientEngine.dll
rmdir %mgbkdir% /S /Q