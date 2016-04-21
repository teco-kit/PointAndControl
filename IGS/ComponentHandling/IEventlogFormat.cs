using System.Collections.Generic;

namespace PointAndControl.ComponentHandling
{
    public interface IEventlogFormat
    {
        bool write(EventLogger.logEntry writeString);
        List<EventLogger.logEntry> read();
        EventLogger.logEntry readSpecifiedEntry(int nr);
        void setLogPath(string logPath);
        
    }
}
