using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Entity_Sql
{
    /// <summary>
    /// 无人机-子任务关联实体 - 对应数据库表 DroneSubTasks
    /// </summary>
    public class DroneSubTaskEntity
    {
        /// <summary>
        /// 无人机ID (对应数据库 DroneId UNIQUEIDENTIFIER)
        /// </summary>
        public Guid DroneId { get; set; }

        /// <summary>
        /// 子任务ID (对应数据库 SubTaskId UNIQUEIDENTIFIER)
        /// </summary>
        public Guid SubTaskId { get; set; }

        /// <summary>
        /// 分配时间 (对应数据库 AssignmentTime DATETIME2)
        /// </summary>
        public DateTime AssignmentTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 当前有效分配 (对应数据库 IsActive BIT)
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
