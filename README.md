# Azure HPC Node Healthchecks

Azure-healthcheck project is a helper that is capable of running custom healthcheck scripts and reporting on any issues with the virtual machine upon its initialization.

This project supports [NHC](https://github.com/mej/nhc) healthcheck scripts and allows the addition of custom scripts. This was achieved with the help of work by Cormac Garvey, [cc_slurm_nhc](https://github.com/Azure/azurehpc/tree/master/experimental/cc_slurm_nhc). To learn more about this project and the advantages of running GPU healthchecks, refer to [this article](https://techcommunity.microsoft.com/t5/azure-global/automated-hpc-ai-compute-node-health-checks-integrated-with-the/ba-p/3113454).


## Table of Contents (by [gh-md-toc](https://github.com/ekalinin/github-markdown-toc))
<!--ts-->
   * [Installation](#installation)
      * [Prerequisites](#prerequisites)
      * [Building the project](#building-the-project)
      * [Uploading the executable files into the blobs storage](#uploading-the-executable-files-into-the-blobs-storage)
      * [Uploading the project to the Azure locker](#uploading-the-project-to-the-azure-locker)
      * [Customizing healtchecks](#customizing-healtchecks)
      * [Importing the cluster template into CycleCloud](#importing-the-cluster-template-into-cyclecloud)
   * [Running NHC healthcheck](#running-nhc-healthcheck)
      * [Designing custom NHC tests](#designing-custom-nhc-tests)
   * [Running custom test scripts](#running-custom-test-scripts)
      * [Designing custom test scripts](#designing-custom-test-scripts)
   * [Running the hcheck binary](#running-the-hcheck-binary)
   * [Changing the script for reporting errors](#changing-the-script-for-reporting-errors)
   * [Testing the project](#testing-the-project)
   * [Sample healthcheck report](#sample-healthcheck-report)
   * [Contributing](#contributing)
   * [Trademarks](#trademarks)

## Installation 

### Prerequisites
The instructions below assume that:

* you have a valid CycleCloud subscription
* you have access to the Azure VM
* your Azure VM runs a linux-based operating system and supports bash commands
* You have CycleCloud CLI installed and congigured. Refer to [this instruction](https://docs.microsoft.com/en-us/azure/cyclecloud/how-to/install-cyclecloud-cli?view=cyclecloud-8) for the installation steps

### Building the project

The project comes with a pre-built binary used to run the test scripts and build reports compatible with linux-x64. If you wish to build the source yourself, you will need to install .NET Core. Please refer to the deploy.sh for an example of steps you need to take.

```bash
cd ./hcheck/hcheck/
dotnet build -r linux-x64 --self-contained
```

### Uploading the executable files into the blobs storage

All the executable files used by the project (including the external script for sending logs) need to be archived and stored in the blobs folder. You can reference deploy.sh to see how this is achieved: 

```bash
VERSION=$(cyclecloud project info | grep Version | cut -d: -f2 | cut -d" " -f2)
DEST_FILE=$(pwd)/blobs/hcheck-linux-$VERSION.tgz
cp ../../../src/send_log ./linux-x64
tar czf $DEST_FILE ./linux-x64
```

### Uploading the project to the Azure locker

In order for you to be able to add the project to your CycleCloud cluster, you will first need to upload it to your Azure Locker. The easiest way to do it is by editing deploy.sh

```bash
cyclecloud project upload your-locker-name
```

### Customizing healtchecks

All the customizable parameters of the healthcheck tool are located under [[[configuration healthchecks]]] section of the cluster template file.

The parameters you can change are:

| Parameter | Meaning | Default value | 
| --- | --- | --- | 
| healthchecks.report | Full path to where the report would be generated | /var/log/healthreport.json |
| healthchecks.custom.pattern | Pattern used to detect custom scripts | *.sh |
| healthchecks.nhc.config | Name of the configuration file you want the NHC tool to use | None |
| healthchecks.nhc.log | Full path to where NHC will store its full report | /var/log/nhc.log | 
| healthchecks.appinsights.InstrumentationKey | Instrumentation Key of your Application Insights | None |
| healthchecks.appinsights.ConnectString | Connection String of your Application Insights | None | 

Most of them can be configured from the "Advanced Settings" tab in CycleCloud Server GUI:

![Alt](/images/advanced_settings.png "Advanced Settings")

### Importing the cluster template into CycleCloud

With CycleCloud CLI, upload the cluster template. Run the commands below to save your cluster settings (such as the region and configuration), and then import the cluster template along with those settings.

```bash
cyclecloud export_parameters MyClusterName > param.json
cyclecloud import_cluster --force -f  slurm.txt -c Slurm MyClusterName -p param.json
```

## Running NHC healthcheck

Which NHC checks are run is based on the .conf file. By default, this project includes a set of cluster-specific configuration files. If you want to use a custom configuration instead, put your .config file into the nhc-config subfolder within your project's files directory and edit the parameter to reflect that name instead:

![Alt](/images/NHC_configuration.png "NHC Config Name")

Alternatively, you can change the cluster template directly. This can be useful if you are planning to set up multiple clusters using that template:

```ini
 [[[configuration healthchecks.nhc]]]
 config = YOUR_CUSTOM_NAME.conf
```

### Designing custom NHC tests

You can write your own test scripts to be run by the healthcheck tool. 

NHC-based tests (.nhc files) have to be placed in the nhc-tests folder. In order for NHC to actually use them, you will need to create your own configuration files. Just place them in nhc-config folder and pass the name to the NHC config name parameter in the settings

## Running custom test scripts

Put the custom scripts you want the healthcheck tool to run into the custom-tests directory. Update healthchecks.custom.pattern in the cluster-ini template to a pattern that the healthcheck will use to determine which test scripts to run.

![Alt](/images/user_pattern.png "Custom pattern")

Alternatively, you can change the cluster template directly. This can be useful if you are planning to set up multiple clusters using that template:

```ini
[[[configuration healthchecks.custom]]] 
pattern = *.sh
```

All your healthchecks should exit with code 0 upon the successfull pass of a healthcheck, and non-0 otherwise.

### Designing custom test scripts

Whether it is a bash or a python script, anything executable can be a test, as long as it adheres to the following rules:

- Your script should contain a [shebang](#https://en.wikipedia.org/wiki/Shebang_(Unix))
- Exit code for a passing test is 0. Any non-zero exit code is considered a failure and will be reported
- To receive a meaningful report on the error, you need to output the message into the stdout
- If you want the report to contain more information than a single message can convey, you can make your script output a json string - just make sure it has a field "message" that would be used to log the error. If you do this, everything but the message field will end up in the "extra-info" part of the report as a valid json (please refer to the [Sample healthcheck report](#sample-healthcheck-report) section for an example). If there are any formatting issues or you fail to include the "message" field, the whole json construction will become the reported message instead

## Running the hcheck binary

You should never have to run the tool manually, but in the case you want to do so, here is a list of supported parameters the tool accepts

| Flag | Use | Example | 
| --- | --- | --- | 
| --append | Add data to the existing report | --append |
| --appin | Pass the Application Key to your App Insights | --appin YOUR-KEY |
| --args | Inline arguments for scripts | --args /tmp/log/report.json |
| --fin | Output the healthcheck results and exit with the non-zero code if errors were detected | --fin |
| -k | Path to the test scripts | -k nvidia-smi | 
| --nr | Number of reruns for the set of scripts | --nr 3 |
| --pt | Pattern for custom script detection | -pt .sh | 
| --rpath | Path to where the report would be generated | --rpath /tmp/log/report.json |
| --rscript | Path to the script reporting the results back to the portal | --rscript ./send_logs |

## Changing the script for reporting errors

Currently, the script reporting errors back to the portal is CycleCloud specific and uses a custom version of jetpack log command to send detailed information. If you wish to use another script to report the errors back, here are the inline parameters that it will be called with:

| Flag | Use | 
| --- | --- |  
| -m | Short message that shows up in CycleCloud logs |
| --level error | Level of the message |
| --info | Extra information about the tests in json format |
| --code | Exit code of the test | 
| --testname | Name of the test | 
| --nodeid | Id of the vm the tests were run on | 
| --time | The time it took to run the test in ms | 
| --error | Error message retured by the test script | 

## Testing the project

You can test the project by putting your custom scripts returning fixed results into the custom-test folder and setting the healthchecks.custom.pattern to the pattern that would detect them.

C# tool itself also comes with unit tests that you can run yourself by going into the hcheck-test directory and running: 

```bash
    dotnet test
```

If you want to test how healthchecks work on a real cluster, you can use the provided evenfail.sh test located in sample-healthchecks subfolder. Just copy it to the ./specs/default/cluster-init/files/custom-tests directory, import the slurm.txt template into your cluster (which should have a single dash in its name, for example - "cycleslurm-demo"), and put "even*.sh" as the custom script pattern parameter. After this, you can run deploy.sh and start the cluster.

## Sample healthcheck report

All healthcheck scripts run by the tool are required to exit with a non-zero code upon an error encountered. If you want to store some extra information into the report and have it as a proper json field, make sure your script outputs a valid json that contains a field "message" - that field will be trimmed from the extra information and would be used as a main output of the script. A failure to add a "message" field or errors in json would result in the whole json string used as a message.

```bash
 {
  "metadata": 
    { 
        "azEnvironment": "AzurePublicCloud",
        "isHostCompatibilityLayerVm": "false",
        .
        .
    }
  "testresults": {
     "nhc": {
      "exit-code": 0,
      "message": "",
      "extra-info": "None"
    },
    "full_test_path": {
      "exit-code": 101,
      "message": "There was an error!!!\n",
      "extra-info": "None"
    },
    "full_test_path2": {
      "exit-code": 100,
      "message": "Fail",
      "extra-info": {
        "custom_field": {
          "custom_subfield": "fail",
          "custom_subfield2": "pass"
        }
      }
    },
}
```

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
