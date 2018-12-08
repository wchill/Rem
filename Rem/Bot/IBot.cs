using System.Threading.Tasks;

namespace Rem.Bot
{
    interface IBot
    {
        Task Start(string token);
    }
}
