
pipeline {
    
    agent none

    environment {
        VersionPrefix = '2.0.0'
        VersionSuffix = 'rc.10'+"+${BUILD_NUMBER}"
        outputEnc = '65001'
    }

    stages {
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
                            
                            stash includes: 'built/**', name: 'buildResults'
                            stash includes: 'tests/native-api/bin*/*.dll', name: 'nativeApiTestsDll'
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
                            unstash 'nativeApiTestsDll'
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
                    
                    bat "chcp $outputEnc > nul\r\n\"${tool 'MSBuild'}\" Build.csproj /t:PackDistributions"
                    
                    archiveArtifacts artifacts: 'built/**', fingerprint: true
                    stash includes: 'built/**', name: 'dist'
                }
            }
        }

        stage ('Publishing night-build') {
            when { 
                anyOf {
                    branch 'develop';
                }
            }
            agent { label 'master' }
            options { skipDefaultCheckout() }

            steps {
                cleanWs()
                
                unstash 'dist'
                unstash 'vsix'

                publishRelease('night-build', false)
            }
        }

        stage ('Publishing preview') {
            when { 
                anyOf {
                    branch 'release/preview';
                }
            }
            agent { label 'master' }
            options { skipDefaultCheckout() }
            
            steps {
                cleanWs()
                checkout scm // чтобы получить файл release-notes
                unstash 'dist'
                unstash 'vsix'
                
                // Положит описание для сайта
                publishReleaseNotes('preview')
                
                // Положит файлы дистрибутива в целевую папку
                publishRelease('preview', true)
            }
        }
        
        stage ('Publishing artifacts to clouds') {
            when {
                anyOf { 
                    branch 'release/latest';
                    branch 'release/preview';
                } 
            }
            
            agent { label 'windows' }

            steps{
                
                unstash 'buildResults'
                
                withCredentials([string(credentialsId: 'NuGetToken', variable: 'NUGET_TOKEN')]) {
                    bat "chcp $outputEnc > nul\r\n\"${tool 'MSBuild'}\" Build.csproj /t:PublishNuget /p:NugetToken=$NUGET_TOKEN"
                }
            }
        }

        stage ('Publishing docker-images') {
            parallel {
                stage('Build v1') {
                    agent { label 'linux' }
                    when { 
                        anyOf {
                            branch 'release/latest'
                            expression { 
                                return env.TAG_NAME && env.TAG_NAME.startsWith('v1.')
                            }
                        }
                    }
                    steps {
                        script {
                            def codename = env.TAG_NAME ? env.TAG_NAME : 'latest'
                            publishDockerImage('v1', codename)
                        }
                    }
                }

                stage('Build v2') {
                    agent { label 'linux' }
                    when { 
                        anyOf {
                            branch 'develop'
                            expression { 
                                return env.TAG_NAME && env.TAG_NAME.startsWith('v2.')
                            }
                        }
                    }
                    steps {
                        script {
                            def codename = env.TAG_NAME ? env.TAG_NAME : 'dev'
                            publishDockerImage('v2', codename)
                        }
                    }
                }
            }
        }
    }
}

def publishRelease(codename, isNumbered) {
    dir('targetContent') {
        sh """
        ZIPS=../built
        NUGET=../built/nuget
        VSIX=../built/vscode
        mv \$ZIPS/*.zip ./
        mv \$VSIX/*.vsix ./
        
        TARGET="/var/www/oscript.io/download/versions/${codename}/"
        mkdir -p \$TARGET
        sudo rsync -rv --delete --exclude mddoc*.zip --exclude *.src.rpm . \$TARGET
        """.stripIndent()
        
        if (isNumbered) {
            def version="${env.VersionPrefix}-${env.VersionSuffix}".replaceAll("\\.", "_")
            
            sh """
            TARGET="/var/www/oscript.io/download/versions/${version}/"
            sudo mkdir -p \$TARGET
            sudo rsync -rv --delete --exclude mddoc*.zip --exclude *.src.rpm . \$TARGET
            """.stripIndent()
        }
    }
}

def publishReleaseNotes(codename) {
    dir('markdownContent') {
        def version="${env.VersionPrefix}-${env.VersionSuffix}".replaceAll("\\.", "_")
        def targetDir='/var/www/oscript.io/markdown/versions'
        
        sh """
        cp ../install/release-notes.md "./${codename}.md"
        cp ../install/release-notes.md "./${version}.md"
        
        sudo rsync -rv . ${targetDir}
        """.stripIndent()        
    }
}

def publishDockerImage(flavour, codename) {
    def imageName = "evilbeaver/onescript:${codename}"

    docker.build(
        imageName,
        "--load -f install/builders/base-image/Dockerfile_${flavour} ."
    ).push()
}

