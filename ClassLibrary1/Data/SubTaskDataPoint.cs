using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Data
{
    public class SubTaskDataPoint
    {
        public Guid PointId { get; set; }
        public Guid SubTaskId { get; set; }
        public DateTime Timestamp { get; set; }
        public TaskStatus OldStatus { get; set; }
        public TaskStatus NewStatus { get; set; }
        public Guid? DroneId { get; set; }
        public string Reason { get; set; }
        public string? Description { get; set; }
    }
}
