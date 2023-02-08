#!/bin/bash

NHC_SYSCONFIG=/etc
OS_SYSCONFIG=/etc/default
NHC_TIMEOUT=200
NHC_VERBOSE=1
NHC_DETACHED_MODE=1
NHC_DEBUG=0
NHC_EXE=/usr/sbin/nhc
NHC_NVIDIA_HEALTHMON=dcgmi
NHC_NVIDIA_HEALTHMON_ARGS="diag -r 1"
SLURM_CONF=/etc/slurm/slurm.conf
SLURM_HEALTH_CHECK_INTERVAL=200
SLURM_HEALTH_CHECK_NODE_STATE=IDLE
NHC_EXTRA_TEST_FILES="csc_nvidia_smi.nhc azure_cuda_bandwidth.nhc azure_gpu_app_clocks.nhc azure_gpu_ecc.nhc azure_gpu_persistence.nhc azure_ib_write_bw_gdr.nhc azure_nccl_allreduce_ib_loopback.nhc azure_ib_link_flapping.nhc azure_gpu_clock_throttling.nhc"

function readJson {
    UNAMESTR=`uname`
    if [[ "$UNAMESTR" == 'Linux' ]]; then
        SED_EXTENDED='-r'
        elif [[ "$UNAMESTR" == 'Darwin' ]]; then
        SED_EXTENDED='-E'
    fi;
    
    VALUE=`grep -m 1 "\"${2}\"" ${1} | sed ${SED_EXTENDED} 's/^ *//;s/.*: *"//;s/",?//'`
    
    if [ ! "$VALUE" ]; then
        echo "Error: Cannot find \"${2}\" in ${1}" >&2;
        exit 1;
    else
        echo $VALUE ;
    fi;
}

function nhc_config() {
    NHC_CONFIG_FILE=${NHC_SYSCONFIG}/nhc/nhc.conf
    cp ${NHC_CONF_FILE_NEW} ${NHC_CONFIG_FILE}
    
    if ! [[ -f ${NHC_CONFIG_FILE}_orig ]]
    then
        mv ${NHC_CONFIG_FILE} ${NHC_CONFIG_FILE}_orig
        cp ${NHC_CONF_FILE_NEW} ${NHC_CONFIG_FILE}
    else
        echo "Warning: Did not set up NHC config (Looks like it has already been set-up)"
    fi
}


function nhc_sysconfig() {
    NHC_SYSCONFIG_FILE=${OS_SYSCONFIG}/nhc
    if ! [[ -f ${NHC_SYSCONFIG_FILE} ]]
    then
        echo "TIMEOUT=$NHC_TIMEOUT" > $NHC_SYSCONFIG_FILE
        echo "VERBOSE=$NHC_VERBOSE" >> $NHC_SYSCONFIG_FILE
        echo "DETACHED_MODE=$NHC_DETACHED_MODE" >> $NHC_SYSCONFIG_FILE
        echo "DEBUG=$NHC_DEBUG" >> $NHC_SYSCONFIG_FILE
        echo "NVIDIA_HEALTHMON=$NHC_NVIDIA_HEALTHMON" >> $NHC_SYSCONFIG_FILE
        echo "NVIDIA_HEALTHMON_ARGS=\"$NHC_NVIDIA_HEALTHMON_ARGS\"" >> $NHC_SYSCONFIG_FILE
    else
        echo "Warning: Did not set up NHC sysconfig (Looks like it has already been set-up)"
    fi
}





function copy_extra_test_files() {
    
    #for test_file in $NHC_EXTRA_TEST_FILES
    #do
    #chmod +x ${CYCLECLOUD_SPEC_PATH}/files/*.nhc
    #cp ${CYCLECLOUD_SPEC_PATH}/files/*.nhc ${NHC_SYSCONFIG}/nhc/scripts
    #done
    #for FILE in ${CYCLECLOUD_SPEC_PATH}/files/*.nhc
    #do
    #     cp $FILE ${NHC_SYSCONFIG}/nhc/scripts/
    #done
    for FILE in ${CYCLECLOUD_SPEC_PATH}/files/nhc-tests/*.nhc
    do
        cp $FILE ${NHC_SYSCONFIG}/nhc/scripts/
    done
}

#mkdir /var/run/nhc
#NHC_CONF_FILE_NEW=`readJson ${CYCLECLOUD_SPEC_PATH}/files/nhc-config.json config` || exit 1;
#NHC_CONF_FILE_NEW=`echo ${CYCLECLOUD_SPEC_PATH}/files/nhc-config.txt` || exit 1;
#$CYCLECLOUD_SPEC_PATH/files/$(jetpack config healthchecks.nhc.config)
#CYCLECLOUD_SPEC_PATH=/mnt/cluster-init/healthcheck/default
FILES=/files/
HCHECK_CONFIG=healthchecks.json
HCHECK_FILES=${CYCLECLOUD_SPEC_PATH}${FILES}
HCHECK_JSON=${HCHECK_FILES}${HCHECK_CONFIG}
#echo $HCHECK_JSON
#NHC_CONF_FILE_NAME=$(jq -r '.nhc.config' ${HCHECK_JSON})
#echo $NHC_CONF_FILE_NAME
#I really don't understand why constants don't concat with variables...
#NHC_CONF_FILE_NEW=${CYCLECLOUD_SPEC_PATH}/files/$(jq -r '.nhc.config' ${HCHECK_JSON})

NHC_CONF_NAME=$(jq -r '.nhc.config' ${HCHECK_JSON})

if [[ $NHC_CONF_NAME == null ]]
then
    NHC_CONF_NAME=$(jetpack config azure.metadata.compute.vmSize).conf
fi





NHC_CONF_FILE_NEW=${HCHECK_FILES}/nhc-config/$NHC_CONF_NAME
nhc_config
nhc_sysconfig
copy_extra_test_files
