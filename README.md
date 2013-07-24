# Source FEWS Adapter

The adapter is used within [GeneralAdapter] modules in [FEWS][fews] to invoke the [eWater Source][source] model as a simulation engine.

There are two main components involved:

1. The core adapter, which works with the [GeneralAdapter] module in FEWS and handles data exchange with Source.
2. A plugin to main FEWS Explorer user interface, which is used to start up and shutdown instances of the Source server when using FEWS in either Standalone or Operator Client mode.

This document covers the basics of compiling the two components, configuring the components in FEWS and how to setup and maintain the Source model.

Configuration requires a general understanding of FEWS configuration and in particular the process for configuring [GeneralAdapters][GeneralAdapter]. This document just covers specific characteristics of the Source FEWS adapter.

## Building the Adapter and the Monitor

The core adapter is C#/.NET based and can be built with Visual Studio 2010. The adapter references TIME.dll, which can found in the Source installation directory.

The FEWS Explorer plugin is written Java. At the present time, the plugin is configured as an Eclipse project. The plugin references several JARs from the FEWS binary directory.

## Standalone and Client Server Configuration

The adapter can be configured to call Source as either a standalone program or in client server mode. Importantly, this distinction is independent of the FEWS configuration: A standalone FEWS can call the client server Source and vice versa.

The client server mode of Source requires more configuration, but offers faster performance, particularly when running models multiple times, such as in river operations scenarios. Client-server mode requires setting up one instance of the Source server software for each Source model linked to FEWS. This setup can be handled externally to FEWS, or can be configured to occur on startup of FEWS using the Explorer plugin.

The performance gains from client server mode will vary depending on the characteristics of the Source model, but speedups of a factor of five have been observed. This is due to the time that the standalone version of Source takes to load the model files on each run.

### Configuring the Source Servers

The Source Server Monitor is a FEWS Explorer plugin for starting and stopping Source servers for use with the core adapter. The Monitor needs to be (a) configured to run when FEWS starts up (from Explorer.xml) and (b) a mapping of Source project files (.rsproj) to Server port numbers.

In Explorer.xml, add an explorerTask to run the Monitor. The Monitor should be passed the name of a CSV configuration file, as described below

```xml
<explorerTask name="Source Server Monitor">
	<arguments>SourceServers.csv</arguments>
	<taskClass>au.flowmatters.fews.plugins.source.SourceServiceController</taskClass>
	<toolbarTask>false</toolbarTask>
	<menubarTask>false</menubarTask>
	<allowMultipleInstances>false</allowMultipleInstances>
	<permission>forecaster</permission>
	<toolWindow>true</toolWindow>
  	<loadAtStartup>true</loadAtStartup>
</explorerTask>
```

The CSV configuration file should be placed in the PiClientConfigFiles folder and should be populated with two column rows, where each row corresponds to an instance of the Source server:

1. The path to a Source project file (.rsproj), typically referenced relative to a system variable such as `%REGION_HOME`, and
2. A port number for the Source server to listen to.

```csv
%REGION_HOME%\Modules\source\catchment1\model\catchment1.rsproj,8765,
%REGION_HOME%\Modules\source\catchment2\model\catchment2.rsproj,8766,
```

### Required Global Properties

The Monitor will check for the following global properties (eg defined in `sa_global.properties` or in `oc_global.properties`):

* `SOURCE_ADAPTER_PATH` containing the path the Source Adapter
* `SOURCE_32EXE_PATH` containing the path to the 32 bit RiverSystem.CommandLine.exe (required for 32 bit installations) and
* `SOURCE_64EXE_PATH` containing the path to the 64 bit RiverSystem.CommandLine.exe (required for 64 bit installations)

These are **required** for the startup of the Monitor, but they are also convenient properties to reference from the General Adapter.

## Basic Operation

The Source FEWS adapter builds as a single executable that can respond to multiple commands, issued as the first command line argument. The following commands are understood and map closely to the GeneralAdapter preprocess, simulate and post-process steps:

* **preadapter** accepts a FEWS run settings file and one or more FEWS PI time series files and then produces input time series files for Source
* **simulation** invokes the Source command line runner with time-window settings from the FEWS run settings file
* **postadapter** transforms the Source outputs to FEWS PI time series for importing back into FEWS.

Thus, the Source FEWS adapter requires three executeActivity steps in the GeneralAdapter. In each case, the command should be first command line argument to the program, and the path to the FEWS run settings file should be the second (implying that your GeneralAdapter will need an exportRunFileActivity element). That's all that is required for the three main commands. For example:

```xml
<executeActivity>
	<description>Run PreAdapter</description>
	<command>
		<executable>%REGION_HOME%/Modules/source/bin/SourceFEWSAdapter.exe</executable>
	</command>
	<arguments>
		<argument>preadapter</argument>
		<argument>%ROOT_DIR%\input\RunParameters.xml</argument>
	</arguments>
	<timeOut>1200000</timeOut>
</executeActivity>
```

