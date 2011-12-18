using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnyTwitter
{
    class Program
    {
        public static CedLib.Logging logger = new CedLib.Logging(true, true);
        static int minutesbetweenpost = 10;
        static string conffile = "settings.conf";
        static void Main(string[] args)
        {
            if (System.IO.File.Exists(conffile))
            {
                string[] conflines = System.IO.File.ReadAllLines(conffile);
                foreach (string confline in conflines)
                {
                    if (confline.Split('|')[0] == "minutesbetweenpost" && confline.Split('|')[0].Length > 1)
                        int.TryParse(confline.Split('|')[1], out minutesbetweenpost);
                }
            }
            else
                System.IO.File.WriteAllText(conffile, "minutesbetweenpost|" + minutesbetweenpost.ToString());
            
            //Below was for the first auth with twitter.
            /*Twitterizer.OAuthTokenResponse otokenresp = Twitterizer.OAuthUtility.GetRequestToken("s4NnGFjXHow8E4sAghj2cA", "J75EVo7fFnRIOyWvWAMv1cj2oIPEJq73CIsULO0k", "oob");
            Console.WriteLine("http://twitter.com/oauth/authorize?oauth_token=" + otokenresp.Token);
            Console.WriteLine("Hit enter when done.");
            Console.Read();
            Console.Write("Enter the pin: ");
            string pin = Console.ReadLine();
            Twitterizer.OAuthTokenResponse otokenrespverified = Twitterizer.OAuthUtility.GetAccessToken("s4NnGFjXHow8E4sAghj2cA", "J75EVo7fFnRIOyWvWAMv1cj2oIPEJq73CIsULO0k", otokenresp.Token, pin);
            Console.WriteLine("Got the following data:\nScreenname: {0}\nToken: {1}\nToken secret: {2}", otokenrespverified.ScreenName, otokenrespverified.Token, otokenrespverified.TokenSecret);
            Console.ReadLine(); */
            Twitterizer.OAuthTokens otokens = new Twitterizer.OAuthTokens();
            otokens.ConsumerKey = "s4NnGFjXHow8E4sAghj2cA";
            otokens.ConsumerSecret = "J75EVo7fFnRIOyWvWAMv1cj2oIPEJq73CIsULO0k";
            otokens.AccessToken = "407275798-F3Jp52bV8YYnQdXkvt9CyfnbgSG5eqm3fWuvKPnV";
            otokens.AccessTokenSecret = "WQcWvYms6QU4jwVOEwzJl0PDAoBxIZaYkL6q5c6Fxc";
            //Twitterizer.TwitterResponse<Twitterizer.TwitterUser> showusreresp = Twitterizer.TwitterUser.Show(otokens, "Onymity");
            //Console.WriteLine(showusreresp.Result);
            while (true)
            {
                System.Net.Sockets.Socket bacon = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                bacon.Connect("localhost", 5000);
                CedLib.Networking.socketfunctions.sendstring(bacon, "talk");
                CedLib.Networking.socketfunctions.waitfordata(bacon, 1000, false);
                string botsays = CedLib.Networking.socketfunctions.receivestring(bacon, false);
                logger.log("Posting: " + botsays, CedLib.Logging.Priority.Notice);
                Console.WriteLine(Twitterizer.TwitterStatus.Update(otokens, "Bot says: " + botsays).Result);
                System.Threading.Thread.Sleep(minutesbetweenpost * 60000);
                if (System.IO.File.Exists(conffile))
                {
                    string[] conflines = System.IO.File.ReadAllLines(conffile);
                    foreach (string confline in conflines)
                    {
                        if (confline.Split('|')[0] == "minutesbetweenpost" && confline.Split('|')[0].Length > 1)
                            int.TryParse(confline.Split('|')[1], out minutesbetweenpost);
                    }
                }
                else
                    System.IO.File.WriteAllText(conffile, "minutesbetweenpost|" + minutesbetweenpost.ToString());
            }
        }
    }
}
