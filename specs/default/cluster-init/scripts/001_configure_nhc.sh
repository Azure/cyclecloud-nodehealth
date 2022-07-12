#!/bin/bash
set -x
#set -e

sudo -i

platform_family=$(jetpack config platform_family)

if [ $platform_family == "ubuntu" ]; then 
    apt install -y jq
fi

if [ $platform_family == "rhel" ]; then 
    yum install -y jq
fi

jetpack config healthchecks --json > $CYCLECLOUD_SPEC_PATH/files/healthchecks.json
chmod +x $CYCLECLOUD_SPEC_PATH/files/configure_nhc.sh
$CYCLECLOUD_SPEC_PATH/files/configure_nhc.sh