Two additional commands are available:
* **probe** is used to check the status of a running Source server. This is primarily used by the SourceServerMonitor to avoid starting up a server where one already exists, and
* **loadplugins** which is used to configure Source to temporarily use a particular set of plugins as required by a given model. Where plugins are required, an additional `<executeActivity>` should be used to call the adapter with the `loadplugins` command

```xml
<executeActivity>
	<description>Run PreAdapter</description>
	<command>
		<executable>%REGION_HOME%/Modules/source/bin/SourceFEWSAdapter.exe</executable>
	</command>
	<arguments>
		<argument>loadplugins</argument>
		<argument>%ROOT_DIR%\input\RunParameters.xml</argument>
	</arguments>
	<timeOut>1200000</timeOut>
</executeActivity>
```

## The FEWS Run Settings File
 
As noted, above, the adapter requires you to configure a Run Settings file to be exported from FEWS. This file captures the information that the adapter needs to both locate the Source model or server and to manage the information exchange with Source.

The General Adapter should include an exportRunFileActivity element, which needs to configure some custom property elements depending on how the Source model is configured and how you plan to run the model (Client Server or Standalone).

| Property | Purpose | Required? |
| --- | --- | --- |
| `SourceOutputFile` | names the csv file that Source will produce (which will get read and converted to FEWS PI format) | Yes |
| `SourceInputFile` | names the CSV file that Source will read (if all time series are imported from one file). Alternatively, the Source inputs can be split based on parameter using a series of `ExpectedFile_<parameter_name>` properties | No |
| `ExpectedFile_<parameter_name>` | Properties of this form map all time series for a given parameter (ie for all locations) to a particular file | No |
| `Source_32EXE` and `Source_64EXE` | locate the 32 bit and 64 bit versions of the `RiverSystem.CommandLine.exe` program. You need both of these if the configuration could be deployed to both 32bit and 64bit systems (including standalone, OC and FSS). | Yes |
| `RSPROJ` | provides the path to the Source project file | Yes |
| `TimeStep` | specify a simulation time step in seconds | No |
| `Port` | Port number of source server (on localhost) | No |
| `URI` | Full URI for source server | No | 
| `ForceTimeStamp` | Used to enforce a timestamp on the Source model outputs when using daily models. Source would otherwise default to a timestampe of 00:00 regardless of the inputs | No |
| `PluginDir` | Used where the source model (`RSPROJ`) requires one or more plugins to be loaded. The plugin DLLs should be placed in the directory as noted. If this option is used, then an additional `<executeActivity>` should be configured to call the adapter with the `loadplugins` command. | No |
| `UserPortOffsets` | Points to a file contain port offsets based on username. Used in multi-user environments, such as Citrix. | No |

The following example (in [groovyFEWS] form) illustrates many of the properties:
 
```groovy
properties() {
   string(key:'ExpectedFile_PET', value:'petfeed.csv')
   string(key:'ExpectedFile_Flow', value:'observedflow.csv')
   string(key:'TimeStep',value:3600) // Optional time step (in seconds)
   string(key:'Port',value:9876) // When the Source service is hosted locally
   string(key:'RSPROJ',value:'%ROOT_DIR%\\mode\\modelfile.rsproj')
   string(key:'Source_32EXE',value:'$SOURCE_32EXE_PATH$')
   string(key:'Source_64EXE',value:'$SOURCE_64EXE_PATH$')
}
```

The `RSPROJ` property is not used when Source is called in client server mode (because the project is loaded on the Server). However it is useful to still specify the path here because the Adapter will try to fall back to standalone mode when the Source server is unavailable.

## Configuring the Source model

The adapter is currently quite specific about how you set up your Source model. These requirements should be relaxed in the future, but for the time being, the Source model should be configured such that:

* All input time series inputs are setup through Data Sources (default on recent versions of Source) and set to reload on run
* The model should be configured in "planning mode" as opposed to operations mode. In recent versions of Source you can switch an operations scenario to planning mode using the Tools|River Operations menu. An implication of planning mode is that operations-mode forecasts are not available. These should be configured from FEWS and input as time series.
* The data sources should be configured as multi-column csv files, with either all inputs in one file or with a separate file for each parameter type (eg rainfall) with all relevant sites in the one file.
* Within any given csv file, the SourceFEWSAdapter will sort columns by location name and then by parameter name. If you wish to force a different order, you can use an IdMap in FEWS to prepend a column number to the location name, as in the following example:

```xml
<map internalLocation="J606256A" internalParameter="E.obs" externalLocation="19_J606256A" externalParameter="E.obs"/>
<map internalLocation="J606256A" internalParameter="H.obs" externalLocation="20_J606256A" externalParameter="H.obs"/>
```

## Importing Source Results To FEWS

The `postAdapter` command will produce a single FEWS Published Interface (PI) file for importing into FEWS. **Importantly** all Source results should be imported via a single `<importTimeSeriesActivity>` element.

