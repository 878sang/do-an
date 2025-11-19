-- Script to create tb_ChatMessage table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tb_ChatMessage]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tb_ChatMessage] (
        [MessageId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId] INT NULL,
        [GuestToken] NVARCHAR(100) NULL,
        [Sender] NVARCHAR(10) NOT NULL,
        [Message] NTEXT NOT NULL,
        [CreatedDate] DATETIME NULL,
        CONSTRAINT [FK_tb_ChatMessage_tb_Account] FOREIGN KEY ([UserId]) 
            REFERENCES [dbo].[tb_Account]([AccountId]) ON DELETE SET NULL
    );
    
    CREATE INDEX [IX_tb_ChatMessage_UserId] ON [dbo].[tb_ChatMessage]([UserId]);
    CREATE INDEX [IX_tb_ChatMessage_GuestToken] ON [dbo].[tb_ChatMessage]([GuestToken]);
    CREATE INDEX [IX_tb_ChatMessage_CreatedDate] ON [dbo].[tb_ChatMessage]([CreatedDate]);
    
    PRINT 'Table tb_ChatMessage created successfully.';
END
ELSE
BEGIN
    PRINT 'Table tb_ChatMessage already exists.';
END

