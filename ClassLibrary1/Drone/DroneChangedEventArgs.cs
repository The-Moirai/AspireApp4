using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Drone
{
    // 事件参数类
    public class DroneChangedEventArgs : EventArgs
    {
        public string Action { get; set; } = "";
        public Drone Drone { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }

}
