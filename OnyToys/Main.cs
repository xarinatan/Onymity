using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OnyLib.SpecialClasses;
using CedLib.Persistence.XMLPersistenceDictionary;

namespace OnyToys
{
    public static class Augmentation
    {
        static Toys unPrivFunctions;
        public static OnyLib.BotStuff Botstuff; 
        public static void Main(OnyLib.BotStuff _BotStuff)
        {
            Botstuff = _BotStuff;
            unPrivFunctions = new Toys(Botstuff);
            addfunctions();
        }

        static void addfunctions()
        {
            Botstuff.OnyVariables.SharedUnprivelegedFunctionDict.Add("roll", new KeyValuePair<string, Func<BotFunctionData, string>>("Rolls dices. e.g \"roll 5d6\" rolls 5 dices with 6 sides.", new Func<BotFunctionData, string>(unPrivFunctions.roll)));
            Botstuff.OnyVariables.SharedUnprivelegedFunctionDict.Add("addnewurl", new KeyValuePair<string, Func<BotFunctionData, string>>("Adds new urls to my recommendation list!", new Func<BotFunctionData, string>(unPrivFunctions.addurl)));
            Botstuff.OnyVariables.SharedUnprivelegedFunctionDict.Add("recommendurl", new KeyValuePair<string, Func<BotFunctionData, string>>("I might know a few nice things around the net! (USE AT VIEWER DESCRETION :3)", new Func<BotFunctionData, string>(unPrivFunctions.geturl)));
            Botstuff.OnyVariables.SharedUnprivelegedFunctionDict.Add("8ball", new KeyValuePair<string, Func<BotFunctionData, string>>("Ask a question that can be answered by yes or no, and i shall deliver!", new Func<BotFunctionData, string>(unPrivFunctions.eightball)));
            Botstuff.OnyVariables.SharedUnprivelegedFunctionDict.Add("getquote", new KeyValuePair<string, Func<BotFunctionData, string>>("Gets a quote on your car insurance, obviously.", new Func<BotFunctionData, string>(unPrivFunctions.GetQuote)));

        }

    }

    public class Toys
    {
        OnyLib.BotStuff BotStuff;
        DateTime LastUpdate = DateTime.Now.AddDays(-1);
        int e621Posts = 0;
        public XMLPersistenceDictionary persistence;
        public string QuotesFile = "quotes.txt";
        public List<string> Quotes = new List<string>();
        public CedLib.Logging logger;

        public Toys(OnyLib.BotStuff _bstuff)
        {
            BotStuff = _bstuff;
            persistence = BotStuff.OnyVariables.persistence;
            logger = BotStuff.OnyVariables.logger;
        }

        public string addurl(BotFunctionData BotInput)
        {
            StringBuilder responsebuilder = new StringBuilder();
            if (BotInput.input.Split(' ').Length < 2)
                return "You have to provide an url, silly.";
            if (!persistence.ContainsKey("urls"))
                persistence.Add(new savenode("urls", "urls!"));
            System.Uri url;
            if (System.Uri.TryCreate(BotInput.input.Split(' ')[1], UriKind.Absolute, out url))
            {
                if (!persistence["urls"].childnodes.ContainsValue(BotInput.input.Split(' ')[1]))
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

        public string geturl(BotFunctionData BotInput)
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

        public string eightball(BotFunctionData BotInput)
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
            else if (persistence["eightballanswers"].childnodes.Count < 1)
            {
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
            if (BotInput.nstruct.debug)
                responsebuilder.AppendFormat("({0}/{1})", x, eightballanswers.childnodes.Count);
            return responsebuilder.ToString();
        }




        public string GetQuote(BotFunctionData BotInput)
        {
            StringBuilder responsebuilder = new StringBuilder();
            string[] splittedstring = BotInput.input.Split(' ');
            if (Quotes.Count < 1)
            {
                try
                {
                    Quotes.AddRange(System.Text.RegularExpressions.Regex.Split(System.IO.File.ReadAllText(QuotesFile), "\r\n.\r\n"));
                }
                catch (System.Exception ex)
                {
                    logger.logerror(ex); return ex.Message;
                }
            }
            Random rnd = new Random();
            responsebuilder.Append(Quotes[rnd.Next(0, Quotes.Count)]);

            return responsebuilder.ToString();
        }



        public string roll(BotFunctionData BotInput)
        {
            StringBuilder responsebuilder = new StringBuilder();
            try
            {
                string[] vars = BotInput.input.Split(' ')[1].Split('d');
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
                responsebuilder.AppendFormat("(avg:{0}, total:{1})", average, total);
            }
            catch (Exception ex)
            {
                responsebuilder.Append(ex.Message);
                logger.logerror(ex);
            }

            return responsebuilder.ToString();
        }

        void updateporn()
        {
            if ((DateTime.Now - LastUpdate).TotalMinutes < 30)
                return;

            System.Net.WebClient client = new System.Net.WebClient();
            string page = client.DownloadString("http://e621.net");

            System.Text.RegularExpressions.Regex image = new System.Text.RegularExpressions.Regex(@"Serving ([0-9],+) posts");
            var match = image.Match(page);

            if (match.Success)
            {
                e621Posts = int.Parse(match.Groups[1].Value.Replace(",",""));
            }
            else
                logger.log("Couldn't read porncount :(", CedLib.Logging.Priority.Error);

            LastUpdate = DateTime.Now;
        }

        public string recommendporn(BotFunctionData BotInput)
        {
            updateporn();
            StringBuilder responsebuilder = new StringBuilder();
            responsebuilder.Append("People apparently like: ");

            if (e621Posts <= 0)
                responsebuilder.Append("Nothing? That's strange...");
            else
            {
                Random postNumber = new Random();
                System.Net.WebClient client = new System.Net.WebClient();
                int post = postNumber.Next(1,e621Posts);
                string page = client.DownloadString("http://e621.net/post/show/" + post.ToString());

                System.Text.RegularExpressions.Regex image = new System.Text.RegularExpressions.Regex("Size:.*href=\"([^\"]+)");
                var match = image.Match(page);

                string imageURL = "Wrong somehow :|";
                if (match.Success)
                    imageURL = match.Groups[1].Value;
                else
                    logger.log("Unable to find image in post " + post.ToString(), CedLib.Logging.Priority.Error);

                responsebuilder.Append("http://e621.net"); 
                responsebuilder.Append(imageURL);
                responsebuilder.Append(" [NSFW]");
            }

            return responsebuilder.ToString();
        }
    }
}
