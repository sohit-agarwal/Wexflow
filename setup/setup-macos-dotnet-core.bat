::@echo off

set version=3.5
set dst=wexflow
set zip=wexflow-%version%-macos-dotnet-core.zip
set dstDir=.\%dst%
set backend=Backend

if exist %zip% del %zip%
if exist %dstDir% rmdir /s /q %dstDir%
mkdir %dstDir%
mkdir %dstDir%\Wexflow\
mkdir %dstDir%\WexflowTesting\
mkdir %dstDir%\%backend%\
mkdir %dstDir%\%backend%\images\
mkdir %dstDir%\%backend%\css\
mkdir %dstDir%\%backend%\css\images\
mkdir %dstDir%\%backend%\js\

:: WexflowTesting
xcopy ..\samples\WexflowTesting\* %dstDir%\WexflowTesting\ /s /e

:: Wexflow
xcopy ..\samples\dotnet-core\macos\* %dstDir%\Wexflow\ /s /e
copy ..\src\dotnet-core\Wexflow.Core\Workflow.xsd %dstDir%\Wexflow\

:: Wexflow back office
copy "..\src\dotnet\Wexflow.Clients.Backend\index.html" %dstDir%\%backend%\
copy "..\src\dotnet\Wexflow.Clients.Backend\forgot-password.html" %dstDir%\%backend%\
copy "..\src\dotnet\Wexflow.Clients.Backend\dashboard.html" %dstDir%\%backend%\
copy "..\src\dotnet\Wexflow.Clients.Backend\manager.html" %dstDir%\%backend%\
copy "..\src\dotnet\Wexflow.Clients.Backend\designer.html" %dstDir%\%backend%\
copy "..\src\dotnet\Wexflow.Clients.Backend\history.html" %dstDir%\%backend%\
copy "..\src\dotnet\Wexflow.Clients.Backend\users.html" %dstDir%\%backend%\

xcopy "..\src\dotnet\Wexflow.Clients.Backend\images\*" %dstDir%\%backend%\images /s /e

xcopy "..\src\dotnet\Wexflow.Clients.Backend\css\images\*" %dstDir%\%backend%\css\images /s /e
copy "..\src\dotnet\Wexflow.Clients.Backend\css\login.min.css" %dstDir%\%backend%\css
copy "..\src\dotnet\Wexflow.Clients.Backend\css\forgot-password.min.css" %dstDir%\%backend%\css
copy "..\src\dotnet\Wexflow.Clients.Backend\css\dashboard.min.css" %dstDir%\%backend%\css
copy "..\src\dotnet\Wexflow.Clients.Backend\css\manager.min.css" %dstDir%\%backend%\css
copy "..\src\dotnet\Wexflow.Clients.Backend\css\designer.min.css" %dstDir%\%backend%\css
copy "..\src\dotnet\Wexflow.Clients.Backend\css\history.min.css" %dstDir%\%backend%\css
copy "..\src\dotnet\Wexflow.Clients.Backend\css\users.min.css" %dstDir%\%backend%\css

copy "..\src\dotnet\Wexflow.Clients.Backend\js\settings.js" %dstDir%\%backend%\js
copy "..\src\dotnet\Wexflow.Clients.Backend\js\login.min.js" %dstDir%\%backend%\js
copy "..\src\dotnet\Wexflow.Clients.Backend\js\forgot-password.min.js" %dstDir%\%backend%\js
copy "..\src\dotnet\Wexflow.Clients.Backend\js\dashboard.min.js" %dstDir%\%backend%\js
copy "..\src\dotnet\Wexflow.Clients.Backend\js\manager.min.js" %dstDir%\%backend%\js

copy "..\src\dotnet\Wexflow.Clients.Backend\js\designer.min.js" %dstDir%\%backend%\js
::copy "..\src\dotnet\Wexflow.Clients.Backend\js\common.js" %dstDir%\%backend%\js
::copy "..\src\dotnet\Wexflow.Clients.Backend\js\authenticate.js" %dstDir%\%backend%\js
::copy "..\src\dotnet\Wexflow.Clients.Backend\js\cytoscape-dagre.min.js" %dstDir%\%backend%\js
::copy "..\src\dotnet\Wexflow.Clients.Backend\js\highlight.pack.js" %dstDir%\%backend%\js
::copy "..\src\dotnet\Wexflow.Clients.Backend\js\designer.js" %dstDir%\%backend%\js

copy "..\src\dotnet\Wexflow.Clients.Backend\js\history.min.js" %dstDir%\%backend%\js
copy "..\src\dotnet\Wexflow.Clients.Backend\js\users.min.js" %dstDir%\%backend%\js

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