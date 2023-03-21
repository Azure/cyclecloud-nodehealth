#!/usr/bin/env bash

set -x
set -e
SCRIPT=$(realpath "$0")
SCRIPTPATH=$(dirname "$SCRIPT")
cd $SCRIPTPATH
VERSION=$(cyclecloud project info | grep Version | cut -d: -f2 | cut -d" " -f2)
DEST_FILE=$(pwd)/blobs/hcheck-linux-$VERSION.tgz
rm -f $DEST_FILE
cd ./hcheck/hcheck/
dotnet clean
dotnet restore
dotnet build --use-current-runtime
#cd ./healthcheck
cd ./bin/Debug/net6.0/
cp ../../../src/send_log linux-x64
tar czf hcheck-linux-$VERSION.tgz linux-x64
cp hcheck-linux-$VERSION.tgz ../../../../../blobs/ 
cd $SCRIPTPATH/
echo \#!/usr/bin/env bash > ./specs/default/cluster-init/files/version.sh
echo export HEALTHCHECK_VERSION=$VERSION >> ./specs/default/cluster-init/files/version.sh
cyclecloud project upload "Cycle Kepler-storage"
