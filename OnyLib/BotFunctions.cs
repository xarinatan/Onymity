using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OnyLib.SpecialClasses;
using CedLib;

namespace OnyLib
{
    namespace BotFunctions
    {
        using CedLib.Persistence.XMLPersistenceDictionary;
        public class NonPriviligedFunctions
        {
            OnyLib.BotStuff.RuntimeVariables RunVars;
            Logging logger;
            public NonPriviligedFunctions(OnyLib.BotStuff.RuntimeVariables _runvars)
            {
                RunVars = _runvars;
                logger = RunVars.logger;
            }

            public string statusword(BotFunctionData BotInput)
            {
                //if (BotInput.Ctype == Data.ChatType.GROUPCHAT)
                //    return "Due to the spamminess of this command it has been made PM-only. Open a new private chat with me and ask it there.";
                StringBuilder responsebuilder = new StringBuilder();
                try
                {
                    StringBuilder sbuildernodes = new StringBuilder();
                    foreach (var childnode in BotInput.nstruct.translatenodes[BotInput.input.Split(' ')[1]].childnodes)
                    {
                        sbuildernodes.Append(childnode.Key.text + "(" + childnode.Value + "),");
                    }
                    responsebuilder.AppendFormat("Node \"{0}\" has {1} childnodes: {2}", new object[]{
                                    BotInput.nstruct.translatenodes[BotInput.input.Split(' ')[1]].text,
                                    BotInput.nstruct.translatenodes[BotInput.input.Split(' ')[1]].childnodes.Count,
                                    sbuildernodes.ToString()

                                });
                }
                catch (Exception ex) { logger.logerror(ex); responsebuilder.Append(ex.Message); }
                return responsebuilder.ToString();
            }

            public delegate void StatusReportHook(StringBuilder ResponseBuilder);
            public event StatusReportHook StatusReportEvent;
            public string statusreport(BotFunctionData BotInput)
            {
                StringBuilder responsebuilder = new StringBuilder();
                responsebuilder.AppendFormat("Report about me:\nI know {0} words, i've ignored {1} words so far, and purged {2} uncommon nodes so far.\nDebugging is {3} and rate limiting is {4}. The rate limit is set to 1 message every {5} seconds.", new object[]
                            {
                                BotInput.nstruct.translatenodes.Count,
                                BotInput.nstruct.wordsignored,
                                BotInput.nstruct.totalnodespurged,
                                BotInput.nstruct.debug,
                                BotInput.nstruct.ratelimit,
                                BotInput.nstruct.secondsbetweenresponse
                            });
                if (StatusReportEvent != null)
                    StatusReportEvent(responsebuilder);
                return responsebuilder.ToString();
            }

            public string uptime(BotFunctionData BotInput)
            {
                StringBuilder responsebuilder = new StringBuilder();
                try
                {
                    System.Diagnostics.PerformanceCounter pc = new System.Diagnostics.PerformanceCounter("System", "System Up Time");
                    pc.NextValue();
                    TimeSpan ts = TimeSpan.FromSeconds(pc.NextValue());
                    responsebuilder.AppendFormat("The PC i'm running on has been up for {0} days, and {1}:{2} hours.", new object[] { ts.Days, ts.Hours, ts.Minutes });
                }
                catch (Exception ex)
                {
                    logger.logerror(ex);
                    responsebuilder.Append("Couldn't fetch system uptime (Are you admin?).");
                }
                TimeSpan myts = (DateTime.Now - RunVars.starttime);
                responsebuilder.AppendFormat(" My current session has been running for {0}:{1}:{2} ({3} days)", new object[] { myts.Hours, myts.Minutes, myts.Seconds, myts.TotalDays });
                return responsebuilder.ToString();
            }


        }



        public class PriviligedFunctions
        {
            BotStuff.RuntimeVariables RunVars;
            Logging logger;
            XMLPersistenceDictionary persistence;
            public PriviligedFunctions(OnyLib.BotStuff.RuntimeVariables _runvars)
            {
                RunVars = _runvars;
                logger = RunVars.logger;
                persistence = RunVars.persistence;
            }

