::@echo off

set version=3.5
set dst=wexflow
set zip=wexflow-%version%-macos-dotnet-core.zip
set dstDir=.\%dst%
set backOffice=Back-office

if exist %zip% del %zip%
if exist %dstDir% rmdir /s /q %dstDir%
mkdir %dstDir%
mkdir %dstDir%\Wexflow\
mkdir %dstDir%\WexflowTesting\
mkdir %dstDir%\%backOffice%\
mkdir %dstDir%\%backOffice%\images\
mkdir %dstDir%\%backOffice%\css\
mkdir %dstDir%\%backOffice%\css\images\
mkdir %dstDir%\%backOffice%\js\

:: WexflowTesting
xcopy ..\samples\WexflowTesting\* %dstDir%\WexflowTesting\ /s /e

:: Wexflow
xcopy ..\samples\dotnet-core\macos\* %dstDir%\Wexflow\ /s /e
copy ..\src\dotnet-core\Wexflow.Core\Workflow.xsd %dstDir%\Wexflow\

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
::copy "..\src\dotnet\Wexflow.Clients.BackOffice\js\common.js" %dstDir%\%backOffice%\js
::copy "..\src\dotnet\Wexflow.Clients.BackOffice\js\authenticate.js" %dstDir%\%backOffice%\js
::copy "..\src\dotnet\Wexflow.Clients.BackOffice\js\cytoscape-dagre.min.js" %dstDir%\%backOffice%\js
::copy "..\src\dotnet\Wexflow.Clients.BackOffice\js\highlight.pack.js" %dstDir%\%backOffice%\js
::copy "..\src\dotnet\Wexflow.Clients.BackOffice\js\designer.js" %dstDir%\%backOffice%\js

copy "..\src\dotnet\Wexflow.Clients.BackOffice\js\history.min.js" %dstDir%\%backOffice%\js
copy "..\src\dotnet\Wexflow.Clients.BackOffice\js\users.min.js" %dstDir%\%backOffice%\js

:: Wexflow server
dotnet publish  ..\src\dotnet-core\Wexflow.Server\Wexflow.Server.csproj --force --output %~dp0\%dstDir%\Wexflow.Server
copy dotnet-core\macos\appsettings.json %dstDir%\Wexflow.Server

:: License
:: copy ..\LICENSE.txt %dstDir%

:: compress
7z.exe a -tzip %zip% %dstDir%

:: Cleanup
rmdir /s /q %dstDir%

pause