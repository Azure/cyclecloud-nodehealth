#!/usr/bin/env bash

set -x
set -e

cd $( dirname $0 )
cd ./hcheck/hcheck/
dotnet build -r linux-x64 --self-contained
cd $( dirname $0 )
#cd ./healthcheck
VERSION=$(cyclecloud project info | grep Version | cut -d: -f2 | cut -d" " -f2)
DEST_FILE=$(pwd)/blobs/hcheck-linux-$VERSION.tgz
rm -f $DEST_FILE
cd ./hcheck/hcheck/bin/Debug/net6.0/
cp ../../../src/send_log ./linux-x64
tar czf $DEST_FILE ./linux-x64
cd $( dirname $0 )/
echo \#!/usr/bin/env bash > ./specs/default/cluster-init/files/version.sh
echo export HEALTHCHECK_VERSION=$VERSION >> ./specs/default/cluster-init/files/version.sh
#cyclecloud project upload azure-storage
