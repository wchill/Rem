using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace BabelJam
{
    internal class LingoJsonData
    {
        public string phrases1;
        public string phrases2;
        public string words1;
        public string words2;
        public string intraword1;
        public string intraword2;
        public string prefixes1;
        public string prefixes2;
        public string suffixes1;
        public string suffixes2;
        public string regex1;
        public string regex2;
        public string rev_regex1;
        public string rev_regex2;
        public string ordering1;
        public string ordering2;
    }

    public class BabelLanguage
    {
        private readonly List<Tuple<string, string>> _phrases;
        private readonly List<Tuple<BabelWord, BabelWord>> _words;
        private readonly List<Tuple<string, string>> _intrawords;
        private readonly List<Tuple<string, string>> _prefixes;
        private readonly List<Tuple<string, string>> _suffixes;
        private readonly List<Tuple<Regex, string>> _regexes;
        private readonly List<Tuple<Regex, string>> _reverseRegexes;
        private readonly List<Tuple<string, string>> _ordering;

        private static readonly string Delimiter = Guid.NewGuid().ToString().Substring(8);
        private static readonly string DoneToken = Guid.NewGuid().ToString().Substring(8);

        private static readonly char[] WordSeparators = {
            ' ', ',', '.', '\'', '!', ':', '?', '"', ';', '/', '<', '>', ')', '(', '%', '$'
        };
        private static readonly string WordSeparatorRegex = 
            string.Join('|', WordSeparators.Select(sep => Regex.Escape(sep.ToString())));

        public bool ReverseTranslationSupported { get; }
        public bool UsesCustomJavascript { get; }
        public string Url { get; }

        public BabelLanguage(string url, bool usesCustomJavascript, bool reverseTranslationSupported, string jsonData)
        {
            Url = url;
            UsesCustomJavascript = usesCustomJavascript;
            ReverseTranslationSupported = reverseTranslationSupported;

            var deserializedData = JsonConvert.DeserializeObject<LingoJsonData>(jsonData, new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error
            });

            Func<string, string> strConstructor = str => str;
            Func<string, BabelWord> wordConstructor = str => new BabelWord(str);

            _phrases = ParseString(deserializedData.phrases1, deserializedData.phrases2, strConstructor);
            _words = ParseString(deserializedData.words1, deserializedData.words2, wordConstructor);
            _intrawords = ParseString(deserializedData.intraword1, deserializedData.intraword2, strConstructor);
            _prefixes = ParseString(deserializedData.prefixes1, deserializedData.prefixes2, strConstructor);
            _suffixes = ParseString(deserializedData.suffixes1, deserializedData.suffixes2, strConstructor);
            _ordering = ParseString(deserializedData.ordering1, deserializedData.ordering2, strConstructor);

            _regexes = ParseRegexes(deserializedData.regex1, deserializedData.regex2);
            _reverseRegexes = ParseRegexes(deserializedData.rev_regex1, deserializedData.rev_regex2);
        }

        public string Translate(string input, bool translateBackwards = false)
        {
            var sentences = input.Split('.');

            var translatedSentences = new List<string>();

            foreach (var sentence in sentences)
            {
                var sb = new StringBuilder();
                if (sentence != "")
                {
                    var startsWithSpace = sentence[0] == ' ';
                    var firstLetterUppercase = sentence.Trim()[0] >= 'A' && sentence.Trim()[0] <= 'Z';

                    var text = sentence;
                    text = IntrawordSwap(text, translateBackwards);
                    text = " " + text.ToLowerInvariant() + " ";
                    text = text.Replace("\n", Delimiter);
                    text = PhraseSwap(text, translateBackwards);
                    text = WordSwap(text, translateBackwards);
                    text = PrefixSwap(text, translateBackwards);
                    text = SuffixSwap(text, translateBackwards);
                    text = RemoveDoneTokens(text);
                    text = RegexReplace(text, translateBackwards);
                    // text = ReorderWords(text, translateBackwards);

                    text = Regex.Replace(text, $" ?{Regex.Escape(Delimiter)} ?", "\n");
                    text = Regex.Replace(text, @"(\b\S+\b)[ ]+\b\1\b", "$1 $1", RegexOptions.IgnoreCase | RegexOptions.ECMAScript);

                    if (startsWithSpace)
                    {
                        sb.Append(' ');
                    }

                    if (firstLetterUppercase)
                    {
                        sb.Append((char) (text[0] + ('A' - 'a')));
                        sb.Append(text.Substring(1));
                    }
                    else
                    {
                        sb.Append(text);
                    }

                    // TODO: Handle duplicates, doApplySentenceCase
                }

                translatedSentences.Add(sb.ToString());
            }

            return string.Join('.', translatedSentences);
        }

        public string IntrawordSwap(string text, bool translateBackwards = false)
        {
            var sb = new StringBuilder();
            
            var start = 0;
            for (var end = 0; end < text.Length + 1; end++)
            {
                var substr = text.Substring(start, end - start);
                foreach (var intraword in _intrawords)
                {
                    var from = translateBackwards ? intraword.Item2 : intraword.Item1;
                    var to = translateBackwards ? intraword.Item1 : intraword.Item2;

                    if (substr.Contains(from))
                    {
                        sb.Append(substr.Replace(from, to));
                        start = end;
                        break;
                    }
                }
            }

            sb.Append(text.Substring(start));
            return sb.ToString();
        }

        public string PhraseSwap(string text, bool translateBackwards = false)
        {
            var to = _phrases.Select(item => Tokenize(translateBackwards ? item.Item1 : item.Item2).Replace("$", "$$")).ToArray();
            var from = _phrases.Select(item => (translateBackwards ? item.Item2 : item.Item1).ToLowerInvariant()).ToArray();

            for (var i = 0; i < from.Length; i++)
            {
                var pattern = $" {Regex.Escape(to[i])}({WordSeparatorRegex})";
                if (to[i] != "")
                {
                    text = Regex.Replace(text, pattern, $" {to[i]}$1");
                }
                else
                {
                    text = Regex.Replace(text, pattern, " ");
                }
            }

            return text;
        }

        public string WordSwap(string text, bool translateBackwards = false)
        {
            // TODO: LingoJam has some stuff for when the words are actually arrays, see if they're relevant (seems not to be the case)
            var to = _words.Select(item => Tokenize(translateBackwards ? item.Item1.Word : item.Item2.Word).Replace("$", "$$")).ToArray();
            var from = _words.Select(item => Regex.Escape((translateBackwards ? item.Item2.Word : item.Item1.Word).ToLowerInvariant())).ToArray();

            text = Regex.Replace(text, @"(\b\S+\b)\s+\b\1\b", "$1  $1", RegexOptions.IgnoreCase | RegexOptions.ECMAScript);

            for (var i = 0; i < from.Length; i++)
            {
                var pattern = $" {Regex.Escape(from[i])}({WordSeparatorRegex})";
                if (from[i][0] == '\'' && from[i].Last() == '\'' ||
                    from[i][0] == '"' && from[i].Last() == '"')
                {
                    text = Regex.Replace(text, pattern, $"{to[i]}$1");
                }
                else if (to[i].Length > 0)
                {
                    text = Regex.Replace(text, pattern, $" {to[i]}$1");
                }
                else
                {
                    // TODO: LingoJam splits the word including the tag. Check if this is the desired behavior
                    text = Regex.Replace(text, pattern, " ");
                }
            }

            return text;
        }

        public string PrefixSwap(string text, bool translateBackwards = false)
        {
            var to = _prefixes.Select(item => Tokenize(translateBackwards ? item.Item1 : item.Item2).Replace("$", "$$")).ToArray();
            var from = _prefixes.Select(item => Regex.Escape(translateBackwards ? item.Item2 : item.Item1)).ToArray();

            for (var i = 0; i < from.Length; i++)
            {
                text = Regex.Replace(text, $"\\s{from[i]}([^\\s])", $" {to[i]}$1", RegexOptions.ECMAScript);
            }

            return text;
        }

        public string SuffixSwap(string text, bool translateBackwards = false)
        {
            var to = _suffixes.Select(item => Tokenize(translateBackwards ? item.Item1 : item.Item2).Replace("$", "$$")).ToArray();
            var from = _suffixes.Select(item => Regex.Escape(translateBackwards ? item.Item2 : item.Item1)).ToArray();

            for (var i = 0; i < from.Length; i++)
            {
                text = Regex.Replace(text, $"\\s{from[i]}([^\\s])", $" {to[i]}$1", RegexOptions.ECMAScript);
            }

            return text;
        }

        public string RegexReplace(string text, bool translateBackwards = false)
        {
            var regexes = translateBackwards ? _regexes : _reverseRegexes;

            foreach (var item in regexes)
            {
                var to = item.Item2.Replace("$", "$$");
                var from = item.Item1;

                text = from.Replace(text, to);
            }

            return text;
        }

        public string ReorderWords(string text, bool translateBackwards = false)
        {
            throw new NotImplementedException();
        }

        private static string RemoveDoneTokens(string input)
        {
            return Regex.Replace(input, Regex.Escape(DoneToken), "").Trim();
        }

        private static string Tokenize(string input)
        {
            return DoneToken + string.Join(DoneToken, input.Split("")) + DoneToken;
        }

        private List<Tuple<T1, T1>> ParseString<T1>(string input1, string input2, Func<string, T1> constructor)
        {
            var list = new List<Tuple<T1, T1>>();
            if (input1 == "" && input2 == "")
            {
                return list;
            }

            var forward = input1.Split("\n");
            var backward = input2.Split("\n");
            if (forward.Length != backward.Length)
            {
                throw new ArgumentException("Forward and backward length mismatches");
            }

            for (var i = 0; i < forward.Length; i++)
            {
                list.Add(Tuple.Create(constructor(forward[i].Trim()), constructor(backward[i].Trim())));
            }

            return list;
        }

        private List<Tuple<Regex, string>> ParseRegexes(string input1, string input2)
        {
            var list = new List<Tuple<Regex, string>>();
            if (input1 == "" && input2 == "")
            {
                return list;
            }

            var regexStrings = input1.Split("\n");
            var substStrings = input2.Split("\n");
            if (regexStrings.Length != substStrings.Length)
            {
                throw new ArgumentException("Forward and backward length mismatches");
            }

            var regexPattern = new Regex(@"^/(.*?)/([gimy]*)$", RegexOptions.Compiled | RegexOptions.ECMAScript);

            for (var i = 0; i < regexStrings.Length; i++)
            {
                var match = regexPattern.Match(regexStrings[i].Trim());
                if (match.Success)
                {
                    var regexString = match.Groups[1].Value;
                    var flags = match.Groups[2].Value;

                    var regexOptions = RegexOptions.ECMAScript;
                    regexOptions |= (flags.Contains('m') ? RegexOptions.Multiline : RegexOptions.None);
                    regexOptions |= (flags.Contains('i') ? RegexOptions.IgnoreCase : RegexOptions.None);

                    var regex = new Regex(regexString, regexOptions);
                    list.Add(Tuple.Create(regex, substStrings[i]));
                }
            }

            return list;
        }
    }
}
