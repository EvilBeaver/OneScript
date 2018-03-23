# Usage:
#      ./build.sh                   to build
#      ./build.sh /t:Clean          to cleanup
#      ./build.sh /p:Platform="x86" to build for x86

cd `dirname $0`
nuget restore ../src/1Script_Mono.sln
msbuild /p:Platform="Any CPU" /p:Configuration="Release" $@ ../src/1Script_Mono.sln

