
pipeline {
    
    agent none

    stages {
        stage('01. Windows Build') {
           agent { label 'windows' }

           environment {
               releaseNumber = 17
           }

            steps {
                git 'https://github.com/EvilBeaver/OneScript.git'
                bat 'git submodule update --init --recursive'

                bat "chcp 65001 > nul\r\n\"${tool 'nuget'}\" restore src/1Script.sln"
                bat "chcp 65001 > nul\r\n\"${tool 'MSBuild'}\" BuildAll.csproj /p:Configuration=Release /p:Platform=x86 /p:ReleaseNumber=$releaseNumber /t:Build"
                bat "chcp 65001 > nul\r\n\"${tool 'MSBuild'}\" BuildAll.csproj /p:Configuration=Release /p:Platform=x86 /p:ReleaseNumber=$releaseNumber /t:xUnitTest"

                junit 'tests/tests.xml'
            
                bat "chcp 65001\r\n > nul\"${tool 'MSBuild'}\" BuildAll.csproj /p:Configuration=Release /p:Platform=x86 /p:ReleaseNumber=$releaseNumber /p:InnoSetupPath=\"${tool 'InnoSetup'}\" /t:CreateZip;CreateMSI;CreateNuget"
            
                archiveArtifacts artifacts: '**/dist/*.exe, **/dist/*.msi, **/dist/*.zip, **/dist/*.nupkg, **/tests/*.xml', fingerprint: true
           }

        }
    }
    
}