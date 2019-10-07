set WexflowTesting=C:\WexflowTesting
set Wexflow=C:\Wexflow-dotnet-core

xcopy WexflowTesting\* %WexflowTesting%\ /s /e
del %Wexflow%\Database\Wexflow.db
xcopy Wexflow-dotnet-core\* %Wexflow%\ /s /e

pause