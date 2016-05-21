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

        public XMLEventLogFormat(string lPath)
        {
            logPath = lPath;
            if (!File.Exists(logPath + FILE_ENDING))
            {
                XElement cleanNode = new XElement("log");
                cleanNode.Save(logPath + FILE_ENDING);
            }
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


            foreach (XmlNode entry in root.ChildNodes)
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

            XmlDocument docConfig = new XmlDocument();
            docConfig.Load(logPath + FILE_ENDING);
            XmlNode root = docConfig.SelectSingleNode("/log");

            //if(root.ChildNodes.Count < nr - 1)
            //{
            //    return null;
            //}

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


        //TODO: If spinLoad or spinSave fails, better behaviour could be implemented
        public bool write(EventLogger.logEntry writeString)
        {

            XmlDocument docConfig = new XmlDocument();

            if (!spinLoad(docConfig))
            {
                Console.WriteLine("Log locked for 2 seconds");
                return false;
            }

            XmlNode root = docConfig.SelectSingleNode("/log");

            XmlElement newEntry = docConfig.CreateElement("Entry");

            newEntry.SetAttribute("Date_And_Time", writeString.dt.ToString());
            newEntry.InnerText = writeString.logText;

            root.AppendChild(newEntry);

            if (!spinSave(docConfig))
            {
                Console.WriteLine("Log locked for 2 seconds");
                return false;
            }

            return true;
        }

        //TODO: trySave and tryLoad and spinSave and spinLoad nearly the same - decide if Interface or deciderVariable should be used to fuse them
        private bool trySave(XmlDocument docConfig)
        {
            try
            {
                docConfig.Save(logPath + FILE_ENDING);
            }
            catch (IOException)
            {
                return false;
            }
            return true;
        }

        private bool tryLoad(XmlDocument docConfig)
        {
            try
            {
                docConfig.Load(logPath + FILE_ENDING);
            }
            catch (IOException)
            {
                return false;
            }
            return true;
        }

        private bool spinLoad(XmlDocument docConfig)
        {
            bool success = false;
            Stopwatch watch = new Stopwatch();

            watch.Start();
            while (!success && watch.ElapsedMilliseconds <= 2000)
            {
                success = tryLoad(docConfig);
            }

            return success;
        }

        private bool spinSave(XmlDocument docConfig)
        {
            bool success = false;
            Stopwatch watch = new Stopwatch();

            watch.Start();
            while (!success && watch.ElapsedMilliseconds <= 2000)
            {
                success = trySave(docConfig);
            }

            return success;
        }
    }
}
