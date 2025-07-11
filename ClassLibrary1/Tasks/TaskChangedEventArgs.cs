using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Tasks
{

    // 事件参数类
    public class TaskChangedEventArgs : EventArgs
    {
        public string Action { get; set; } = "";
        public MainTask MainTask { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }
}
