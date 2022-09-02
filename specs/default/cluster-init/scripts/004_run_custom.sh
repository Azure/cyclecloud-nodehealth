#!/bin/bash
set -x
set -e

#sudo yum -y update
#sudo rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm
#sudo yum -y install dotnet-sdk-6.0
#CYCLECLOUD_SPEC_PATH=/mnt/cluster-init/healthcheck/default/
sudo -i
#chmod +x $CYCLECLOUD_SPEC_PATH/files/run-custom.sh
#sudo $CYCLECLOUD_SPEC_PATH/files/run-custom.sh

INSTALL_DIR=/opt/cycle/nodehealth
HCHECK_SETTINGS_PATH=$INSTALL_DIR/healthchecks.json
TEST_PATTERN=$(jq -r '.custom.pattern' ${HCHECK_SETTINGS_PATH})

for FILE in $INSTALL_DIR/custom-tests/$TEST_PATTERN
do
    sudo chmod +x $FILE || true
done
REPORT_PATH=$(jq -r '.report' ${HCHECK_SETTINGS_PATH})

#whereis jetpack
#exit 1


APPLICATIONINSIGHTS_CONNECTION_STRING=$(jq -r '.appinsights.ConnectString' ${HCHECK_SETTINGS_PATH})
INSTRUMENTATION_KEY=$(jq -r '.appinsights.InstrumentationKey' ${HCHECK_SETTINGS_PATH})
$INSTALL_DIR/linux-x64/hcheck -k $INSTALL_DIR/custom-tests --pt "$TEST_PATTERN" --append --rpath $REPORT_PATH
$INSTALL_DIR/linux-x64/hcheck --rpath $REPORT_PATH --fin --appin $INSTRUMENTATION_KEY --rscript $INSTALL_DIR/sbin/send_log
# |
# while IFS= read -r line
# do
# jetpack log --level error "$line";
# done
# exit ${PIPESTATUS[0]}
#exit $?