A Source model typically produces many outputs, which can be configured in the Source GUI. It is preferable to disable outputs that aren't required for FEWS as this reduces both the runtime of the simulation model and the various file export and import steps.

The results from Source typically need to be mapped to FEWS locations and parameters by specifying an `<importIdMap>` in the `<general>` options of the General Adapter. As with all IdMaps in FEWS, these files can be configured with 'generic' rules:

``` xml
<parameter external="Downstream Flow Volume" internal="Q.simulated.forecast"/>
<location external="link for catchment SC #0" internal="G407210D"/>
```

or specific rules
```xml
<map internalLocation="SourceOfftakeNode" internalParameter="Q.simulated.supplied" externalLocation="N400999" externalParameter="DemandModel@Ordered Water Supplied@Ordered Water Supplied"/>
```

The external parameter (ie the Source parameter) can be found by looking in the FEWS PI output file and searching for `<parameterId>` elements. The exact set of parameters depends on the setup of the Source model. 

## Dealing with Missing Values

It is important to configure FEWS to deal correctly with any missing values that could be passed to Source.

Most inputs in Source require complete series and will not work with missing values. Importantly, the command line version of Source doesn't appear to check for missing values and will just treat the missing value (typically -9999) as a regular input!

The Adapter **passes** any missing values from FEWS to Source as an input: **no checking takes place in the adapter**. This is because Source is able to accept missing values for some inputs (such as observed streamflow at gauging stations) and the adapter has no information about which inputs can and cannot include gaps.

Where Source model inputs cannot be null, the appropriate parameter should be configured with `allowMissing="false"` in Parameters.xml, and the corresponding export should be configured with `checkMissing=true` in the GeneralAdapter.

```xml
<!--Parameters -->
<parameter id="P.obs" name="Observed Precipitation">
	<shortName>P.obs</shortName>
	<allowMissing>false</allowMissing>
</parameter>
```

```xml
<!-- General Adapter -->
<exportTimeSeriesActivity>
	<exportFile>Rain.xml</exportFile>
	<timeSeriesSets>
		<!-- ... -->
		<parameterId>P.obs</parameterId>
		<!-- ... -->
	</timeSeriesSets>
	<checkMissing>true</checkMissing>
</exportTimeSeriesActivity>
```

## Maintaining the Source model

It is important to realise that the setup of your Source model is *coupled* to your FEWS configuration: Changes in one can affect the other and in some cases break the linkage.

Key things to watch for:

* The data mappings in Source are to columns, so if the column order changes, the data mappings will change **without warning**.
* Adding and removing time series to the FEWS export will affect the input mapping. Importantly, if you are exporting time series based on a locationSet and that composition of that locationSet changes, then the export will change and the Source linkage will break. **In many case this change will not produce an error message due to Source still having enough columns**

## Multi-User Environments

When multiple users run FEWS Operator Clients on the same machine, for example, by using terminal services or Citrix to connect to the same Windows Servers, there would be a risk that multiple users would try to access the same instances of the Source service, causing conflicts. The Source Server Monitor and the Adapter can work around this by using Port 'offsets' based on username. With this scheme, the port number for a given Source model service is the configured port number + a port offset for the current user. So, for example, if `ModelA.rsproj` is configured for port 9100 and the current user, Bob, has a port offset of 3, the service for ModelA will be started on port 9103.

Configuring the Source server environment in a multi-user environment is a three step process:

1. Enter a series of per user, port offsets, in `Config/PiClientConfigFiles/UserSourceServerPorts.csv`. Each line should contain a username (ie the Windows logon) and a port offset.
2. List the mappings of Source model to network ports, as described above (eg `SourceServers.csv`),
3. Add a `UserPortOffsets` property to the `<exportRunFileActivity>` in the general adapter to tell the adapter about the offsets.

The two CSV files need to be constructed to avoid clashes. So, if the user offsets increment by 1, then the project port numbers need to step by at least the number of users:
```csv
joel,1,
butcher,2,
baker,3,
candlestickmaker,4,
```

```csv
%REGION_HOME%\Modules\source\catchment1\model\catchment1.rsproj,8760,
%REGION_HOME%\Modules\source\catchment2\model\catchment2.rsproj,8770,
```

## Limitations

There are a few big things that the adapter doesn't currently support and a few gotchas to be aware of.

* Only time series can be modified, there is currently no provision for changing Source model parameters (though this is planned for the adapter)
* Source cannot current start rainfall runoff models with an explicitly defined starting state (eg for soil moisture). This results in a need to run relatively long warmup periods for forecasts. 

## License

The Source FEWS Adapter is licensed under the [LGPLv3]

[fews]: http://www.deltares.nl/en/software/479962/delft-fews
[groovyFEWS]: http://github.com/flowmatters/groovyFEWS
[GeneralAdapter]: https://publicwiki.deltares.nl/display/FEWSDOC/05+General+Adapter+Module
[source]: http://www.ewater.com.au/products/ewater-source/
[LGPLv3]: http://www.gnu.org/copyleft/lesser.html
