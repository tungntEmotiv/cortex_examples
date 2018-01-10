using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CortexExamples
{
    class Session
    {
        //constructor
        public Session() { }
        
        public Session(JObject jSession)
        {
            //need check null
            SessionID = (string)jSession["id"];
            Status = (string)jSession["status"];
            LicenseID = (string)jSession["license"];
            
            string headset = (string)jSession["headset"];
            Owner = (string)jSession["owner"];
            //Headset = (Headset)jSession["headset"];
        }

        //Field
        private string _sessionID;
        private string _licenseID;
        private string _status;
        private string _owner;
        //private string _headsetId;
        //private string _experimentId;
        //isRecording
        //start
        //stop
        //stream {}
        //tags
        //subject


        private Headset _headset;

        public string SessionID
        {
            get
            {
                return _sessionID;
            }

            set
            {
                _sessionID = value;
            }
        }

        public string LicenseID
        {
            get
            {
                return _licenseID;
            }

            set
            {
                _licenseID = value;
            }
        }

        public string Status
        {
            get
            {
                return _status;
            }

            set
            {
                _status = value;
            }
        }

        public string Owner
        {
            get
            {
                return _owner;
            }

            set
            {
                _owner = value;
            }
        }

        public  Headset Headset
        {
            get
            {
                return _headset;
            }

            set
            {
                _headset = value;
            }
        }
    }
}
