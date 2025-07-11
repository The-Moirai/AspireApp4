using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Entity_Sql
{
    /// <summary>
    /// 子任务实体 - 对应数据库表 SubTasks
    /// </summary>
    public class SubTaskEntity
    {
        /// <summary>
        /// 子任务唯一标识符 (对应数据库 Id UNIQUEIDENTIFIER PRIMARY KEY)
        /// </summary>
        public Guid Id { get; set; }

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
        /// 分配时间 (对应数据库 AssignedTime DATETIME2)
        /// </summary>
        public DateTime? AssignedTime { get; set; }

        /// <summary>
        /// 完成时间 (对应数据库 CompletedTime DATETIME2)
        /// </summary>
        public DateTime? CompletedTime { get; set; }

        /// <summary>
        /// 父任务ID (对应数据库 ParentTask UNIQUEIDENTIFIER)
        /// </summary>
        public Guid ParentTask { get; set; }

        /// <summary>
        /// 重分配次数 (对应数据库 ReassignmentCount INT)
        /// </summary>
        public int ReassignmentCount { get; set; } = 0;

        /// <summary>
        /// 分配的无人机名称 (对应数据库 AssignedDrone NVARCHAR(100))
        /// </summary>
        public string? AssignedDrone { get; set; }
    }
}
