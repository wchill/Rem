using System;
using BabelJam;
using Xunit;

namespace Tests
{
    public class BabelJamTests
    {
        [Theory]
        [InlineData("https://lingojam.com/EnglishtoShakespearean", false)]
        [InlineData("https://lingojam.com/ROBLOXTR4NSLATOR", false)]
        [InlineData("https://lingojam.com/MorseCodeTranslator", true)]
        public void ParseLanguage(string url, bool expected)
        {
            var parser = new LingoParser();
            var language = parser.ParseLingoTranslation(url).Result;
            Assert.NotNull(language);
            Assert.Equal(expected, language.UsesCustomJavascript);
        }

        [Fact]
        public void CompareResults()
        {
            var text =
                "What the fuck did you just fucking say about me, you little bitch? I'll have you know I graduated top of my class in the Navy Seals," +
                " and I've been involved in numerous secret raids on Al-Quaeda, and I have over 300 confirmed kills. I am trained in gorilla warfare " +
                "and I'm the top sniper in the entire US armed forces. You are nothing to me but just another target. I will wipe you the fuck out wi" +
                "th precision the likes of which has never been seen before on this Earth, mark my fucking words. You think you can get away with say" +
                "ing that shit to me over the Internet? Think again, fucker. As we speak I am contacting my secret network of spies across the USA an" +
                "d your IP is being traced right now so you better prepare for the storm, maggot. The storm that wipes out the pathetic little thing " +
                "you call your life. You're fucking dead, kid. I can be anywhere, anytime, and I can kill you in over seven hundred ways, and that's " +
                "just with my bare hands. Not only am I extensively trained in unarmed combat, but I have access to the entire arsenal of the United " +
                "States Marine Corps and I will use it to its full extent to wipe your miserable ass off the face of the continent, you little shit. " +
                "If only you could have known what unholy retribution your little \"clever\" comment was about to bring down upon you, maybe you woul" +
                "d have held your fucking tongue. But you couldn't, you didn't, and now you're paying the price, you goddamn idiot. I will shit fury " +
                "all over you and you will drown in it. You're fucking dead, kiddo.";
            var expected =
                "Wat da heck did u just hecking say about me, u wittwe meanie? me'ww have u know me gwaduated top of my cwass in da navy seaws, and m" +
                "e've been invowved in numewous secwet waids on aw-quaeda, and me have ovew 300 confiwmed kiwws. Me am twained in gowiwwa wawfawe and" +
                " am da top snipew in da entiwe us awmed fowces. U awe nothing to me but just anothew tawget. Me wiww wipe u da heck out with pwecisi" +
                "on da wikes of which has nevew been seen befowe on dis eawth, mawk my hecking wowds. U fink u can get away with saying that heckers " +
                "to me ovew da intewnet? fink again, fuckew. As we speak me am contacting my secwet netwowk of spies acwoss da usa and youw ip ish be" +
                "ing twaced wight now so u bettew pwepawe fow da stowm, maggot. Da stowm that wipes out da pathetic wittwe thing u caww youw wife. U'" +
                "we hecking dead, kid. Me can be anywhewe, anytime, and me can kiww u in ovew seven hundwed ways, and dats just with my bawe hands. N" +
                "ot onwy am me extensivewy twained in unawmed combat, but me have access to da entiwe awsenaw of da united states mawine cowps and me" +
                " wiww use it to its fuww extent to wipe youw misewabwe ass off da face of da continent, u wittwe heckers. If onwy u couwd have known" +
                " wat unhowy wetwibution youw wittwe \"cwevew\" comment was about to bwing down upon u, maybe u wouwd have hewd youw hecking tongue. " +
                "But u couwdn't, u didn't, and now u'we paying da pwice, u goddamn idiot. Me wiww heckers fuwy aww ovew u and u wiww dwown in it. U'w" +
                "e hecking dead, kiddo.";
            var parser = new LingoParser();
            var language = parser.ParseLingoTranslation("https://lingojam.com/uwu-ify").Result;

            Assert.NotNull(language);
            Assert.False(language.UsesCustomJavascript);
            Assert.Equal(expected, language.Translate(text));
        }
    }
}