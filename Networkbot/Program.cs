using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CedLib;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;

namespace Networkbot
{
    class Program
    {
        static nodestruct nstruct = new nodestruct(true);
        public static bool run = true;
        public static Logging logger = new Logging(true, true);
        public static DateTime starttime = DateTime.Now;
        static int port = 5000;
        static Socket mainsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static Dictionary<string, KeyValuePair<string, Func<nodestruct, string, string, string, Data.ChatType, string>>> functiondict = new Dictionary<string, KeyValuePair<string, Func<nodestruct, string, string, string, Data.ChatType, string>>>();
        static Dictionary<string, KeyValuePair<string, Func<nodestruct, string, string, string, Data.ChatType, string>>> privfunctiondict = new Dictionary<string, KeyValuePair<string, Func<nodestruct, string, string, string, Data.ChatType, string>>>();

        [DllImport("CedTurboLib.dll")]
        static extern void printshit(string stufftoprint);


        static void Main(string[] args)
        {
            printshit("test");
            addfunctionstodict();
            nstruct.blacklist.Add("onymity");
            if (!Networking.socketfunctions.trybindsocket(mainsock, ref port, true, 50, IPAddress.Any))
            { logger.log("FAILED TO BIND TO ANY PORT. EXITTING", Logging.Priority.Critical); return; }
            logger.log("Initialized main function", Logging.Priority.Info);
            logger.log("Loading variables..",Logging.Priority.Notice);

            try
            { botfunctions.loadnow(nstruct, "", "","",Data.ChatType.PM); }
            catch (Exception ex)
            { logger.logerror(ex); }
            logger.log("Trying to load playback file..", Logging.Priority.Notice);
            // nstruct.loadplayback(); 
            try { nstruct.loadplayback(); }
            catch (Exception ex) { logger.logerror(ex); }
            long amountloops = 0;
            while (run)
            {
                if ((amountloops % 10) == 0) { nstruct.saveplayback(); amountloops = 0; }
                logger.log("Waiting for incoming commands", Logging.Priority.Info);
                Socket incomingsock = mainsock.Accept();
                System.Diagnostics.Stopwatch swatch = new System.Diagnostics.Stopwatch();
                swatch.Start();
                logger.log("Got connection from: " + incomingsock.RemoteEndPoint, Logging.Priority.Notice);
                if (Networking.socketfunctions.waitfordata(incomingsock, 10000, true))
                {
                    string[] incomingstring = Networking.socketfunctions.receivestring(incomingsock, true).Replace("\r\n", "\n").Split('\n');
                    try
                    {
                        switch (incomingstring[0])
                        {
                            case "learn":
                                string learnstring = incomingstring[4];
                                nstruct.parsestring(learnstring);
                                break;

                            case "talk":
                                Networking.socketfunctions.sendstring(incomingsock, nstruct.makeresponse());
                                break;

                            case "talkifcalled":
                                nstruct.parsestring(incomingstring[1]);
                                if (incomingstring[1].Contains(nstruct.myname))
                                    Networking.socketfunctions.sendstring(incomingsock, nstruct.makeresponse());
                                break;

                            case "talkandlearn":
                                logger.log("Got: " + incomingstring[1], Logging.Priority.Info);
                                nstruct.parsestring(incomingstring[1]);
                                string response = nstruct.makeresponse();
                                logger.log("Reply: " + response, Logging.Priority.Info);
                                Networking.socketfunctions.sendstring(incomingsock, response);
                                break;

                            case "interactsteam":
                                Data.ChatType ctype;
                                if (!Enum.TryParse(incomingstring[3], false, out ctype))
                                {
                                    Networking.socketfunctions.sendstring(incomingsock, "(CRITICAL) Chat type not recognized!!");
                                    logger.log("INVALID CHAT ENTRY WAS USED AND NO REPLY WAS SENT. CTYPE USED WAS: " + incomingstring[3], Logging.Priority.Critical);
                                }
                                Networking.socketfunctions.sendstring(incomingsock, interactsteam(incomingstring[4], incomingstring[1], incomingstring[2], ctype));
                                break;

                            default:
                                Networking.socketfunctions.sendstring(incomingsock, "Command not recognized.");
                                break;
                        }
                    }
                    catch (Exception ex)
                    { logger.logerror(ex);  }
                    finally
                    {
                        incomingsock.Shutdown(SocketShutdown.Both);
                        incomingsock.Close();
                    }
                    swatch.Stop();

                    logger.log("Session time: " + swatch.ElapsedMilliseconds, Logging.Priority.Info);
                    amountloops++;
                }

            }
            Console.WriteLine("Going down!");
            logger.log("Shutting down bot.", Logging.Priority.Notice);
            Environment.Exit(0);
        }

