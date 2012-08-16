using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CedLib.Persistence.XMLPersistenceDictionary;

namespace OnyLib
{
    namespace AI
    {
        #region Dictionary-based sentence AI
        //This is a work in progress.
        public class DictionaryConstruct
        {
            #region vars
            XMLPersistenceDictionary AllNodes = new XMLPersistenceDictionary("AIDictionary.xml", false);


            #endregion


            public DictionaryConstruct(bool prelearn)
            {
                
            }
        }


        #endregion


        #region node-based word AI
        public class nodestruct
        {
            #region vars
            public Dictionary<string, node> translatenodes = new Dictionary<string, node>();
            public List<string> playback = new List<string>();


            public List<string> blacklist = new List<string>();
            public List<string> stripchars = new List<string>() { };
            public List<string> aliasses = new List<string>() { "onymity", "ony", "ony!", "ony,", "ony.", "ony?", ",ony" };


            FileInfo playbackfile = new FileInfo("./playback.mem");
            public string myname = "onymity";
            string prevreceived = "";
            DateTime prevreceivedtime = DateTime.Now.Subtract(new TimeSpan(1, 1, 1));
            DateTime prevsentresponse = DateTime.Now.Subtract(new TimeSpan(1, 1, 1));
            public bool ratelimit = false;
            public int secondsbetweenresponse = 30;
            public int maxresponselength = 10;
            public bool debug = false;
            public long wordsignored = 0;
            public long totalnodespurged = 0;
            public bool purgenodes = true;
            public bool isprelearning = false;


            #endregion

            public nodestruct(bool prelearn)
            {
                blacklist.Add(myname);
                foreach (string alias in aliasses)
                {
                    blacklist.Add(alias);
                }
                if (prelearn)
                    foreach (string sentence in new string[] {
                    //"Mijn naam is ony",
                    //"Ik ben een chatbot.",
                    //"De kat krabt de krullen van de trap.",
                    //"Liesje leerde lotje lopen langs de lange linden laan",
                    //"Kaas is lekker"

                    "My name is ony",
                "The cat is a limbo dancer.",
                "The dog hates cats.", 
                "Little Mary had a flute.", 
                "Cheese is always a good answer.", 
                "Benji is a faggot.", 
                "Faggots are cigarretes.", 
                "Y U NO CHEESE" ,

                })
                    {
                        parsestring(sentence);
                    }
            }

            ~nodestruct()
            {
                saveplayback();
            }
            public void toggledebug()
            {
                debug = !debug;
            }
            public void toggleratelimit()
            {
                ratelimit = !ratelimit;
            }

            public void saveplayback(string filename = "./playback.mem")
            {
                lock (playbackfile)
                {
                    playbackfile = new FileInfo(filename);
                    if (File.Exists(filename))
                        File.Delete(filename);

                    File.WriteAllLines(playbackfile.FullName, playback.ToArray());
                }
            }

            public string loadplayback(string filename = "./playback.mem")
            {
                System.Diagnostics.Stopwatch swatch = new System.Diagnostics.Stopwatch();
                playbackfile = new FileInfo(filename);
                swatch.Start();
                purgenodes = false;
                isprelearning = true;
                float linesdone = 0;
                string[] lines = File.ReadAllLines(filename);
                playback.AddRange(lines);
                foreach (string line in lines)
                {
                    parsestring(line);
                    linesdone++;
                    if (linesdone % 10 == 0)
                        Console.Write("\rLearning from playback file.. {0}% complete.({1}/{2})", ((linesdone / (float)lines.LongLength) * 100).ToString(), linesdone, lines.LongLength);
                }
                Console.Write("\rLearning from playback file.. {0}% complete.({1}/{2})", ((linesdone / (float)lines.LongLength) * 100).ToString(), linesdone, lines.LongLength);
                Console.WriteLine();
                purgenodes = true;
                isprelearning = false;
                int purgecount = 0;
                while (purgecount < 20)
                {
                    purgebadnodes();
                    purgecount++;
                }
                swatch.Stop();
                string report = string.Format("Done learning!\nWords ignored: {0}\nTotal nodes count: {1}\nTime taken: {2}ms", wordsignored, translatenodes.Count, swatch.ElapsedMilliseconds);
                Console.WriteLine(report);
                return report;

            }
            public string stripnames(string input)
            {
                StringBuilder output = new StringBuilder();
                string[] splittedinput = input.Split(' ');
                foreach (var word in splittedinput)
                {
                    if (!aliasses.Contains(word.ToLower()))
                        output.Append(word + " ");
                }
                return output.ToString();
            }



