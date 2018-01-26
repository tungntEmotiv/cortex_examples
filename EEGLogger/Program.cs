using System;
using CortexAccess;
using System.Threading;

namespace EEGLogger
{
    class Program
    {
        const string Username = "your_username";
        const string Password = "your_password";
        const string LicenseId = "your_license";
        public static AutoResetEvent m_LoginEvent = new AutoResetEvent(false);
        const int DebitNumber = 2; // default number of debit

        static void Main(string[] args)
        {
            Process p = new Process();
            Thread.Sleep(5000); //wait for querrying user login
            if (String.IsNullOrEmpty(p.GetUserLogin()))
            {
                p.Login(Username, Password);
                Thread.Sleep(5000); //wait for login
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
                p.CreateSession();
                Thread.Sleep(5000); //wait for creating session

                if(p.IsCreateSession)
                {
                    Console.WriteLine("Session have created successfully");
                    // Subcribe data
                    p.SubcribeData("eeg");
                    Thread.Sleep(5000);
                }

            }

            Console.WriteLine("Press Enter to exit");
            while (Console.ReadKey().Key != ConsoleKey.Enter) { }
        }

    }
}
