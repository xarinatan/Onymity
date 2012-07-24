using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CedLib;
using OnyLib;
using System.Net;

namespace OnySteamInteraction
{
    public static class Augmentation
    {
        static Dictionary<string, KeyValuePair<string, Func<SteamStuff.BotFunctionData, string>>> unPrivFunctDict = new Dictionary<string, KeyValuePair<string, Func<SteamStuff.BotFunctionData, string>>>();
        static Dictionary<string, KeyValuePair<string, Func<SteamStuff.BotFunctionData, string>>> PrivFunctDict = new Dictionary<string, KeyValuePair<string, Func<SteamStuff.BotFunctionData, string>>>();
        static SteamToys unprivSteamToys;
        public static OnyLib.BotStuff Botstuff;
        static SteamKit2.SteamClient sclient;

        static List<IPAddress> AllowedSteamClientIPs = new List<IPAddress>() { IPAddress.Loopback, IPAddress.Parse("192.168.5.200"), IPAddress.Parse("192.168.5.50") };
        public static void Main(object _BotStuff)
        {
            Botstuff = (OnyLib.BotStuff)_BotStuff;
            unprivSteamToys = new SteamToys(Botstuff);
            addfunctions();
            Botstuff.OnyEvents.InComingMessage += new BotStuff.BotEvents.IncomingMessageHook(OnyEvents_InComingMessage);
            Botstuff.OnyVariables.logger.log("Hello from SteamInteraction!", Logging.Priority.Notice);
            sclient = new SteamKit2.SteamClient(System.Net.Sockets.ProtocolType.Tcp);
            SteamKit2.SteamFriends steamFriends = sclient.GetHandler<SteamKit2.SteamFriends>();
            SteamKit2.SteamUser sUser = sclient.GetHandler<SteamKit2.SteamUser>();
            sclient.Connect();
            sUser.LogOn(new SteamKit2.SteamUser.LogOnDetails() { Username = "megal33t", Password = "0fqwoh" });
            steamFriends.SendChatMessage(new SteamKit2.SteamID("STEAM_0:1:16516144"), SteamKit2.EChatEntryType.ChatMsg, "Hai dar");
        }

        static void OnyEvents_InComingMessage(OnyLib.BotStuff.BotEvents.IncomingMessageEventData Args)
        {
            if (Args.msg.CommandName == "interactsteam")
            {
                if (!AllowedSteamClientIPs.Contains(Args.msg.IP.Address))
                    throw new Exception("ERROR! REMOTE ENDPOINT IS NOT IN ALLOWED ENDPOINTS, SUSPECTED HACK ATTEMPT!");
                SteamStuff.ChatType ctype;
                if (!Enum.TryParse(Args.msg.ExtraneousLines[1], false, out ctype))
                {
                    Botstuff.OnyVariables.logger.log("INVALID CHAT ENTRY WAS USED AND NO REPLY WAS SENT. CTYPE USED WAS: " + Args.msg.ExtraneousLines[1], Logging.Priority.Critical);
                    Args.ToReturn.Add("(CRITICAL) Chat type not recognized!!");
                }
                Args.ToReturn.Add(InteractSteam(new SteamStuff.BotFunctionData(Botstuff.OnyVariables.nstruct, Args.msg.ExtraneousLines[2], Args.msg.ExtraneousLines[0], Args.msg.UserName, ctype)));
            }
            
        }

