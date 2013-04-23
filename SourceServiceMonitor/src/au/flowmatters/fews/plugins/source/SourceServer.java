package au.flowmatters.fews.plugins.source;
//import java.io.BufferedReader;
import java.io.IOException;
//import java.io.BufferedReader;
import java.io.InputStream;
import java.io.BufferedReader;
import java.io.InputStreamReader;
//import java.io.InputStream;
//import java.io.InputStreamReader;
import java.io.OutputStream;
import java.io.PrintWriter;
import java.text.ParseException;
//import java.util.Scanner;

import org.apache.log4j.Logger;

import nl.wldelft.fews.system.data.config.GlobalProperties;


public class SourceServer {
	private static final String EXECUTABLE_32_PROPERTY = "SOURCE_32EXE_PATH";
	private static final String EXECUTABLE_64_PROPERTY = "SOURCE_64EXE_PATH";
	private static final String ADAPTER_PROPERTY = "SOURCE_ADAPTER_PATH";
	private static Logger log = Logger.getLogger(SourceServer.class);
	private boolean started;
	
	public SourceServer(String projectFile, int portnumber)
	{
		this.projectFile = projectFile;
		this.portNumber = portnumber;
	}

	public static boolean checkRequiredProperties()
	{
		boolean result = confirmProperty(ADAPTER_PROPERTY);
		result &= windows64()||confirmProperty(EXECUTABLE_32_PROPERTY);
		result &= (!windows64())||confirmProperty(EXECUTABLE_64_PROPERTY);
		return result;
	}
	
	public static boolean confirmProperty(String property)	
	{
		String propertyValue = GlobalProperties.get(property);
		if( (propertyValue==null)||(propertyValue=="") )
		{
			log.error("Required property " + property + " is missing from global properties configuration");
			return false;
		}
		return true;
	}
	
	public void start() throws IOException, ParseException, InterruptedException
	{
		String sourceExecutablePath = selectSourceExecutable();
		
		if(serverRunning())
		{
			log.warn("Source server appears to be already running at " + serverEndpoint());
			return;
		}
		
		String[] args = new String[]{sourceExecutablePath, "-p", GlobalProperties.resolvePropertyTags(projectFile), "-m", "Server",
									"-a",serverEndpoint()};
		log.info(commandLine(args));
		ProcessBuilder pb = new ProcessBuilder(args);
		
		pb = pb.redirectErrorStream(true);
		process = pb.start();
		
//		InputStreamReader inputStreamReader = new InputStreamReader(process.getInputStream());
//		scanner = new Scanner(process.getInputStream());
		inputStream = process.getInputStream();
		outputStream = process.getOutputStream();
		started = true;
	}

	private boolean serverRunning() throws IOException, InterruptedException {
		String sourceAdapterPath = GlobalProperties.get(ADAPTER_PROPERTY);
		log.info("Testing for existing service endpoint at " + serverEndpoint());
		
//		if((sourceAdapterPath==null)||(sourceAdapterPath==""))
//		{
//			log.error(ADAPTER_PROPERTY +" not set in properties file");
//			return true;
//		}

		String[] args = new String[]{sourceAdapterPath,"probe",serverEndpoint()};
		
		ProcessBuilder pb = new ProcessBuilder(args);
		pb = pb.redirectErrorStream(true);
		Process probeProcess = pb.start();
		BufferedReader probeInputs = new BufferedReader(new InputStreamReader(probeProcess.getInputStream()));
		probeProcess.waitFor();
		String result = probeInputs.readLine();
		return result.indexOf("OK")==0;
	}

	private String serverEndpoint() {
		return "net.tcp://localhost:"+Integer.toString(portNumber)+"/eWater/Services/RiverSystemService";
	}

	private String selectSourceExecutable() {
		if(windows64())
			return GlobalProperties.get(EXECUTABLE_64_PROPERTY);
		return GlobalProperties.get(EXECUTABLE_32_PROPERTY);
	}
	
	private static boolean windows64() {
		if(System.getProperty("os.name").contains("Windows"))
			return System.getenv("ProgramFiles(x86)")!=null;
		return System.getProperty("os.arch").indexOf("64") != -1;
	}

	private String commandLine(String[] command) {
		String result = "";
		for(String s : command)
			result += s + " ";
		return result;
	}

	public boolean getStarted()
	{
		return started;
	}
	
	public void stop()
	{
		stop(false);
	}
	
	public void stop(boolean waitForProcessToExit)
	{
		logAllMessages();

		if(outputStream == null)
			log.info("Server not started");
		else
		{
			log.info("Stopping server on port " + Integer.toString(portNumber));
			PrintWriter pw = new PrintWriter(outputStream);
			pw.println();
			pw.flush();
			
			if(waitForProcessToExit)
			{
				try{ process.waitFor();	}
				catch(InterruptedException e){}
			}
		}
	}

	public void logAllMessages() {
		if(inputStream != null)
		{			
			try {
				int bytesAvailable = inputStream.available(); 
				if(bytesAvailable>0)
				{
					byte[] logInput = new byte[bytesAvailable];
					inputStream.read(logInput);
					String logString = new String(logInput);
					String[] lines = logString.split("\\n");
					for(String line:lines)
					{
						if(line != "")
							log.debug("SOURCE("+Integer.toString(portNumber)+"): " + line);
					}
				}
			}
			catch (IOException e) {
				log.error("SOURCE("+Integer.toString(portNumber)+"): " + "Error reading server output.");
			}
		}
	}
	
	private Process process;
	private String projectFile;
	private int portNumber;	
	private OutputStream outputStream;
	private InputStream inputStream;
	
	public String getProjectFile() {
		return projectFile;
	}

	public int getPortNumber() {
		return portNumber;
	}
}
