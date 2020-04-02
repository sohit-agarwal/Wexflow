::@echo off

set version=5.3
set dst=wexflow-%version%-windows-dotnet-core
set dstDir=.\%dst%
set backend=Backend

if exist %dstDir% rmdir /s /q %dstDir%
mkdir %dstDir%
mkdir %dstDir%\Wexflow-dotnet-core\
mkdir %dstDir%\Wexflow-dotnet-core\Database\
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
xcopy ..\samples\dotnet-core\windows\WexflowTesting\* %dstDir%\WexflowTesting\ /s /e

:: Wexflow-dotnet-core
xcopy ..\samples\dotnet-core\windows\Wexflow\* %dstDir%\Wexflow-dotnet-core\ /s /e
copy ..\src\dotnet-core\Wexflow.Core\GlobalVariables.xml %dstDir%\Wexflow-dotnet-core\
copy ..\src\dotnet-core\Wexflow.Core\Wexflow.xml %dstDir%\Wexflow-dotnet-core\
copy ..\src\dotnet-core\Wexflow.Core\Workflow.xsd %dstDir%\Wexflow-dotnet-core\

:: Wexflow backend
copy "..\src\backend\Wexflow.Backend\index.html" %dstDir%\%backend%\
copy "..\src\backend\Wexflow.Backend\forgot-password.html" %dstDir%\%backend%\
copy "..\src\backend\Wexflow.Backend\dashboard.html" %dstDir%\%backend%\
copy "..\src\backend\Wexflow.Backend\manager.html" %dstDir%\%backend%\
copy "..\src\backend\Wexflow.Backend\designer.html" %dstDir%\%backend%\
copy "..\src\backend\Wexflow.Backend\editor.html" %dstDir%\%backend%\
copy "..\src\backend\Wexflow.Backend\approval.html" %dstDir%\%backend%\
copy "..\src\backend\Wexflow.Backend\history.html" %dstDir%\%backend%\
copy "..\src\backend\Wexflow.Backend\users.html" %dstDir%\%backend%\
copy "..\src\backend\Wexflow.Backend\profiles.html" %dstDir%\%backend%\

xcopy "..\src\backend\Wexflow.Backend\images\*" %dstDir%\%backend%\images\ /s /e

xcopy "..\src\backend\Wexflow.Backend\assets\*" %dstDir%\%backend%\assets\ /s /e

xcopy "..\src\backend\Wexflow.Backend\css\images\*" %dstDir%\%backend%\css\images`\ /s /e
copy "..\src\backend\Wexflow.Backend\css\login.min.css" %dstDir%\%backend%\css
copy "..\src\backend\Wexflow.Backend\css\forgot-password.min.css" %dstDir%\%backend%\css
copy "..\src\backend\Wexflow.Backend\css\dashboard.min.css" %dstDir%\%backend%\css
copy "..\src\backend\Wexflow.Backend\css\manager.min.css" %dstDir%\%backend%\css
copy "..\src\backend\Wexflow.Backend\css\editor.min.css" %dstDir%\%backend%\css
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
copy "..\src\backend\Wexflow.Backend\js\worker-json.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\mode-json.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\ext-searchbox.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\ext-prompt.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\ext-keybinding_menu.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\ext-settings_menu.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\theme-*.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\editor.min.js" %dstDir%\%backend%\js

copy "..\src\backend\Wexflow.Backend\js\designer.min.js" %dstDir%\%backend%\js

copy "..\src\backend\Wexflow.Backend\js\approval.min.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\history.min.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\users.min.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\profiles.min.js" %dstDir%\%backend%\js

:: Wexflow server
dotnet publish ..\src\dotnet-core\Wexflow.Server\Wexflow.Server.csproj --force --output %~dp0\%dstDir%\Wexflow.Server
copy dotnet-core\windows\install.bat %dstDir%
copy dotnet-core\windows\run.bat %dstDir%

:: MongoDB script
dotnet publish ..\src\dotnet-core\Wexflow.Scripts.MongoDB\Wexflow.Scripts.MongoDB.csproj --force --output %~dp0\%dstDir%\Wexflow.Scripts.MongoDB
copy dotnet-core\windows\MongoDB\appsettings.json %dstDir%\Wexflow.Scripts.MongoDB
xcopy "..\samples\workflows\dotnet-core\windows\*" %dstDir%\Wexflow.Scripts.MongoDB\Workflows /s /e
copy dotnet-core\windows\install-MongoDB.bat %dstDir%

:: RavenDB script
dotnet publish ..\src\dotnet-core\Wexflow.Scripts.RavenDB\Wexflow.Scripts.RavenDB.csproj --force --output %~dp0\%dstDir%\Wexflow.Scripts.RavenDB
copy dotnet-core\windows\RavenDB\appsettings.json %dstDir%\Wexflow.Scripts.RavenDB
copy dotnet-core\windows\install-RavenDB.bat %dstDir%

:: CosmosDB script
dotnet publish ..\src\dotnet-core\Wexflow.Scripts.CosmosDB\Wexflow.Scripts.CosmosDB.csproj --force --output %~dp0\%dstDir%\Wexflow.Scripts.CosmosDB
copy dotnet-core\windows\CosmosDB\appsettings.json %dstDir%\Wexflow.Scripts.CosmosDB
copy dotnet-core\windows\install-CosmosDB.bat %dstDir%

:: PostgreSQL script
dotnet publish ..\src\dotnet-core\Wexflow.Scripts.PostgreSQL\Wexflow.Scripts.PostgreSQL.csproj --force --output %~dp0\%dstDir%\Wexflow.Scripts.PostgreSQL
copy dotnet-core\windows\PostgreSQL\appsettings.json %dstDir%\Wexflow.Scripts.PostgreSQL
copy dotnet-core\windows\install-PostgreSQL.bat %dstDir%

:: SQLServer script
dotnet publish ..\src\dotnet-core\Wexflow.Scripts.SQLServer\Wexflow.Scripts.SQLServer.csproj --force --output %~dp0\%dstDir%\Wexflow.Scripts.SQLServer
copy dotnet-core\windows\SQLServer\appsettings.json %dstDir%\Wexflow.Scripts.SQLServer
copy dotnet-core\windows\install-SQLServer.bat %dstDir%

:: MySQL script
dotnet publish ..\src\dotnet-core\Wexflow.Scripts.MySQL\Wexflow.Scripts.MySQL.csproj --force --output %~dp0\%dstDir%\Wexflow.Scripts.MySQL
copy dotnet-core\windows\MySQL\appsettings.json %dstDir%\Wexflow.Scripts.MySQL
copy dotnet-core\windows\install-MySQL.bat %dstDir%

:: SQLite script
dotnet publish ..\src\dotnet-core\Wexflow.Scripts.SQLite\Wexflow.Scripts.SQLite.csproj --force --output %~dp0\%dstDir%\Wexflow.Scripts.SQLite
copy dotnet-core\windows\SQLite\appsettings.json %dstDir%\Wexflow.Scripts.SQLite
copy dotnet-core\windows\install-SQLite.bat %dstDir%

:: Wexflow.Clients.CommandLine
dotnet publish ..\src\dotnet-core\Wexflow.Clients.CommandLine\Wexflow.Clients.CommandLine.csproj --force --output %~dp0\%dstDir%\Wexflow.Clients.CommandLine

:: License
:: copy ..\LICENSE.txt %dstDir%

:: compress
7z.exe a -tzip %dst%.zip %dstDir%

:: Cleanup
rmdir /s /q %dstDir%

pause