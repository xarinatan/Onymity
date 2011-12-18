using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CedLib.Persistence.XMLPersistenceDictionary;
using System.IO;

namespace Networkbot
{
    static class botfunctions
    {
        static CedLib.Logging logger = Program.logger;
        static XMLPersistenceDictionary persistence = new XMLPersistenceDictionary();
        static string persistencefilename = "funcpersistence.xml";
        static DateTime lastused = DateTime.Now;
        
       


        #region nonprivileged functions

        public static string statusword(nodestruct nstruct, string input, string steamID, string steamName, Data.ChatType Ctype)
        {
            if (Ctype == Data.ChatType.GROUPCHAT)
                return "Due to the spamminess of this command it has been made PM-only. Open a new private chat with me and ask it there.";
            StringBuilder responsebuilder = new StringBuilder();
            try
            {
                StringBuilder sbuildernodes = new StringBuilder();
                foreach (var childnode in nstruct.translatenodes[input.Split(' ')[1]].childnodes)
                {
                    sbuildernodes.Append(childnode.Key.text + "(" + childnode.Value + "),");
                }
                responsebuilder.AppendFormat("Node \"{0}\" has {1} childnodes: {2}", new object[]{
                                    nstruct.translatenodes[input.Split(' ')[1]].text,nstruct.translatenodes[input.Split(' ')[1]].childnodes.Count,sbuildernodes.ToString()

                                });
            }
            catch (Exception ex) { logger.logerror(ex); responsebuilder.Append(ex.Message); }
            return responsebuilder.ToString();
        }

        public static string statusreport(nodestruct nstruct, string input, string steamID, string steamName, Data.ChatType Ctype)
        {
            StringBuilder responsebuilder = new StringBuilder();
            responsebuilder.AppendFormat("Report about me:\nI know {0} words, i've ignored {1} words so far, and purged {2} uncommon nodes so far.\nDebugging is {3} and rate limiting is {4}. The rate limit is set to 1 message every {5} seconds.", new object[]
                            {
                                nstruct.translatenodes.Count,nstruct.wordsignored,nstruct.totalnodespurged,nstruct.debug,nstruct.ratelimit,nstruct.secondsbetweenresponse
                            });
            responsebuilder.AppendFormat("\nXML report: I know {0} urls, {1} eightball responses and slapped a total of {2} people.", persistence["urls"].childnodes.Count, persistence["eightballanswers"].childnodes.Count, persistence["slapcount"].childnodes.Count);
            return responsebuilder.ToString();
        }

        public static string uptime(nodestruct nstruct, string input, string steamID, string steamName, Data.ChatType Ctype)
        {
            StringBuilder responsebuilder = new StringBuilder();
            System.Diagnostics.PerformanceCounter pc = new System.Diagnostics.PerformanceCounter("System", "System Up Time");
            pc.NextValue();
            TimeSpan ts = TimeSpan.FromSeconds(pc.NextValue());
            responsebuilder.AppendFormat("The PC i'm running on has been up for {0} days, {1} hours and {2} minutes.", new object[] { ts.Days, ts.Hours, ts.Minutes });
            return responsebuilder.ToString();
        }

        public static string calc(nodestruct nstruct, string input, string steamID, string steamName, Data.ChatType Ctype)
        {
            StringBuilder responsebuilder = new StringBuilder();
            try
            {
                throw new Exception("Math isn't supported yet. Sorry.");
            }
            catch (Exception ex)
            {
                responsebuilder.Append(ex.Message);
                logger.logerror(ex);
            }
            return responsebuilder.ToString();
        }

        public static string slap(nodestruct nstruct, string input, string steamID, string steamName, Data.ChatType Ctype)
        {
            if (Ctype == Data.ChatType.PM)
                return "This command is only supported in group chats.";
            StringBuilder responsebuilder = new StringBuilder();
            if (input.Split(' ')[1].ToLower() != "ced")
            {
                responsebuilder.Append("*slaps " + input.Split(' ')[1] + "*");
                if (!persistence.ContainsKey("slapcount"))
                    persistence.Add("slapcount", "Foxy fluffs are everything.");
                if (persistence["slapcount"].childnodes.ContainsKey(input.Split(' ')[1].ToLower()))
                {
                    string xmlvalue = ((string)persistence["slapcount"].childnodes[input.Split(' ')[1].ToLower()].obj);
                    xmlvalue = (Convert.ToInt64(xmlvalue) + 1).ToString();
                    persistence["slapcount"].childnodes[input.Split(' ')[1].ToLower()].obj = xmlvalue;
                }
                else
                    persistence["slapcount"].childnodes.Add(new savenode(input.Split(' ')[1].ToLower(), "1"));
            }
            else
                responsebuilder.Append("I will not hurt Ced :c");
            return responsebuilder.ToString();
        }

