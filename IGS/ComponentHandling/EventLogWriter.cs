using System.Collections.Concurrent;
using System.Linq;

namespace PointAndControl.ComponentHandling
{
    public class EventLogWriter
    {
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

            while (eventQueue.Count() > 0)
            {
                eventQueue.TryDequeue(out dequeued);
                format.write(dequeued);
            }
           
        }
    }
}
