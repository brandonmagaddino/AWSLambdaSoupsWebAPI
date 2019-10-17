using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AWSLambdaSoupsWebAPI.Crawler
{

    public class SoupCrawler
    {
        List<string> TodaysSoups = new List<string>();

        public SoupCrawler()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string StrHTMLText = client.DownloadString(@"https://copenhagenbakery.net/daily-specials/");

                    // Lets Clean up some of their text
                    StrHTMLText = StrHTMLText.Replace("&nbsp;", string.Empty);
                    StrHTMLText = StrHTMLText.Replace("\n", string.Empty);

                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(StrHTMLText);

                    var flattenedHtml = FlattenHTML(doc.DocumentNode);

                    // Getting todays soups
                    var AllPsChildren = flattenedHtml.Where(arg => arg.ParentNode.Name.ToLower() == "p" && arg.ParentNode.HasAttributes
                        || (arg?.ParentNode?.PreviousSibling != null && arg.ParentNode.PreviousSibling.Name.ToLower() == "p" && arg.ParentNode.PreviousSibling.HasAttributes)
                        || (arg?.ParentNode?.NextSibling != null && arg.ParentNode.NextSibling.Name.ToLower() == "p" && arg.ParentNode.NextSibling.HasAttributes)).ToList();
                    var CleanedSoupList = AllPsChildren.Select(arg => arg.InnerText.Trim()).ToList();
                    TodaysSoups.AddRange(CleanedSoupList);

                    // Getting todats special soup
                    var SoupSpecialNode = flattenedHtml.Where(arg => arg?.InnerText != null && arg?.ParentNode?.Name != null && arg.InnerText.Contains("&#8211;") && arg.ParentNode.Name.ToLower() == "span").FirstOrDefault();
                    if (SoupSpecialNode?.InnerText != null)
                    {
                        int cutLocation = SoupSpecialNode.InnerText.IndexOf(';') + 1;

                        if (cutLocation < SoupSpecialNode.InnerText.Length)
                            TodaysSoups.Add("Soup Special: " + SoupSpecialNode.InnerText.Substring(cutLocation).Trim());
                    }
                }
            } catch(Exception e)
            {
                TodaysSoups.Add("Error getting soups..Brandon must have broke something..");
            }
        }

        private List<HtmlNode> FlattenHTML(HtmlNode head)
        {
            if (head?.ChildNodes == null || head.ChildNodes.Count == 0)
                return new List<HtmlNode>() { head };

            var ret = new List<HtmlNode>();
            foreach (var child in head.ChildNodes)
            {
                ret.AddRange(FlattenHTML(child));
            }
            return ret;
        }

        private bool ContainsDayOfWeek(string str)
        {
            string[] daysOfWeek = { "monday", "tuesday", "wednesday", "thursday", "friday" };

            foreach (var day in daysOfWeek)
            {
                if (str.ToLower().Contains(day))
                    return true;
            }
            return false;
        }

        private bool ContainsDigit(string str)
        {
            return str.Any(cha => char.IsDigit(cha));
        }

        public List<string> GetTodaysSoups()
        {
            return TodaysSoups;
        }

    }
}
