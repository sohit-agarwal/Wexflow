::@echo off

set version=4.9
set dst=wexflow
set zip=wexflow-%version%-linux-dotnet-core.zip
set dstDir=.\%dst%
set backend=Backend

if exist %zip% del %zip%
if exist %dstDir% rmdir /s /q %dstDir%
mkdir %dstDir%
mkdir %dstDir%\Wexflow\
mkdir %dstDir%\Wexflow\Database\
mkdir %dstDir%\WexflowTesting\
mkdir %dstDir%\%backend%\
mkdir %dstDir%\%backend%\images\
mkdir %dstDir%\%backend%\css\
mkdir %dstDir%\%backend%\css\images\
mkdir %dstDir%\%backend%\js\
mkdir %dstDir%\Wexflow.Scripts.MongoDB
mkdir %dstDir%\Wexflow.Scripts.MongoDB\Workflows
mkdir %dstDir%\Wexflow.Scripts.RavenDB
mkdir %dstDir%\Wexflow.Scripts.CosmosDB

:: WexflowTesting
xcopy ..\samples\WexflowTesting\* %dstDir%\WexflowTesting\ /s /e
xcopy ..\samples\dotnet-core\linux\WexflowTesting\* %dstDir%\WexflowTesting\ /s /e

:: Wexflow
xcopy ..\samples\dotnet-core\linux\Wexflow\* %dstDir%\Wexflow\ /s /e
copy ..\src\dotnet-core\Wexflow.Core\Workflow.xsd %dstDir%\Wexflow\

:: Wexflow backend
copy "..\src\backend\Wexflow.Backend\index.html" %dstDir%\%backend%\
copy "..\src\backend\Wexflow.Backend\forgot-password.html" %dstDir%\%backend%\
copy "..\src\backend\Wexflow.Backend\dashboard.html" %dstDir%\%backend%\
copy "..\src\backend\Wexflow.Backend\manager.html" %dstDir%\%backend%\
copy "..\src\backend\Wexflow.Backend\designer.html" %dstDir%\%backend%\
copy "..\src\backend\Wexflow.Backend\approval.html" %dstDir%\%backend%\
copy "..\src\backend\Wexflow.Backend\history.html" %dstDir%\%backend%\
copy "..\src\backend\Wexflow.Backend\users.html" %dstDir%\%backend%\
copy "..\src\backend\Wexflow.Backend\profiles.html" %dstDir%\%backend%\

xcopy "..\src\backend\Wexflow.Backend\images\*" %dstDir%\%backend%\images\ /s /e

xcopy "..\src\backend\Wexflow.Backend\css\images\*" %dstDir%\%backend%\css\images\ /s /e
copy "..\src\backend\Wexflow.Backend\css\login.min.css" %dstDir%\%backend%\css
copy "..\src\backend\Wexflow.Backend\css\forgot-password.min.css" %dstDir%\%backend%\css
copy "..\src\backend\Wexflow.Backend\css\dashboard.min.css" %dstDir%\%backend%\css
copy "..\src\backend\Wexflow.Backend\css\manager.min.css" %dstDir%\%backend%\css
copy "..\src\backend\Wexflow.Backend\css\designer.min.css" %dstDir%\%backend%\css
copy "..\src\backend\Wexflow.Backend\css\approval.min.css" %dstDir%\%backend%\css
copy "..\src\backend\Wexflow.Backend\css\history.min.css" %dstDir%\%backend%\css
copy "..\src\backend\Wexflow.Backend\css\users.min.css" %dstDir%\%backend%\css
copy "..\src\backend\Wexflow.Backend\css\profiles.min.css" %dstDir%\%backend%\css

copy "..\src\backend\Wexflow.Backend\js\settings.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\login.min.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\forgot-password.min.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\dashboard.min.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\manager.min.js" %dstDir%\%backend%\js

copy "..\src\backend\Wexflow.Backend\js\ace.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\worker-xml.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\mode-xml.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\ext-searchbox.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\ext-prompt.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\ext-keybinding_menu.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\ext-settings_menu.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\theme-*.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\designer.min.js" %dstDir%\%backend%\js
::copy "..\src\backend\Wexflow.Backend\js\common.js" %dstDir%\%backend%\js
::copy "..\src\backend\Wexflow.Backend\js\authenticate.js" %dstDir%\%backend%\js
::copy "..\src\backend\Wexflow.Backend\js\cytoscape-dagre.min.js" %dstDir%\%backend%\js
::copy "..\src\backend\Wexflow.Backend\js\highlight.pack.js" %dstDir%\%backend%\js
::copy "..\src\backend\Wexflow.Backend\js\designer.js" %dstDir%\%backend%\js

copy "..\src\backend\Wexflow.Backend\js\approval.min.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\history.min.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\users.min.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\profiles.min.js" %dstDir%\%backend%\js

:: Wexflow server
dotnet publish ..\src\dotnet-core\Wexflow.Server\Wexflow.Server.csproj --force --output %~dp0\%dstDir%\Wexflow.Server
copy dotnet-core\linux\appsettings.json %dstDir%\Wexflow.Server

:: MongoDB script
dotnet publish ..\src\dotnet-core\Wexflow.Scripts.MongoDB\Wexflow.Scripts.MongoDB.csproj --force --output %~dp0\%dstDir%\Wexflow.Scripts.MongoDB
copy dotnet-core\linux\MongoDB\appsettings.json %dstDir%\Wexflow.Scripts.MongoDB
xcopy "..\samples\workflows\dotnet-core\linux\*" %dstDir%\Wexflow.Scripts.MongoDB\Workflows /s /e

:: RavenDB script
dotnet publish ..\src\dotnet-core\Wexflow.Scripts.RavenDB\Wexflow.Scripts.RavenDB.csproj --force --output %~dp0\%dstDir%\Wexflow.Scripts.RavenDB
copy dotnet-core\linux\RavenDB\appsettings.json %dstDir%\Wexflow.Scripts.RavenDB

:: CosmosDB script
dotnet publish ..\src\dotnet-core\Wexflow.Scripts.CosmosDB\Wexflow.Scripts.CosmosDB.csproj --force --output %~dp0\%dstDir%\Wexflow.Scripts.CosmosDB
copy dotnet-core\linux\CosmosDB\appsettings.json %dstDir%\Wexflow.Scripts.CosmosDB

:: Wexflow.Clients.CommandLine
dotnet publish ..\src\dotnet-core\Wexflow.Clients.CommandLine\Wexflow.Clients.CommandLine.csproj --force --output %~dp0\%dstDir%\Wexflow.Clients.CommandLine

:: License
:: copy ..\LICENSE.txt %dstDir%

:: compress
7z.exe a -tzip %zip% %dstDir%

:: Cleanup
rmdir /s /q %dstDir%

pause