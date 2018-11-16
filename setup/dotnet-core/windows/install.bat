set WexflowTesting=C:\WexflowTesting
set Wexflow=C:\Wexflow-dotnet-core

xcopy WexflowTesting\* %WexflowTesting%\ /s /e
xcopy Wexflow-dotnet-core\* %Wexflow%\ /s /e

pause