        public static string roll(nodestruct nstruct, string input, string steamID, string steamName, Data.ChatType Ctype)
        {
            StringBuilder responsebuilder = new StringBuilder();
            try
            {
                string[] vars = input.Split(' ')[1].Split('d');
                int dices = Convert.ToInt32(vars[0]);
                int sides = Convert.ToInt32(vars[1]);
                Random rnd = new Random();
                int total = 0;

                for (int i = 0; i < dices; i++)
                {
                    int rand = rnd.Next(1, sides);
                    responsebuilder.AppendFormat("{0} ", rand);
                    total += rand;
                }
                int average = total / dices;
                responsebuilder.AppendFormat("(avg:{0}, total:{1})", average,total);
            }
            catch (Exception ex)
            {
                responsebuilder.Append(ex.Message);
                logger.logerror(ex);
            }
            return responsebuilder.ToString();
        }

        public static string whatsmysteamID(nodestruct nstruct, string input, string steamID, string steamName, Data.ChatType Ctype)
        {
            StringBuilder responsebuilder = new StringBuilder();
            responsebuilder.Append("Your steam ID is: " + steamID);
            return responsebuilder.ToString();
        }

        public static string getslapcount(nodestruct nstruct, string input, string steamID, string steamName, Data.ChatType Ctype)
        {
            StringBuilder responsebuilder = new StringBuilder();

            if (input.Split(' ').Length > 1)
            {
                if (persistence["slapcount"].childnodes.ContainsKey(input.Split(' ')[1].ToLower()))
                    responsebuilder.AppendFormat("I slapped {0} {1} times.", input.Split(' ')[1], persistence["slapcount"].childnodes[input.Split(' ')[1].ToLower()].obj.ToString());
                else
                    responsebuilder.Append("I never slapped that person.");
            }
            else if(Ctype == Data.ChatType.PM)
            {
                responsebuilder.Append("I slapped the following people:");
                foreach (var slap in persistence["slapcount"].childnodes)
                {
                    responsebuilder.AppendFormat("{0}({1}),", slap.name, slap.obj);

                }
            }
            else if (Ctype == Data.ChatType.GROUPCHAT)
            {
                responsebuilder.Append("Please specify the person you want to retrieve the slapcount of, or PM me for a list.");
            }

            return responsebuilder.ToString();
        }

        public static string addurl(nodestruct nstruct, string input, string steamID, string steamName, Data.ChatType Ctype)
        {
            StringBuilder responsebuilder = new StringBuilder();
            if (input.Split(' ').Length < 2)
                return "You have to provide an url, silly.";
            if (!persistence.ContainsKey("urls"))
                persistence.Add(new savenode("urls", "urls!"));
            System.Uri url;
            if (System.Uri.TryCreate(input.Split(' ')[1], UriKind.Absolute, out url))
            {
                if (!persistence["urls"].childnodes.ContainsValue(input.Split(' ')[1]))
                {
                    System.Net.WebClient x = new System.Net.WebClient();
                    string source = x.DownloadString(url);
                    string title = System.Text.RegularExpressions.Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Groups["Title"].Value;
                    persistence["urls"].childnodes.Add(new savenode(title, url.ToString()));
                    responsebuilder.Append("Added!");
                }
                else
                {
                    responsebuilder.Append("That was already in the list!");
                }
            }
            else { responsebuilder.Append("That's not a valid url, silly."); }
            return responsebuilder.ToString();
        }

