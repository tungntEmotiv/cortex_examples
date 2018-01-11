using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CortexAccess;

namespace EEGLogger
{
    class Program
    {
        const string Username = "your_uername";
        const string Password = "your_password";
        const string LicenseId = "your_license";
        static void Main(string[] args)
        {
            Access a = new Access("eeg", LicenseId);

            if (!a.IsLogined)
            {
                a.Login(Username, Password);
            }
            while (!a.IsLogined || string.IsNullOrEmpty(a.SelectedHeadsetId) || string.IsNullOrEmpty(a.CortexAccessToken))
            {
                //if(string.IsNullOrEmpty(a.SelectedHeadsetId))
                Console.WriteLine("Wait for login and headset connected");
            }
            //create session
            a.CreateSession();

            Console.ReadKey();
        }
    }
}
