﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Tasks
{
    public class SubTask
    {/// <summary>
     /// 子任务的唯一标识符
     /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();
        /// <summary>
        /// 子任务的描述信息
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 子任务的状态
        /// </summary>
        public TaskStatus Status { get; set; }
        /// <summary>
        /// 子任务的创建时间
        /// </summary>
        public DateTime CreationTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 子任务的开始时间
        /// </summary>
        public DateTime? AssignedTime { get; set; }
        /// <summary>
        /// 子任务的完成时间
        /// </summary>
        public DateTime? CompletedTime { get; set; }
        /// <summary>
        /// 子任务所属的大任务
        /// </summary>
        public Guid ParentTask { get; set; }
        /// <summary>
        /// 子任务重分配次数
        /// </summary>
        public int ReassignmentCount { get; set; } = 0;
        /// <summary>
        /// 子任务分配的无人机
        /// </summary>
        public string AssignedDrone { get; set; }

        /// <summary>
        /// 子任务处理结果图片数据列表
        /// </summary>
        public List<SubTaskImage> Images { get; set; } = new List<SubTaskImage>();

        public int GetTotalImageCount()
        { // 如果内存中有图片元数据，直接返回
            if (Images?.Any() == true)
            {
                return Images.Count;
            }
            return 0;
        }
    }
}
