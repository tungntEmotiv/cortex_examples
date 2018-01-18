using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CortexAccess
{
    public enum StreamID : int
    {
        CORTEX_VERSION_STREAM = 10,
        AUTHORIZE_STREAM,
        SESSION_STREAM,
        QUERY_HEADSETS_STREAM,
        MOTION_STREAM,
        DEVICE_STREAM,
        SUBSCRIBE_DATA,
        HEADSET_SETTING,
        BLE_CONNECTION,
        PERF_METRICS_STREAM,
        MENTAL_CMD_DATA_STREAM,
        FACIAL_EXP_DATA_STREAM,
        PROFILE_STREAM,
        SYS_STREAM,
        MENTAL_CMD_TRAIN_STREAM,
        FACIAL_EXP_TRAIN_STREAM
    }

    public class Utils
    {
        
    }
}
