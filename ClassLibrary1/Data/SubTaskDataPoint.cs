using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Data
{
    public class SubTaskDataPoint
    {
        /// <summary>
        /// 历史记录唯一标识符 (对应数据库 Id BIGINT IDENTITY(1,1))
        /// </summary>
        public Guid PointId { get; set; }
        /// <summary>
        /// 子任务ID (对应数据库 SubTaskId UNIQUEIDENTIFIER)
        /// </summary>
        public Guid SubTaskId { get; set; }
        /// <summary>
        /// 变更时间 (对应数据库 ChangeTime DATETIME2)
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// 原状态 (对应数据库 OldStatus TINYINT)
        /// </summary>
        public TaskStatus OldStatus { get; set; }
        /// <summary>
        /// 新状态 (对应数据库 NewStatus TINYINT)
        /// </summary>
        public TaskStatus NewStatus { get; set; }
        /// <summary>
        /// 操作者 (对应数据库 ChangedBy NVARCHAR(128))
        /// </summary>
        public string? ChangedBy { get; set; }

        /// <summary>
        /// 关联无人机ID (对应数据库 DroneId UNIQUEIDENTIFIER)
        /// </summary>
        public Guid? DroneId { get; set; }

        /// <summary>
        /// 变更原因 (对应数据库 Reason NVARCHAR(255))
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// 附加信息JSON格式 (对应数据库 AdditionalInfo NVARCHAR(MAX))
        /// </summary>
        public string? AdditionalInfo { get; set; }
    }
}
