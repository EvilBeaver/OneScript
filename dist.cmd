@echo Full distr and tests
@echo add MSBuild 12 to your path

MSBuild.exe ./BuildAll.csproj
MSBuild.exe ./BuildAll.csproj /t:CreateZipForUpdateDll
MSBuild.exe ./BuildAll.csproj /t:xUnitTest