        static void addfunctionstodict()
        {
            //nonprivileged functions
            functiondict.Add("statusword", new KeyValuePair<string, Func<nodestruct, string, string, string, Data.ChatType,string>>("Prints the childnodes this wordnode is referring to, with the weight of each childnode.", new Func<nodestruct, string, string, string, Data.ChatType, string>(botfunctions.statusword)));
            functiondict.Add("statusreport", new KeyValuePair<string, Func<nodestruct, string, string, string, Data.ChatType, string>>("Prints out a report about the status of the bot and its learning process.", new Func<nodestruct, string, string, string, Data.ChatType, string>(botfunctions.statusreport)));
            functiondict.Add("uptime", new KeyValuePair<string, Func<nodestruct, string, string, string, Data.ChatType, string>>("Prints the uptime of the computer the bot is running on.", new Func<nodestruct, string, string, string, Data.ChatType, string>(botfunctions.uptime)));
            functiondict.Add("calc", new KeyValuePair<string, Func<nodestruct, string, string, string, Data.ChatType, string>>("Calculates stuff. Doesn't work yet, need to find a safe way to do math.", new Func<nodestruct, string, string, string, Data.ChatType, string>(botfunctions.calc)));
            functiondict.Add("slap", new KeyValuePair<string, Func<nodestruct, string, string, string, Data.ChatType, string>>("I'm a cool guy, eh, i slap people, doesn't afraid of them.", new Func<nodestruct, string, string, string, Data.ChatType, string>(botfunctions.slap)));
            functiondict.Add("roll", new KeyValuePair<string, Func<nodestruct, string, string, string, Data.ChatType, string>>("Rolls dices. e.g \"roll 5d6\" rolls 5 dices with 6 sides.", new Func<nodestruct, string, string, string, Data.ChatType, string>(botfunctions.roll)));
            functiondict.Add("slapcount", new KeyValuePair<string, Func<nodestruct, string, string, string, Data.ChatType, string>>("Gets the amount of times i slapped someone, if no name is specified i'll print it for everyone.", new Func<nodestruct, string, string, string, Data.ChatType, string>(botfunctions.getslapcount)));
            functiondict.Add("addnewurl", new KeyValuePair<string, Func<nodestruct, string, string, string, Data.ChatType, string>>("Adds new urls to my recommendation list!", new Func<nodestruct, string, string, string, Data.ChatType, string>(botfunctions.addurl)));
            functiondict.Add("recommendurl", new KeyValuePair<string, Func<nodestruct, string, string, string, Data.ChatType, string>>("I might know a few nice things around the net! (USE AT VIEWER DESCRETION :3)", new Func<nodestruct, string, string, string, Data.ChatType, string>(botfunctions.geturl)));
            functiondict.Add("8ball", new KeyValuePair<string, Func<nodestruct, string, string, string, Data.ChatType, string>>("Ask a question that can be answered by yes or no, and i shall deliver!", new Func<nodestruct, string, string, string, Data.ChatType, string>(botfunctions.eightball)));
            functiondict.Add("whatis", new KeyValuePair<string, Func<nodestruct, string, string, string, Data.ChatType, string>>("Ask me questions and i'll answer them! Use -v for verbose replies.", new Func<nodestruct, string, string, string, Data.ChatType, string>(botfunctions.AskWolframAlpha)));
            functiondict.Add("whoami", new KeyValuePair<string, Func<nodestruct, string, string, string, Data.ChatType, string>>("Shows your true nature!", new Func<nodestruct, string, string, string, Data.ChatType, string>(botfunctions.WhoAmI)));
            functiondict.Add("whatsmysteamid", new KeyValuePair<string, Func<nodestruct, string, string, string, Data.ChatType, string>>("Enriches you with unlimited wealth and sexual desires.", new Func<nodestruct, string, string, string, Data.ChatType, string>(botfunctions.whatsmysteamID)));

            //privileged functions
            privfunctiondict.Add("removeconjunction", new KeyValuePair<string, Func<nodestruct, string, string, string, Data.ChatType, string>>("Removes a certain word conjunction. usage: removeconjunction someword someotherword", new Func<nodestruct, string, string, string, Data.ChatType, string>(botfunctions.removeconjunction)));
            privfunctiondict.Add("purgenow", new KeyValuePair<string, Func<nodestruct, string, string, string, Data.ChatType, string>>("Runs the bad node purge algorhythm now. Don't run this too much, it'll dumb the bot down, though it might help now and then.", new Func<nodestruct, string, string, string, Data.ChatType, string>(botfunctions.purgenow)));
            privfunctiondict.Add("setratelimit", new KeyValuePair<string, Func<nodestruct, string, string, string, Data.ChatType, string>>("Set a rate limit.", new Func<nodestruct, string, string, string, Data.ChatType, string>(botfunctions.setratelimit)));
            privfunctiondict.Add("toggleratelimit", new KeyValuePair<string, Func<nodestruct, string, string, string, Data.ChatType, string>>("Toggles the ratelimit.", new Func<nodestruct, string, string, string, Data.ChatType, string>(botfunctions.toggleratelimit)));
            privfunctiondict.Add("toggledebug", new KeyValuePair<string, Func<nodestruct, string, string, string, Data.ChatType, string>>("Toggles debug.", new Func<nodestruct, string, string, string, Data.ChatType, string>(botfunctions.toggledebug)));
            privfunctiondict.Add("loadmemfile", new KeyValuePair<string, Func<nodestruct, string, string, string, Data.ChatType, string>>("Loads a memory file. Usage: loadmemfile _filename relative to executable_", new Func<nodestruct, string, string, string, Data.ChatType, string>(botfunctions.loadmemfile)));
            privfunctiondict.Add("savexmlnow", new KeyValuePair<string, Func<nodestruct, string, string, string, Data.ChatType, string>>("Saves the settings to XML settings right now. Overrides normal save rate.", new Func<nodestruct, string, string, string, Data.ChatType, string>(botfunctions.savenow)));
            privfunctiondict.Add("loadxmlnow", new KeyValuePair<string, Func<nodestruct, string, string, string, Data.ChatType, string>>("Loads XML settings right now.", new Func<nodestruct, string, string, string, Data.ChatType, string>(botfunctions.loadnow)));
            privfunctiondict.Add("changexml", new KeyValuePair<string, Func<nodestruct, string, string, string, Data.ChatType, string>>("Changes XML persistent settings. Syntax: changexml add|del|delchild|change %dictitem% %itemtoadd|del|change% %change|addargument% .", new Func<nodestruct, string, string, string, Data.ChatType, string>(botfunctions.changexml)));
            privfunctiondict.Add("printxml", new KeyValuePair<string, Func<nodestruct, string, string, string, Data.ChatType, string>>("Prints the contents of an xml dictionary, usage: printxml _dictionaryname_", new Func<nodestruct, string, string, string, Data.ChatType, string>(botfunctions.printxml)));
            privfunctiondict.Add("setmaxresponselength", new KeyValuePair<string, Func<nodestruct, string, string, string, Data.ChatType, string>>("Sets the max response length of the bot (5 words may be added), with a minimum of 2.", new Func<nodestruct, string, string, string, Data.ChatType, string>(botfunctions.SetMaxSentenceLength)));
            privfunctiondict.Add("shutdowngracefully", new KeyValuePair<string, Func<nodestruct, string, string, string, Data.ChatType, string>>("Gracefully saves all data and shuts the bot down.", new Func<nodestruct, string, string, string, Data.ChatType, string>(botfunctions.ShutdownGracefully)));

        }

