set dst=wexflow
set dstDir=.\%dst%
set tgz=%dst%.tar.gz
set sh=%dstDir%\wexflow.sh
if exist %dstDir% rmdir /s /q %dstDir%
if exist %tgz% del %tgz%
mkdir %dstDir%
copy ..\src\Wexflow.Clients.Eto.Manager\bin\Debug\Eto.dll %dstDir%
copy ..\src\Wexflow.Clients.Eto.Manager\bin\Debug\Eto.Gtk2.dll %dstDir%
copy ..\src\Wexflow.Clients.Eto.Manager\bin\Debug\Newtonsoft.Json.dll %dstDir%
copy ..\src\Wexflow.Clients.Eto.Manager\bin\Debug\Wexflow.Core.Service.Client.dll %dstDir%
copy ..\src\Wexflow.Clients.Eto.Manager\bin\Debug\Wexflow.Core.Service.Contracts.dll %dstDir%
copy ..\src\Wexflow.Clients.Eto.Manager\bin\Debug\Wexflow.Clients.Eto.Manager.exe %dstDir%
copy ..\src\Wexflow.Clients.Eto.Manager\bin\Debug\Wexflow.Clients.Eto.Manager.exe.config %dstDir%
copy ..\LICENSE.txt %dstDir%
copy ..\VERSION.txt %dstDir%
@echo off
@echo #!/bin/bash> %sh%
@echo|set /p="mono /opt/wexflow/Wexflow.Clients.Eto.Manager.exe">> %sh%
dos2unix.exe %sh%
7z.exe a -ttar -so %dst%.tar %dstDir% | 7z.exe a -si %tgz%
rmdir /s /q %dstDir%