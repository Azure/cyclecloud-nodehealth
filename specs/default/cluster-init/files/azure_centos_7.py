# Copyright 2016-2021 Swiss National Supercomputing Centre (CSCS/ETH Zurich)
# ReFrame Project Developers. See the top-level LICENSE file for details.
#
# SPDX-License-Identifier: BSD-3-Clause

#
# Generic fallback configuration
#

site_configuration = {
    'systems': [
        {
            'name': 'fs_v2',
            'descr': 'Azure FV2',
            'vm_data_file': 'azure_nhc/vm_info/azure_vms_dataset.json',
            'vm_size': 'F2s_v2',
            'hostnames': ['*'],
            'modules_system': 'tmod32',
            'partitions': [
                {
                    'name': 'hpc',
                    'scheduler': 'slurm',
                    'launcher': 'srun',
                    'max_jobs': 100,
                    'access': ['-p hpc'],
                    'environs': ['builtin'],
                    'prepare_cmds': ['source /etc/profile.d/modules.sh']
                }
            ]
        },
        {
            'name': 'hbrs',
            'descr': 'Azure HB',
            'vm_data_file': 'azure_nhc/vm_info/azure_vms_dataset.json',
            'vm_size': 'HB60rs',
            'hostnames': ['*_hb_*'],
            'modules_system': 'tmod32',
            'partitions': [
                {
                    'name': 'hb',
                    'scheduler': 'slurm',
                    'launcher': 'srun',
                    'max_jobs': 100,
                    'access': ['-p hb'],
                    'environs': ['gnu-azhpc-cos7'],
                    'prepare_cmds': ['source /etc/profile.d/modules.sh']
                }
            ]
        },
        {
            'name': 'hbrs_v2',
            'descr': 'Azure HBv2',
            'vm_data_file': 'azure_nhc/vm_info/azure_vms_dataset.json',
            'vm_size': 'HB120rs_v2',
            'hostnames': ['*_hbv2_*'],
            'modules_system': 'tmod32',
            'partitions': [
                {
                    'name': 'hbv2',
                    'scheduler': 'slurm',
                    'launcher': 'srun',
                    'max_jobs': 100,
                    'access': ['-p hbv2'],
                    'environs': ['gnu-azhpc-cos7'],
                    'prepare_cmds': ['source /etc/profile.d/modules.sh']
                }
            ]
        },
        {
            'name': 'hbrs_v3',
            'descr': 'Azure HBv3',
            'vm_data_file': 'azure_nhc/vm_info/azure_vms_dataset.json',
            'vm_size': 'HB120rs_v3',
            'hostnames': ['*_hbv3_*'],
            'modules_system': 'tmod32',
            'partitions': [
                {
                    'name': 'hbv3',
                    'scheduler': 'slurm',
                    'launcher': 'srun',
                    'max_jobs': 100,
                    'access': ['-p hbv3'],
                    'environs': ['gnu-azhpc-cos7'],
                    'prepare_cmds': ['source /etc/profile.d/modules.sh']
                }
            ]
        },
        {
            'name': 'hcrs',
            'descr': 'Azure HC',
            'vm_data_file': 'azure_nhc/vm_info/azure_vms_dataset.json',
            'hostnames': ['*_hc_*'],
            'modules_system': 'tmod32',
            'partitions': [
                {
                    'name': 'default',
                    'scheduler': 'local',
                    'launcher': 'local',
                    'environs': ['gnu-azhpc-cos7'],
                    'prepare_cmds': ['source /etc/profile.d/modules.sh']
                }
            ]
        },
        {
            'name': 'ndamsr_a100_v4',
            'descr': 'Azure NDm v4',
            'vm_data_file': 'azure_nhc/vm_info/azure_vms_dataset.json',
            'hostnames': [''],
            'modules_system': 'tmod4',
            'partitions': [
                {
                    'name': 'gpu',
                    'scheduler': 'local',
                    'launcher': 'local',
                    'environs': ['gnu-azhpc'],
                    'prepare_cmds': ['source /etc/profile.d/modules.sh']
                }
            ]
        },
        {
            'name': 'ndasr_v4',
            'descr': 'Azure ND v4',
            'vm_data_file': 'azure_nhc/vm_info/azure_vms_dataset.json',
            'hostnames': [''],
            'modules_system': 'tmod4',
            'partitions': [
                {
                    'name': 'gpu',
                    'scheduler': 'local',
                    'launcher': 'local',
                    'environs': ['gnu-azhpc'],
                    'prepare_cmds': ['source /etc/profile.d/modules.sh']
                }
            ]
        },
        {
            'name': 'generic',
            'descr': 'Generic example system',
            'hostnames': ['.*'],
            'partitions': [
                {
                    'name': 'default',
                    'scheduler': 'local',
                    'launcher': 'local',
                    'environs': ['builtin'],
                    'prepare_cmds': ['source /etc/profile.d/modules.sh']
                }
            ]
        }
    ],
    'environments': [
        {
            'name': 'builtin',
            'cc': 'cc',
            'cxx': '',
            'ftn': ''
        },
        {
            'name': 'gnu-azhpc',
            'modules': ['gcc-9.2.0', 'mpi/hpcx'],
            'cc': 'gcc',
            'cxx': 'g++',
            'ftn': 'gfortran'
        },
        {
            'name': 'gnu-azhpc-cos7',
            'modules': ['gcc-9.2.0', 'mpi/hpcx'],
            'cc': 'gcc',
            'cxx': 'g++',
            'ftn': 'gfortran'
        },
        {
            'name': 'gnu-azhpc-cos8',
            'modules': ['gcc-9.2.1', 'mpi/hpcx'],
            'cc': 'gcc',
            'cxx': 'g++',
            'ftn': 'gfortran'
        },
        {
            'name': 'gnu',
            'cc': 'gcc',
            'cxx': 'g++',
            'ftn': 'gfortran'
        },
    ],
    'logging': [
        {
            'handlers': [
                {
                    'type': 'stream',
                    'name': 'stdout',
                    'level': 'info',
                    'format': '%(message)s'
                },
                {
                    'type': 'file',
                    'level': 'debug',
                    'format': '[%(asctime)s] %(levelname)s: %(check_info)s: %(message)s',   # noqa: E501
                    'append': False
                }
            ],
            'handlers_perflog': [
                {
                    'type': 'filelog',
                    'prefix': '%(check_system)s/%(check_partition)s',
                    'level': 'info',
                    'format': (
                        '%(check_job_completion_time)s|reframe %(version)s|'
                        '%(check_info)s|jobid=%(check_jobid)s|'
                        '%(check_perf_var)s=%(check_perf_value)s|'
                        'ref=%(check_perf_ref)s '
                        '(l=%(check_perf_lower_thres)s, '
                        'u=%(check_perf_upper_thres)s)|'
                        '%(check_perf_unit)s'
                    ),
                    'append': True
                }
            ]
        }
    ],
}