package au.flowmatters.fews.plugins.source;

import java.awt.FlowLayout;
import java.io.IOException;
import java.util.ArrayList;

import javax.swing.JButton;
import javax.swing.JPanel;

import nl.wldelft.fews.gui.explorer.FewsEnvironment;
import nl.wldelft.fews.gui.explorer.FewsExplorerPlugin;
import nl.wldelft.fews.system.data.DataStore;
import nl.wldelft.fews.system.data.DataStoreException;
import nl.wldelft.fews.system.data.config.Config;
import nl.wldelft.fews.system.data.config.files.ConfigFile;
import nl.wldelft.fews.system.data.config.region.PiClientDescriptor;
import org.apache.log4j.Logger;

public class SourceServiceController extends JPanel implements FewsExplorerPlugin {
	private static final long serialVersionUID = -8617095560285855001L;
	private static final Logger log = Logger.getLogger(SourceServiceController.class);
	private FewsEnvironment environment;
	private ArrayList<SourceServer> servers = new ArrayList<SourceServer>();
	private long lastLogTime = 0;
	private boolean shutDown = false;
	
	public SourceServiceController() {
		this.setLayout(new FlowLayout());
		this.add(new JButton("Hello!"));
	}

	@Override
	public void dispose() {
		stopAllServers();
		
	}

	private void stopAllServers() {
		log.info("Stopping all Source servers");
		
		for( SourceServer s : servers )
		{
			log.info("Stopping a server for " + s.getProjectFile() + " on port " + Integer.toString(s.getPortNumber()));
	    	s.stop();
		}
    	log.info("All Servers stopped");
		shutDown = true;
	}

	@Override
	public boolean isAlive() {
		updateLogs();
		
		return !shutDown;
	}

	private void updateLogs() {
		long now = System.nanoTime();
		if((now-lastLogTime)>1e7)
		{
			for(SourceServer s : servers)
			{
				if(s.getStarted())
					s.logAllMessages();
			}
			lastLogTime = System.nanoTime();
		}		
	}

	@Override
	public void run(FewsEnvironment env, String arguments) throws Exception {
        log.info("Starting up Source Service Controller");
    	long before = System.nanoTime();
        try
        {
            this.environment = env;
            String[] configurationLines = loadConfiguration(arguments);
            
            for(String line : configurationLines)
            {
            	String[] components = line.split(",");
            	String projectFile = components[0];            	
            	int portNumber = Integer.parseInt(components[1].replaceAll("(\\r|\\n)", ""));
            	
            	SourceServer server = new SourceServer(projectFile,portNumber);
            	servers.add(server);
            }
            
            for(SourceServer s : servers)
            {
            	log.info("Starting a server for " + s.getProjectFile() + " on port " + Integer.toString(s.getPortNumber()));
            	s.start();
            }
        }
        catch(Error e)
        {
        	log.error(e.getMessage());
        	throw e;
        }
        
        long after = System.nanoTime();
        log.info("Startup time = " + Long.toString((after-before)/(long)1e6));
	}

	private String[] loadConfiguration(String filename) throws DataStoreException,
			IOException {
		DataStore dataStore = environment.getDataStore();
        Config config = dataStore.getConfig();

        PiClientDescriptor descriptor = new PiClientDescriptor(filename);
        ConfigFile cf = config.getPiClientConfigFiles().getActive(descriptor);
    	
        String configText = cf.getText();

        if(configText.isEmpty())
        	log.error("No configuration found for Source Server Controller in " + filename);
                
    	return configText.split("\n");
	}

	@Override
	public void toFront() {
		// TODO Auto-generated method stub
		
	}

	
}
