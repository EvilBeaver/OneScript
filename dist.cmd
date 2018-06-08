@echo Full distr and tests
@echo add MSBuild 15 to your path

MSBuild.exe ./Build.csproj
MSBuild.exe ./Build.csproj /t:CreateZipForUpdateDll
MSBuild.exe ./Build.csproj /t:xUnitTest