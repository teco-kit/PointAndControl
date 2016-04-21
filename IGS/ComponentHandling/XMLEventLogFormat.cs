using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace PointAndControl.ComponentHandling
{
    public class XMLEventLogFormat : IEventlogFormat
    {
        private string logPath;
        private const string FILE_ENDING = ".xml";
        private Stopwatch timeoutTimer { get; set; }


        public XMLEventLogFormat(string lPath)
        {
            logPath = lPath;
            timeoutTimer = new Stopwatch(); 
        }

        public List<EventLogger.logEntry> read()
        {
            List<EventLogger.logEntry> entries = new List<EventLogger.logEntry>();
            if (!File.Exists(logPath + FILE_ENDING))
            {
                return entries;
            }
           
            XmlDocument docConfig = new XmlDocument();
            docConfig.Load(logPath + FILE_ENDING);
            XmlNode root = docConfig.SelectSingleNode("/log");

            
            foreach(XmlNode entry in root.ChildNodes)
            {
                EventLogger.logEntry lEntry = new EventLogger.logEntry();

                lEntry.dt = DateTime.Parse(entry.Attributes[0].Value);
                lEntry.logText = entry.Value;

                entries.Add(lEntry);
            }

            return entries;
            
        }

        public EventLogger.logEntry readSpecifiedEntry(int nr)
        {
            if (!File.Exists(logPath + FILE_ENDING))
            {
                
            }

            XmlDocument docConfig = new XmlDocument();
            docConfig.Load(logPath + FILE_ENDING);
            XmlNode root = docConfig.SelectSingleNode("/log");

            XmlNode lookedFor = root.ChildNodes[nr - 1];

            EventLogger.logEntry entry = new EventLogger.logEntry();

            entry.dt = DateTime.Parse(lookedFor.Attributes[0].Value);
            entry.logText = lookedFor.Value;

            return entry;
            
        }

        public void setLogPath(string logPath)
        {
            this.logPath = logPath;
        }

        public bool write(EventLogger.logEntry writeString)
        {
            if(!File.Exists(logPath + FILE_ENDING))
            {
                XElement cleanNode = new XElement("log");
                cleanNode.Save(logPath + FILE_ENDING);
            }

            XmlDocument docConfig = new XmlDocument();
            docConfig.Load(logPath + FILE_ENDING);
            XmlNode root = docConfig.SelectSingleNode("/log");

            XmlElement newEntry = docConfig.CreateElement("Entry");
            
            newEntry.SetAttribute("Date_And_Time", writeString.dt.ToString());
            newEntry.InnerText = writeString.logText;

            root.AppendChild(newEntry);

            bool writeSuccess = false;
            bool writeFailure = false;

            while (writeSuccess == false && writeFailure == false)
            {
                writeSuccess = trySave(docConfig);
                if (writeSuccess == false && !timeoutTimer.IsRunning == false)
                {
                    timeoutTimer.Start();
                }
                else if (writeSuccess == false && timeoutTimer.IsRunning == true && timeoutTimer.ElapsedMilliseconds > 2000)
                {
                    writeFailure = true;
                }
            }

            timeoutTimer.Reset();

            if (writeFailure == true)
            {
                //handle a failure behavor
                Console.WriteLine("Log locked for 2 seconds");
                return false;
            }

            return true;
        }


        private bool trySave(XmlDocument docConfig)
        {
            try
            {
                docConfig.Save(logPath + FILE_ENDING);
                return true;
            } catch (IOException)
            {
                return false;
            }
        }
    }
}
