#!/usr/bin/env bash
SHARED_DIR_PATH=/shared/home/aevdokimova
if [ -f $SHARED_DIR_PATH/"failed.txt" ]; 
then
rm $SHARED_DIR_PATH/failed.txt; exit 0; 
else
echo "There was a hcheck error before" > $SHARED_DIR_PATH/failed.txt;
echo "failed"; exit 1; 
fi

node_index=$(jetpack config cyclecloud.node.name | cut -d- -f5)
if [[ $(expr $node_index % 2) == 0 ]]; then 
echo failed; exit 1; 
fi