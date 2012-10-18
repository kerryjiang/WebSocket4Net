@echo off

set fdir=%WINDIR%\Microsoft.NET\Framework64

if not exist %fdir% (
	set fdir=%WINDIR%\Microsoft.NET\Framework
)

set msbuild=%fdir%\v4.0.30319\msbuild.exe
set solutionDir=%cd%\

%msbuild% WebSocket4Net\WebSocket4Net.Net45.csproj /p:Configuration=Debug;SolutionDir=%solutionDir%;TargetFrameworkProfile= /t:Clean;Rebuild
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

%msbuild% WebSocket4Net\WebSocket4Net.Net45.csproj /p:Configuration=Release;SolutionDir=%solutionDir%;TargetFrameworkProfile= /t:Clean;Rebuild
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G

pause
