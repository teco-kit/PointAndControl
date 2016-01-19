using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace IGS.ComponentHandling
{ 
    public class EventLogger
    {
        public struct logEntry
        {
            public string logText { get; set; }
            public DateTime dt { get; set; }
        }
        private ConcurrentQueue<logEntry> eventQueue { get; set; }
        
        private Thread workerThread { get; set; }

        public IEventlogFormat logFormat { get; set; }

        public EventLogWriter writer { get; set; }

        public EventLogger()
        {
            eventQueue = new ConcurrentQueue<logEntry>();
            logFormat = new XMLEventLogFormat(AppDomain.CurrentDomain.BaseDirectory + "program_log");
            writer = new EventLogWriter(eventQueue, logFormat);
            workerThread = new Thread(writer.doWork);
        }

        public void enqueueEntry(string text)
        {
            DateTime dt = DateTime.Now;
            logEntry entry = new logEntry();
            entry.dt = dt;
            entry.logText = text;

            eventQueue.Enqueue(entry);
            
            if (!workerThread.IsAlive)
            {
                Console.WriteLine("EnqueEntry Start");
                workerThread = new Thread(writer.doWork);
                workerThread.Start();
            } else
            {
                Console.WriteLine("Still in working");
            }
        }
    }
}
