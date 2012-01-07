@echo off

set fdir=%WINDIR%\Microsoft.NET\Framework64

if not exist %fdir% (
	set fdir=%WINDIR%\Microsoft.NET\Framework
)

set msbuild=%fdir%\v4.0.30319\msbuild.exe

%msbuild% WebSocket4Net\WebSocket4Net.csproj /p:Configuration=Release /t:Clean;Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\websocket4net.snk
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

%msbuild% WebSocket4Net\WebSocket4Net.Mono.csproj /p:Configuration=Release /t:Clean;Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\websocket4net.snk
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

%msbuild% WebSocket4Net\WebSocket4Net.Net35.csproj /p:Configuration=Release /t:Clean;Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\websocket4net.snk
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

set fdir=%WINDIR%\Microsoft.NET\Framework
set msbuild=%fdir%\v4.0.30319\msbuild.exe

%msbuild% WebSocket4Net.Silverlight\WebSocket4Net.Silverlight.csproj  /p:Configuration=Release /t:Clean;Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\websocket4net.snk
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

pause