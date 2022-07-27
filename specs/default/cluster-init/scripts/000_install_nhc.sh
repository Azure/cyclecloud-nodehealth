#!/bin/bash
set -x
set -e

chmod +x $CYCLECLOUD_SPEC_PATH/files/install_nhc.sh
$CYCLECLOUD_SPEC_PATH/files/install_nhc.sh
