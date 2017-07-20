echo "dont forget pip install codecov"
echo "and dont forget set CODECOV_TOKEN"

codecov -f $(pwd)/src/OpenCover/opencover.xml
