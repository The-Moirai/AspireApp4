var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var sql = builder.AddSqlServer("sql")
                 .WithLifetime(ContainerLifetime.Persistent);


var databaseName = "app-db";
var creationScript = $$"""
    IF DB_ID('{{databaseName}}') IS NULL
        CREATE DATABASE [{{databaseName}}];
    GO

    -- Use the database
    USE [{{databaseName}}];
    GO

    -- ���˻���
    CREATE TABLE Drones (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Name NVARCHAR(100) NOT NULL,  -- ���˻�����
        ModelStatus TINYINT NOT NULL CHECK (ModelStatus IN (0, 1)),  -- 0:True, 1:Vm
        ModelType NVARCHAR(50) NOT NULL DEFAULT '',  -- ģ���������ƣ�������ʾ��
        RegistrationDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

        INDEX IX_Drones_Name (Name),
        INDEX IX_Drones_ModelStatus (ModelStatus)
    );
    GO

    -- ���ö��ע��
    EXEC sp_addextendedproperty 
        @name = N'MS_Description', @value = '0:True(ʵ��), 1:Vm(����)',
        @level0type = N'SCHEMA', @level0name = 'dbo',
        @level1type = N'TABLE', @level1name = 'Drones',
        @level2type = N'COLUMN', @level2name = 'ModelStatus';
    GO

    -- �������
    CREATE TABLE MainTasks (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Name NVARCHAR(200) NULL,  -- ��������
        Description NVARCHAR(500) NOT NULL,
        Status TINYINT NOT NULL CHECK (Status BETWEEN 0 AND 7),  -- System.Threading.Tasks.TaskStatusö��ֵ
        CreationTime DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        StartTime DATETIME2 NULL,  -- ��ʼʱ��
        CompletedTime DATETIME2 NULL,
        CreatedBy NVARCHAR(128) NOT NULL DEFAULT SUSER_SNAME(),

        INDEX IX_MainTasks_Status (Status),
        INDEX IX_MainTasks_CreationTime (CreationTime DESC),
        INDEX IX_MainTasks_Status_CreationTime (Status, CreationTime DESC)
    );
    GO

    -- ���״̬ö��ע��
    EXEC sp_addextendedproperty 
        @name = N'MS_Description', @value = '0:Created, 1:WaitingForActivation, 2:WaitingToRun, 3:Running, 4:WaitingForChildrenToComplete, 5:RanToCompletion, 6:Canceled, 7:Faulted',
        @level0type = N'SCHEMA', @level0name = 'dbo',
        @level1type = N'TABLE', @level1name = 'MainTasks',
        @level2type = N'COLUMN', @level2name = 'Status';
    GO

    -- �������
    CREATE TABLE SubTasks (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Description NVARCHAR(500) NOT NULL,
        Status TINYINT NOT NULL CHECK (Status BETWEEN 0 AND 7),  -- System.Threading.Tasks.TaskStatusö��ֵ
        CreationTime DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        AssignedTime DATETIME2 NULL,
        CompletedTime DATETIME2 NULL,
        ParentTask UNIQUEIDENTIFIER NOT NULL,  -- ��Ӧ���е�ParentTask�ֶ�
        ReassignmentCount INT NOT NULL DEFAULT 0,
        AssignedDrone NVARCHAR(100) NULL,  -- ��������˻�����

        -- ���Լ��
        CONSTRAINT FK_SubTasks_MainTasks FOREIGN KEY (ParentTask) 
            REFERENCES MainTasks(Id) ON DELETE CASCADE,

        -- ����
        INDEX IX_SubTasks_ParentTask (ParentTask),
        INDEX IX_SubTasks_Status_Completion (Status, CompletedTime),
        INDEX IX_SubTasks_CreationTime (CreationTime DESC),
        INDEX IX_SubTasks_ParentTask_CreationTime (ParentTask, CreationTime),
        INDEX IX_SubTasks_Status_ParentTask (Status, ParentTask)
    );
    GO

    -- ������ͼƬ��
    CREATE TABLE SubTaskImages (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        SubTaskId UNIQUEIDENTIFIER NOT NULL,
        ImageData VARBINARY(MAX) NOT NULL,  -- ͼƬ����������
        FileName NVARCHAR(255) NOT NULL,  -- ԭʼ�ļ���
        FileExtension NVARCHAR(10) NOT NULL,  -- �ļ���չ��
        FileSize BIGINT NOT NULL,  -- �ļ���С���ֽڣ�
        ContentType NVARCHAR(100) NOT NULL DEFAULT 'image/png',  -- MIME����
        ImageIndex INT NOT NULL DEFAULT 1,  -- ͼƬ���
        UploadTime DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),  -- �ϴ�ʱ��
        Description NVARCHAR(500) NULL,  -- ͼƬ����

        -- ���Լ��
        CONSTRAINT FK_SubTaskImages_SubTasks FOREIGN KEY (SubTaskId) 
            REFERENCES SubTasks(Id) ON DELETE CASCADE,

        -- ����
        INDEX IX_SubTaskImages_SubTaskId (SubTaskId),
        INDEX IX_SubTaskImages_UploadTime (UploadTime DESC),
        INDEX IX_SubTaskImages_SubTask_Index (SubTaskId, ImageIndex),
        INDEX IX_SubTaskImages_FileInfo (FileName, FileExtension)
    );
    GO

    -- ���˻�״̬��ʷ��
    CREATE TABLE DroneStatusHistory (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        DroneId UNIQUEIDENTIFIER NOT NULL,
        Status TINYINT NOT NULL CHECK (Status BETWEEN 0 AND 5),  -- DroneStatusö��
        Timestamp DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CpuUsage DECIMAL(5,2) NULL,  -- CPUʹ����%
        BandwidthAvailable DECIMAL(6,2) NULL,  -- ���ô��� Mbps
        MemoryUsage DECIMAL(6,2) NULL,  -- �ڴ�ʹ����%
        Latitude DECIMAL(10,7) NULL,
        Longitude DECIMAL(10,7) NULL,

        -- ���Լ��
        FOREIGN KEY (DroneId) REFERENCES Drones(Id) ON DELETE CASCADE,

        -- �����Ż�
        INDEX IX_DroneStatusHistory_DroneTime (DroneId, Timestamp DESC),
        INDEX IX_DroneStatusHistory_Location (Latitude, Longitude),
        INDEX IX_DroneStatusHistory_Timestamp (Timestamp DESC),
        INDEX IX_DroneStatusHistory_Status_Time (Status, Timestamp DESC),
        INDEX IX_DroneStatusHistory_TimeRange (Timestamp, DroneId)  -- ֧��ʱ�䷶Χ��ѯ
    );
    GO

    -- ���˻�-�����������
    CREATE TABLE DroneSubTasks (
        DroneId UNIQUEIDENTIFIER NOT NULL,
        SubTaskId UNIQUEIDENTIFIER NOT NULL,
        AssignmentTime DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        IsActive BIT NOT NULL DEFAULT 1,  -- ��ǰ��Ч����

        -- ������Լ��
        PRIMARY KEY (DroneId, SubTaskId),
        FOREIGN KEY (DroneId) REFERENCES Drones(Id) ON DELETE CASCADE,
        FOREIGN KEY (SubTaskId) REFERENCES SubTasks(Id) ON DELETE CASCADE,

        -- ����
        INDEX IX_DroneSubTasks_Time (AssignmentTime DESC),
        INDEX IX_DroneSubTasks_Active (IsActive),
        INDEX IX_DroneSubTasks_DroneActive (DroneId, IsActive),
        INDEX IX_DroneSubTasks_SubTaskActive (SubTaskId, IsActive)
    );
    GO

    -- ��������ʷ�����ı����¼��
    CREATE TABLE SubTaskHistory (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        SubTaskId UNIQUEIDENTIFIER NOT NULL,
        OldStatus TINYINT NULL,  -- ԭ״̬
        NewStatus TINYINT NOT NULL,  -- ��״̬
        ChangeTime DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        ChangedBy NVARCHAR(128) NULL DEFAULT SUSER_SNAME(),  -- ������
        DroneId UNIQUEIDENTIFIER NULL,  -- �������˻�
        Reason NVARCHAR(255) NULL,  -- ���ԭ��
        AdditionalInfo NVARCHAR(MAX) NULL,  -- ������Ϣ(JSON��ʽ)

        -- ���Լ��
        FOREIGN KEY (SubTaskId) REFERENCES SubTasks(Id) ON DELETE CASCADE,
        FOREIGN KEY (DroneId) REFERENCES Drones(Id),

        -- ����
        INDEX IX_SubTaskHistory_SubTask (SubTaskId),
        INDEX IX_SubTaskHistory_ChangeTime (ChangeTime DESC),
        INDEX IX_SubTaskHistory_StatusChange (NewStatus, ChangeTime),
        INDEX IX_SubTaskHistory_DroneTime (DroneId, ChangeTime DESC)
    );
    GO
    """;
var db = sql.AddDatabase(databaseName)
            .WithCreationScript(creationScript);
var apiService = builder.AddProject<Projects.WebApplication1>("webapplication1")
    .WithExternalHttpEndpoints()
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(db)
    .WaitFor(db);

builder.AddProject<Projects.BlazorApp1>("blazorapp1")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithReference(cache)
    .WaitFor(cache);



builder.Build().Run();
