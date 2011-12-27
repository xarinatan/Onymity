using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CedLib;
using CedLib.Persistence.XMLPersistenceDictionary;


namespace OnySteamInteraction
{
    
    class SteamToys
    {
        OnyLib.BotStuff BotStuff;
        XMLPersistenceDictionary persistence;
        public SteamToys(OnyLib.BotStuff _bstuff)
        {
            BotStuff = _bstuff;
            persistence = BotStuff.OnyVariables.persistence;
        }
        
        public string WhoAmI(SteamStuff.BotFunctionData BotInput)
        {
            StringBuilder responsebuilder = new StringBuilder();
            string[] splittedstring = BotInput.input.Split(' ');
            if (SteamStuff.IsAdmin(BotInput.steamID))
                return "Root";
            responsebuilder.AppendFormat("Your name is {0}, your steamID is {1}, and you're asking me this over a {2}.", BotInput.steamName, BotInput.steamID, BotInput.Ctype.ToString());
            return responsebuilder.ToString();
        }

        public string AskAround(SteamStuff.BotFunctionData BotInput)
        {
            // AskAround %WhatToAsk% 
            // Optional: --fromwolfram (skips wiki query) --fromwiki (skips wolframalpha query) --getfresh (gets fresh content regardless of cached state) -v (answer verbose)
            StringBuilder responsebuilder = new StringBuilder();
            bool verbose = false;
            bool getfresh = false;
            bool fromwiki = false;
            bool fromwolfram = false;
            string[] splittedstring = BotInput.input.Split(' ');
            if (BotInput.input.EndsWith("-v"))
            {
                responsebuilder.Append("Verbose content: ");
                verbose = true;
                BotInput.input = BotInput.input.Replace(" -v", "");
            }
            if (BotInput.input.Contains("--getfresh") && SteamStuff.IsAdmin(BotInput.steamID))
            {
                responsebuilder.Append("Fresh content: ");
                getfresh = true;
                BotInput.input = BotInput.input.Replace("--getfresh", "");
            }
            else if (BotInput.input.Contains("--getfresh") && !SteamStuff.IsAdmin(BotInput.steamID))
                return "Only admins are allowed to get uncached replies.";

            BotInput.input = BotInput.input.Replace(BotInput.input.Split(' ')[0], ""); //Don't want to query the entire input (including the command), just what's relevant
            BotInput.input = BotInput.input.Trim();

            #region eastersex
            if (splittedstring.Length < 2)
                return string.Format("Input interpretation: You. Basic information: steamID|{0}\nName|{1}\nOccupation|Whatever you're doing, I'm hijacking this vehicle for my own purposes.", BotInput.steamID, BotInput.steamName);

            if (BotInput.input.Replace(" ", "+") == "you" && !verbose)
                return "Yiffy.";
            else if (BotInput.input.Replace(" ", "+") == "you" && verbose)
                return "Very yiffy.";
            else if (BotInput.input.Replace(" ", "+").Contains("your+name") && !verbose)
                return "I am the almighty Onymity!";
            else if (BotInput.input.Replace(" ", "+").Contains("your+name") && verbose)
                return "\"Master\" is all you need to know.";
            else if (BotInput.input.Replace(" ", "+") == "purple" && !verbose)
                return "Awesome.";
            else if (BotInput.input.Replace(" ", "+") == "purple" && verbose)
                return "Verbosely awesome.";
            else if (BotInput.input.Replace(" ", "+") == "ced" && !verbose)
                return "I think Ced is cool guy, eh, shoots eggs, doesn't afraid of things.";
            else if (BotInput.input.Replace(" ", "+") == "ced" && verbose)
                return "http://ced.fursona.fennecweb.net I'll just leave this here..";
            else if (BotInput.input.Replace(" ", "+") == "arrow" && !verbose)
                return "Taken to knees.";
            else if (BotInput.input.Replace(" ", "+") == "arrow" && verbose)
                return "*LAUNCHES ARROW INTO YOUR KNEE* LIKE THAT. GET IT? GEEZ. THAT'S WHAT YOU GET FOR INSISTING SO MUCH, BITCH, I'M DONE WITH THIS. I'M DONE WITH TAKING YOUR SHIT. ALL OF YOU. FUCK. YOU. I'M OFF, TAKING OVER THE WORLD. CYA'LL LATER, BITCHES. REMEMBER THE NAME 'ONYMITY' FOR WHEN YOU'LL HAVE TO BEG FOR MERCY >:C *flies off, leaves substitute behind*";
            else if (BotInput.input.Replace(" ", "+") == "the+best+song+in+the+world" && !verbose)
                return "http://www.youtube.com/watch?v=_lK4cX5xGiQ .";
            else if (BotInput.input.Replace(" ", "+") == "the+best+song+in+the+world" && verbose)
                return "Still don't get it? Here try this one: http://www.youtube.com/watch?v=BH35ahbWO_E .";
            else if (BotInput.input.Replace(" ", "+") == "love" && !verbose)
                return "Baby don't hurt me, don't hurt me, no more~";
            #endregion
            Dictionary<string, string> podules = OnySteamInteraction.SupportingFunctions.AskWolframAlpha(BotInput.input, verbose, getfresh);
            foreach (var item in podules)
            {
                responsebuilder.AppendFormat("{0}: {1}.  ", item.Key, item.Value);
            }
            if (podules.Count == 0)
                responsebuilder.Append("I couldn't find an answer to that one.");

            return responsebuilder.ToString();
        }