            public string removeconjunction(BotFunctionData BotInput)
            {
                StringBuilder responsebuilder = new StringBuilder();
                if (BotInput.input.Split(' ').Length > 2)
                {
                    try
                    {
                        string[] splittedstring = BotInput.input.Split(' ');
                        BotInput.nstruct.translatenodes[splittedstring[1]].childnodes.Remove(BotInput.nstruct.translatenodes[splittedstring[2]]);
                        responsebuilder.AppendFormat("Succesfully removed {0} from {1}", splittedstring[2], splittedstring[1]);
                    }
                    catch (Exception ex)
                    { responsebuilder.Append(ex.Message); logger.logerror(ex); }
                }
                else
                    responsebuilder.Append("Not enough arguments specified.");
                return responsebuilder.ToString();
            }

            public string purgenow(BotFunctionData BotInput)
            {
                StringBuilder responsebuilder = new StringBuilder();
                string[] splittedinput = BotInput.input.Split(' ');
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
                        long nodesbefore = BotInput.nstruct.totalnodespurged;
                        while (i < purgecount)
                        {
                            BotInput.nstruct.purgebadnodes();
                            i++;
                        }
                        long nodespurged = BotInput.nstruct.totalnodespurged - nodesbefore;
                        swatch.Stop();
                        responsebuilder.Append(string.Format("Done. Nodes purged: {0}. Operation took {1}ms.", nodespurged, swatch.ElapsedMilliseconds));
                    }
                }
                else
                    responsebuilder.Append(BotInput.nstruct.purgebadnodes());
                return responsebuilder.ToString();
            }

            public string setratelimit(BotFunctionData BotInput)
            {
                StringBuilder responsebuilder = new StringBuilder();
                try
                {
                    int ratetime = 30;
                    int.TryParse(BotInput.input.Split(' ')[1], out ratetime);
                    BotInput.nstruct.secondsbetweenresponse = ratetime;
                    responsebuilder.AppendFormat("Set rate limit to {0} seconds.", BotInput.nstruct.secondsbetweenresponse);
                }
                catch (Exception ex)
                {
                    logger.logerror(ex);
                    responsebuilder.Append(ex.Message);
                }
                return responsebuilder.ToString();
            }

            public string toggleratelimit(BotFunctionData BotInput)
            {
                StringBuilder responsebuilder = new StringBuilder();
                BotInput.nstruct.toggleratelimit();
                responsebuilder.Append("Set ratelimit to " + BotInput.nstruct.ratelimit.ToString() + ".");
                return responsebuilder.ToString();
            }

            public string toggledebug(BotFunctionData BotInput)
            {
                StringBuilder responsebuilder = new StringBuilder();
                BotInput.nstruct.toggledebug();
                responsebuilder.Append("Toggled debug.");
                return responsebuilder.ToString();
            }

            public string loadmemfile(BotFunctionData BotInput)
            {
                StringBuilder responsebuilder = new StringBuilder();
                try
                {
                    responsebuilder.Append(BotInput.nstruct.loadplayback(BotInput.input.Split(' ')[1]));
                }
                catch (Exception ex)
                {
                    logger.logerror(ex);
                    responsebuilder.Append(ex.Message);
                }
                return responsebuilder.ToString();
            }

            public string savenow(BotFunctionData BotInput)
            {
                StringBuilder responsebuilder = new StringBuilder();
                System.Diagnostics.Stopwatch swatch = new System.Diagnostics.Stopwatch();
                swatch.Start();
                lock (persistence)
                {
                    persistence.save(persistence.filename);
                }
                swatch.Stop();
                responsebuilder.AppendFormat("Saved to {0} in {1}ms!", persistence.filename, swatch.ElapsedMilliseconds);
                return responsebuilder.ToString();
            }

