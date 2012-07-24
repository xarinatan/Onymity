using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;

namespace Steam_test
{
    class Program
    {
        static SteamClient sclient = new SteamClient(System.Net.Sockets.ProtocolType.Tcp);
        static SteamFriends sFriends = sclient.GetHandler<SteamFriends>();
        static SteamUser sUser = sclient.GetHandler<SteamUser>();
        static string username = "megal33t";
        static string password = "0fqwoh";

        static void Main(string[] args)
        {

            sclient.Connect();
            CallbackMsg msg = sclient.GetCallback();
            Type a = msg.GetType();


            msg = sclient.GetCallback();
            msg.ToString();
            var asdf = (SteamUser.LoggedOnCallback)msg;
            if (asdf.Result == EResult.AccountLogonDenied)
            {
                Console.Write("Please enter the steam access code here: ");
                string steamaccesscode = Console.ReadLine();
                sUser.LogOn(new SteamUser.LogOnDetails() { Username = username, Password = password, AuthCode = steamaccesscode });
                msg = sclient.GetCallback();
                asdf = (SteamUser.LoggedOnCallback)msg;

            }
            if (asdf.Result == EResult.OK)
            {
                sFriends.SendChatMessage(new SteamID("STEAM_0:1:16516144"), EChatEntryType.ChatMsg, "Fennecs :D");
                Console.WriteLine(); Console.WriteLine("Logged in succesfully. Press any key to exit");
                sclient.Disconnect();
            }

        }

        static bool handlestuff = true;
        static void HandleCallbacks()
        {
            while (handlestuff)
            {
                CallbackMsg msg = sclient.GetCallback();
                if (msg == null)
                    return;
                if (msg.IsType<SteamClient.ConnectedCallback>())
                    sUser.LogOn(new SteamUser.LogOnDetails() { Username = username, Password = password });
            }
        }
    }
}
