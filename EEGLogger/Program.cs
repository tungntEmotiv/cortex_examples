using System;
using CortexAccess;
using System.Threading;

namespace EEGLogger
{
    class Program
    {
        const string Username = "***";
        const string Password = "***";
        const string LicenseId = "";
        public static AutoResetEvent m_LoginEvent = new AutoResetEvent(false);
        const int DebitNumber = 2; // default number of debit

        static void Main(string[] args)
        {
            //Access a = new Access("eeg", LicenseId);

            //if (!a.IsLogined)
            //{
            //    a.Login(Username, Password);
            //}
            //while (!a.IsLogined || string.IsNullOrEmpty(a.SelectedHeadsetId) || string.IsNullOrEmpty(a.CortexAccessToken))
            //{
            //    //if(string.IsNullOrEmpty(a.SelectedHeadsetId))
            //    Console.WriteLine("Wait for login and headset connected");
            //}
            ////create session
            //a.CreateSession();

            //while(string.IsNullOrEmpty(a.SessionId))
            //{
            //    Console.WriteLine("Wait for create session");
            //}
            Process p = new Process();
            Thread.Sleep(10000); //wait for querrying user login
            if (String.IsNullOrEmpty(p.GetUserLogin()))
            {
                p.Login(Username, Password);
                Thread.Sleep(1000); //wait for login
            }
            if (p.AccessCtr.IsLogin)
            {
                // Send Authorize
                p.Authorize(LicenseId, DebitNumber);
                Thread.Sleep(1000); //wait for authorize
            }
            if (!String.IsNullOrEmpty(p.GetSelectedHeadsetId()) && !String.IsNullOrEmpty(p.GetAccessToken()))
            {
                // Create Sesssion
                p.CreateSession();
                Thread.Sleep(1000); //wait for creating session

                if(p.SessionCtr.IsCreateSession)
                {
                    Console.WriteLine("Session have created successfully");
                    // Subcribe data
                    p.SubcribeData("eeg");



                }

            }


            Console.ReadKey();
        }

    }
}
