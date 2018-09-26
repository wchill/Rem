using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rem.Bot
{
    interface IBot
    {
        Task Start(string token);
    }
}