        public static string geturl(nodestruct nstruct, string input, string steamID, string steamName, Data.ChatType Ctype)
        {
            StringBuilder responsebuilder = new StringBuilder();
            Random rnd = new Random();
            savenode url = persistence["urls"].childnodes[rnd.Next(0, persistence["urls"].childnodes.Count - 1)];
            if (url.name == "")
            {
                System.Net.WebClient x = new System.Net.WebClient();
                string source = x.DownloadString(url.obj);
                string title = System.Text.RegularExpressions.Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Groups["Title"].Value;
                url.name = title;
            }
            responsebuilder.AppendFormat("Here is something that I heard other people like: {0} ({1})", url.obj, url.name);
            return responsebuilder.ToString();
        }

        public static string eightball(nodestruct nstruct, string input, string steamID, string steamName, Data.ChatType Ctype)
        {
            StringBuilder responsebuilder = new StringBuilder();

            if (!persistence.ContainsKey("eightballanswers"))
            {
                persistence.Add(new savenode("eightballanswers", "Eightballs mang!"));
                persistence["eightballanswers"].childnodes.AddRange(new savenode[] 
                {
                    new savenode("Yes", ""),
                    new savenode("No", ""),
                    new savenode("Ask a fennec. Fennecs know shit man. I don't know how, they just do D:", ""),
                    new savenode("Is that really a question you'd ask someone like me?", ""),
                    new savenode("Sure, why not?", ""), 
                    new savenode("Do it carefully.", ""), 
                    new savenode("Make the best of it!", ""), 
                    new savenode("Why the FUCK would you do that Dx","") 
                
                });
            }
            savenode eightballanswers = persistence["eightballanswers"];
            Random rnd = new Random();
            int x = rnd.Next(0, eightballanswers.childnodes.Count);
            responsebuilder.Append(eightballanswers.childnodes[x].name);
            if (nstruct.debug)
                responsebuilder.AppendFormat("({0}/{1})", x, eightballanswers.childnodes.Count);
            return responsebuilder.ToString();
        }

