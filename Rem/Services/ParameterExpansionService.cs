using System.Collections.Generic;
using Rem.Attributes;
using Rem.Utilities;

namespace Rem.Services
{
    [Service(typeof(IParameterExpansionService))]
    public class ParameterExpansionService : IParameterExpansionService
    {
        private readonly List<IStringTransformer> _transformers;

        public ParameterExpansionService()
        {
            _transformers = new List<IStringTransformer>()
            {
                new StringImageHttpDownloadTransformer()
            };
        }

        public object[] Expand(string input)
        {
            var output = new List<object>();
            foreach (var transformer in _transformers)
            {
                var success = transformer.TryTransform(input, out var transformed);
                if (success)
                {
                    output.Add(transformed);
                }
            }

            output.Add(input);
            return output.ToArray();
        }
        /*
         * 
                    inputs[i] = await context.Guild.ResolveIds(inputs[i]);
                    */
    }
}