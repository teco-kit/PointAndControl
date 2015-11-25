using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGS.ComponentHandling
{
    public interface IEventlogFormat
    {
        bool write(EventLogger.logEntry writeString);
        List<EventLogger.logEntry> read();
        EventLogger.logEntry readSpecifiedEntry(int nr);
        void setLogPath(string logPath);
        
    }
}
