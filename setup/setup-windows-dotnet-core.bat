::@echo off

set version=3.3
set dst=wexflow-%version%-windows-dotnet-core
set dstDir=.\%dst%
set backOffice=Back-office

if exist %dstDir% rmdir /s /q %dstDir%
mkdir %dstDir%
mkdir %dstDir%\Wexflow-dotnet-core\
mkdir %dstDir%\WexflowTesting\
mkdir %dstDir%\%backOffice%\
mkdir %dstDir%\%backOffice%\images\
mkdir %dstDir%\%backOffice%\css\
mkdir %dstDir%\%backOffice%\css\images\
mkdir %dstDir%\%backOffice%\js\

:: WexflowTesting
xcopy ..\samples\WexflowTesting\* %dstDir%\WexflowTesting\ /s /e

:: Wexflow-dotnet-core
xcopy ..\samples\dotnet-core\windows\* %dstDir%\Wexflow-dotnet-core\ /s /e
copy ..\src\dotnet-core\Wexflow.Core\GlobalVariables.xml %dstDir%\Wexflow-dotnet-core\
copy ..\src\dotnet-core\Wexflow.Core\Wexflow.xml %dstDir%\Wexflow-dotnet-core\
copy ..\src\dotnet-core\Wexflow.Core\Workflow.xsd %dstDir%\Wexflow-dotnet-core\

:: Wexflow back office
copy "..\src\dotnet\Wexflow.Clients.BackOffice\index.html" %dstDir%\%backOffice%\
copy "..\src\dotnet\Wexflow.Clients.BackOffice\forgot-password.html" %dstDir%\%backOffice%\
copy "..\src\dotnet\Wexflow.Clients.BackOffice\dashboard.html" %dstDir%\%backOffice%\
copy "..\src\dotnet\Wexflow.Clients.BackOffice\manager.html" %dstDir%\%backOffice%\
copy "..\src\dotnet\Wexflow.Clients.BackOffice\designer.html" %dstDir%\%backOffice%\
copy "..\src\dotnet\Wexflow.Clients.BackOffice\history.html" %dstDir%\%backOffice%\
copy "..\src\dotnet\Wexflow.Clients.BackOffice\users.html" %dstDir%\%backOffice%\

xcopy "..\src\dotnet\Wexflow.Clients.BackOffice\images\*" %dstDir%\%backOffice%\images /s /e

xcopy "..\src\dotnet\Wexflow.Clients.BackOffice\css\images\*" %dstDir%\%backOffice%\css\images /s /e
copy "..\src\dotnet\Wexflow.Clients.BackOffice\css\login.min.css" %dstDir%\%backOffice%\css
copy "..\src\dotnet\Wexflow.Clients.BackOffice\css\forgot-password.min.css" %dstDir%\%backOffice%\css
copy "..\src\dotnet\Wexflow.Clients.BackOffice\css\dashboard.min.css" %dstDir%\%backOffice%\css
copy "..\src\dotnet\Wexflow.Clients.BackOffice\css\manager.min.css" %dstDir%\%backOffice%\css
copy "..\src\dotnet\Wexflow.Clients.BackOffice\css\designer.min.css" %dstDir%\%backOffice%\css
copy "..\src\dotnet\Wexflow.Clients.BackOffice\css\history.min.css" %dstDir%\%backOffice%\css
copy "..\src\dotnet\Wexflow.Clients.BackOffice\css\users.min.css" %dstDir%\%backOffice%\css

copy "..\src\dotnet\Wexflow.Clients.BackOffice\js\settings.js" %dstDir%\%backOffice%\js
copy "..\src\dotnet\Wexflow.Clients.BackOffice\js\login.min.js" %dstDir%\%backOffice%\js
copy "..\src\dotnet\Wexflow.Clients.BackOffice\js\forgot-password.min.js" %dstDir%\%backOffice%\js
copy "..\src\dotnet\Wexflow.Clients.BackOffice\js\dashboard.min.js" %dstDir%\%backOffice%\js
copy "..\src\dotnet\Wexflow.Clients.BackOffice\js\manager.min.js" %dstDir%\%backOffice%\js
copy "..\src\dotnet\Wexflow.Clients.BackOffice\js\designer.min.js" %dstDir%\%backOffice%\js
copy "..\src\dotnet\Wexflow.Clients.BackOffice\js\history.min.js" %dstDir%\%backOffice%\js
copy "..\src\dotnet\Wexflow.Clients.BackOffice\js\users.min.js" %dstDir%\%backOffice%\js

:: Wexflow server
dotnet publish  ..\src\dotnet-core\Wexflow.Server\Wexflow.Server.csproj --force --output %~dp0\%dstDir%\Wexflow.Server
copy dotnet-core\windows\run.bat %dstDir%
copy dotnet-core\windows\install.bat %dstDir%

:: License
:: copy ..\LICENSE.txt %dstDir%

:: compress
7z.exe a -tzip %dst%.zip %dstDir%

:: Cleanup
rmdir /s /q %dstDir%

pause