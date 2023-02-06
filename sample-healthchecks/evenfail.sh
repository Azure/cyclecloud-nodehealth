#!/usr/bin/env bash
#this is set to work on HPC nodes of a cluster that has a single dash in its name. 
#you might need to change the number in -f5 parameter of cut to adapt it to a different number of dashes in the cluster name
node_index=$(jetpack config cyclecloud.node.name | cut -d- -f5)
if [[ $(expr $node_index % 2) == 0 ]]; then 
echo failed; exit 1; 
fi