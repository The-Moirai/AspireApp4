using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Entity_Sql
{
    /// <summary>
    /// 主任务实体 - 对应数据库表 MainTasks
    /// </summary>
    public class MainTaskEntity
    {
        /// <summary>
        /// 主任务唯一标识符 (对应数据库 Id UNIQUEIDENTIFIER PRIMARY KEY)
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 任务名称 (对应数据库 Name NVARCHAR(200))
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 任务描述 (对应数据库 Description NVARCHAR(500))
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 任务状态 (对应数据库 Status TINYINT)
        /// 0:Created, 1:WaitingForActivation, 2:WaitingToRun, 3:Running, 
        /// 4:WaitingForChildrenToComplete, 5:RanToCompletion, 6:Canceled, 7:Faulted
        /// </summary>
        public TaskStatus Status { get; set; }

        /// <summary>
        /// 创建时间 (对应数据库 CreationTime DATETIME2)
        /// </summary>
        public DateTime CreationTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 开始时间 (对应数据库 StartTime DATETIME2)
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 完成时间 (对应数据库 CompletedTime DATETIME2)
        /// </summary>
        public DateTime? CompletedTime { get; set; }

        /// <summary>
        /// 创建者 (对应数据库 CreatedBy NVARCHAR(128))
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;
    }
}
