@echo off


set fdir=%WINDIR%\Microsoft.NET\Framework
set msbuild=%fdir%\v4.0.30319\msbuild.exe
set solutionDir=%cd%\

%msbuild% WebSocket4Net.Silverlight\WebSocket4Net.SL40.csproj /p:SolutionDir=%solutionDir%;Configuration=Debug /t:Clean;Rebuild
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

%msbuild% WebSocket4Net.Silverlight\WebSocket4Net.SL40.csproj /p:SolutionDir=%solutionDir%;Configuration=Release /t:Clean;Rebuild
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

%msbuild% WebSocket4Net.Silverlight\WebSocket4Net.SL50.csproj /p:SolutionDir=%solutionDir%;Configuration=Debug /t:Clean;Rebuild
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

%msbuild% WebSocket4Net.Silverlight\WebSocket4Net.SL50.csproj /p:SolutionDir=%solutionDir%;Configuration=Release /t:Clean;Rebuild
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

pause
