
pipeline {
    
    agent none

    environment {
        ReleaseNumber = '1.7.0'
        outputEnc = '65001'
    }

    stages {
        stage('Build'){
            parallel {
                stage('Prepare Linux Environment') {
                    agent{ label 'master'}
                    steps{
                        dir('install'){
                            sh 'chmod +x make-dockers.sh && ./make-dockers.sh'
                        }
                        withCredentials([usernamePassword(credentialsId: 'docker-hub', passwordVariable: 'dockerpassword', usernameVariable: 'dockeruser')]) {
                            sh """
                            docker login -p $dockerpassword -u $dockeruser
                            docker push oscript/onescript-builder:deb
                            docker push oscript/onescript-builder:rpm
                            """.stripIndent()
                        }
                    }
                }

                stage('Windows Build') {
                    agent { label 'windows' }

                    // пути к инструментам доступны только когда
                    // нода уже определена
                    environment {
                        NugetPath = "${tool 'nuget'}"
                        StandardLibraryPacks = "${tool 'os_stdlib'}"
                    }

                    steps {
                        
                        // в среде Multibranch Pipeline Jenkins первращает имена веток в папки
                        // а для веток Gitflow вида release/* экранирует в слэш в %2F
                        // При этом MSBuild, видя urlEncoding, разэкранирует его обратно, ломая путь (появляется слэш, где не надо)
                        //
                        // Поэтому, применяем костыль с кастомным workspace
                        // см. https://issues.jenkins-ci.org/browse/JENKINS-34564
                        //
                        // А еще Jenkins под Windows постоянно добавляет в конец папки какую-то мусорную строку.
                        // Для этого отсекаем все, что находится после последнего дефиса
                        // см. https://issues.jenkins-ci.org/browse/JENKINS-40072
                        
                        ws(env.WORKSPACE.replaceAll("%", "_").replaceAll(/(-[^-]+$)/, ""))
                        {
                            step([$class: 'WsCleanup'])
                            checkout scm

                            bat "chcp $outputEnc > nul\r\n\"${tool 'MSBuild'}\" src/1Script.sln /t:restore && mkdir doctool"
                            bat "chcp $outputEnc > nul\r\n dotnet publish src/OneScriptDocumenter/OneScriptDocumenter.csproj -c Release -o doctool"
                            bat "chcp $outputEnc > nul\r\n\"${tool 'MSBuild'}\" Build_Core.csproj /t:CleanAll;PrepareDistributionFiles"
                            
                            stash includes: 'tests, built/**', name: 'buildResults'
                        }
                    }
                }
            }
        }
        stage('VSCode debugger Build') {
            agent {
                docker {
                    image 'node'
                    label 'linux'
                }
            }

            steps {
                unstash 'buildResults'
                sh 'npm install vsce'
                script {
                    def vsceBin = pwd() + "/node_modules/.bin/vsce"
                    sh "cd built/vscode && ${vsceBin} package"
                    archiveArtifacts artifacts: 'built/vscode/*.vsix', fingerprint: true
                    stash includes: 'built/vscode/*.vsix', name: 'vsix' 
                }
            }
        }

        stage('Testing'){
            parallel{
                stage('Windows testing') {
                    agent { label 'windows' }
					environment {
                        OSCRIPT_CONFIG = 'systemlanguage=ru'
                    }
                    steps {
                        ws(env.WORKSPACE.replaceAll("%", "_").replaceAll(/(-[^-]+$)/, ""))
                        {
                            dir('install/build'){
                                deleteDir()
                            }
                            unstash 'buildResults'
                            bat "chcp $outputEnc > nul\r\n\"${tool 'MSBuild'}\" Build_Core.csproj /t:Test"

                            junit 'tests/tests.xml'
                        }
                    }
                }

                stage('Linux testing') {
						agent{ 
                            docker {
                                image 'mcr.microsoft.com/dotnet/sdk:5.0'
                                label 'linux' 
                            }
                        }
                    }

                    steps {
                        
                        dir('built'){
                            deleteDir()
                        }
                        
                        unstash 'buildResults'

                        sh '''\
                        if [ ! -d lintests ]; then
                            mkdir lintests
                        fi
                        rm lintests/*.xml -f
                        cd tests
                        dotnet ../built/linux-x64/bin/oscript.dll testrunner.os -runall . xddReportPath ../lintests || true
                        exit 0
                        '''.stripIndent()

                        junit 'lintests/*.xml'
                        archiveArtifacts artifacts: 'lintests/*.xml', fingerprint: true
                    }
                }
            }
        }
        
        stage('Packaging') {
            parallel {
                stage('Zip distribution'){
                    agent { label 'windows' }

                    steps {
                        ws(env.WORKSPACE.replaceAll("%", "_").replaceAll(/(-[^-]+$)/, ""))
                        {
                            dir('built'){
                                deleteDir()
                            }
                            
                            unstash 'buildResults'
                            script
                            {
                                if (env.BRANCH_NAME == "preview") {
                                    echo 'Building preview'
                                    bat "chcp $outputEnc > nul\r\n\"${tool 'MSBuild'}\" Build_Core.csproj /t:PackDistributions /p:Suffix=-pre%BUILD_NUMBER%"
                                }
                                else{
                                    bat "chcp $outputEnc > nul\r\n\"${tool 'MSBuild'}\" Build_Core.csproj /t:PackDistributions"
                                }
                            }
                            archiveArtifacts artifacts: 'built/**', fingerprint: true
                            stash includes: 'built/**', name: 'winDist'
                        }
                    }
                }
            }
        }
    }
    
}