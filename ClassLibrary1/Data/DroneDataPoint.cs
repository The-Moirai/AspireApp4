using ClassLibrary1.Drone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Data
{
    public class DroneDataPoint
    {
        /// <summary>
        /// 数据点的唯一表示符
        /// </summary>
        public Guid? PointId { get; set; }
        /// <summary>
        /// 无人机的唯一标识符
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 无人机的状态
        /// </summary>
        public DroneStatus Status { get; set; }
        /// <summary>
        /// 数据点的时间戳 
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public decimal Latitude { get; set; } 
        /// <summary>
        /// 经度
        /// </summary>
        public decimal Longitude { get; set; } 
        /// <summary>
        /// 无人机的 CPU 使用率
        /// </summary>
        public double cpu_used_rate { get; set; }
        /// <summary>
        /// 带宽
        /// </summary>
        public double left_bandwidth { get; set; }
        /// <summary>
        /// 内存
        /// </summary>
        public double memory { get; set; }
    }
}
