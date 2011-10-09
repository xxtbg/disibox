using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace WebServerFlood
{
    class Program
    {
static void Main(string[] args) {
    const int NUMBER_OF_THREADS_TO_CREATE = 10;
    const int NUMBER_OF_REQUESTS_PER_THREAD = 5;
    //var webAddressToContact = new Uri("http://127.0.0.1:85/HeavyLoadPage.aspx");

    for (var i = 0; i < NUMBER_OF_THREADS_TO_CREATE; i++) {
        var thread = new Thread
            (() => {
                    for (int j = 0; j < NUMBER_OF_REQUESTS_PER_THREAD; j++) {
                        var client = new WebClient();
                        client.DownloadStringAsync(webAddressToContact);
                    }
                }
            );
        thread.Start();
    }

    Console.WriteLine("Just maked " + 
        NUMBER_OF_THREADS_TO_CREATE + 
        " threads, each one with " + 
        NUMBER_OF_REQUESTS_PER_THREAD + 
        " requests!\nPress any key to close.");

    Console.ReadLine();
}
    }
}