        static string InteractSteam(SteamStuff.BotFunctionData BotData)
        {
            Botstuff.OnyVariables.nstruct.parsestring(BotData.input);
            string[] splittedinput = BotData.input.Split(' ');
            StringBuilder sbuilder = new StringBuilder();
            for (int i = 0; i < splittedinput.Length; i++)
            {
                if (!Botstuff.OnyVariables.nstruct.blacklist.Contains(splittedinput[i]))
                {
                    sbuilder.Append(splittedinput[i]);
                    if (i < splittedinput.Length-1)
                        sbuilder.Append(" ");
                }
            }
            BotData.input = sbuilder.ToString();
            string commandword = BotData.input.Split(' ')[0];
            if(unPrivFunctDict.ContainsKey(commandword))
            {
                return unPrivFunctDict[BotData.input.Split(' ')[0]].Value(BotData);
            }
            else if (Botstuff.OnyVariables.SharedUnprivelegedFunctionDict.ContainsKey(commandword))
            {
                return Botstuff.OnyVariables.SharedUnprivelegedFunctionDict[commandword].Value(new OnyLib.SpecialClasses.BotFunctionData(BotData.nstruct, BotData.input, BotData.steamName));
            }
            else if(SteamStuff.IsAdmin(BotData.steamID) && Botstuff.OnyVariables.SharedPrivelegedFunctionDict.ContainsKey(commandword))
            {
                return Botstuff.OnyVariables.SharedPrivelegedFunctionDict[commandword].Value(new OnyLib.SpecialClasses.BotFunctionData(BotData.nstruct, BotData.input, BotData.steamName));
            }
            else if (commandword == "manual")
            {
                if (BotData.input.Split(' ').Length < 2)
                {
                    StringBuilder rbuilder = new StringBuilder();
                    foreach (var item in unPrivFunctDict)
                    {
                        rbuilder.Append(item.Key + ",");
                    }
                    foreach (var item in Botstuff.OnyVariables.SharedUnprivelegedFunctionDict)
                    {
                        rbuilder.Append(item.Key + ",");
                    }
                    if (SteamStuff.IsAdmin(BotData.steamID))
                        foreach (var item in Botstuff.OnyVariables.SharedPrivelegedFunctionDict)
                        {
                            rbuilder.Append(item.Key + ",");
                        }
                    return rbuilder.ToString();
                }
                else if (BotData.input.Split(' ').Length > 1)
                {
                    if (SteamStuff.IsAdmin(BotData.steamID) && Botstuff.OnyVariables.SharedPrivelegedFunctionDict.ContainsKey(BotData.input.Split(' ')[1]))
                        return Botstuff.OnyVariables.SharedPrivelegedFunctionDict[BotData.input.Split(' ')[1]].Key;
                    else if (Botstuff.OnyVariables.SharedUnprivelegedFunctionDict.ContainsKey(BotData.input.Split(' ')[1]))
                        return Botstuff.OnyVariables.SharedUnprivelegedFunctionDict[BotData.input.Split(' ')[1]].Key;
                }

            }
            return Botstuff.OnyVariables.nstruct.MakeResponse(BotData.input);
        }

        static void addfunctions()
        {

            unPrivFunctDict.Add("whatis",new KeyValuePair<string,Func<SteamStuff.BotFunctionData,string>>("Ask me questions and I'll answer them! Use -v for verbose replies.", new Func<SteamStuff.BotFunctionData,string>(unprivSteamToys.AskAround)));
            unPrivFunctDict.Add("whoami",new KeyValuePair<string,Func<SteamStuff.BotFunctionData,string>>("Shows your true nature!", new Func<SteamStuff.BotFunctionData,string>(unprivSteamToys.WhoAmI)));
            unPrivFunctDict.Add("whatsmysteamid",new KeyValuePair<string,Func<SteamStuff.BotFunctionData,string>>("Enriches you with unlimited wealth and sexual favors, obviously.", new Func<SteamStuff.BotFunctionData,string>(unprivSteamToys.whatsmysteamID)));
            unPrivFunctDict.Add("slap",new KeyValuePair<string,Func<SteamStuff.BotFunctionData,string>>("I'm a cool guy, eh, i slap people, doesn't afraid of them.", new Func<SteamStuff.BotFunctionData,string>(unprivSteamToys.slap)));
            unPrivFunctDict.Add("slapcount",new KeyValuePair<string,Func<SteamStuff.BotFunctionData,string>>("Gets the amount of times i slapped someone, if no name is specified i'll print it for everyone.", new Func<SteamStuff.BotFunctionData,string>(unprivSteamToys.getslapcount)));


        }
    }
    public static class SteamStuff
    {
        public class BotFunctionData
        {
            public BotFunctionData(OnyLib.AI.nodestruct _nstruct = null, string _input = null, string _steamID = null, string _steamName = null, ChatType _Ctype = ChatType.PM)
            {
                nstruct = _nstruct;
                input = _input;
                steamID = _steamID;
                steamName = _steamName;
                Ctype = _Ctype;
            }
            public OnyLib.AI.nodestruct nstruct { get; set; }
            public string input { get; set; }
            public string steamID { get; set; }
            public string steamName { get; set; }
            public ChatType Ctype { get; set; }
        }
        public static List<string> adminIDs = new List<string>() //need to get this from persistence file later instead of hardcoded.
        { 
            "STEAM_0:1:16516144" //Ced
        };

        public enum ChatType //valid, known chat types.
        {
            GROUPCHAT, PM
        }

        public static bool IsAdmin(string SteamID)
        {
            return adminIDs.Contains(SteamID);
        }
    }

}
