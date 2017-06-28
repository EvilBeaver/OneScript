
pipeline {
    
    agent none

    environment {
        ReleaseNumber = 17
        outputEnc = '65001'
    }

    stages {
        stage('Windows Build') {
            agent { label 'windows' }

            // пути к инструментам доступны только когда
            // нода уже определена
            environment {
                NugetPath = "${tool 'nuget'}"
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

                    bat "chcp $outputEnc > nul\r\n\"${tool 'MSBuild'}\" BuildAll.csproj /p:Configuration=Release /p:Platform=x86 /t:Build"
                    
                    stash includes: 'tests, install/build/**', name: 'buildResults'
                }
           }

        }

        stage('Windows testing') {
            agent { label 'windows' }

            steps {
                ws("$workspace".replaceAll("%", "_"))
                {
                    unstash 'buildResults'
                    bat "chcp $outputEnc > nul\r\n\"${tool 'MSBuild'}\" BuildAll.csproj /p:Configuration=Release /p:Platform=x86 /t:xUnitTest"

                    junit 'tests/tests.xml'
                }
            }
        }

        stage('Linux testing') {

            agent { label 'master' }

            steps {
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
                    unstash 'buildResults'
                    bat "chcp $outputEnc > nul\r\n\"${tool 'MSBuild'}\" BuildAll.csproj /p:Configuration=Release /p:Platform=x86 /t:CreateZip;CreateNuget"
                    archiveArtifacts artifacts: '**/dist/*.exe, **/dist/*.msi, **/dist/*.zip, **/dist/*.nupkg, **/tests/*.xml', fingerprint: true
                }
            }
        }

        stage ('Packaging DEB and RPM') {
            agent { label 'master' }

            steps {

                checkout scm
                unstash 'buildResults'

                sh 'prepare-build.sh'

                echo 'Building DEB'
                sh '''\
                cd install
                chmod +x deb-build.sh
                DISTPATH=`pwd`/build
                
                sh ./deb-build.sh $DISTPATH
                '''.stripIndent()

                echo 'Building RPM'
                sh '''\
                cd install
                chmod +x rpm-build.sh
                DISTPATH=`pwd`/build
                TMPDIR=oscript-tmp

                if [ -d "$TMPDIR" ] ; then
                    rm -rf $TMPDIR
                fi

                mkdir $TMPDIR

                cp -r $DISTPATH/* $TMPDIR
                ./rpm-build.sh $TMPDIR

                TARGET=$WORKSPACE/output

                if [ ! -d "$TARGET" ] ; then
                    mkdir $TARGET
                fi

                cp $TMPDIR/bin/*.rpm $TARGET

                rm -rf $TMPDIR
                '''.stripIndent()

                archiveArtifacts artifacts: 'output/*', fingerprint: true

            }

        }

    }
    
}