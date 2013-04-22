# Source FEWS Adapter

The adapter is used within [GeneralAdapter] modules in [FEWS][fews] to invoke the [eWater Source][source] model as a simulation engine.

Configuration requires an understanding of the general process for configuring [GeneralAdapters][GeneralAdapter]. This document just covers specific characteristics of the Source FEWS adapter.

## Basic Operation

The Source FEWS adapter builds as a single executable that can respond to multiple commands, issued as the first command line argument. The following commands are understood and map closely to the GeneralAdapter preprocess, simulate and post-process steps:

* **preadapter** accepts a FEWS run settings file and one or more FEWS PI time series files and then produces input time series files for Source
* **simulation** invokes the Source command line runner with time-window settings from the FEWS run settings file
* **postadapter** transforms the Source outputs to FEWS PI time series for importing back into FEWS.

Thus, the Source FEWS adapter requires three executeActivity steps in the GeneralAdapter. In each case, the command should be first command line argument to the program, and the path to the FEWS run settings file should be the second (implying that your GeneralAdapter will need an exportRunFileActivity element). That's all that is required for the preadapter and postadapter commands. The simulation command requires three additional arguments:

* the path to the Source command line program (ie RiverSystem.CommandLine.exe)
* the path to the Source model file (ie the .rsproj file)
* a path to the file to be used for saving Source results (ie a .csv) (TODO: This last one should be dropped from here because it's also required in the FEWS Run Settings file, see below) 

## The FEWS Run Settings File
 
As noted, above, the adapter requires you to configure a Run Settings file to be exported from FEWS. Your exportRunFileActivity element needs to configure some custom property elements, which vary depending on how the Source model is configured and how you plan to run the model (Client Server or Standalone). Properties include:

1. name the `SourceOutputFile` (which will get read and converted to FEWS PI format),
1. to map Source input time series to a CSV file, using either a single file for all inputs (`SourceInputFile`) or splitting inputs based on parameter (`ExpectedFile_<parameter_name>`),
1. optionally specify a simulation `TimeStep` if not daily, and
1. optionally specify either a `Port` or a `URI` for an existing Source server. 
 
The Source output file is specified using a property with key='SourceOutputFile'. Parameters are mapped to Source input files using properties where the key follows the pattern 'ExpectedFile_propertyName', as in the following example (in [groovyFEWS] form):
 
```groovy
properties() {
   string(key:'ExpectedFile_PET', value:'petfeed.csv')
   string(key:'ExpectedFile_Flow', value:'observedflow.csv')
   string(key:'TimeStep',value:3600) // Optional time step (in seconds)
   string(key:'Port',value:9876) // When the Source service is hosted locally
}
```

## Configuring the Source model

The adapter is currently quite specific about how you set up your Source model. These requirements should be relaxed in the future, but for the time being, the Source model should be configured such that:

* All input time series inputs are setup through Data Sources (default on recent versions of Source) and set to reload on run
* The model should be configured in "planning mode" as opposed to operations mode. In recent versions of Source you can switch an operations scenario to planning mode using the Tools|River Operations menu. An implication of planning mode is that operations-mode forecasts are not available. These should be configured from FEWS and input as time series.
* Those data sources should be configured as multi-column csv files, with either all inputs in one file or with a separate file for each parameter type (eg rainfall) with all relevant sites in the one file.
* Within any given csv file, the SourceFEWSAdapter will sort columns by location name and then by parameter name. If you wish to force a different order, you can use an IdMap in FEWS to prepend a column number to the location name, as in the following example:

```xml
<map internalLocation="J606256A" internalParameter="E.obs" externalLocation="19_J606256A" externalParameter="E.obs"/>
<map internalLocation="J606256A" internalParameter="H.obs" externalLocation="20_J606256A" externalParameter="H.obs"/>
```

## Maintaining the Source model

It is important to realise that the setup of your Source model is *coupled* to your FEWS configuration: Changes in one can affect the other and in some cases break the linkage.


## Limitations

There are a few big things that the adapter doesn't currently support and a few gotchas to be aware of.

* Only time series can be modified, there is currently no provision for changing Source model parameters (though this is planned for the adapter)

## License

The Source FEWS Adapter is licensed under the [LGPLv3]

[fews]: http://www.deltares.nl/en/software/479962/delft-fews
[groovyFEWS]: http://github.com/flowmatters/groovyFEWS
[GeneralAdapter]: https://publicwiki.deltares.nl/display/FEWSDOC/05+General+Adapter+Module
[source]: http://www.ewater.com.au/products/ewater-source/
[LGPLv3]: http://www.gnu.org/copyleft/lesser.html