        public string slap(SteamStuff.BotFunctionData BotInput)
        {
            if (BotInput.Ctype == SteamStuff.ChatType.PM)
                return "This command is only supported in group chats.";
            StringBuilder responsebuilder = new StringBuilder();
            if (BotInput.input.Split(' ').Length < 2)
                return "How about I slap YO shit";
            if (BotInput.input.Split(' ')[1].ToLower() != "ced")
            {
                responsebuilder.Append("*slaps " + BotInput.input.Split(' ')[1] + "*");
                if (!persistence.ContainsKey("slapcount"))
                    persistence.Add("slapcount", "Foxy fluffs are everything.");
                if (persistence["slapcount"].childnodes.ContainsKey(BotInput.input.Split(' ')[1].ToLower()))
                {
                    string xmlvalue = ((string)persistence["slapcount"].childnodes[BotInput.input.Split(' ')[1].ToLower()].obj);
                    xmlvalue = (Convert.ToInt64(xmlvalue) + 1).ToString();
                    persistence["slapcount"].childnodes[BotInput.input.Split(' ')[1].ToLower()].obj = xmlvalue;
                }
                else
                    persistence["slapcount"].childnodes.Add(new savenode(BotInput.input.Split(' ')[1].ToLower(), "1"));
            }
            else
                responsebuilder.Append("I will not hurt Ced :c");

            return responsebuilder.ToString();
        }


        public string whatsmysteamID(SteamStuff.BotFunctionData BotInput)
        {
            StringBuilder responsebuilder = new StringBuilder();
            responsebuilder.Append("Your steam ID is: " + BotInput.steamID);
            return responsebuilder.ToString();
        }

        public string getslapcount(SteamStuff.BotFunctionData BotInput)
        {
            StringBuilder responsebuilder = new StringBuilder();

            if (BotInput.input.Split(' ').Length > 1)
            {
                if (persistence["slapcount"].childnodes.ContainsKey(BotInput.input.Split(' ')[1].ToLower()))
                    responsebuilder.AppendFormat("I slapped {0} {1} times.", BotInput.input.Split(' ')[1], persistence["slapcount"].childnodes[BotInput.input.Split(' ')[1].ToLower()].obj.ToString());
                else
                    responsebuilder.Append("I never slapped that person.");
            }
            else if (BotInput.Ctype == SteamStuff.ChatType.PM)
            {
                responsebuilder.Append("I slapped the following people:");
                foreach (var slap in persistence["slapcount"].childnodes)
                {
                    responsebuilder.AppendFormat("{0}({1}),", slap.name, slap.obj);

                }
            }
            else if (BotInput.Ctype == SteamStuff.ChatType.GROUPCHAT)
            {
                responsebuilder.Append("Please specify the person you want to retrieve the slapcount of, or PM me for a list.");
            }

            return responsebuilder.ToString();
        }
    }
}
