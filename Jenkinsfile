
pipeline {
    
    agent none

    environment {
        releaseNumber = 17
        outputEnc = '65001'
    }

    stages {
        stage('01. Windows Build') {
           agent { label 'windows' }

            steps {
                git 'https://github.com/EvilBeaver/OneScript.git'
                bat 'git submodule update --init --recursive'

                bat "chcp $outputEnc > nul\r\n\"${tool 'nuget'}\" restore src/1Script.sln"
                bat "chcp $outputEnc > nul\r\n\"${tool 'MSBuild'}\" BuildAll.csproj /p:Configuration=Release /p:Platform=x86 /p:ReleaseNumber=$releaseNumber /t:Build"
                
                stash includes: 'tests, install/build/**', name: 'buildResults'
           }

        }

        stage('02. Windows testing') {
            agent { label 'windows' }

            steps {
                unstash 'buildResults'
                bat "chcp $outputEnc > nul\r\n\"${tool 'MSBuild'}\" BuildAll.csproj /p:Configuration=Release /p:Platform=x86 /t:xUnitTest"

                junit 'tests/tests.xml'
            }
        }
        
        stage('03. Packaging') {
            agent { label 'windows' }

            steps {
                unstash 'buildResults'
                bat "chcp $outputEnc > nul\r\n\"${tool 'MSBuild'}\" BuildAll.csproj /p:Configuration=Release /p:Platform=x86 /p:ReleaseNumber=$releaseNumber /p:InnoSetupPath=\"${tool 'InnoSetup'}\" /t:CreateZip;CreateMSI;CreateNuget"
                archiveArtifacts artifacts: '**/dist/*.exe, **/dist/*.msi, **/dist/*.zip, **/dist/*.nupkg, **/tests/*.xml', fingerprint: true
            }
        }

    }
    
}