            public bool StartsWithCapital(string input)
            {
                if (input.Length > 0)
                    return Char.IsUpper(input[0]);
                else
                    return false;
            }


            public string MakeResponse(string input = "")
            {
                if (debug)
                    Console.WriteLine("start making response");
                if (ratelimit && (prevsentresponse.AddSeconds(secondsbetweenresponse) >= DateTime.Now))
                    return "";
                StringBuilder sbuilder = new StringBuilder();
                Random r = new Random();
                int length;
                if (translatenodes.Count < 20)
                    length = r.Next(0, translatenodes.Count);
                else
                    length = r.Next(1, maxresponselength);
                string prevword = translatenodes.Keys.ToArray()[r.Next(0, translatenodes.Count - 1)];
                while (!StartsWithCapital(prevword))
                    prevword = translatenodes.Keys.ToArray()[r.Next(0, translatenodes.Count - 1)];
                sbuilder.Append(prevword + " ");
                int loopsdone = 0;
                int maxnodevalue = 0;
                foreach (KeyValuePair<string, node> transkp in translatenodes)
                {
                    if (transkp.Value.childnodes.Count > 0)
                        foreach (KeyValuePair<node, int> kp in transkp.Value.childnodes)
                        {
                            if (kp.Value > maxnodevalue)
                                maxnodevalue = kp.Value;
                        }
                }
                bool curnodehaschildren = false;
                int continuedbecauseofchildren = 0;
                int lastwordssize = 20;
                string[] lastwords = new string[lastwordssize];
                int lastnodescounter = 0;
                if (debug)
                    Console.WriteLine("Finish initializing response maker");
                while (sbuilder.ToString().Split(' ').Length < length || loopsdone < (length * 5) || (curnodehaschildren && continuedbecauseofchildren < 5))
                {
                    //if(!(sbuilder.ToString().Split(' ').Length > length +5))
                    if (translatenodes[prevword].childnodes.Count > 0)
                    {
                        if (prevword[prevword.Length - 1] == '.')
                        {
                            Console.Write("%");
                            break;
                        }
                        if (lastnodescounter > lastwordssize - 1)
                            lastnodescounter = 0;
                        lastwords[lastnodescounter] = translatenodes[prevword].text;
                        lastnodescounter++;
                        maxnodevalue = 0;
                        foreach (KeyValuePair<node, int> kp in translatenodes[prevword].childnodes)
                        {
                            if (kp.Value > maxnodevalue)
                                maxnodevalue = kp.Value;
                        }
                        Random rnd = new Random();
                        int x = rnd.Next(0, maxnodevalue);
                        int y = 0;
                        if (x < maxnodevalue / 3)
                            y = rnd.Next(0, maxnodevalue / 2);
                        else
                            y = rnd.Next(maxnodevalue / 2, maxnodevalue);
                        if (debug)
                            Console.WriteLine("about to do some random shoopmahwhoop");
                        List<KeyValuePair<node, int>> results = translatenodes[prevword].childnodes.ToList().FindAll(kp => kp.Value > y);
                        KeyValuePair<node, int> result;
                        if (results.Count > 1)
                            result = results[rnd.Next(0, results.Count - 1)];
                        else if (results.Count == 1)
                            result = results[0];
                        else
                        {
                            //very rare but possible fuck up.
                            while (results.Count < 1)
                                results = translatenodes[translatenodes.Keys.ToArray()[r.Next(0, translatenodes.Count - 1)]].childnodes.ToList().FindAll(kp => kp.Value > 0);
                            result = results[0];
                        }
                        if (debug)
                            Console.WriteLine("Done with that, about to check for loops");
                        if (!lastwords.Contains(result.Key.text))
                        {
                            //if (translatenodes[prevword].childnodes[result.Key] > maxnodevalue / 2)
                            //    translatenodes[prevword].childnodes[result.Key]--;
                            prevword = result.Key.text;
                        }
                        else
                        {
                            if (debug)
                                sbuilder.Append("{loopbroken}");
                            prevword = translatenodes.Keys.ToArray()[r.Next(0, translatenodes.Count - 1)];
                            Console.Write("#");
                        }
                        if (debug)
                            sbuilder.Append(string.Format("[{0}]", translatenodes[prevword].childnodes.Count));
                        sbuilder.Append(prevword + " ");

                        if (translatenodes[prevword].childnodes.Count > 0)
                        {
                            if (sbuilder.ToString().Split(' ').Length > length)
                                continuedbecauseofchildren++;
                            curnodehaschildren = true;
                        }
                        else
                        {
                            curnodehaschildren = false;
                        }
                    }
                    else if (sbuilder.ToString().Split(' ').Count() >= length)
                    { break; }
                    else if (sbuilder.ToString().Split(' ').Count() < 3)
                    {
                        prevword = translatenodes.Keys.ToArray()[r.Next(0, translatenodes.Count - 1)];
                        sbuilder.Append(". " + prevword + " ");
                        if (debug)
                            sbuilder.Append("{out of childnodes, switching context}");
                        Console.Write("$");
                        if (translatenodes[prevword].childnodes.Count > 0)
                        {
                            continuedbecauseofchildren++;
                            curnodehaschildren = true;
                        }
                        else
                        {
                            curnodehaschildren = false;
                        }
                    }
                    else break;
                    loopsdone++;
                    if (debug)
                        Console.WriteLine("Done, new loop?");
                }

                prevsentresponse = DateTime.Now;
                return sbuilder.ToString();
            }


