using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace BabelJam
{
    public class LingoParser
    {
        public async Task<string> FetchLingoTranslationScript(string translationUrl)
        {
            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync(translationUrl);
            var node = htmlDoc.DocumentNode.SelectSingleNode("//html/body/script[6]");

            return node.InnerHtml;
        }

        public async Task<BabelLanguage> ParseLingoTranslation(string translationUrl)
        {
            var translationScript = await FetchLingoTranslationScript(translationUrl);
            var lines = translationScript.Split("\n").Select(line => line.Trim()).ToArray();

            var indexes = FindCdataIndexes(lines);

            var hasCustomJavascript = HasCustomJavascript(lines, indexes.Item1, indexes.Item2);
            var isReverseSupported = IsReverseSupported(lines, indexes.Item1, indexes.Item2);
            var jsonData = GetJsonData(lines, indexes.Item1, indexes.Item2);

            return new BabelLanguage(translationUrl, hasCustomJavascript, isReverseSupported, jsonData);
        }

        public bool HasCustomJavascript(string[] lines, int cdataStart, int cdataEnd)
        {
            var cdataFound = false;
            for (var i = cdataStart; i < cdataEnd + 1; i++)
            {
                var line = lines[i];
                if (!cdataFound && line.StartsWith("//<![CDATA["))
                {
                    cdataFound = true;
                }
                if (cdataFound && line.EndsWith("//]]>"))
                {
                    return false;
                }
                if (cdataFound && line.Contains("forward"))
                {
                    return true;
                }
            }

            throw new ArgumentException("Could not identify if input uses custom Javascript");
        }

        public bool IsReverseSupported(string[] lines, int cdataStart, int cdataEnd)
        {
            for (var i = 0; i < cdataStart + 1; i++)
            {
                var line = lines[i];
                if (line.StartsWith("var reverseIsDisabled = false;"))
                {
                    return true;
                }
                if (line.StartsWith("var reverseIsDisabled = true;"))
                {
                    return false;
                }
            }

            throw new ArgumentException("Could not find reverseIsDisabled in input");
        }

        public string GetJsonData(string[] lines, int cdataStart, int cdataEnd)
        {
            var regex = new Regex(@"^var jsonData = ({.*});", RegexOptions.Compiled);
            for (var i = cdataEnd + 1; i < lines.Length; i++)
            {
                var line = lines[i];
                var match = regex.Match(line);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }

            throw new ArgumentException("Could not find jsonData in input");
        }

        private static Tuple<int, int> FindCdataIndexes(string[] lines)
        {
            var cdataFound = false;
            var cdataStartIndex = 0;
            var cdataEndIndex = 0;
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (!cdataFound && line.StartsWith("//<![CDATA["))
                {
                    cdataFound = true;
                    cdataStartIndex = i;
                }
                else if (cdataFound && line.EndsWith("//]]>"))
                {
                    cdataEndIndex = i;
                }
            }

            return Tuple.Create(cdataStartIndex, cdataEndIndex);
        }
    }
}