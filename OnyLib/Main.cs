using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CedLib;
using CedLib.Persistence.XMLPersistenceDictionary;

namespace OnyLib
{
    namespace SpecialClasses
    {
        public class BotFunctionData
        {
            public BotFunctionData(AI.nodestruct _nstruct = null, string _input = null, string _name = "Guest")
            {
                nstruct = _nstruct;
                input = _input;
                Name = _name;
            }
            public AI.nodestruct nstruct { get; set; }
            public string input { get; set; }
            public string Name { get; set; }
        }

        public class IncomingMessageData
        {
            private string _CommandName;
            private string _UserName;
            private System.Net.IPEndPoint _IP;
            private string[] _ExtraneousLines;
            public IncomingMessageData(string[] _messageData, System.Net.IPEndPoint _remoteIP)
            {
                _CommandName = _messageData[0];
                if (_messageData.Length > 1)
                    _UserName = _messageData[1];
                _IP = _remoteIP;
                if (_messageData.Length > 2)
                {
                    List<string> blargh = new List<string>();
                    for (int i = 2; i < _messageData.Length; i++)
                    {
                        blargh.Add(_messageData[i]);
                    }
                    _ExtraneousLines = blargh.ToArray();
                }
            }
            public string CommandName { get { return _CommandName; } }
            public string UserName { get { return _UserName; } }
            public System.Net.IPEndPoint IP { get { return _IP; } }
            public string[] ExtraneousLines { get { return _ExtraneousLines; } }
        }
    }
    namespace FunctionHandlers
    {

    }
    public class BotStuff
    {
        public BotEvents OnyEvents = new BotEvents();
        public RuntimeVariables OnyVariables = new RuntimeVariables();
        public BotFunctionStuff OnyFunctions;
        public BotStuff()
        {
            OnyFunctions = new BotFunctionStuff(OnyVariables);
        }
        public class BotEvents
        {
            public class IncomingMessageEventData : EventArgs
            {
                public IncomingMessageEventData(SpecialClasses.IncomingMessageData _msg, List<string> _toreturn)
                {
                    msg = _msg;
                    ToReturn = _toreturn;
                }
                public SpecialClasses.IncomingMessageData msg { get; set; }
                public List<string> ToReturn { get; set; }
            }
            public delegate void IncomingMessageHook(IncomingMessageEventData Args);
            public event IncomingMessageHook InComingMessage;
            public string NewMessage(string[] _message, System.Net.IPEndPoint RemoteIP)
            {
                if (InComingMessage != null)
                {
                    IncomingMessageEventData data = new IncomingMessageEventData(new SpecialClasses.IncomingMessageData(_message, RemoteIP), new List<string>());
                    string ToReturn = "";
                    InComingMessage(data);
                    foreach (var item in data.ToReturn)
                    {
                        if (item.Length > 0)
                            ToReturn = item;
                    }
                    return ToReturn;
                }
                else return null;
            }

        }
        public class RuntimeVariables
        {
            public RuntimeVariables()
            {
                persistence = new XMLPersistenceDictionary(logger, "funcpersistence.xml",false);
            }
            public Logging logger = new Logging(true,false);
            public XMLPersistenceDictionary persistence;
            public DateTime lastused = DateTime.Now;

            public bool run = true;
            public System.Threading.ManualResetEvent Donify = new System.Threading.ManualResetEvent(false);


            public int OnySpamThreshold = 512;
            public bool OnySpamProtection = true;


            public DateTime starttime = DateTime.Now;
            public long amountloops = 0;




            public AI.nodestruct nstruct = new AI.nodestruct(true);

