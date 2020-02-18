using System;
using System.Diagnostics;
using System.IO;

namespace BreweryConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Trace.Listeners.Add(
                new TextWriterTraceListener(
                    new StreamWriter(@"c:\Logfiler\Brewery.txt")));
            BreweryWorker worker = new BreweryWorker();
            worker.Start();

            Console.ReadLine();
        }
    }
}
