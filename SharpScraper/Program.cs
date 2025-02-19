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

// to do: threading and press q to exit

namespace SharpScraper
{
    class Program
    {
        // For convenience, print a string of strings
        static void PrintList(List<string> list)
        {
            for (int i = 0; i < list.Count(); i++)
            {
                Console.WriteLine(list[i]);
            }
        }

        static void Main(string[] args)
        {
            string banner =
@" ############################################################################ 
 #             SharpScraper - A pastebin scraper written in C#              # 
 #                        Made by TrinTragula in 2017.                      # 
 #                   Translated by ModernAesthetics in 2019.                # 
 #                           Open source and free.                          # 
 ############################################################################ 

";

            var mongoDB = new Database();
            var sharpScraper = new Scraper();
            // Choose the regex to use here
            string regex = @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
            Console.SetWindowSize( Math.Min(78, Console.LargestWindowWidth), Math.Min(30, Console.LargestWindowHeight));
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(banner);
            Console.ResetColor();

            sharpScraper.startScraping(regex, mongoDB);
            Console.ReadKey();

        }
    }
}
