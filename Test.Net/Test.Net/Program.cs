using System;
using System.Net;
using System.IO;

namespace Test.Net
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string url = "https://myfin.by/crypto-rates";
            File.Create("Text.txt");
            WebClient webClient = new WebClient();
            webClient.DownloadStringAsync(Uri url, "Text.txt");
        }
    }
}