            public Dictionary<string, KeyValuePair<string, Func<SpecialClasses.BotFunctionData, string>>> SharedUnprivelegedFunctionDict = new Dictionary<string, KeyValuePair<string, Func<SpecialClasses.BotFunctionData, string>>>();
            public Dictionary<string, KeyValuePair<string, Func<SpecialClasses.BotFunctionData, string>>> SharedPrivelegedFunctionDict = new Dictionary<string, KeyValuePair<string, Func<SpecialClasses.BotFunctionData, string>>>();


        }
        public class BotFunctionStuff
        {
            RuntimeVariables RunVars;
            public BotFunctions.NonPriviligedFunctions nonPrivFunctions;
            public BotFunctions.PriviligedFunctions PrivFunctions;
            public BotFunctionStuff(RuntimeVariables _runvars)
            {
                RunVars = _runvars;
                nonPrivFunctions = new BotFunctions.NonPriviligedFunctions(RunVars);
                PrivFunctions = new BotFunctions.PriviligedFunctions(RunVars);
                AddFunctions();
            }
            void AddFunctions()
            {
                RunVars.SharedUnprivelegedFunctionDict.Add("statusword", new KeyValuePair<string,Func<SpecialClasses.BotFunctionData,string>>("Prints the childnodes this wordnode is referring to, with the weight of each childnode.", new Func<SpecialClasses.BotFunctionData,string>(nonPrivFunctions.statusword)));
                RunVars.SharedUnprivelegedFunctionDict.Add("statusreport",new KeyValuePair<string,Func<SpecialClasses.BotFunctionData,string>>("Prints out a report about the status of the bot and its learning process.", new Func<SpecialClasses.BotFunctionData,string>(nonPrivFunctions.statusreport)));
                RunVars.SharedUnprivelegedFunctionDict.Add("uptime",new KeyValuePair<string,Func<SpecialClasses.BotFunctionData,string>>("Prints the uptime of the computer the bot is running on.", new Func<SpecialClasses.BotFunctionData,string>(nonPrivFunctions.uptime)));

                RunVars.SharedPrivelegedFunctionDict.Add("removeconjunction", new KeyValuePair<string,Func<SpecialClasses.BotFunctionData,string>> ("Removes a certain word conjunction. usage: removeconjunction someword someotherword", new Func<SpecialClasses.BotFunctionData,string>(PrivFunctions.removeconjunction)));
                RunVars.SharedPrivelegedFunctionDict.Add("purgenow",new KeyValuePair<string,Func<SpecialClasses.BotFunctionData,string>>("Runs the bad node purge algorhythm now. Don't run this too much, it'll dumb the bot down, though it might help now and then.", new Func<SpecialClasses.BotFunctionData,string>(PrivFunctions.purgenow)));
                RunVars.SharedPrivelegedFunctionDict.Add("setratelimit",new KeyValuePair<string,Func<SpecialClasses.BotFunctionData,string>>("Set a rate limit.", new Func<SpecialClasses.BotFunctionData,string>(PrivFunctions.setratelimit)));
                RunVars.SharedPrivelegedFunctionDict.Add("toggleratelimit",new KeyValuePair<string,Func<SpecialClasses.BotFunctionData,string>>("Toggles the ratelimit.", new Func<SpecialClasses.BotFunctionData,string>(PrivFunctions.toggleratelimit)));
                RunVars.SharedPrivelegedFunctionDict.Add("toggledebug", new KeyValuePair<string, Func<SpecialClasses.BotFunctionData, string>>("Toggles debug.", new Func<SpecialClasses.BotFunctionData, string>(PrivFunctions.toggledebug)));
                RunVars.SharedPrivelegedFunctionDict.Add("loadmemfile",new KeyValuePair<string,Func<SpecialClasses.BotFunctionData,string>>("Loads a memory file. Usage: loadmemfile _filename relative to executable_", new Func<SpecialClasses.BotFunctionData,string>(PrivFunctions.loadmemfile)));
                RunVars.SharedPrivelegedFunctionDict.Add("savexmlnow",new KeyValuePair<string,Func<SpecialClasses.BotFunctionData,string>>("Saves the settings to XML settings right now. Overrides normal save rate.", new Func<SpecialClasses.BotFunctionData,string>(PrivFunctions.savenow)));
                RunVars.SharedPrivelegedFunctionDict.Add("loadxmlnow",new KeyValuePair<string,Func<SpecialClasses.BotFunctionData,string>>("Loads XML settings right now.",new Func<SpecialClasses.BotFunctionData,string>(PrivFunctions.loadnow)));
                RunVars.SharedPrivelegedFunctionDict.Add("changexml",new KeyValuePair<string,Func<SpecialClasses.BotFunctionData,string>>("Changes XML persistent settings. Syntax: changexml add|del|delchild|change %dictitem% %itemtoadd|del|change% %change|addargument% .", new Func<SpecialClasses.BotFunctionData,string>(PrivFunctions.changexml)));
                RunVars.SharedPrivelegedFunctionDict.Add("printxml",new KeyValuePair<string,Func<SpecialClasses.BotFunctionData,string>>("Prints the contents of an xml dictionary, usage: printxml _dictionaryname_", new Func<SpecialClasses.BotFunctionData,string>(PrivFunctions.printxml)));
                RunVars.SharedPrivelegedFunctionDict.Add("setmaxresponselength",new KeyValuePair<string,Func<SpecialClasses.BotFunctionData,string>>("Sets the max response length of the bot (5 words may be added), with a minimum of 2.", new Func<SpecialClasses.BotFunctionData,string>(PrivFunctions.SetMaxSentenceLength)));
                RunVars.SharedPrivelegedFunctionDict.Add("shutdowngracefully",new KeyValuePair<string,Func<SpecialClasses.BotFunctionData,string>>("Gracefully saves all data and shuts the bot down.",new Func<SpecialClasses.BotFunctionData,string>(PrivFunctions.ShutdownGracefully)));
                RunVars.SharedPrivelegedFunctionDict.Add("toggleonyantispam",new KeyValuePair<string,Func<SpecialClasses.BotFunctionData,string>>("Toggles onymity's builtin anti spam protection (The one that prevents ony from spamming, not from being spammed).", new Func<SpecialClasses.BotFunctionData,string>(PrivFunctions.ToggleAntiOnySpam)));
                RunVars.SharedPrivelegedFunctionDict.Add("setonyantispam",new KeyValuePair<string,Func<SpecialClasses.BotFunctionData,string>>("Sets the max length of the return string if ony's antispam is enabled. No commands will print the current setting.", new Func<SpecialClasses.BotFunctionData,string>(PrivFunctions.OnyAntiSpamMaxReturnLength)));
            }

        }
    }   
}
