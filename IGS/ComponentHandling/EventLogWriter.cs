using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGS.ComponentHandling
{ 
    public class EventLogWriter
    {
        private volatile bool _shouldStop;
    
        public ConcurrentQueue<EventLogger.logEntry> eventQueue { get; set; }
        public IEventlogFormat format { get; set; }
        public EventLogWriter(ConcurrentQueue<EventLogger.logEntry> queue, IEventlogFormat logFormat)
        {
            eventQueue = queue;
            format = logFormat;
        }

        public void doWork()
        {

            EventLogger.logEntry dequeued = new EventLogger.logEntry();

            while (eventQueue.Count() > 0 || !_shouldStop)
            {
                eventQueue.TryDequeue(out dequeued);
                format.write(dequeued);
            }
            requestStop();
        }

        public void requestStop()
        {
            _shouldStop = true;
        }


    }
}