            long timesparsedsincelastpurge = 0;
            public void parsestring(string stringtoparse)
            {
                if (stringtoparse.Length == 0)
                    return;

                if (!isprelearning)
                    playback.Add(stringtoparse);

                foreach (string blackchar in stripchars)
                {
                    stringtoparse = stringtoparse.Replace(blackchar, "");
                }
                string[] splittedstring = stringtoparse.Split(' ');
                List<string> toParseString = new List<string>();
                for (int i = 0; i < splittedstring.Length; i++)
                {
                    if (splittedstring.Length-1 <= i)
                        break;
                    System.Uri asdfasdf;
                    if (System.Uri.TryCreate(splittedstring[i], UriKind.Absolute, out asdfasdf))
                        continue; //Don't want to add URIs to bot!
                    toParseString.Add(splittedstring[i] + " " + splittedstring[i + 1]);
                    i++;
                }
                int parsetimes = 3;
                for (int i = 0; i < parsetimes; i++)
                {
                    runparse(toParseString.ToArray());
                }

                timesparsedsincelastpurge++;
                if (timesparsedsincelastpurge > 1000 && purgenodes)
                    purgebadnodes();
            }
            void runparse(string[] splittedstring)
            {
                for (int i = 0; i < splittedstring.Length; i++)
                {
                    if (!(splittedstring[i].Length == 0))
                        if (!blacklist.Contains(splittedstring[i].ToLower())) //if the word is not in the blacklist
                        {
                            
                            //splittedstring[i] = splittedstring[i].ToLower(); //make all the letters of a sentence small. Possibly bad idea because of smilies.
                            if (i == 0)
                            {
                                char firstchar = splittedstring[i][0];
                                splittedstring[i] = splittedstring[i].Remove(0, 1);
                                splittedstring[i] = firstchar.ToString().ToUpper() + splittedstring[i];
                            } //make the first letter of the first word of a sentence a capital.
                            if (i == splittedstring.Length - 1
                                && (splittedstring[i][splittedstring[i].Length - 1] != '.')
                                && (splittedstring[i][splittedstring[i].Length - 1] != '!')
                                && (splittedstring[i][splittedstring[i].Length - 1] != '?')
                                )
                            {
                                splittedstring[i] += ".";

                            } //End the sentence with a period, if there's no other sentence-ending character.

                            if (!translatenodes.Keys.Contains(splittedstring[i])) //if translatenodes' keys DOESNT contain the current word of the sentence
                            {
                                node newnode = new node(splittedstring[i]); //make a new node
                                translatenodes.Add(splittedstring[i], newnode); //add new node to translatenodes
                            }

                            if (i < splittedstring.Length - 1) //if this is not the last word of the sentence
                            {
                                //if the current wordnode's childnodelist DOESNT contain the next word in the sentence
                                if (!translatenodes[splittedstring[i]].childnodes.Keys.ToList().Exists(x => x.text == splittedstring[i + 1]))
                                {
                                    //if the translatenodes dict DOES contain the node associated with the next word in the dictionary
                                    //(But it isn't part of the current node's childnode dictionary)
                                    if (translatenodes.ContainsKey(splittedstring[i + 1]))
                                    {
                                        translatenodes[splittedstring[i]].childnodes.Add(translatenodes[splittedstring[i + 1]], 5); //add the node from the translatenodes to the childnode of the current wordnode.
                                    }
                                }
                                //if the current wordnode's childnodelist DOES contain the next word in the sentence
                                else
                                    translatenodes[splittedstring[i]].childnodes[translatenodes[splittedstring[i + 1]]]+= 5; //Up the current wordnode's next wordnode's value by one.
                            }
                        }
                        else
                            wordsignored++;
                }
            }
            public string purgebadnodes()
            {
                System.Diagnostics.Stopwatch swatch = new System.Diagnostics.Stopwatch();
                swatch.Start();
                int purgednodes = 0;
                int orphanednodes = 0;
                //Make a list that is the reverse of translatenodes
                Dictionary<node, List<node>> nodereferencedby = new Dictionary<node, List<node>>();

                translatenodes.Values.ToList().ForEach //for each word node
                    (n => nodereferencedby.Add(n, new List<node>())); //add 
                foreach (KeyValuePair<string, node> transnode in translatenodes)
                {
                    if (transnode.Value.childnodes.Count > 0)
                    {
                        int avg = (int)transnode.Value.childnodes.Values.Average();
                        for (int i = 0; i < transnode.Value.childnodes.Count; i++ )
                        {
                            var childnode = transnode.Value.childnodes.ToList()[i];
                            if (childnode.Value < 1)
                            {
                                transnode.Value.childnodes.Remove(childnode.Key);
                                orphanednodes++;
                                break;
                            }
                            transnode.Value.childnodes[transnode.Value.childnodes.Keys.ToList()[i]]--;
                        }
                    }
                }

                //foreach (KeyValuePair<string, node> transnode in translatenodes)
                //{
                //    if (transnode.Value.childnodes.Count > 0)
                //    {
                //        int avg = (int)transnode.Value.childnodes.Values.Average();
                //        for (int i = 0; i < transnode.Value.childnodes.Count; i++)
                //        {
                //            transnode.Value.childnodes.Values.ToArray()[i]--;
                //        }
                //        foreach (var childnode in transnode.Value.childnodes)
                //        {

                //        }
                //    }
                //}
                translatenodes.Values.ToList().ForEach(tn => tn.childnodes.Keys.ToList().ForEach(cn => nodereferencedby[cn].Add(tn)));


                foreach (var node in nodereferencedby)
                {
                    if (node.Value.Count == 0 && node.Key.childnodes.Count == 0) //If a node Isn't referred to, nor refers to other nodes, delete it!
                    {
                        translatenodes.Remove(node.Key.text);
                        purgednodes++;
                    }
                }

                swatch.Stop();
                Console.WriteLine("Purged {0} nodes and orphaned {2} nodes in {1}ms.", purgednodes, swatch.ElapsedMilliseconds, orphanednodes);
                totalnodespurged += purgednodes;
                Console.WriteLine("Total nodes purged: {0}", totalnodespurged);
                timesparsedsincelastpurge = 0;
                return string.Format("Purged {0} nodes and orphaned {2} nodes in {1}ms.", purgednodes, swatch.ElapsedMilliseconds, orphanednodes);
            }

        }


        public class node
        {
            public node(string _text)
            {
                text = _text;
            }
            public string text = "";
            public Dictionary<node, int> childnodes = new Dictionary<node, int>();
        }
        #endregion
    }
}
