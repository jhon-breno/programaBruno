using System;
using System.Configuration;
using System.Diagnostics;

namespace Pangea.Util.SFTP
{
    public class EventLogFactory
    {

        public AppSettingsReader configurationAppSettings{get; private set;}

        public EventLogFactory(AppSettingsReader configurationAppSettings)
        {
            this.configurationAppSettings = configurationAppSettings;
        }
        
        public EventLog GetEventLog(string source)
        {
            EventLog eventLog = new EventLog();
            eventLog.Source = configurationAppSettings.GetValue(source, typeof(String)).ToString();
            return eventLog;
        }
    }
}
