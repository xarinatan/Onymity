using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CedLib;
using CedLib.Networking;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;

namespace Onymity
{
    class Program
    {
        public static bool run = true;
        public static Logging logger = new Logging(true, true);
        public static DateTime starttime = DateTime.Now;

        static int port = 5000;
        static Socket mainsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static List<IPAddress> AllowedSteamClientIPs = new List<IPAddress>() { IPAddress.Loopback };

        static nodestruct nstruct = new nodestruct(true);
        static Dictionary<string, KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>> functiondict = new Dictionary<string, KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>>();
        static Dictionary<string, KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>> privfunctiondict = new Dictionary<string, KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>>();




        static void Main(string[] args)
        {
            addfunctionstodict();
            nstruct.blacklist.Add("onymity");
            if (!socketfunctions.trybindsocket(mainsock, ref port, true, 50, IPAddress.Any))
            { logger.log("FAILED TO BIND TO ANY PORT. EXITTING", Logging.Priority.Critical); return; }
            logger.log("Initialized main function", Logging.Priority.Info);
            logger.log("Loading variables..",Logging.Priority.Notice);

            try
            { botfunctions.loadnow(new botfunctions.BotFunctionData(nstruct)); }
            catch (Exception ex)
            { logger.logerror(ex); }
            logger.log("Trying to load playback file..", Logging.Priority.Notice);
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
                if (socketfunctions.waitfordata(incomingsock, 10000, true))
                {
                    string[] incomingstring = socketfunctions.receivestring(incomingsock, true).Replace("\r\n", "\n").Split('\n');
                    try
                    {
                        switch (incomingstring[0])
                        {
                            case "learn":
                                string learnstring = incomingstring[4];
                                nstruct.parsestring(learnstring);
                                break;

                            case "talk":
                                socketfunctions.sendstring(incomingsock, nstruct.makeresponse());
                                break;

                            case "talkifcalled":
                                nstruct.parsestring(incomingstring[1]);
                                if (incomingstring[1].Contains(nstruct.myname))
                                    socketfunctions.sendstring(incomingsock, nstruct.makeresponse());
                                break;

                            case "talkandlearn":
                                logger.log("Got: " + incomingstring[1], Logging.Priority.Info);
                                nstruct.parsestring(incomingstring[1]);
                                string response = nstruct.makeresponse();
                                logger.log("Reply: " + response, Logging.Priority.Info);
                                socketfunctions.sendstring(incomingsock, response);
                                break;

                            case "interactsteam":
                                if (!AllowedSteamClientIPs.Contains(((IPEndPoint)incomingsock.RemoteEndPoint).Address))
                                    throw new Exception("ERROR! REMOTE ENDPOINT IS NOT IN ALLOWED ENDPOINTS, SUSPECTED HACK ATTEMPT!");
                                Data.ChatType ctype;
                                if (!Enum.TryParse(incomingstring[3], false, out ctype))
                                {
                                    socketfunctions.sendstring(incomingsock, "(CRITICAL) Chat type not recognized!!");
                                    logger.log("INVALID CHAT ENTRY WAS USED AND NO REPLY WAS SENT. CTYPE USED WAS: " + incomingstring[3], Logging.Priority.Critical);
                                }
                                socketfunctions.sendstring(incomingsock, interactsteam(new botfunctions.BotFunctionData(nstruct, incomingstring[4], incomingstring[1], incomingstring[2], ctype)));
                                break;

                            default:
                                socketfunctions.sendstring(incomingsock, "Command not recognized.");
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
            functiondict.Add("statusword", new KeyValuePair<string, Func<botfunctions.BotFunctionData,string>>("Prints the childnodes this wordnode is referring to, with the weight of each childnode.", new Func<botfunctions.BotFunctionData, string>(botfunctions.statusword)));
            functiondict.Add("statusreport", new KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>("Prints out a report about the status of the bot and its learning process.", new Func<botfunctions.BotFunctionData, string>(botfunctions.statusreport)));
            functiondict.Add("uptime", new KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>("Prints the uptime of the computer the bot is running on.", new Func<botfunctions.BotFunctionData, string>(botfunctions.uptime)));
            functiondict.Add("calc", new KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>("Calculates stuff. Doesn't work yet, need to find a safe way to do math.", new Func<botfunctions.BotFunctionData, string>(botfunctions.calc)));
            functiondict.Add("slap", new KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>("I'm a cool guy, eh, i slap people, doesn't afraid of them.", new Func<botfunctions.BotFunctionData, string>(botfunctions.slap)));
            functiondict.Add("roll", new KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>("Rolls dices. e.g \"roll 5d6\" rolls 5 dices with 6 sides.", new Func<botfunctions.BotFunctionData, string>(botfunctions.roll)));
            functiondict.Add("slapcount", new KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>("Gets the amount of times i slapped someone, if no name is specified i'll print it for everyone.", new Func<botfunctions.BotFunctionData, string>(botfunctions.getslapcount)));
            functiondict.Add("addnewurl", new KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>("Adds new urls to my recommendation list!", new Func<botfunctions.BotFunctionData, string>(botfunctions.addurl)));
            functiondict.Add("recommendurl", new KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>("I might know a few nice things around the net! (USE AT VIEWER DESCRETION :3)", new Func<botfunctions.BotFunctionData, string>(botfunctions.geturl)));
            functiondict.Add("8ball", new KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>("Ask a question that can be answered by yes or no, and i shall deliver!", new Func<botfunctions.BotFunctionData, string>(botfunctions.eightball)));
            functiondict.Add("whatis", new KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>("Ask me questions and i'll answer them! Use -v for verbose replies.", new Func<botfunctions.BotFunctionData, string>(botfunctions.AskAround)));
            functiondict.Add("whoami", new KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>("Shows your true nature!", new Func<botfunctions.BotFunctionData, string>(botfunctions.WhoAmI)));
            functiondict.Add("whatsmysteamid", new KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>("Enriches you with unlimited wealth and sexual desires, obviously.", new Func<botfunctions.BotFunctionData, string>(botfunctions.whatsmysteamID)));
            functiondict.Add("getquote", new KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>("Gets a quote on your car insurance, obviously.", new Func<botfunctions.BotFunctionData, string>(botfunctions.GetQuote)));

            //privileged functions
            privfunctiondict.Add("removeconjunction", new KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>("Removes a certain word conjunction. usage: removeconjunction someword someotherword", new Func<botfunctions.BotFunctionData, string>(botfunctions.removeconjunction)));
            privfunctiondict.Add("purgenow", new KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>("Runs the bad node purge algorhythm now. Don't run this too much, it'll dumb the bot down, though it might help now and then.", new Func<botfunctions.BotFunctionData, string>(botfunctions.purgenow)));
            privfunctiondict.Add("setratelimit", new KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>("Set a rate limit.", new Func<botfunctions.BotFunctionData, string>(botfunctions.setratelimit)));
            privfunctiondict.Add("toggleratelimit", new KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>("Toggles the ratelimit.", new Func<botfunctions.BotFunctionData, string>(botfunctions.toggleratelimit)));
            privfunctiondict.Add("toggledebug", new KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>("Toggles debug.", new Func<botfunctions.BotFunctionData, string>(botfunctions.toggledebug)));
            privfunctiondict.Add("loadmemfile", new KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>("Loads a memory file. Usage: loadmemfile _filename relative to executable_", new Func<botfunctions.BotFunctionData, string>(botfunctions.loadmemfile)));
            privfunctiondict.Add("savexmlnow", new KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>("Saves the settings to XML settings right now. Overrides normal save rate.", new Func<botfunctions.BotFunctionData, string>(botfunctions.savenow)));
            privfunctiondict.Add("loadxmlnow", new KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>("Loads XML settings right now.", new Func<botfunctions.BotFunctionData, string>(botfunctions.loadnow)));
            privfunctiondict.Add("changexml", new KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>("Changes XML persistent settings. Syntax: changexml add|del|delchild|change %dictitem% %itemtoadd|del|change% %change|addargument% .", new Func<botfunctions.BotFunctionData, string>(botfunctions.changexml)));
            privfunctiondict.Add("printxml", new KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>("Prints the contents of an xml dictionary, usage: printxml _dictionaryname_", new Func<botfunctions.BotFunctionData, string>(botfunctions.printxml)));
            privfunctiondict.Add("setmaxresponselength", new KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>("Sets the max response length of the bot (5 words may be added), with a minimum of 2.", new Func<botfunctions.BotFunctionData, string>(botfunctions.SetMaxSentenceLength)));
            privfunctiondict.Add("shutdowngracefully", new KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>("Gracefully saves all data and shuts the bot down.", new Func<botfunctions.BotFunctionData, string>(botfunctions.ShutdownGracefully)));
            privfunctiondict.Add("toggleonyantispam", new KeyValuePair<string, Func<botfunctions.BotFunctionData, string>>("Toggles onymity's builtin anti spam protection (The one that prevents ony from spamming, not from being spammed).", new Func<botfunctions.BotFunctionData, string>(botfunctions.ToggleAntiOnySpam)));

        }

        //interactsteam specific variables
        static int lastsave = 0;
        static string interactsteam(botfunctions.BotFunctionData BotData)
        {
            StringBuilder responsebuilder = new StringBuilder();
            logger.log("Got: " + BotData.input, Logging.Priority.Info);
            lastsave++;
            if (lastsave > 20)
            {
                botfunctions.savenow(new botfunctions.BotFunctionData());
                lastsave = 0;
            }
            BotData.input = nstruct.stripnames(BotData.input).Trim() ;
            try
            {
                if (functiondict.ContainsKey(BotData.input.Split(' ')[0]))
                {
                    responsebuilder.Append(functiondict[BotData.input.Split(' ')[0]].Value.Invoke(BotData));
                    if (responsebuilder.Length > botfunctions.SpamThreshold && botfunctions.OnySpamProtection)
                        return "(Spam prevention) Answer too long! Ask this again in a PM or ask an Admin to disable spam protection.";
                }
                else if (Data.IsAdmin(BotData.steamID) && privfunctiondict.ContainsKey(BotData.input.Split(' ')[0]))
                {
                    responsebuilder.Append(privfunctiondict[BotData.input.Split(' ')[0]].Value.Invoke(BotData));
                }
                else
                    if (BotData.input.Split(' ').Length > 0)
                        switch (BotData.input.Split(' ')[0])
                        {

                            case "manual":
                                if (BotData.input.Split(' ').Length == 2)
                                {
                                    if (Data.IsAdmin(BotData.steamID))
                                    {
                                        if (functiondict.ContainsKey(BotData.input.Split(' ')[1]))
                                            try { responsebuilder.AppendFormat("Man page for {0}: {1}", BotData.input.Split(' ')[1], functiondict[BotData.input.Split(' ')[1]].Key); }
                                            catch (Exception ex) { responsebuilder.Append(ex.Message); }
                                        else
                                            try { responsebuilder.AppendFormat("Man page for {0}: {1}", BotData.input.Split(' ')[1], privfunctiondict[BotData.input.Split(' ')[1]].Key); }
                                            catch (Exception ex) { responsebuilder.Append(ex.Message); }
                                    }
                                    else
                                        try { responsebuilder.AppendFormat("Man page for {0}: {1}", BotData.input.Split(' ')[1], functiondict[BotData.input.Split(' ')[1]].Key); }
                                        catch (Exception ex) { responsebuilder.Append(ex.Message); }
                                }
                                else
                                {
                                    StringBuilder sbuilder = new StringBuilder(); functiondict.Keys.ToList().ForEach(s => sbuilder.Append(s + ", "));
                                    if (Data.IsAdmin(BotData.steamID))
                                        privfunctiondict.Keys.ToList().ForEach(s => sbuilder.Append(s + ", "));
                                    responsebuilder.AppendFormat("Please specify a command to get the manual from. Current usable commands to you: {0}", sbuilder.ToString());
                                }
                                break;

                            default:
                                nstruct.parsestring(BotData.input);
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
