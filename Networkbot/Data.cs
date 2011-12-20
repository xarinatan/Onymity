using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CedLib.Persistence.XMLPersistenceDictionary;

namespace Onymity
{
    public static class Data
    {
        public static XMLPersistenceDictionary xmldict = new XMLPersistenceDictionary();

        #region Steam related

        //Part of the anti spam mechanism
        public static Dictionary<string, DateTime> LastTalked = new Dictionary<string, DateTime>();

        public static List<string> adminIDs = new List<string>()
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

        #endregion

    }
}
