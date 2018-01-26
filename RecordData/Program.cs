using CortexAccess;
using System;
using System.Threading;

namespace RecordData
{
    class Program
    {
        const string Username = "your_username";
        const string Password = "your_password";
        const string LicenseId = "your_license";
        const int DebitNumber = 2; // default number of debit

        static void Main(string[] args)
        {

            Process p = new Process();
            Thread.Sleep(5000); //wait for querrying user login
            if (String.IsNullOrEmpty(p.GetUserLogin()))
            {
                p.Login(Username, Password);
                Thread.Sleep(1000); //wait for login
            }
            if (p.AccessCtr.IsLogin)
            {
                // Send Authorize
                p.Authorize(LicenseId, DebitNumber);
                Thread.Sleep(5000); //wait for authorize
            }
            if (!String.IsNullOrEmpty(p.GetSelectedHeadsetId()) && !String.IsNullOrEmpty(p.GetAccessToken()))
            {
                // Create Sesssion
                if(!p.IsCreateSession)
                {
                    p.CreateSession();
                    Thread.Sleep(5000); //wait for creating session
                }

                if (p.IsCreateSession)
                {
                    Console.WriteLine("Session have created successfully");
                    // Start record data
                    p.StartRecord("260118_3", "test4", "helloworld");

                    Thread.Sleep(5000);
                }

            }
            Console.WriteLine("Press Q to querry session and quit");
            Console.WriteLine("Press A,B,C to inject marker");
            Console.WriteLine("Press S to stop");
            Console.WriteLine("Press N to start a new Record");
            while (true) {

                int key = (int)Console.ReadKey().Key;

                if (key == (int)ConsoleKey.S)
                {
                    p.StopRecord();
                    Thread.Sleep(5000);
                }
                else if (key == (int)ConsoleKey.N) // next start record
                {
                    p.StartRecord("260118_4", "test5", "helloyou");
                    Thread.Sleep(5000);
                }
                else if(key == (int)ConsoleKey.A)
                {
                    // Inject marker
                    p.InjectMarker("A", 1,Utils.GetEpochTimeNow());

                }
                else if (key == (int)ConsoleKey.B)
                {
                    // Inject marker
                    p.InjectMarker("B", 2, Utils.GetEpochTimeNow());

                }
                else if (key == (int)ConsoleKey.C)
                {
                    // Inject marker
                    p.InjectMarker("C", 3, Utils.GetEpochTimeNow());

                }
                else if (key == (int)ConsoleKey.Q)
                {
                    // Querry Sessions before quit
                    p.QuerySession();
                    Thread.Sleep(5000);
                    break;

                }
            }
            Console.WriteLine("End of Program");

        }
    }
}
