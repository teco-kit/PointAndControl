using System;
using System.Collections.Generic;
using PointAndControl.Devices;
using System.Net;
using System.IO;
using System.Xml;

namespace PointAndControl.ThirdPartyRepos
{ 
     class OpenHABDeviceTranslation
    {
        public bool isTopSitemap { get; set; }
        public string sitemapName { get; set; }
        public string itemName { get; set; }
        public OpenHABDeviceTranslation()
        {}
    }
    public class OpenHABDeviceReader : IRepoDeviceReader
    {
        private const string APPSITEMAP = "/openhab.app?sitemap=";
        private List<ExternalDevice> devices { get; set; }
        private string URL { get; set; } 
        private string restURL { get; set; }
        public OpenHABDeviceReader(String baseUrl)
        {
            devices = new List<ExternalDevice>();
            URL = baseUrl;
            restURL = URL + "/rest/sitemaps";
        }


        private void startReadSitemaps()
        {
            string entrySitemaps = getRestSitemaps(restURL);
            if (entrySitemaps == "")
                return;
            XmlDocument doc = new XmlDocument();
            XmlNode currentNode;
            doc.LoadXml(entrySitemaps);


            foreach(XmlNode n in doc.ChildNodes)
            {
                if (n.Name == "xml")
                    continue;

                foreach(XmlNode innerNode in n.ChildNodes)
                {
                    if (innerNode.Name == "sitemap")
                    {
                        currentNode = findLinkAndRetrieveSitemapNode(innerNode);
                        if (currentNode == null)
                            continue;

                        readSitemap(currentNode.ChildNodes[1]);
                    }
                }

                
            }

            return;

        }

        private void readSitemap(XmlNode sitemapNode)
        {
            OpenHABDeviceTranslation translation = new OpenHABDeviceTranslation();
            translation.isTopSitemap = true;
            foreach(XmlNode n in sitemapNode.ChildNodes)
            {
                switch (n.Name)
                {
                    case "name":
                        translation.sitemapName = n.InnerText;
                        break;
                    //case "label":
                    //    break;
                    //case "link":
                    //    break;
                    case "homepage":
                        readHomepage(n, translation);
                        break;
                    default:
                        continue;
                }
            }
        }

        private void readHomepage(XmlNode homepageNode, OpenHABDeviceTranslation translation)
        {
            bool leaf = false;
            foreach(XmlNode n in homepageNode.ChildNodes)
            {
                switch (n.Name)
                {
                    //case "id":
                    //    break;
                    //case "title":
                    //    break;
                    //case "link":
                    //    break;
                    case "leaf":
                        leaf = bool.Parse(n.InnerText);
                        break;
                    case "widget":
                        readWidget(n, translation);
                        break;
                    default:
                        continue;
                }
            }
        }

        private void readWidget(XmlNode widgetNode, OpenHABDeviceTranslation translation)
        {
            foreach(XmlNode n in widgetNode.ChildNodes)
            {
                switch (n.Name)
                {
                    //case "widgetId":
                    //    break;
                    //case "type":
                    //    break;
                    //case "label":
                    //    break;
                    //case "icon":
                    //    break;
                    case "widget":
                        readWidget(n, translation);
                        break;
                    case "item":
                        readItem(n, translation);
                        break;
                    case "linkedPage":
                        readLinkedPage(n, translation);
                        break;
                    default:
                        continue;
                }
            }
        }

        private void readItem(XmlNode itemNode, OpenHABDeviceTranslation translation)
        {
            foreach(XmlNode n in itemNode.ChildNodes)
            {
                switch (n.Name)
                {
                    case "type":
                        if(n.InnerText == "GroupItem")
                        {
                            return;
                        } else
                        {
                            break;
                        }
                    case "name":
                        translation.itemName = n.InnerText;
                        break;
                    //case "state":
                    //    break;
                    //case "link":
                    //    break;
                    default:
                        continue;
                }
            }

            addDevice(translation);
        }

        private void readLinkedPage(XmlNode lpNode, OpenHABDeviceTranslation translation)
        {
            foreach(XmlNode n in lpNode.ChildNodes)
            {
                switch (n.Name)
                {
                    case "id":
                        translation.isTopSitemap = false;
                        translation.sitemapName = n.InnerText;
                        break;
                    //case "title":
                    //    break;
                    //case "icon":
                    //    break;
                    //case "link":
                    //    break;
                    //case "leaf":
                    //    break;
                    case "widget":
                        readWidget(n, translation);
                        break;
                }
            }
        }

        private string generateAppPath(OpenHABDeviceTranslation translation)
        {
            String appPath = "";
            if (translation.isTopSitemap)
            {
                appPath = URL + APPSITEMAP + translation.sitemapName;
            } else
            {
                appPath = URL + APPSITEMAP + "#_" + translation.sitemapName;
            }
            return appPath;
        }

        private string getRestSitemaps(string pointingURL)
        {
            string responseFromServer = "";

            
            try
            {
                WebRequest request = WebRequest.Create(pointingURL);
                request.Credentials = CredentialCache.DefaultCredentials;
                WebResponse response = request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                responseFromServer = reader.ReadToEnd();

                reader.Close();
                response.Close();
            }
            catch (WebException)
            {
                Console.WriteLine("Could not connect to server, probably not started");
            }


            return responseFromServer;

            // String response = connection.Send(pointingURL);

        
        }

        private XmlNode findLinkAndRetrieveSitemapNode(XmlNode sitemap)
        {
            XmlDocument doc = new XmlDocument();
            string xml = "";
            foreach(XmlNode n in sitemap.ChildNodes)
            {
                if(n.Name == "link")
                {
                    xml = getRestSitemaps(n.InnerText);
                }
            }
            if(xml == "")
            {
                Console.WriteLine("Sitemap could not be loaded - connection error");
                return null;
            }
            doc.LoadXml(xml);

            return doc;
        }

        public List<ExternalDevice> read()
        {
            devices.Clear();
            startReadSitemaps();
            return devices;
        }

        private void addDevice(OpenHABDeviceTranslation translation)
        {
            if (devices.Exists(device => translation.itemName == device.Id))
                return;

            devices.Add(new ExternalDevice(translation.itemName, translation.itemName, generateAppPath(translation), new List<Ball>()));
        }
       
    }
}
