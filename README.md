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

As noted, above, the adapter requires you to configure a Run Settings file to be exported from FEWS. Your exportRunFileActivity element needs to configure some custom property elements to (a) name the Source output file (which will get read and converted to FEWS PI format), and (b) to map individual time series to a CSV file based on parameter.

The Source output file is specified using a property with key='SourceOutputFile'. Parameters are mapped to Source input files using properties where the key follows the pattern 'ExpectedFile_propertyName', as in the following example (in [groovyFEWS] form):

```groovy
properties() {
  string(key:'SourceOutputFile', value:'source_output.csv')
  string(key:'ExpectedFile_Rainfall', value:'rainfallfeed.csv')
  string(key:'ExpectedFile_PET', value:'petfeed.csv')
  string(key:'ExpectedFile_Flow', value:'observedflow.csv')
}
```

## Configuring the Source model

The adapter is currently quite specific about how you set up your Source model. These requirements should be relaxed in the future, but for the time being, the Source model should be configured such that:

* All input time series inputs are setup through Data Sources (default on recent versions of Source) and set to reload on run
* Those data sources should be configured as multi-column csv files, with all inputs for a given parameter (eg rainfall) in the same file, regardless of how many locations are involved. That is, the rainfall for all sites should be in a single csv file
* Within those files, columns should be sorted by the location name (from FEWS).

## Limitations

There are a few big things that the adapter doesn't currently support and a few gotchas to be aware of.

* Only time series can be modified, there is currently no provision for changing Source model parameters (though this is planned for the adapter)


[fews]: http://www.deltares.nl/en/software/479962/delft-fews
[groovyFEWS]: http://github.com/flowmatters/groovyFEWS
[GeneralAdapter]: https://publicwiki.deltares.nl/display/FEWSDOC/05+General+Adapter+Module
[source]: http://www.ewater.com.au/products/ewater-source/
