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