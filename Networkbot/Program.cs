using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CedLib;
using CedLib.Networking;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using OnyLib;

namespace Onymity
{
    using System.IO;
    using System.Reflection;
    class Program
    {
        static int port = 5000;
        static Socket mainsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static OnyLib.BotStuff bstuff;
        static System.Threading.ManualResetEvent Donify;

        static Logging logger ;
        static OnyLib.AI.nodestruct nstruct;

        static Dictionary<Assembly, bool> Plugins = new Dictionary<Assembly, bool>();
        

        static void Main(string[] args)
        {
            bstuff = new BotStuff();
            Donify = bstuff.OnyVariables.Donify;
            logger = bstuff.OnyVariables.logger;
            nstruct = bstuff.OnyVariables.nstruct;
            if (!File.Exists("funcpersistence.xml"))
                bstuff.OnyVariables.persistence.save();
            bstuff.OnyVariables.persistence.load();
            LoadPlugins();
            bstuff.OnyEvents.InComingMessage += new BotStuff.BotEvents.IncomingMessageHook(OnyEvents_InComingMessage);
            if (!socketfunctions.trybindsocket(mainsock, ref port, true, 50, IPAddress.Any))
            { logger.log("FAILED TO BIND TO ANY PORT. EXITTING", Logging.Priority.Critical); return; }
            logger.log("Initialized main function", Logging.Priority.Info);
            logger.log("Loading variables..",Logging.Priority.Notice);

            try
            { bstuff.OnyFunctions.PrivFunctions.loadnow(new OnyLib.SpecialClasses.BotFunctionData(nstruct)); }
            catch (Exception ex)
            { logger.logerror(ex); }
            logger.log("Trying to load playback file..", Logging.Priority.Notice);
            try { nstruct.loadplayback(); }
            catch (Exception ex) { logger.logerror(ex); }

            while (bstuff.OnyVariables.run)
            {
                if ((bstuff.OnyVariables.amountloops % 10) == 0) { nstruct.saveplayback(); bstuff.OnyVariables.amountloops = 0; bstuff.OnyVariables.persistence.save(); }
                logger.log("Waiting for incoming commands", Logging.Priority.Info);
                Donify.Reset();
                mainsock.BeginAccept(new AsyncCallback(acceptIncomingConnection), mainsock);
                Donify.WaitOne();
            }
            nstruct.saveplayback();
            bstuff.OnyVariables.persistence.save();
            Console.WriteLine("Going down!");
            logger.log("Shutting down bot.", Logging.Priority.Notice);
            logger.log("Uptime: " + (DateTime.Now - bstuff.OnyVariables.starttime).ToString(), Logging.Priority.Info);
            Environment.Exit(0);
        }

        static void OnyEvents_InComingMessage(OnyLib.BotStuff.BotEvents.IncomingMessageEventData Args)
        {

            switch (Args.msg.CommandName)
            {
                case "learn":
                    string learnstring = Args.msg.ExtraneousLines[0];
                    nstruct.parsestring(learnstring);
                    break;

                case "tell":
                    learnstring = Args.msg.ExtraneousLines[2]; //is a steam learning call!
                    nstruct.parsestring(learnstring);
                    break;

                case "talk":
                    Args.ToReturn.Add(nstruct.MakeResponse());
                    break;

                case "talkifcalled":
                    nstruct.parsestring(Args.msg.ExtraneousLines[0]);
                    if (Args.msg.ExtraneousLines.Contains(nstruct.myname))
                        Args.ToReturn.Add(nstruct.MakeResponse());
                    break;

                case "talkandlearn":
                    logger.log("Got: " + Args.msg.ExtraneousLines[2], Logging.Priority.Info);
                    nstruct.parsestring(Args.msg.ExtraneousLines[2]);
                    string response = nstruct.MakeResponse();
                    logger.log("Reply: " + response, Logging.Priority.Info);
                    Args.ToReturn.Add(response);
                    break;


            }
        }

        static void LoadPlugins(string _dir = "./Plugins/")
        {
            DirectoryInfo PluginDir = new System.IO.DirectoryInfo(_dir);
            if (!PluginDir.Exists)
                PluginDir.Create();
            foreach (FileInfo finfo in PluginDir.GetFiles("*.dll", SearchOption.TopDirectoryOnly))
            {
                Plugins.Add(Assembly.LoadFile(finfo.FullName), false);
            }

            foreach (var plugin in Plugins)
            {
                    try
                    {
                        Type[] types = plugin.Key.GetTypes();
                        logger.log("Loading plugin: " + plugin.Key.GetName(), Logging.Priority.Notice);
                        MethodInfo main = types.First(kp => kp.Name == "Augmentation").GetMethod("Main", BindingFlags.Public | BindingFlags.Static);
                        main.Invoke(new object(), new object[] { bstuff });
                        logger.log("Succesfully loaded: " + plugin.Key.GetName(), Logging.Priority.Info);
                    }
                    catch (Exception ex)
                    {
                        logger.logerror(ex); logger.log("Failed to load plugin: " + plugin.Key.FullName, Logging.Priority.Warning);
                    }
            }
        }



