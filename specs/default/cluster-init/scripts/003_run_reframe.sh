#!/bin/bash
set -x
set -e

sudo -i


INSTALL_DIR=/opt/cycle/nodehealth
HCHECK_SETTINGS_PATH=$INSTALL_DIR/healthchecks.json
TEST_PATTERN=$(jq -r '.reframe.pattern' ${HCHECK_SETTINGS_PATH})
# Determine the OS version

cd $INSTALL_DIR

if ! [[ -f  $INSTALL_DIR/reframe/bin/reframe ]]
    then
    cd $CYCLECLOUD_SPEC_PATH/files/reframe
    ./bootstrap.sh
    ./bin/reframe -V
else
    echo "Warning: Did not install ReFrame (looks like it already has been installed)"
    cd reframe  
fi
version=`/bin/bash $INSTALL_DIR/reframe/azure_nhc/utils/common.sh`
export PATH=/opt/cycle/jetpack/bin:$PATH

reframe_cfg="azure_ex.py"
if [ "$version" == "almalinux-8" ]
then
    reframe_cfg="azure_almalinux_8.py"
elif [ "$version" == "centos-7" ]
then
    export PATH=/opt/rh/rh-python38/root/usr/bin:$PATH
    reframe_cfg="azure_centos_7.py"
elif [ "$version" == "centos-8" ]
then
    reframe_cfg="azure_centos_8.py"
elif [ "$version" == "ubuntu-20" ]
then
    reframe_cfg="azure_ubuntu_20.py"
fi

for FILE in $INSTALL_DIR/reframe/azure_nhc/run_level_2
do
    sudo chmod +x $FILE || true
done
REPORT_PATH=$(jq -r '.report' ${HCHECK_SETTINGS_PATH})

APPLICATIONINSIGHTS_CONNECTION_STRING=$(jq -r '.appinsights.ConnectString' ${HCHECK_SETTINGS_PATH})
INSTRUMENTATION_KEY=$(jq -r '.appinsights.InstrumentationKey' ${HCHECK_SETTINGS_PATH})
#$INSTALL_DIR/linux-x64/hcheck -k $INSTALL_DIR/reframe/azure_nhc/run_level_2  --append --rpath $REPORT_PATH --reframe $INSTALL_DIR/reframe/bin/reframe --config $INSTALL_DIR/reframe/azure_nhc/config/${reframe_cfg}
#$INSTALL_DIR/linux-x64/hcheck --rpath $REPORT_PATH --fin --appin $INSTRUMENTATION_KEY --rscript $INSTALL_DIR/sbin/send_log