using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CortexAccess
{
    public sealed class HeadsetController : BaseController
    {
        public enum HeadsetRqType : int
        {
            QUERRY_HEADSET = 10,
            HEADSET_SETTING = 11
        }

        // Member variable
        private static readonly HeadsetController _instance = new HeadsetController();
        private string _selectedHeadsetId; //selected headset
        private List<Headset> _headsetLists;

        public event EventHandler<List<Headset>> OnQuerryHeadsetOK;

        // Constructor
        static HeadsetController()
        {

        }
        private HeadsetController()
        {
            Console.WriteLine("HeadsetController constructor");
            HeadsetLists = new List<Headset>();
            _selectedHeadsetId = "";

        }

        // Properties
        public static HeadsetController Instance
        {
            get
            {
                return _instance;
            }
        }

        public string SelectedHeadsetId
        {
            get
            {
                return _selectedHeadsetId;
            }

            set
            {
                _selectedHeadsetId = value;
            }
        }

        public List<Headset> HeadsetLists
        {
            get
            {
                return _headsetLists;
            }
            set
            {
                _headsetLists = value;
            }
        }


        // Methods
        public override void ParseData(JObject data, int requestType)
        {

            if (data["result"] != null)
            {
                switch (requestType)
                {
                    case (int)HeadsetRqType.QUERRY_HEADSET:

                        //send event queryHeadsets OK
                        JArray jHeadsetArr = (JArray)data["result"];

                        foreach (JObject item in jHeadsetArr)
                        {
                            HeadsetLists.Add(new Headset(item));
                        }
                        if (HeadsetLists.Count > 0)
                        {
                            // Set element 0 as current headset
                            SelectedHeadsetId = HeadsetLists[0].HeadsetID;
                            Console.WriteLine("Selected HeadsetID " + SelectedHeadsetId);
                            OnQuerryHeadsetOK(this, new List<Headset>( HeadsetLists));
                        }
                        else
                        {
                            Console.WriteLine("No headset avaible");
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        // Send request to Websocket client
        public void QueryHeadsets()
        {
            JObject param = new JObject();
            CortexClient.Instance.SendTextMessage(param, (int)StreamID.QUERY_HEADSETS_STREAM, "queryHeadsets", true, (int)HeadsetRqType.QUERRY_HEADSET);
        }
    }
}
