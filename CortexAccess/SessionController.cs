using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CortexAccess
{
    public class SessionController : BaseController
    {
        public enum SessionReqType : int
        {
            CREATE_SESSION = 10,
            UPDATE_SESSION = 11,
            START_RECORD = 12,
            STOP_RECORD = 13,
            INJECT_MARKER = 14,
            UPDATE_NOTE = 15
        }

        // Member variables
        private Session _currentSession;
        private string _currentSessionID;
        //private Record _currentRecord;
        private int _nextStatus;
        private bool _isCreateSession;
        private bool _isRecording;

        // Constructor
        public SessionController() { }

        // Properties

        // Method
        // Request 
        // Create Session
        public void CreateSession(string headsetID, string token, string status, string experimentID = "")
        {
            JObject param = new JObject(
                    new JProperty("_auth", token),
                    new JProperty("headset", headsetID),
                    new JProperty("status", status));

            if (!string.IsNullOrEmpty(experimentID))
            {
                param.Add("experimentID", experimentID);
            }

            CortexClient.Instance.SendTextMessage(param, (int)StreamID.SESSION_STREAM, "createSession", true, (int)SessionReqType.CREATE_SESSION);
        }

        // Update Session
        // start Record
        // Stop Record
        public void UpdateSession(string token,string sessionID, string status, string recordingName = "", string recordingNote= "", string recordingSubject="")
        {
            JObject param = new JObject(
                    new JProperty("_auth", token),
                    new JProperty("session", sessionID),
                    new JProperty("status", status));

            if (status == "startRecord" || status == "stopRecord")
            {
                if (!string.IsNullOrEmpty(recordingName))
                    param.Add("recordingName", recordingName);
                if (!string.IsNullOrEmpty(recordingName))
                    param.Add("recordingSubject", recordingSubject);
                if (!string.IsNullOrEmpty(recordingName))
                    param.Add("recordingNote", recordingNote);
            }
            CortexClient.Instance.SendTextMessage(param, (int)StreamID.SESSION_STREAM, "updateSession", true, (int)SessionReqType.UPDATE_SESSION);
        }
        // Query Sessions

        // Update Note
        public void UpdateNote(string token, string sessionID, string recordID, string note)
        {
            JObject param = new JObject(
                    new JProperty("_auth", token),
                    new JProperty("session", sessionID),
                    new JProperty("record", recordID),
                    new JProperty("note", note));
            CortexClient.Instance.SendTextMessage(param, (int)StreamID.SESSION_STREAM, "updateNote", true, (int)SessionReqType.UPDATE_NOTE);
        }

        // Inject markers
        public bool InjectMarker(string token, string port, string label, string value, double epocTime)
        {
            JObject param = new JObject(
                    new JProperty("_auth", token),
                    new JProperty("port", port),
                    new JProperty("label", label),
                    new JProperty("value", value),
                    new JProperty("time", epocTime));
            CortexClient.Instance.SendTextMessage(param, (int)StreamID.SESSION_STREAM, "injectMarker", true, (int)SessionReqType.INJECT_MARKER);
            return true;
        } 


        // Handle Event Reponse
        public override void ParseData(JObject data, int requestType)
        {
            throw new NotImplementedException();
        }

        // Set Record Information

        // Get Current Record Information


    }
}