        static void acceptIncomingConnection(IAsyncResult ar)
        {
            Socket mainsock = (Socket)ar.AsyncState;
            Socket incomingsock = mainsock.EndAccept(ar);
            Donify.Set();

            System.Diagnostics.Stopwatch swatch = new System.Diagnostics.Stopwatch();
            swatch.Start();

            logger.log("Got connection from: " + incomingsock.RemoteEndPoint, Logging.Priority.Notice);
            if (socketfunctions.waitfordata(incomingsock, 10000, true))
            {
                string[] incomingstring = socketfunctions.receivestring(incomingsock, true).Replace("\r\n", "\n").Split('\n');
                try
                {
                    string response = bstuff.OnyEvents.NewMessage(incomingstring, (IPEndPoint)incomingsock.RemoteEndPoint);
                    logger.log("got from plugins: " + response, Logging.Priority.Notice);
                    if (response == null || response == "")
                        socketfunctions.sendstring(incomingsock, "Blargh.");
                    else
                        socketfunctions.sendstring(incomingsock, response);
                }
                catch (Exception ex)
                { logger.logerror(ex); }

                incomingsock.Shutdown(SocketShutdown.Both);
                incomingsock.Close();

                swatch.Stop();

                logger.log("Session time: " + swatch.ElapsedMilliseconds, Logging.Priority.Info);

                bstuff.OnyVariables.amountloops++;
            }

        }

        static void addfunctionstodict()
        {
            //nonprivileged functions

            //privileged functions
            

        }

        //interactsteam specific variables
        static int lastsave = 0;
        //static string interactsteam(OnyLib.SpecialClasses.BotFunctionData BotData)
        //{
        //    StringBuilder responsebuilder = new StringBuilder();
        //    logger.log("Got: " + BotData.input, Logging.Priority.Info);
        //    lastsave++;
        //    if (lastsave > 20)
        //    {
        //        bstuff.OnyFunctions.PrivFunctions.savenow(new OnyLib.SpecialClasses.BotFunctionData());
        //        lastsave = 0;
        //    }
        //    BotData.input = nstruct.stripnames(BotData.input).Trim() ;
        //    try
        //    {
        //        if (functiondict.ContainsKey(BotData.input.Split(' ')[0]))
        //        {
        //            responsebuilder.Append(functiondict[BotData.input.Split(' ')[0]].Value.Invoke(BotData));
        //            if (responsebuilder.Length > botfunctions.OnySpamThreshold && botfunctions.OnySpamProtection && BotData.Ctype != Data.ChatType.PM)
        //                return "(Spam prevention) Answer too long! Ask this again in a PM or ask an Admin to disable spam protection.";
        //        }
        //        else if (Data.IsAdmin(BotData.steamID) && privfunctiondict.ContainsKey(BotData.input.Split(' ')[0]))
        //        {
        //            responsebuilder.Append(privfunctiondict[BotData.input.Split(' ')[0]].Value.Invoke(BotData));
        //        }
        //        else
        //            if (BotData.input.Split(' ').Length > 0)
        //                switch (BotData.input.Split(' ')[0])
        //                {

        //                    case "manual":
        //                        if (BotData.input.Split(' ').Length == 2)
        //                        {
        //                            if (Data.IsAdmin(BotData.steamID))
        //                            {
        //                                if (functiondict.ContainsKey(BotData.input.Split(' ')[1]))
        //                                    try { responsebuilder.AppendFormat("Man page for {0}: {1}", BotData.input.Split(' ')[1], functiondict[BotData.input.Split(' ')[1]].Key); }
        //                                    catch (Exception ex) { responsebuilder.Append(ex.Message); }
        //                                else
        //                                    try { responsebuilder.AppendFormat("Man page for {0}: {1}", BotData.input.Split(' ')[1], privfunctiondict[BotData.input.Split(' ')[1]].Key); }
        //                                    catch (Exception ex) { responsebuilder.Append(ex.Message); }
        //                            }
        //                            else
        //                                try { responsebuilder.AppendFormat("Man page for {0}: {1}", BotData.input.Split(' ')[1], functiondict[BotData.input.Split(' ')[1]].Key); }
        //                                catch (Exception ex) { responsebuilder.Append(ex.Message); }
        //                        }
        //                        else
        //                        {
        //                            StringBuilder sbuilder = new StringBuilder(); functiondict.Keys.ToList().ForEach(s => sbuilder.Append(s + ", "));
        //                            if (Data.IsAdmin(BotData.steamID))
        //                                privfunctiondict.Keys.ToList().ForEach(s => sbuilder.Append(s + ", "));
        //                            responsebuilder.AppendFormat("Please specify a command to get the manual from. Current usable commands to you: {0}", sbuilder.ToString());
        //                        }
        //                        break;

        //                    default:
        //                        nstruct.parsestring(BotData.input);
        //                        responsebuilder.Append(nstruct.MakeResponse());
        //                        break;
        //                }
        //            else
        //                responsebuilder.Append(nstruct.MakeResponse());

        //    }
        //    catch (Exception ex)
        //    { logger.log(ex.ToString(), Logging.Priority.Error); responsebuilder.Append("You tit, that almost made me crash ;3; (" + ex.Message + ")"); }

        //    logger.log("Reply: " + responsebuilder.ToString(), Logging.Priority.Info);
        //    return responsebuilder.ToString();
        //}
    }
}
