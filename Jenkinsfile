
pipeline {
    
    agent none

    environment {
        ReleaseNumber = 18
        outputEnc = '65001'
    }

    stages {
        stage('Windows Build') {
            agent { label 'windows' }

            // пути к инструментам доступны только когда
            // нода уже определена
            environment {
                NugetPath = "${tool 'nuget'}"
                OneScriptDocumenter = "${tool 'documenter'}"
            }

            steps {
                
                // в среде Multibranch Pipeline Jenkins первращает имена веток в папки
                // а для веток Gitflow вида release/* экранирует в слэш в %2F
                // При этом MSBuild, видя urlEncoding, разэкранирует его обратно, ломая путь (появляется слэш, где не надо)
                //
                // Поэтому, применяем костыль с кастомным workspace
                // см. https://issues.jenkins-ci.org/browse/JENKINS-34564
                
                ws("$workspace".replaceAll("%", "_"))
                {
                    checkout scm

                    bat 'set'
                    bat "chcp $outputEnc > nul\r\n\"${tool 'MSBuild'}\" BuildAll.csproj /p:Configuration=Release /p:Platform=x86 /t:Build"
                    
                    stash includes: 'tests, install/build/**, mddoc/**', name: 'buildResults'
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
                    sh 'cd install/build/vscode'
                    sh "${vsceBin} package"
                    archiveArtifacts artifacts: '*.vsix', fingerprint: true
                    sh 'cd -'
                }
            }
        }

        stage('Windows testing') {
            agent { label 'windows' }

            steps {
                ws("$workspace".replaceAll("%", "_"))
                {
                    dir('install/build'){
						deleteDir()
					}
					unstash 'buildResults'
                    bat "chcp $outputEnc > nul\r\n\"${tool 'MSBuild'}\" BuildAll.csproj /p:Configuration=Release /p:Platform=x86 /t:xUnitTest"

                    junit 'tests/tests.xml'
                }
            }
        }

        stage('Linux testing') {

            agent { label 'master' }

            steps {
                
				dir('install/build'){
					deleteDir()
				}
				
				unstash 'buildResults'

                sh '''\
                if [ ! -d lintests ]; then
                    mkdir lintests
                fi

                rm lintests/*.xml -f
                cd tests
                mono ../install/build/bin/oscript.exe testrunner.os -runall . xddReportPath ../lintests || true
                exit 0
                '''.stripIndent()

                junit 'lintests/*.xml'
                archiveArtifacts artifacts: 'lintests/*.xml', fingerprint: true
            }



        }
        
        stage('Packaging') {
            agent { label 'windows' }

            environment {
                NugetPath = "${tool 'nuget'}"
                InnoSetupPath = "${tool 'InnoSetup'}"
            }
            
            steps {
                ws("$workspace".replaceAll("%", "_"))
                {
                    dir('install/build'){
						deleteDir()
					}
					unstash 'buildResults'
                    //unstash 'sitedoc'
                    bat "chcp $outputEnc > nul\r\n\"${tool 'MSBuild'}\" BuildAll.csproj /p:Configuration=Release /p:Platform=x86 /t:CreateZip;CreateInstall;CreateNuget"
                    archiveArtifacts artifacts: '**/dist/*.exe, **/dist/*.msi, **/dist/*.zip, **/dist/*.nupkg', fingerprint: true
                    stash includes: 'dist/*.exe, **/dist/*.msi, **/dist/*.zip', name: 'winDist'
                }
            }
        }

        stage ('Packaging DEB and RPM') {
            agent { label 'master' }

            steps {

                dir('install/build'){
					deleteDir()
				}
				checkout scm
                unstash 'buildResults'

                sh '''
                cd install
                chmod +x prepare-build.sh
                chmod +x deb-build.sh
                chmod +x rpm-build.sh

                sh ./prepare-build.sh
                
                DISTPATH=`pwd`/build
                
                sh ./deb-build.sh $DISTPATH
                sh ./rpm-build.sh $DISTPATH
                '''.stripIndent()
                
                archiveArtifacts artifacts: 'output/*', fingerprint: true
                stash includes: 'output/*', name: 'linDist'
                
            }

        }

        stage ('Publishing night-build') {
            when { branch 'develop' }
            agent { label 'master' }

            steps {
                unstash 'winDist'
                unstash 'linDist'
                
                sh '''
                TARGET="/var/www/oscript.io/download/versions/night-build/"
                sudo rsync -rv --delete --exclude mddoc*.zip --exclude *.nuget dist/* $TARGET
                sudo rsync -rv --delete --exclude *.src.rpm output/* $TARGET
                '''.stripIndent()
            }
        }

    }
    
}