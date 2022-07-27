#!/bin/bash
set -x
set -e

source $CYCLECLOUD_SPEC_PATH/files/version.sh



#sudo yum -y update
#sudo rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm
#sudo yum -y install dotnet-sdk-6.0
#CYCLECLOUD_SPEC_PATH=/mnt/cluster-init/healthcheck/default/
platform_family=$(jetpack config platform_family)

if [ $platform_family == "ubuntu" ]; then 
    apt install -y libicu
fi

if [ $platform_family == "rhel" ]; then 
    yum install -y libicu
fi
sudo -i
mkdir -p /opt/cycle/nodehealth
INSTALL_DIR=/opt/cycle/nodehealth
cp -R $CYCLECLOUD_SPEC_PATH/files/* $INSTALL_DIR


jetpack download hcheck-linux-$HEALTHCHECK_VERSION.tgz --project healthcheck $INSTALL_DIR
cd $INSTALL_DIR
sudo tar xzf $INSTALL_DIR/hcheck-linux-$HEALTHCHECK_VERSION.tgz
mkdir -p sbin
mv $INSTALL_DIR/linux-x64/send_log $INSTALL_DIR/sbin/send_log
chmod +x $INSTALL_DIR/sbin/send_log




chmod +x $INSTALL_DIR/linux-x64/hcheck
chmod +x $INSTALL_DIR/nhc-runner.sh
HCHECK_SETTINGS_PATH=$INSTALL_DIR/healthchecks.json
REPORT_PATH=$(jq -r '.report' ${HCHECK_SETTINGS_PATH})
$INSTALL_DIR/linux-x64/hcheck -k $INSTALL_DIR/nhc-runner.sh --rpath $REPORT_PATH
$INSTALL_DIR/nhc-runner.sh -L || true
APPLICATIONINSIGHTS_CONNECTION_STRING=$(jq -r '.appinsights.ConnectString' ${HCHECK_SETTINGS_PATH})
INSTRUMENTATION_KEY=$(jq -r '.appinsights.InstrumentationKey' ${HCHECK_SETTINGS_PATH})
sudo -i
$INSTALL_DIR/linux-x64/hcheck --rpath $REPORT_PATH --fin --appin $INSTRUMENTATION_KEY --rscript $INSTALL_DIR/sbin/send_log
#| 
#while IFS= read -r line
 # do
  # jetpack log --level error "$line";
 # done
# exit ${PIPESTATUS[0]} 
exit $?