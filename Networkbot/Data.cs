using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CedLib;
using CedLib.Persistence.XMLPersistenceDictionary;

namespace Onymity
{
    public static class Data
    {



        #region Steam related

        //Part of the anti spam mechanism
        public static Dictionary<string, DateTime> LastTalked = new Dictionary<string, DateTime>();



        #endregion

    }
}
