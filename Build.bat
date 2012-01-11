@echo off

set fdir=%WINDIR%\Microsoft.NET\Framework64

if not exist %fdir% (
	set fdir=%WINDIR%\Microsoft.NET\Framework
)

set msbuild=%fdir%\v4.0.30319\msbuild.exe

%msbuild% WebSocket4Net\WebSocket4Net.csproj /p:Configuration=Release /t:Clean;Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\websocket4net.snk
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

set mgdir=Bin\Net40
set ver=v4
set ref=
set mergejson=1
call :Merge

%msbuild% WebSocket4Net\WebSocket4Net.Net35.csproj /p:Configuration=Release /t:Clean;Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\websocket4net.snk
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

set mgdir=Bin\Net35
set ver=v2
set ref=
set mergejson=1
call :Merge

%msbuild% WebSocket4Net\WebSocket4Net.MonoDroid.csproj /p:Configuration=Release /t:Clean;Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\websocket4net.snk
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

set mgdir=Bin\MonoDroid
set ver=v4
set mddir=%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\MonoAndroid\v1.0
if not exist "%mddir%" (
	set mddir=%ProgramFiles%\Reference Assemblies\Microsoft\Framework\MonoAndroid\v1.0
)
set ref=,"%mddir%"
set mergejson=
call :Merge

set fdir=%WINDIR%\Microsoft.NET\Framework
set msbuild=%fdir%\v4.0.30319\msbuild.exe

%msbuild% WebSocket4Net.Silverlight\WebSocket4Net.Silverlight.csproj  /p:Configuration=Release /t:Clean;Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\websocket4net.snk
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

set mgdir=Bin\Silverlight
set ver=v4
set mergejson=1
set sldir=%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\Silverlight\v4.0
if not exist "%sldir%" (
	set sldir=%ProgramFiles%\Reference Assemblies\Microsoft\Framework\Silverlight\v4.0
)
set ref=,"%sldir%"
call :Merge

%msbuild% WebSocket4Net.Silverlight\WebSocket4Net.WindowsPhone.csproj  /p:Configuration=Release /t:Clean;Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\websocket4net.snk
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

set mgdir=Bin\WindowsPhone
set ver=v4
set mergejson=
set wpdir=%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\Silverlight\v4.0\Profile\WindowsPhone71
if not exist "%wpdir%" (
	set wpdir=%ProgramFiles%\Reference Assemblies\Microsoft\Framework\Silverlight\v4.0\Profile\WindowsPhone71
)
set ref=,"%wpdir%"
call :Merge

pause
goto:eof

:Merge
set srcdir=%mgdir%\Release
set mgbkdir=%srcdir%\Merge
if not exist %mgbkdir% (
	mkdir %mgbkdir%
)
set json=
if "%mergejson%" equ "1" (
	set json=%mgbkdir%\Newtonsoft.Json.dll
)
move %srcdir%\* %mgbkdir%
Tools\ILMerge /keyfile:websocket4net.snk /targetplatform:%ver%%ref% /ndebug /log /out:%mgdir%\WebSocket4Net.dll %mgbkdir%\WebSocket4Net.dll %mgbkdir%\SuperSocket.ClientEngine.dll %json%
if "%mergejson%" neq "1" (
	copy %mgbkdir%\Newtonsoft.Json.dll %mgdir%
)
rmdir %mgbkdir% /S /Q