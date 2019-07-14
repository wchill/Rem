using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace BabelJam
{
    public class BabelWord
    {
        public string Word { get; }
        public string Tag { get; }

        private static readonly Regex ParseRegex = new Regex(@"(.*)({{.*}})?", RegexOptions.Compiled | RegexOptions.ECMAScript);

        public BabelWord(string input)
        {
            var match = ParseRegex.Match(input);
            if (!match.Success)
            {
                throw new ArgumentException("Did not match regex");
            }

            Word = match.Groups[1].Value;
            Tag = match.Groups[2].Value;
        }
    }
}
