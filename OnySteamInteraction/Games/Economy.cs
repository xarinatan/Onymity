using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Onymity.Games
{
    //below is a WIP!
    namespace Economy
    {
        class Wallet
        {
            public Wallet(string _username, long _money = 0)
            {
                privUsername = _username;
                privMoney = _money;
            }
            public string Username { get { return privUsername; } }
            public long Money { get { return privMoney; } set { privMoney = value; } }
            private string privUsername;
            private long privMoney; 
        }

        class BankAccount
        {
            
        }
    }
}
