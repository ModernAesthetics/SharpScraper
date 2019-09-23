using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScrapySharp.Network;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using System.Text.RegularExpressions;
using System.Threading;
using MongoDB.Driver;
using MongoDB.Bson;

namespace SharpScraper
{
    /* Interface for my scraper
     * It will have the following 3 functions: */
    interface IScraper
    {
        List<string> getRecentPaste(); // Get the latest latest pastes from http://pastebin.com/archive
        string getPasteText(string url); // Get the text of the paste at the indicated url (/ string), http://pastebin.com/raw
        bool isKeywordPresent(string text, string regex); // Check if the paste contains one of the keywords we are looking for
    }

    // Class definition for pastebin
    class Scraper : IScraper
    {
        // url hard-coded by pastebin, to be changed in the case of web interface changes
        public string pasteBinUrl = "http://pastebin.com/archive";
        public string pasteBinArchiveUrl = "http://pastebin.com/archive";
        public string pasteBinRawUrl = "http://pastebin.com/raw";
        public int timeOut = 10000; // time to wait between successive requests in ms (so as not to be blocked and respect the TOS)
        private int idScraper;
        public static int numberOfScrapers = 0;
        ScrapingBrowser scraperBrowser;

        public Scraper()
        {
            // I create a browser to download the page of pastes
            this.scraperBrowser = new ScrapingBrowser();
            this.scraperBrowser.AllowAutoRedirect = true;
            this.scraperBrowser.AllowMetaRedirect = true;
            this.idScraper = numberOfScrapers;
            numberOfScrapers++;
        }

        // Get the latest latest pastes from http://pastebin.com/archive
        public List<string> getRecentPaste()
        {
            List<string> pastebins = new List<string>();
            // I make the request
            WebPage responsePage = scraperBrowser.NavigateToPage(new Uri("http://pastebin.com/archive"));
            // Use HtmlAgilityPack to select the HTML page element with maintable class
            // or the table containing the links to recent pastes
            var pastebinsTable = responsePage.Html.CssSelect(".maintable").First();
            // I select the table members
            var row = pastebinsTable.SelectNodes("tr/td");
            // Each paste is composed of three elements
            // The first element is the link in <a href="/page"> format
            // The second is how long ago the paste was created
            // The third indicates the programming language, if present, used in the paste
            // Cycle with an increment of 3
            for (int i = 0; i < row.Count; i += 3)
            {
                // I get the link from inside the quotes
                string s = row[i].LastChild.OuterHtml;
                int start = s.IndexOf('"') + 1;
                int end = s.IndexOf('"', start);
                string actualLink = s.Substring(start, end - start);
                // I get the time (could be useful in the future)
                string timeAgo = row[i + 1].LastChild.OuterHtml;
                // I get the language used
                string languageUsed = row[i + 2].LastChild.OuterHtml;
                // I add the link to the list
                pastebins.Add(actualLink);
            }
            return pastebins;
        }

        public string getPasteText(string url)
        {
            // Download the paste from pastebin.com/raw/url, in pure text
            string actualUrl = pasteBinRawUrl + url;
            try
            {
                WebPage responsePage = scraperBrowser.NavigateToPage(new Uri(actualUrl));
                return responsePage.Content;
            }
            catch
            {
                string response = "404 not found";
                return response;
            }


        }

        // Function to compare a string with a regular expression
        public bool isKeywordPresent(string text, string regex)
        {
            Regex r = new Regex(regex);
            bool isKeywordHere = r.IsMatch(text);
            return isKeywordHere;
        }

        // Routine to begin monitoring
        public void startScraping(string regex, Database mongoDB)
        {
            decimal i = 0;
            string paste;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Scraping according to the regex {0}...", regex);
            Console.ResetColor();
            // endless loop, until the user presses ctrl + c
            for (;;)
            {
                Console.WriteLine("");
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Request number {0}... (30s timeout to avoid a ban, ctrl + c to exit)", i + 1);
                Console.ResetColor();
                // Between each batch of requests, wait a little longer
                Thread.Sleep(30000);
                List<string> pasteList = getRecentPaste();
                Console.WriteLine("{0} pastes obtained!", pasteList.Count());          
                for (int j = 0; j < pasteList.Count(); j++)
                {
                    Thread.Sleep(timeOut); // in order not to generate too much traffic, wait between one request and another
                    paste = getPasteText(pasteList[j]);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("\rI'm looking for pastes {0} of {1}   ", j+1, pasteList.Count());
                    Console.ResetColor();
                    if (isKeywordPresent(paste, regex))
                    {
                        Console.WriteLine("");
                        string actualUrl = pasteBinRawUrl + pasteList[j];
                        Console.BackgroundColor = ConsoleColor.Green;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine("### A correspondence found! Url: {0} ###", actualUrl);
                        Console.ResetColor();
                        // Enter in the database the paste you have sent and corresponds to our search criteria
                        mongoDB.insertPaste(paste, actualUrl);
                        Console.WriteLine("");
                    }
                }
                // Increase to remember how many requests I made
                i++;
            }

        }

    }
}
