package au.flowmatters.fews.plugins.source;
//import java.io.BufferedReader;
import java.io.IOException;
//import java.io.BufferedReader;
import java.io.InputStream;
//import java.io.InputStream;
//import java.io.InputStreamReader;
import java.io.OutputStream;
import java.io.PrintWriter;
import java.text.ParseException;
//import java.util.Scanner;

import org.apache.log4j.Logger;

import nl.wldelft.fews.system.data.config.GlobalProperties;


public class SourceServer {
	private static Logger log = Logger.getLogger(SourceServer.class);
	private boolean started;
	
	public SourceServer(String projectFile, int portnumber)
	{
		this.projectFile = projectFile;
		this.portNumber = portnumber;
	}

	public void start() throws IOException, ParseException
	{
		String sourceExecutablePath = GlobalProperties.get("SOURCE_EXE_PATH");
		
		
		String[] args = new String[]{sourceExecutablePath, "-p", GlobalProperties.resolvePropertyTags(projectFile), "-m", "Server",
									"-a","net.tcp://localhost:"+Integer.toString(portNumber)+"/eWater/Services/RiverSystemService"};
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
		logAllMessages();

		if(outputStream == null)
			log.info("Server not started");
		else
		{
			log.info("Stopping server on port " + Integer.toString(portNumber));
			PrintWriter pw = new PrintWriter(outputStream);
			pw.println();
			pw.flush();
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
