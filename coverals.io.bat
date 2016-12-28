echo "Publish coverals io info"
echo "dont forget set secure repo token ;-) COVERALLS_REPO_TOKEN"
echo "dont forget use branch and pull request params"

"%CD%\src\packages\coveralls.net.0.7.0\tools\csmacnz.Coveralls.exe" --useRelativePaths --commitBranch %BRANCH% --opencover -i %CD%\src\OpenCover\opencover.xml
