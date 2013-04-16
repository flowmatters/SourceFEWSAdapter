import clr
import sys
import traceback
import System
import xml.dom.minidom

sys.path.Add("C:\\src\\SourceFEWSAdapter\\SourceFEWSAdapter.Core\\bin\\Debug")
#sys.path.Add("C:\\src\\trunk\\Solutions\\Output")

clr.AddReferenceToFile("SourceFEWSAdapter.Core.dll")
clr.AddReferenceToFile("TIME.dll")
import TIME
io = TIME.Management.NonInteractiveIO
#TimeSeries = TIME.DataTypes.TimeSeries
#TimeStep = TIME.DataTypes.TimeStep

import SourceFEWSAdapter.Core
PIproxy = SourceFEWSAdapter.Core.FEWSPIProxy
TIMEproxy = SourceFEWSAdapter.Core.TIMEProxy

timeStepUnits = SourceFEWSAdapter.Core.FEWSPI.timeStepUnitEnumStringType
Diagnostics = SourceFEWSAdapter.Core.Diagnostics

run_file = None
run_file = sys.argv[1]
if run_file is None:
  pi_path = "C:\\FEWS\\Ovens\\OvensFEWS\\Modules\\source\\ovens\\input\\"
  run_file = pi_path+"RunParameters.xml"

run_data = PIproxy.ReadRunFile(run_file)
#sys.stdout = open(run_data.outputDiagnosticFile,'a')
diagnostics = Diagnostics(run_data.outputDiagnosticFile)

source_results = io.Load(run_data.workDir + "\\" + run_data.Property("SourceOutputFile"))
diagnostics.Log(3,"Loaded " + source_results.Length.ToString() + " time series")

piCollection = TIMEproxy.FromTimeSeriesCollection(source_results)
fews_timeseries_file = run_data.outputTimeSeriesFile[0]

diagnostics.Log(3,"Writing time series out in PI format to " + fews_timeseries_file)
PIproxy.WriteTimeSeriesCollection(fews_timeseries_file,piCollection)

diagnostics.Log(3,"Post Adapter - All done")
diagnostics.Save()
