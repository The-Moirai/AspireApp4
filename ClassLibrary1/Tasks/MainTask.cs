﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Tasks
{
    public class MainTask
    { /// <summary>
      /// 大任务的唯一标识符
      /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();
        /// <summary>
        /// 大任务的名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 大任务的描述信息
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 大任务的状态
        /// </summary>
        public System.Threading.Tasks.TaskStatus Status { get; set; }
        /// <summary>
        /// 大任务的创建时间
        /// </summary>
        public DateTime CreationTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 大任务的开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 大任务的完成时间
        /// </summary>
        public DateTime? CompletedTime { get; set; }
        /// <summary>
        /// 大任务的子任务列表
        /// </summary>
        public List<SubTask> SubTasks { get; set; } = new();
    }
}
