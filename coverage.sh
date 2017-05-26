#!/bin/bash

set -e

# Install OpenCover and ReportGenerator, and save the path to their executables.
nuget install -Verbosity quiet -OutputDirectory packages -Version 4.6.519 OpenCover
nuget install -Verbosity quiet -OutputDirectory packages -Version 2.4.5.0 ReportGenerator

OPENCOVER=$PWD/packages/OpenCover.4.6.519/tools/OpenCover.Console.exe
REPORTGENERATOR=$PWD/packages/ReportGenerator.2.4.5.0/tools/ReportGenerator.exe

CONFIG=Release
# Arguments to use for the build
DOTNET_BUILD_ARGS="-c $CONFIG"
# Arguments to use for the test
DOTNET_TEST_ARGS="$DOTNET_BUILD_ARGS"

echo CLI args: $DOTNET_BUILD_ARGS

echo Restoring

dotnet restore

echo Building

dotnet build $DOTNET_BUILD_ARGS

echo Testing

coverage=./coverage
rm -rf $coverage
mkdir $coverage

for testdir in test/*.UnitTests
do
  # For example the test project directory is test/Hystrix.Dotnet.UnitTests
  # We need to get the name of the project, to know what include in the OpenCover filter, so we have to extract the "Hystrix.Dotnet" part from the path string.
  project=`echo $testdir | cut -d/ -f2 | sed -e 's/\(\.UnitTests\)*$//g'`

  dirname=$(basename "$testdir")

  projectFile="${testdir}/${dirname}.csproj"

  echo "Executing unit tests in $testdir"
  dotnet test $DOTNET_TEST_ARGS --no-build $projectFile

  if [ "$project" = "Hystrix.Dotnet.AspNet" ]; then
	searchdirs=$testdir/bin/$CONFIG/net452
  else
	searchdirs=$testdir/bin/$CONFIG/netcoreapp1.1
  fi

  echo "Calculating coverage with OpenCover for $project"
  $OPENCOVER \
    -target:"c:\Program Files\dotnet\dotnet.exe" \
    -targetargs:"test $DOTNET_TEST_ARGS --no-build $projectFile" \
    -mergeoutput \
    -hideskipped:File \
    -output:$coverage/coverage.xml \
    -oldStyle \
    -filter:"+[$project*]* -[Hystrix.*Tests*]*" \
    -excludebyfile:*Startup.cs \
    -searchdirs:$searchdirs \
    -register:user
done

echo "Generating HTML report"
$REPORTGENERATOR \
  -reports:$coverage/coverage.xml \
  -targetdir:$coverage \
  -verbosity:Error