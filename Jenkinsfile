
pipeline {
    
    agent none

    environment {
        VersionPrefix = '2.0.0'
        VersionSuffix = 'rc4'+"-${(long)(currentBuild.startTimeInMillis/60000)}"
        outputEnc = '65001'
    }

    stages {
        //stage('Prepare Linux Environment') {
        //    agent{ label 'master'}
        //    steps{
        //        dir('install'){
        //            sh 'chmod +x make-dockers.sh && ./make-dockers.sh'
        //        }
        //        withCredentials([usernamePassword(credentialsId: 'docker-hub', passwordVariable: 'dockerpassword', usernameVariable: 'dockeruser')]) {
        //            sh """
        //            docker login -p $dockerpassword -u $dockeruser
        //            docker push oscript/onescript-builder:deb
        //            docker push oscript/onescript-builder:rpm
        //            docker push oscript/onescript-builder:gcc
        //            """.stripIndent()
        //        }
        //    }
        //}
        stage('Build'){
            parallel {
                stage('Windows Build') {
                    agent { label 'windows' }

                    options { skipDefaultCheckout() }

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
                            bat "chcp $outputEnc > nul\r\n\"${tool 'MSBuild'}\" Build.csproj /t:CleanAll;PrepareDistributionFiles;CreateNuget"
                            
                            stash includes: 'tests, built/**', name: 'buildResults'
                        }
                    }
                }
                
                stage('Linux Build') {
                    agent {
                        docker {
                            image 'oscript/onescript-builder:gcc'
                            label 'linux'
                        }
                    }
                    
                    steps {
                        sh 'mkdir -p built/tmp/na-proxy && mkdir -p built/tmp/na-tests'
                        dir('src/ScriptEngine.NativeApi') {
                            sh './build.sh'
                            sh 'cp *.so ../../built/tmp/na-proxy'
                        }
                        dir('tests/native-api') {
                            sh './build.sh'
                            sh 'cp *.so ../../built/tmp/na-tests'
                        }
                        dir('output') {
                            sh 'cp -Rv ../built/tmp/* .'
                        }
                        stash includes: 'output/na-proxy/*.so', name: 'nativeApiSo'
                        stash includes: 'output/na-tests/*.so', name: 'nativeApiTestsSo'
                    }
                }
            }
        }
        stage('VSCode debugger Build') {
            agent {
                docker {
                    image 'node:lts-alpine3.20'
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
                    options { skipDefaultCheckout() }
                    environment {
                        OSCRIPT_CONFIG = 'systemlanguage=ru'
                    }
                    steps {
                        ws(env.WORKSPACE.replaceAll("%", "_").replaceAll(/(-[^-]+$)/, ""))
                        {
                            step([$class: 'WsCleanup'])
                            checkout scm
                            
                            dir('install/build'){
                                deleteDir()
                            }
                            unstash 'buildResults'
                            bat "chcp $outputEnc > nul\r\n\"${tool 'MSBuild'}\" Build.csproj /t:Test"

                            junit 'tests/*.xml'
                        }
                    }
                }

                stage('Linux testing') {
                    agent{ 
                        docker {
                            image 'mcr.microsoft.com/dotnet/sdk:6.0'
                            label 'linux' 
                        }
                    }
                    environment {
                        OSCRIPT_CONFIG = 'systemlanguage=ru'
                    }

                    steps {
                        
                        dir('built'){
                            deleteDir()
                        }
                        
                        unstash 'buildResults'
                        unstash 'nativeApiSo'
                        unstash 'nativeApiTestsSo'
                        
                        sh 'cp output/na-proxy/*.so ./built/linux-x64/bin/'
                        sh 'mkdir -p tests/native-api/build64 && cp output/na-tests/*.so ./tests/native-api/build64/'

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
            agent { label 'windows' }

            options { skipDefaultCheckout() }

            steps {
                ws(env.WORKSPACE.replaceAll("%", "_").replaceAll(/(-[^-]+$)/, ""))
                {
                    step([$class: 'WsCleanup'])
                    checkout scm
                    
                    dir('built'){
                        deleteDir()
                    }
                    
                    unstash 'buildResults'
                    unstash 'nativeApiSo'
                    
                    bat '''
                    chcp 65001 > nul
                    dir output\\na-proxy
                    xcopy output\\na-proxy\\*64.so built\\linux-x64\\bin\\ /F
                    '''.stripIndent()
                    
                    script
                    {
                        if (env.BRANCH_NAME == "preview") {
                            echo 'Building preview'
                            bat "chcp $outputEnc > nul\r\n\"${tool 'MSBuild'}\" Build.csproj /t:PackDistributions /p:Suffix=-pre%BUILD_NUMBER%"
                        }
                        else{
                            bat "chcp $outputEnc > nul\r\n\"${tool 'MSBuild'}\" Build.csproj /t:PackDistributions"
                        }
                    }
                    archiveArtifacts artifacts: 'built/**', fingerprint: true
                    stash includes: 'built/**', name: 'dist'
                }
            }
        }

        stage ('Publishing night-build') {
            when { anyOf {
                branch 'develop';
                }
            }
            agent { label 'master' }

            steps {
                cleanWs()
                unstash 'dist'
                unstash 'vsix'

                dir('targetContent') {
                    sh '''
                    ZIPS=../built
                    NUGET=../built/nuget
                    VSIX=../built/vscode
                    mv $ZIPS/*.zip ./
                    mv $VSIX/*.vsix ./
                    
                    TARGET="/var/www/oscript.io/download/versions/night-build/"
                    sudo rsync -rv --delete --exclude mddoc*.zip --exclude *.src.rpm . $TARGET
                    '''.stripIndent()
                }
            }
        }

        stage ('Publishing preview') {
            when { anyOf {
                branch 'release/preview';
                }
            }
            agent { label 'master' }

            steps {
                
                unstash 'dist'
                unstash 'vsix'

                dir('targetContent') {
                    sh '''
                    ZIPS=../built
                    NUGET=../built/nuget
                    VSIX=../built/vscode
                    mv $ZIPS/*.zip ./
                    mv $VSIX/*.vsix ./
                    
                    TARGET="/var/www/oscript.io/download/versions/preview/"
                    sudo rsync -rv --delete --exclude mddoc*.zip --exclude *.src.rpm . $TARGET
                    '''.stripIndent()
                }
            }
        }

        stage ('Publishing latest') {
            when { anyOf {
                branch 'latest';
                }
            }
            agent { label 'master' }

            steps {
                
                unstash 'dist'
                unstash 'vsix'

                dir('targetContent') {
                    sh '''
                    ZIPS=../built
                    NUGET=../built/nuget
                    VSIX=../built/vscode
                    mv $ZIPS/*.zip ./
                    mv $VSIX/*.vsix ./
                    
                    TARGET="/var/www/oscript.io/download/versions/latest/"
                    sudo rsync -rv --delete --exclude mddoc*.zip --exclude *.src.rpm . $TARGET
                    '''.stripIndent()
                }
            }
        }
    }
}
