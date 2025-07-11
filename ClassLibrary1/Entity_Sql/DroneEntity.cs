using ClassLibrary1.Drone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Entity_Sql
{
    /// <summary>
    /// 无人机实体 - 对应数据库表 Drones
    /// </summary>
    public class DroneEntity
    {
        /// <summary>
        /// 无人机唯一标识符 (对应数据库 Id UNIQUEIDENTIFIER PRIMARY KEY)
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 无人机名称 (对应数据库 Name NVARCHAR(100))
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 模型状态 (对应数据库 ModelStatus TINYINT)
        /// 0:True(实体), 1:Vm(虚拟)
        /// </summary>
        public ModelStatus ModelStatus { get; set; }

        /// <summary>
        /// 模型类型名称 (对应数据库 ModelType NVARCHAR(50))
        /// </summary>
        public string ModelType { get; set; } = string.Empty;

        /// <summary>
        /// 注册日期 (对应数据库 RegistrationDate DATETIME2)
        /// </summary>
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
    }
}
