set dst=Wexflow.app
set dstDir=.\%dst%
set monoBundleDir=%dstDir%\Contents\MonoBundle
if exist %dstDir% rmdir /s /q %dstDir%
mkdir %dstDir%
xcopy ..\src\Wexflow.Clients.Eto.Manager\Wexflow.app %dstDir% /s /e
copy ..\src\Wexflow.Clients.Eto.Manager\bin\Debug\Eto.dll %monoBundleDir%
copy ..\src\Wexflow.Clients.Eto.Manager\bin\Debug\Eto.Mac.dll %monoBundleDir%
copy ..\src\Wexflow.Clients.Eto.Manager\bin\Debug\MonoMac.dll %monoBundleDir%
copy ..\src\Wexflow.Clients.Eto.Manager\bin\Debug\Newtonsoft.Json.dll %monoBundleDir%
copy ..\src\Wexflow.Clients.Eto.Manager\bin\Debug\Wexflow.Core.Service.Client.dll %monoBundleDir%
copy ..\src\Wexflow.Clients.Eto.Manager\bin\Debug\Wexflow.Core.Service.Contracts.dll %monoBundleDir%
copy ..\src\Wexflow.Clients.Eto.Manager\bin\Debug\Wexflow.Clients.Eto.Manager.exe %monoBundleDir%
copy ..\src\Wexflow.Clients.Eto.Manager\bin\Debug\Wexflow.Clients.Eto.Manager.exe.config %monoBundleDir%
copy ..\LICENSE.txt %monoBundleDir%
copy ..\VERSION.txt %monoBundleDir%