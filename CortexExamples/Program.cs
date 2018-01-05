using System;

namespace CortexExamples
{
    class Program
    {
        const string ClientId = "qutpR0xoZAJrqsaag41ZQlGmTVGocUSRX9tE7QAp";
        const string ClientSecret = "Z6Dx6t6BHrFsFFdK4BKyGnrj9X8sGYLW9hCjrrzVu6QZGlUeI6dhPq5Q0BuzZeL24tmFd0dyb9V6hM4gdOgODcqLnr6aglpXZD5mYU6ks6dnD9aMaJ8IvwxOUnjlFohO";
        const string Username = "tung789";
        const string Password = "***";
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World");
            //new websocket
            Access a = new Access();

            //query headset
            a.QueryHeadset();

            //stop access
            a.Stop();
            //open websocket client

            //Send data



            Console.ReadKey();
        }
    }
}
