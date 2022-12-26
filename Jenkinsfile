
pipeline {
    
    agent none

    environment {
        ReleaseNumber = '1.8.2'
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
                            docker push oscript/onescript-builder:gcc
							
							# TODO: Немного через жопу собираются .so для NativеApi
							# при сборке образа, а не при его запуске
                            docker create --name gcc-$BUILD_NUMBER oscript/onescript-builder:gcc
                            docker cp gcc-$BUILD_NUMBER:/built .
                            docker rm gcc-$BUILD_NUMBER
                            """.stripIndent()
                        }
                        script {
                            stash includes: 'built/** ', name: 'builtNativeApi'
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
                            bat "chcp $outputEnc > nul\r\n\"${tool 'MSBuild'}\" Build.csproj /t:CleanAll;PrepareDistributionContent /p:OneScriptDocumenter=\"%WORKSPACE%/doctool/OneScriptDocumenter.exe\""
                            
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

                    steps {
                        ws(env.WORKSPACE.replaceAll("%", "_").replaceAll(/(-[^-]+$)/, ""))
                        {
                            dir('install/build'){
                                deleteDir()
                            }
                            unstash 'buildResults'
                            bat "chcp $outputEnc > nul\r\n\"${tool 'MSBuild'}\" Build.csproj /t:xUnitTest"

                            junit 'tests/tests.xml'
                        }
                    }
                }

                stage('Linux testing') {
                    agent{
                        docker{
                            image 'evilbeaver/mono-ru:5.4'
                            label 'master'
                        }
                    }

                    steps {
                        
                        dir('install/build'){
                            deleteDir()
                        }
                        
                        unstash 'buildResults'
                        unstash 'builtNativeApi'

                        sh '''\
                        if [ ! -d lintests ]; then
                            mkdir lintests
                        fi
                        rm lintests/*.xml -f
                        cd tests
                        mono ../built/tmp/bin/oscript.exe testrunner.os -runall . xddReportPath ../lintests || true
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
                stage('Windows distribution'){
                    agent { label 'windows' }

                    environment {
                        InnoSetupPath = "${tool 'InnoSetup'}"
                    }
                    
                    steps {
                        ws(env.WORKSPACE.replaceAll("%", "_").replaceAll(/(-[^-]+$)/, ""))
                        {
                            dir('built'){
                                deleteDir()
                            }
                            
                            unstash 'buildResults'
                            unstash 'builtNativeApi'
                            script
                            {
                                if (env.BRANCH_NAME == "preview") {
                                    echo 'Building preview'
                                    bat "chcp $outputEnc > nul\r\n\"${tool 'MSBuild'}\" Build.csproj /t:CreateDistributions /p:Suffix=-pre%BUILD_NUMBER%"
                                }
                                else{
                                    bat "chcp $outputEnc > nul\r\n\"${tool 'MSBuild'}\" Build.csproj /t:CreateDistributions"
                                }
                            }
                            archiveArtifacts artifacts: 'built/**', fingerprint: true
                            stash includes: 'built/**', name: 'winDist'
                        }
                    }
                }

                stage('DEB distribution') {
                    agent { 
                        docker {
                            image 'oscript/onescript-builder:deb'
                            label 'master' 
                        }
                    }

                    steps {
                        unstash 'buildResults'
                        unstash 'builtNativeApi'
                        sh '/bld/build.sh'
                        archiveArtifacts artifacts: 'out/deb/*', fingerprint: true
                        stash includes: 'out/deb/*', name: 'debian'
                    }
                }

                stage('RPM distribution') {
                    agent { 
                        docker {
                            image 'oscript/onescript-builder:rpm'
                            label 'master' 
                        }
                    }

                    steps {
                        unstash 'buildResults'
                        unstash 'builtNativeApi'
                        sh '/bld/build.sh'
                        archiveArtifacts artifacts: 'out/rpm/*', fingerprint: true
                        stash includes: 'out/rpm/*', name: 'redhat'
                    }
                }
            }
        }

        stage ('Publishing revision') {
            when { anyOf {
				// TODO сделать автовычисление маркера lts или latest и согласовать его с путём к папке на стр. 250 (TARGET=..._)
				branch 'release/latest'
				}
			}
			
            agent { label 'master' }

            steps {
                
                unstash 'winDist'
                unstash 'debian'
                unstash 'redhat'
                unstash 'vsix'

                dir('targetContent') {
                    sh '''
                    WIN=../built
                    DEB=../out/deb
                    RPM=../out/rpm
                    mkdir x64
                    mv $WIN/OneScript*-x64*.exe x64/
                    mv $WIN/OneScript*-x64*.zip x64/
                    mv $WIN/vscode/*.vsix x64/
                    mv $WIN/OneScript*-x86*.exe ./
                    mv $WIN/OneScript*-x86*.zip ./
                    mv $RPM/*.rpm x64/
                    mv $DEB/*.deb x64/
                    TARGET="/var/www/oscript.io/download/versions/latest/"
                    sudo rsync -rv --delete --exclude mddoc*.zip --exclude *.src.rpm . $TARGET
                    '''.stripIndent()
                }
            }
        }
    }
    
}