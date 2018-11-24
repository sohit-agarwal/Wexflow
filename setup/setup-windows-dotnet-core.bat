::@echo off

set version=3.6
set dst=wexflow-%version%-windows-dotnet-core
set dstDir=.\%dst%
set backend=Backend

if exist %dstDir% rmdir /s /q %dstDir%
mkdir %dstDir%
mkdir %dstDir%\Wexflow-dotnet-core\
mkdir %dstDir%\WexflowTesting\
mkdir %dstDir%\%backend%\
mkdir %dstDir%\%backend%\images\
mkdir %dstDir%\%backend%\css\
mkdir %dstDir%\%backend%\css\images\
mkdir %dstDir%\%backend%\js\

:: WexflowTesting
xcopy ..\samples\WexflowTesting\* %dstDir%\WexflowTesting\ /s /e

:: Wexflow-dotnet-core
xcopy ..\samples\dotnet-core\windows\* %dstDir%\Wexflow-dotnet-core\ /s /e
copy ..\src\dotnet-core\Wexflow.Core\GlobalVariables.xml %dstDir%\Wexflow-dotnet-core\
copy ..\src\dotnet-core\Wexflow.Core\Wexflow.xml %dstDir%\Wexflow-dotnet-core\
copy ..\src\dotnet-core\Wexflow.Core\Workflow.xsd %dstDir%\Wexflow-dotnet-core\

:: Wexflow backend
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
copy "..\src\dotnet\Wexflow.Clients.Backend\js\history.min.js" %dstDir%\%backend%\js
copy "..\src\dotnet\Wexflow.Clients.Backend\js\users.min.js" %dstDir%\%backend%\js

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