        //interactsteam specific variables
        static int lastsave = 0;
        static string interactsteam(string input, string steamID, string steamName, Data.ChatType chatType)
        {
            StringBuilder responsebuilder = new StringBuilder();
            logger.log("Got: " + input, Logging.Priority.Info);
            lastsave++;
            if (lastsave > 20)
            {
                botfunctions.savenow(nstruct, "","", "", Data.ChatType.PM);
                lastsave = 0;
            }
            input = nstruct.stripnames(input).Trim() ;
            try
            {
                if (functiondict.ContainsKey(input.Split(' ')[0]))
                {
                    responsebuilder.Append(functiondict[input.Split(' ')[0]].Value.Invoke(nstruct, input, steamID, steamName, chatType));
                }
                else if (Data.IsAdmin(steamID) && privfunctiondict.ContainsKey(input.Split(' ')[0]))
                {
                    responsebuilder.Append(privfunctiondict[input.Split(' ')[0]].Value.Invoke(nstruct, input, steamID, steamName, chatType));
                }
                else
                    if (input.Split(' ').Length > 0)
                        switch (input.Split(' ')[0])
                        {

                            case "manual":
                                if (input.Split(' ').Length == 2)
                                {
                                    if (Data.IsAdmin(steamID))
                                    {
                                        if (functiondict.ContainsKey(input.Split(' ')[1]))
                                            try { responsebuilder.AppendFormat("Man page for {0}: {1}", input.Split(' ')[1], functiondict[input.Split(' ')[1]].Key); }
                                            catch (Exception ex) { responsebuilder.Append(ex.Message); }
                                        else
                                            try { responsebuilder.AppendFormat("Man page for {0}: {1}", input.Split(' ')[1], privfunctiondict[input.Split(' ')[1]].Key); }
                                            catch (Exception ex) { responsebuilder.Append(ex.Message); }
                                    }
                                    else
                                        try { responsebuilder.AppendFormat("Man page for {0}: {1}", input.Split(' ')[1], functiondict[input.Split(' ')[1]].Key); }
                                        catch (Exception ex) { responsebuilder.Append(ex.Message); }
                                }
                                else
                                {
                                    StringBuilder sbuilder = new StringBuilder(); functiondict.Keys.ToList().ForEach(s => sbuilder.Append(s + ", "));
                                    if (Data.IsAdmin(steamID))
                                        privfunctiondict.Keys.ToList().ForEach(s => sbuilder.Append(s + ", "));
                                    responsebuilder.AppendFormat("Please specify a command to get the manual from. Current usable commands to you: {0}", sbuilder.ToString());
                                }
                                break;

                            default:
                                nstruct.parsestring(input);
                                responsebuilder.Append(nstruct.makeresponse());
                                break;
                        }
                    else
                        responsebuilder.Append(nstruct.makeresponse());

            }
            catch (Exception ex)
            { logger.log(ex.ToString(), Logging.Priority.Error); responsebuilder.Append("You tit, that almost made me crash ;3; (" + ex.Message + ")"); }

            logger.log("Reply: " + responsebuilder.ToString(), Logging.Priority.Info);
            return responsebuilder.ToString();
        }
    }
}
