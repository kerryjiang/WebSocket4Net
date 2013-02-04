@echo off

set solutionDir=%cd%\

set fdir=%WINDIR%\Microsoft.NET\Framework
set msbuild=%fdir%\v4.0.30319\msbuild.exe

%msbuild% WebSocket4Net\WebSocket4Net.MonoDroid.csproj /p:SolutionDir=%solutionDir%;Configuration=Debug /t:Clean;Rebuild
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

%msbuild% WebSocket4Net\WebSocket4Net.MonoDroid.csproj /p:SolutionDir=%solutionDir%;Configuration=Release /t:Clean;Rebuild
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

pause