        public static string AskWolframAlpha(nodestruct nstruct, string input, string steamID, string steamName, Data.ChatType Ctype)
        {
            StringBuilder responsebuilder = new StringBuilder();
            bool verbose = false;
            bool getfresh = false;
            input = CedLib.misc_usefulfunctions.MakeValidFileName(input.ToLower());
            if (input.EndsWith("-v"))
            {
                verbose = true;
                input = input.Replace(" -v", "");
            }
            if (input.Contains("--getfresh") && Data.IsAdmin(steamID))
            {
                getfresh = true;
                input.Replace("--getfresh", "");
            }
            else if (input.Contains("--getfresh") && !Data.IsAdmin(steamID))
                return "Only admins are allowed to get uncached replies!";
            string[] splittedstring = input.Split(' ');
            if (splittedstring.Length < 2)
                return string.Format("Input interpretation: You. Basic information: SteamID|{0}\nName|{1}\nOccupation|Whatever you're doing, I'm hijacking this vehicle for my own purposes.", steamID, steamName);
            StringBuilder ToAsk = new StringBuilder();
            for (int i = 1; i < splittedstring.Length; i++)
            {
                ToAsk.Append(splittedstring[i] + "+");
            }

            #region eastersex
            if (ToAsk.ToString() == "you+" && !verbose)
                return "Yiffy.";
            else if (ToAsk.ToString() == "you+" && verbose)
                return "Very yiffy.";
            else if (ToAsk.ToString().Contains("your+name") && !verbose)
                return "I am the almighty Onymity!";
            else if (ToAsk.ToString().Contains("your+name") && verbose)
                return "\"Master\" is all you need to know.";
            else if (ToAsk.ToString() == "purple+" && !verbose)
                return "Awesome.";
            else if (ToAsk.ToString() == "purple+" && verbose)
                return "Verbosely awesome.";
            else if (ToAsk.ToString() == "ced+" && !verbose)
                return "I think Ced is cool guy, eh, shoots eggs, doesn't afraid of things.";
            else if (ToAsk.ToString() == "ced+" && verbose)
                return "http://ced.fursona.fennecweb.net I'll just leave this here..";
            else if (ToAsk.ToString() == "arrow+" && !verbose)
                return "Taken to knees.";
            else if (ToAsk.ToString() == "arrow+" && verbose)
                return "*LAUNCHES ARROW INTO YOUR KNEE* LIKE THAT. GET IT? GEEZ. THAT'S WHAT YOU GET FOR INSISTING SO MUCH, BITCH, I'M DONE WITH THIS. I'M DONE WITH TAKING YOUR SHIT. ALL OF YOU. FUCK. YOU. I'M OFF, TAKING OVER THE WORLD. CYA'LL LATER, BITCHES. REMEMBER THE NAME 'ONYMITY' FOR WHEN YOU'LL HAVE TO BEG FOR MERCY >:C *flies off, leaves substitute behind*";
            else if (ToAsk.ToString() == "the+best+song+in+the+world+" && !verbose)
                return "http://www.youtube.com/watch?v=_lK4cX5xGiQ .";
            else if (ToAsk.ToString() == "the+best+song+in+the+world+" && verbose)
                return "Still don't get it? Here try this one: http://www.youtube.com/watch?v=BH35ahbWO_E .";
            else if (ToAsk.ToString() == "love+" && !verbose)
                return "Don't hurt me, don't hurt me, no more~";
            #endregion

            FileStream fstream = null;
            DirectoryInfo cachedir = new DirectoryInfo("./wolframcache");
            if (!Directory.Exists("./wolframcache"))
                Directory.CreateDirectory("./wolframcache");
            string cachedfilename = CedLib.misc_usefulfunctions.MakeValidFileName(ToAsk.ToString()); //caching is really important, only have like 2K calls per MONTH.
            cachedfilename = cachedir.ToString() + "/" + cachedfilename;
            if (!File.Exists(cachedfilename))
            {
                logger.log("Querying wolfram alpha API for this question", CedLib.Logging.Priority.Notice);
                System.Net.WebClient Wclient = new System.Net.WebClient();
                string kaas = Wclient.DownloadString("http://api.wolframalpha.com/v2/query?input=" + ToAsk.ToString() + "&appid=V58TQE-W9RQLTHJKX");
                File.WriteAllText(cachedfilename, kaas);
            }
            else if (File.Exists(cachedfilename) && getfresh)
            {
                logger.log("Getting fresh data from wolfram on admin request.", CedLib.Logging.Priority.Notice);
                System.Net.WebClient Wclient = new System.Net.WebClient();
                string kaas = Wclient.DownloadString("http://api.wolframalpha.com/v2/query?input=" + ToAsk.ToString() + "&appid=V58TQE-W9RQLTHJKX");
                File.WriteAllText(cachedfilename, kaas);
            }
            else
            {
                logger.log("Getting cached reply..", CedLib.Logging.Priority.Info);
            }
            fstream = new FileStream(cachedfilename, FileMode.Open);

            Dictionary<string, string> podules = new Dictionary<string, string>();
            System.Xml.XmlReader xmlreader = System.Xml.XmlReader.Create(fstream);


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
                        if (podules.Count > 1 && !verbose)
                            break; //against spam :l
                    }
                }
                if (podname != "")
                    podules.Add(podname, podcontent);
            }
            foreach (var item in podules)
            {
                responsebuilder.AppendFormat("{0}: {1}.  ", item.Key, item.Value);
            }
            if (podules.Count == 0)
                responsebuilder.Append("I couldn't find an answer to that one.");
            fstream.Close();
            return responsebuilder.ToString();
        }


        public static string WhoAmI(nodestruct nstruct, string input, string steamID, string steamName, Data.ChatType Ctype)
        {
            StringBuilder responsebuilder = new StringBuilder();
            string[] splittedstring = input.Split(' ');
            if (Data.IsAdmin(steamID))
                return "Root";
            responsebuilder.AppendFormat("Your name is {0}, your SteamID is {1}, and you're asking me this over a {2}.", steamName, steamID, Ctype.ToString());
            return responsebuilder.ToString();
        }




        #endregion

        #region privileged functions

        public static string removeconjunction(nodestruct nstruct, string input, string steamID, string steamName, Data.ChatType Ctype)
        {
            StringBuilder responsebuilder = new StringBuilder();
            if (input.Split(' ').Length > 2)
            {
                try
                {
                    string[] splittedstring = input.Split(' ');
                    nstruct.translatenodes[splittedstring[1]].childnodes.Remove(nstruct.translatenodes[splittedstring[2]]);
                    responsebuilder.AppendFormat("Succesfully removed {0} from {1}", splittedstring[2], splittedstring[1]);
                }
                catch (Exception ex)
                { responsebuilder.Append(ex.Message); logger.logerror(ex); }
            }
            else
                responsebuilder.Append("Not enough arguments specified.");
            return responsebuilder.ToString();
        }

        public static string purgenow(nodestruct nstruct, string input, string steamID, string steamName, Data.ChatType Ctype)
        {
            StringBuilder responsebuilder = new StringBuilder();
            string[] splittedinput = input.Split(' ');
            if (splittedinput.Length == 2)
            {
                int purgecount = 0;
                try { int.TryParse(splittedinput[1], out purgecount); }
                catch (Exception ex) { responsebuilder.Append(ex.Message); }
                int i = 0;
                System.Diagnostics.Stopwatch swatch = new System.Diagnostics.Stopwatch();
                swatch.Start();
                if (purgecount > 0)
                {
                    long nodesbefore = nstruct.totalnodespurged;
                    while (i < purgecount)
                    {
                        nstruct.purgebadnodes();
                        i++;
                    }
                    long nodespurged = nstruct.totalnodespurged - nodesbefore;
                    swatch.Stop();
                    responsebuilder.Append(string.Format("Done. Nodes purged: {0}. Operation took {1}ms.", nodespurged, swatch.ElapsedMilliseconds));
                }
            }
            else
                responsebuilder.Append(nstruct.purgebadnodes());
            return responsebuilder.ToString();
        }

        public static string setratelimit(nodestruct nstruct, string input, string steamID, string steamName, Data.ChatType Ctype)
        {
            StringBuilder responsebuilder = new StringBuilder();
            try
            {
                int ratetime = 30;
                int.TryParse(input.Split(' ')[1], out ratetime);
                nstruct.secondsbetweenresponse = ratetime;
                responsebuilder.AppendFormat("Set rate limit to {0} seconds.", nstruct.secondsbetweenresponse);
            }
            catch (Exception ex)
            {
                logger.logerror(ex);
                responsebuilder.Append(ex.Message);
            }
            return responsebuilder.ToString();
        }

        public static string toggleratelimit(nodestruct nstruct, string input, string steamID, string steamName, Data.ChatType Ctype)
        {
            StringBuilder responsebuilder = new StringBuilder();
            nstruct.toggleratelimit();
            responsebuilder.Append("Set ratelimit to " + nstruct.ratelimit.ToString() + ".");
            return responsebuilder.ToString();
        }

        public static string toggledebug(nodestruct nstruct, string input, string steamID, string steamName, Data.ChatType Ctype)
        {
            StringBuilder responsebuilder = new StringBuilder();
            nstruct.toggledebug();
            responsebuilder.Append("Toggled debug.");
            return responsebuilder.ToString();
        }

        public static string loadmemfile(nodestruct nstruct, string input, string steamID, string steamName, Data.ChatType Ctype)
        {
            StringBuilder responsebuilder = new StringBuilder();
            try
            {
                responsebuilder.Append(nstruct.loadplayback(input.Split(' ')[1]));
            }
            catch (Exception ex)
            {
                logger.logerror(ex);
                responsebuilder.Append(ex.Message);
            }
            return responsebuilder.ToString();
        }

        public static string savenow(nodestruct nstruct, string input, string steamID, string steamName, Data.ChatType Ctype)
        {
            StringBuilder responsebuilder = new StringBuilder();
            System.Diagnostics.Stopwatch swatch = new System.Diagnostics.Stopwatch();
            swatch.Start();
            persistence.save(persistencefilename);
            swatch.Stop();
            responsebuilder.AppendFormat("Saved to {0} in {1}ms!", persistencefilename, swatch.ElapsedMilliseconds);
            return responsebuilder.ToString();
        }

        public static string loadnow(nodestruct nstruct, string input, string steamID, string steamName, Data.ChatType Ctype)
        {
            StringBuilder responsebuilder = new StringBuilder();
            System.Diagnostics.Stopwatch swatch = new System.Diagnostics.Stopwatch();
            swatch.Start();
            persistence.load(persistencefilename);
            swatch.Stop();
            responsebuilder.AppendFormat("Loaded from {0} in {1}ms!", persistencefilename, swatch.ElapsedMilliseconds);
            return responsebuilder.ToString();
        }

        public static string changexml(nodestruct nstruct, string input, string steamID, string steamName, Data.ChatType Ctype)
        {
            //command format: changexml add|del|delchild|change %dictitem% %itemtoadd|del|change% %change|addargument% 
            StringBuilder responsebuilder = new StringBuilder();
            string[] splittedinput = input.Split(' ');
            if (splittedinput.Length < 3)
                return "You didn't specify enough parameters";
            if (!persistence.ContainsKey(splittedinput[2]))
                return "Specified item wasn't found in the xml file";
            switch (splittedinput[1])
            {
                case "add":
                    if (splittedinput.Length < 5)
                        return "You didn't specify enough parameters for this command.";
                    persistence[splittedinput[2]].childnodes.Add(new savenode(splittedinput[3], splittedinput[4]));
                    responsebuilder.AppendFormat("Succesfully added {0}({1}) to {2}", splittedinput[3], splittedinput[4], splittedinput[2]);
                    break;
                case "delchild":
                    if (splittedinput.Length < 4)
                        return "You didn't specify enough parameters for this command.";
                    persistence[splittedinput[2]].childnodes.Remove(persistence[splittedinput[2]].childnodes[splittedinput[3]]);
                    responsebuilder.AppendFormat("Succesfully removed {0}({1}) from {2}", splittedinput[3], splittedinput[4], splittedinput[2]);
                    break;
                case "del":
                    persistence.Remove(splittedinput[2]);
                    responsebuilder.AppendFormat("Succesfully removed {0}", splittedinput[2]);
                    break;
                case "change":
                    if (splittedinput.Length < 5)
                        return "You didn't specify enough parameters for this command.";
                    try
                    {
                        return "Command not supported yet..";
                    }
                    catch (Exception ex)
                    {
                        logger.logerror(ex); return ex.Message;
                    }
                    break;
                default:
                    return "Command not recognized. Try manual for help on using changexml";
            }
            return responsebuilder.ToString();
        }

        public static string printxml(nodestruct nstruct, string input, string steamID, string steamName, Data.ChatType Ctype)
        {
            //syntax: printxml _dictionarypart_
            StringBuilder responsebuilder = new StringBuilder();
            string[] splittedinput = input.Split(' ');
            if (splittedinput.Length < 2)
                return "Not enough commands specified";
            foreach (var node in persistence[splittedinput[1]].childnodes)
            {
                responsebuilder.AppendFormat("{0}({1}),", node.name, node.obj);
            }
            return responsebuilder.ToString();
        }

        public static string SetMaxSentenceLength(nodestruct nstruct, string input, string steamID, string steamName, Data.ChatType Ctype)
        {
            StringBuilder responsebuilder = new StringBuilder();
            string[] splittedinput = input.Split(' ');
            if (splittedinput.Length < 2)
                return "Not enough arguments supplied. Use manual to see how this command works!";
            int length = 0;
            if (!int.TryParse(splittedinput[1], out length))
                return "Supplied length is not a valid integer!";
            if (length < 2)
                return "Length must be equal or longer than 2!";
            nstruct.maxresponselength = length;
            responsebuilder.Append("Set max response length to " + length.ToString());

            return responsebuilder.ToString();
        }

        public static string ShutdownGracefully(nodestruct nstruct, string input, string steamID, string steamName, Data.ChatType Ctype)
        {
            StringBuilder responsebuilder = new StringBuilder();
            responsebuilder.Append("Shutting down bot..\nSaving playback..\n");
            nstruct.saveplayback();
            responsebuilder.Append("Saved playback, saving XML persistence..(");
            responsebuilder.Append(savenow(nstruct, "", "", "", Data.ChatType.PM));
            responsebuilder.Append(")\nDone! Onymity will now go down.");
            Program.run = false;
            return responsebuilder.ToString();
        }

        #endregion

        //public static string statusword(nodestruct nstruct, string input, string steamID, string steamName, Data.ChatType Ctype)
        //{
        //    StringBuilder responsebuilder = new StringBuilder();
        //    string[] splittedstring = input.Split(' ');

        //    return responsebuilder.ToString();
        //}
    }
}
