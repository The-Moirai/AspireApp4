using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Entity_Sql
{
    /// <summary>
    /// 子任务图片实体 - 对应数据库表 SubTaskImages
    /// </summary>
    public class SubTaskImageEntity
    {
        /// <summary>
        /// 图片唯一标识符 (对应数据库 Id UNIQUEIDENTIFIER PRIMARY KEY)
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 所属子任务ID (对应数据库 SubTaskId UNIQUEIDENTIFIER)
        /// </summary>
        public Guid SubTaskId { get; set; }

        /// <summary>
        /// 图片二进制数据 (对应数据库 ImageData VARBINARY(MAX))
        /// </summary>
        public byte[] ImageData { get; set; } = new byte[0];

        /// <summary>
        /// 原始文件名 (对应数据库 FileName NVARCHAR(255))
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// 文件扩展名 (对应数据库 FileExtension NVARCHAR(10))
        /// </summary>
        public string FileExtension { get; set; } = string.Empty;

        /// <summary>
        /// 文件大小（字节） (对应数据库 FileSize BIGINT)
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// MIME内容类型 (对应数据库 ContentType NVARCHAR(100))
        /// </summary>
        public string ContentType { get; set; } = "image/png";

        /// <summary>
        /// 图片序号 (对应数据库 ImageIndex INT)
        /// </summary>
        public int ImageIndex { get; set; } = 1;

        /// <summary>
        /// 上传时间 (对应数据库 UploadTime DATETIME2)
        /// </summary>
        public DateTime UploadTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 图片描述 (对应数据库 Description NVARCHAR(500))
        /// </summary>
        public string? Description { get; set; }
    }
}
