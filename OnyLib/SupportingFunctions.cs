using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnyLib
{
    public static class SupportingFunctions
    {
        public static Dictionary<string, string> AskWolframAlpha(string AskString, bool verbose = false, bool getfresh = false)
        {
            Dictionary<string, string> kaas = new Dictionary<string, string>();

            string[] splittedstring = AskString.Split(' ');

            StringBuilder ToAsk = new StringBuilder();
            for (int i = 1; i < splittedstring.Length; i++)
            {
                ToAsk.Append(Uri.EscapeDataString(splittedstring[i]));
                if (i < splittedstring.Length - 1)
                    ToAsk.Append("+");
            }


            //wolfram api code is appid=V58TQE-W9RQLTHJKX
            //http://api.wolframalpha.com/v2/query?input=

            Uri requesturi = new Uri("http://api.wolframalpha.com/v2/query?input=" + AskString + "&appid=V58TQE-W9RQLTHJKX");
            CedLib.Networking.WebFunctions.CachedReply Cachedreply = CedLib.Networking.WebFunctions.CachedRequest(requesturi, getfresh, null, null);

            System.Xml.XmlReader xmlreader = System.Xml.XmlReader.Create(Cachedreply.Reply);


            while (xmlreader.Read())
            {
                string podname = "";
                string podcontent = "";
                if (xmlreader.NodeType != System.Xml.XmlNodeType.Whitespace)
                {
                    if (xmlreader.IsStartElement() && xmlreader.Name == "pod")
                    {
                        podname = xmlreader.GetAttribute("title");
                        bool continuelooping = true;
                        while (xmlreader.Read() && continuelooping)
                        {
                            if (xmlreader.NodeType != System.Xml.XmlNodeType.Whitespace && xmlreader.Name == "plaintext")
                            {
                                podcontent = xmlreader.ReadElementContentAsString();
                                continuelooping = false;
                            }
                        }
                        if (kaas.Count > 1 && !verbose)
                            break; //against spam :l
                    }
                }
                if (podname != "")
                    kaas.Add(podname, podcontent);
            }

            Cachedreply.Reply.Close();
            return kaas;
        }

        public static Dictionary<string, string> AskWikipedia(string AskString, bool verbose = false, bool getfresh = false)
        {
            Dictionary<string, string> kaas = new Dictionary<string, string>();

            string[] splittedstring = AskString.Split(' ');

            StringBuilder ToAsk = new StringBuilder();
            for (int i = 1; i < splittedstring.Length; i++)
            {
                ToAsk.Append(Uri.EscapeDataString(splittedstring[i]));
                if (i < splittedstring.Length - 1)
                    ToAsk.Append("+");
            }


            //wolfram api code is appid=V58TQE-W9RQLTHJKX
            //http://api.wolframalpha.com/v2/query?input=

            Uri requesturi = new Uri("http://api.wolframalpha.com/v2/query?input=" + AskString + "&appid=V58TQE-W9RQLTHJKX");
            CedLib.Networking.WebFunctions.CachedReply Cachedreply = CedLib.Networking.WebFunctions.CachedRequest(requesturi, getfresh, null, null);

            System.Xml.XmlReader xmlreader = System.Xml.XmlReader.Create(Cachedreply.Reply);


            while (xmlreader.Read())
            {
                string podname = "";
                string podcontent = "";
                if (xmlreader.NodeType != System.Xml.XmlNodeType.Whitespace)
                {
                    if (xmlreader.IsStartElement() && xmlreader.Name == "pod")
                    {
                        podname = xmlreader.GetAttribute("title");
                        bool continuelooping = true;
                        while (xmlreader.Read() && continuelooping)
                        {
                            if (xmlreader.NodeType != System.Xml.XmlNodeType.Whitespace && xmlreader.Name == "plaintext")
                            {
                                podcontent = xmlreader.ReadElementContentAsString();
                                continuelooping = false;
                            }
                        }
                        if (kaas.Count > 1 && !verbose)
                            break; //against spam :l
                    }
                }
                if (podname != "")
                    kaas.Add(podname, podcontent);
            }

            Cachedreply.Reply.Close();
            return kaas;
        }

    }
}
