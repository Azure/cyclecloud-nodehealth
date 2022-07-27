#!/bin/bash
set -x
#set -e

#nhc NHC_CHECK_ALL=1 LOGFILE=-
while getopts L x; do
  nhc NHC_CHECK_ALL=1 MARK_OFFLINE=0 LOGFILE=$(jq '.nhc.log' ./healthchecks.json);
  exit $?
done; OPTIND=0
nhc NHC_CHECK_ALL=0 LOGFILE=- MARK_OFFLINE=0;
exit $?