using CortexAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MentalCommandTraining
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
            // get Detection Information
            p.QuerryDetectionInfo("mentalCommand");
            Thread.Sleep(2000); //wait for get detection information

            if (!String.IsNullOrEmpty(p.GetSelectedHeadsetId()) && !String.IsNullOrEmpty(p.GetAccessToken()))
            {
                // Create Sesssion
                p.CreateSession();
                Thread.Sleep(5000); //wait for creating session

                if (p.IsCreateSession)
                {
                    Console.WriteLine("Session have created successfully");
                    // Subcribe sys event
                    p.SubcribeData("sys");
                    Thread.Sleep(5000);
                }
            }
            // Create / load a profile
            p.LoadProfile("24_1_18_3"); // Or create a new Profile
            Thread.Sleep(2000);
            // Training neutral
            p.StartCmd("neutral");
            Thread.Sleep(20000);
            p.AcceptCmd();
            Thread.Sleep(2000);
            // Training push
            p.StartCmd("push");
            Thread.Sleep(10000);
            p.AcceptCmd();
            Thread.Sleep(2000);

            // Save profile
            p.SaveProfile();
            Thread.Sleep(3000);
            // Subcribe com event -> show training result
            p.SubcribeData("com");
            Thread.Sleep(5000);

        }
    }
}
