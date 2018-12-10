using System.Collections.Generic;

namespace Rem.Services
{
    public interface IParameterExpansionService
    {
        object[] Expand(string input);
    }
}