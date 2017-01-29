copy ..\..\src\Wexflow.Clients.Eto.Manager\bin\Debug\*.dll wexflow
copy ..\..\src\Wexflow.Clients.Eto.Manager\bin\Debug\Wexflow.Clients.Eto.Manager.exe wexflow
copy ..\..\src\Wexflow.Clients.Eto.Manager\bin\Debug\Wexflow.Clients.Eto.Manager.exe.config wexflow
7z.exe a -ttar -so wexflow.tar wexflow | 7z.exe a -si wexflow.tar.gz
