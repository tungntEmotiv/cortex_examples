using System;

namespace CortexExamples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World");
            //new websocket
            CortexClient coreClient = new CortexClient();

            coreClient.open();

            //open websocket client

            //Send data



            Console.ReadKey();
        }
    }
}
