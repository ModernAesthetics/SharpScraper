# SharpScraper
A pastebin scraper written in C#, using MongoDB as a db.

![](http://pastebin.com/i/pastebin_logo_side_outline.png)


## What you need
You need to have a MongoDB database up and running in order to get it to work. The code should be commented well enough.

## Why is it so slow?
The program waits a lot between requests in order not to get banned by pastebin's very strict policy against scraping.
Don't reduce the timeout between calls. If you do it, you'll probably have your ip address blocked in a matter of minutes.
[Here is a statement from Pastebin about scraping their website](http://pastebin.com/scraping)

## Who made it?
Daniele aka TrinTragula. It's completly free to use, share and modify.

## Who translated it?
Me, ModernAesthetics. Original was in Italian.
