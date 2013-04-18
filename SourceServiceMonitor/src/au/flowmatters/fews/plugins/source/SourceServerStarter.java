package au.flowmatters.fews.plugins.source;

import java.io.IOException;
import java.text.ParseException;
import java.util.ArrayList;

import org.apache.log4j.Logger;

public class SourceServerStarter extends Thread {
	private ArrayList<SourceServer> servers;
	private Logger log = Logger.getLogger(SourceServerStarter.class);
	
	public SourceServerStarter(ArrayList<SourceServer> servers)
	{
		this.servers = servers;
	}
	
	public void run()
	{
        for(SourceServer s : servers)
        {
        	log.info("Starting a server for " + s.getProjectFile() + " on port " + Integer.toString(s.getPortNumber()));
        	try {
				s.start();
			} catch (IOException e) {
				
			} catch (ParseException e) {
			}
        }
	}
}