            public string loadnow(BotFunctionData BotInput)
            {
                StringBuilder responsebuilder = new StringBuilder();
                System.Diagnostics.Stopwatch swatch = new System.Diagnostics.Stopwatch();
                swatch.Start();
                persistence.load(persistence.filename);
                swatch.Stop();
                responsebuilder.AppendFormat("Loaded from {0} in {1}ms!", persistence.filename, swatch.ElapsedMilliseconds);
                return responsebuilder.ToString();
            }

            public string changexml(BotFunctionData BotInput)
            {
                //command format: changexml add|del|delchild|change %dictitem% %itemtoadd|del|change% %change|addargument% 
                StringBuilder responsebuilder = new StringBuilder();
                string[] splittedinput = BotInput.input.Split(' ');
                if (splittedinput.Length < 3)
                    return "You didn't specify enough parameters";
                if (!persistence.ContainsKey(splittedinput[2]))
                    return "Specified item wasn't found in the xml file";
                switch (splittedinput[1])
                {
                    case "add":
                        if (splittedinput.Length < 5)
                            return "You didn't specify enough parameters for this command.";
                        persistence[splittedinput[2]].childnodes.Add(new  savenode(splittedinput[3], splittedinput[4]));
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

            public string printxml(BotFunctionData BotInput)
            {
                //syntax: printxml _dictionarypart_
                StringBuilder responsebuilder = new StringBuilder();
                string[] splittedinput = BotInput.input.Split(' ');
                if (splittedinput.Length < 2)
                    return "Not enough commands specified";
                foreach (var node in persistence[splittedinput[1]].childnodes)
                {
                    responsebuilder.AppendFormat("{0}({1}),", node.name, node.obj);
                }
                return responsebuilder.ToString();
            }

            public string SetMaxSentenceLength(BotFunctionData BotInput)
            {
                StringBuilder responsebuilder = new StringBuilder();
                string[] splittedinput = BotInput.input.Split(' ');
                if (splittedinput.Length < 2)
                    return "Not enough arguments supplied. Use manual to see how this command works!";
                int length = 0;
                if (!int.TryParse(splittedinput[1], out length))
                    return "Supplied length is not a valid integer!";
                if (length < 2)
                    return "Length must be equal or longer than 2!";
                BotInput.nstruct.maxresponselength = length;
                responsebuilder.Append("Set max response length to " + length.ToString());

                return responsebuilder.ToString();
            }

            public string ShutdownGracefully(BotFunctionData BotInput)
            {
                StringBuilder responsebuilder = new StringBuilder();
                responsebuilder.Append("Shutting down bot..\nSaving playback..\n");
                BotInput.nstruct.saveplayback();
                responsebuilder.Append("Saved playback, saving XML persistence..(");
                responsebuilder.Append(savenow(new BotFunctionData(BotInput.nstruct)));
                responsebuilder.Append(")\nDone! Onymity will now go down.");
                RunVars.run = false;
                RunVars.Donify.Set();
                return responsebuilder.ToString();
            }

            public string ToggleAntiOnySpam(BotFunctionData BotInput)
            {
                // Using this command will toggle Anti ony spam
                StringBuilder responsebuilder = new StringBuilder();
                string[] splittedstring = BotInput.input.Split(' ');

                RunVars.OnySpamProtection = !RunVars.OnySpamProtection;

                responsebuilder.Append("Done! Set spam protection to " + RunVars.OnySpamProtection.ToString());

                return responsebuilder.ToString();
            }

            public string OnyAntiSpamMaxReturnLength(BotFunctionData BotInput)
            {
                // OnyAntiSpamMaxReturnLength %Length%
                string[] splittedstring = BotInput.input.Split(' ');
                int _OnySpamThreshold = 0;
                if (splittedstring.Length < 2)
                    return "Currently, the max response length in characters is set to: " + RunVars.OnySpamThreshold.ToString();
                if (!int.TryParse(splittedstring[1], out _OnySpamThreshold))
                    return "I sense a disturbance in the integer.";
                RunVars.OnySpamThreshold = _OnySpamThreshold;
                return "Succesfully set spam threshold to: " + RunVars.OnySpamThreshold;
            }
        }


    }
}
