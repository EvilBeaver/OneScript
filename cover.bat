echo "Run testrunner.os behind NUnit with OpenCover"
echo "generate OpneCover.xml to external services like Coverals.io"
echo "Create local report for contributor ;-)"

"%CD%\src\packages\OpenCover.4.6.519\tools\OpenCover.Console.exe" -output:"%CD%\src\OpenCover\opencover.xml" -target:./src/packages/NUnit.ConsoleRunner.3.5.0/tools/nunit3-console.exe -targetargs:"./src/NUnitTests/bin/Release/NUnitTests.dll" -register:user

"%CD%\src\packages\ReportGenerator.2.5.1\tools\ReportGenerator.exe" -reports:"%CD%\src\OpenCover\opencover.xml" -targetdir:"%CD%\src\OpenCover\CodeCovLocal"
