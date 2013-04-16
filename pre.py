import clr
import sys
import traceback
import System
import xml.dom.minidom

sys.path.Add("C:\\src\\SourceFEWSAdapter\\SourceFEWSAdapter.Core\\bin\\Debug")
sys.path.Add("C:\\src\\trunk\\Solutions\\Output")

clr.AddReferenceToFile("SourceFEWSAdapter.Core.dll")
clr.AddReferenceToFile("TIME.dll")
import TIME
io = TIME.Management.NonInteractiveIO
TimeSeries = TIME.DataTypes.TimeSeries
TimeStep = TIME.DataTypes.TimeStep

import SourceFEWSAdapter.Core
proxy = SourceFEWSAdapter.Core.FEWSPIProxy
timeStepUnits = SourceFEWSAdapter.Core.FEWSPI.timeStepUnitEnumStringType
var = SourceFEWSAdapter.Core.Diagnostics

def timeStepFromPI(piData):
  if piData.unit == timeStepUnits.second:
    if piData.multiplier == '3600':
      return(TimeStep.Hourly)
    if piData.multiplier == '86400':
      return(TimeStep.Daily)
  None

def buildTimeSeries(pi_timeseries):
  h = pi_timeseries.header
  tsStart = proxy.DateTimeFromPI(h.startDate)
  tsEnd = proxy.DateTimeFromPI(h.endDate)
  tsStep = timeStepFromPI(h.timeStep)
  timeSeries = TimeSeries(tsStart,tsEnd,tsStep)
  
  for e in pi_timeseries.event:
    dt = proxy.Merge(e.date,e.time)
    timeSeries.setTime(dt,float(e.value))
  
  timeSeries.name = h.parameterId + "_" + h.locationId  
  return(timeSeries)

def fileForParameter(parameter):
	marker = 'ExpectedFile_'
	for property in run_data.properties.Items:
		if property.key.rfind(marker) == 0:
			if property.key.replace(marker,"") == parameter:
				return property.value
	return "defaultFile.csv"

	
# def writeLogFile(path):
   # doc = xml.dom.minidom.Document()
   # diag = doc.createElement("Diag")
   # diag.attributes["xmlns:xsi"]="http://www.w3.org/2001/XMLSchema-instance"
   # diag.attributes["xmlns"]="http://www.wldelft.nl/fews/PI"
   # diag.attributes["xsi:schemaLocation"]="http://www.wldelft.nl/fews/PI http://fews.wldelft.nl/schemas/version1.0/pi-schemas/pi_run.xsd"
   # diag.attributes["version"]="1.5"
   # doc.childNodes.append(diag)
   
   # line = doc.createElement("line")
   # line.attributes["level"] = "3"
   # line.attributes["description"] = "Success"
  ##text = doc.createTextNode("Preprocesser successful")
  ##line.childNodes.append(text)
   
   # diag.childNodes.append(line)
   # doc.writexml(open(path,'a'),addindent="  ")

#def writeLogFile(path):
#  diagnostics = Diagnostics(path)
#  diagnostics.Log(3,"Using the C# proxy")
#  diagnostics.Save()


#print sys.argv
run_file = None
run_file = sys.argv[1]
if run_file is None:
  pi_path = "C:\\FEWS\\Ovens\\OvensFEWS\\Modules\\source\\ovens\\input\\"
  run_file = pi_path+"RunParameters.xml"

run_data = proxy.ReadRunFile(run_file)
#sys.stdout = open(run_data.outputDiagnosticFile,'a')
diagnostics = Diagnostics(run_data.outputDiagnosticFile)

runStart = proxy.DateTimeFromPI(run_data.startDateTime)
runEnd = proxy.DateTimeFromPI(run_data.endDateTime)

timeSeries=[]
for fn in run_data.inputTimeSeriesFile:
  print(fn)
  c = proxy.ReadTimeSeries(fn)
  for ts in c.series: timeSeries.append(ts)

#ts0 = timeSeries[4]
#timeStepFromPI(ts0.header.timeStep)
#tts0 = buildTimeSeries(ts0)

outputSets={}
convertedTimeSeries=[]

timeSeries.sort(lambda x,y: (x.header.locationId>y.header.locationId))
for ts in timeSeries:
	destFile = fileForParameter(ts.header.parameterId)
	if not outputSets.Contains(destFile):
		outputSets[destFile] = TIME.DataTypes.IO.CsvFileIo.CSVFileIO()
		
	converted = buildTimeSeries(ts)
	convertedTimeSeries.append(converted)
	outputSets[destFile].use(converted.name,converted)

[ts.name for ts in convertedTimeSeries]
nullCounts = [ts.countEqual(-9999) for ts in convertedTimeSeries]
nullCounts
# vals = [convertedTimeSeries[0][i] for i in range(0,convertedTimeSeries[0].count()-1)]
# vals

System.IO.Directory.CreateDirectory(run_data.workDir)

# Save to individual files
#for ts in convertedTimeSeries:
#  fn = run_data.workDir + "\\" + ts.name + ".csv"
#  diagnostics.Log(3,"Writing time series to " + fn)
#  io.Save(fn,ts)


for outputFile in outputSets:
	outputSets[outputFile].save(run_data.workDir + "\\" + outputFile)


#csv = TIME.DataTypes.IO.CsvFileIo.CSVFileIO()
#for ts in convertedTimeSeries:
#	csv.use(ts.name,ts)
#
#csv.save(run_data.workDir + "\\allcatchments.csv")

#writeLogFile(run_data.outputDiagnosticFile)
diagnostics.Log(3,"Pre Adapter - All done")
diagnostics.Save()

