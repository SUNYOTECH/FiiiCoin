CREATE TABLE [dbo].[OutputList]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY,
	[Index] INT NOT NULL, 
    [TransactionHash] VARCHAR(64) NOT NULL, 
    [ReceiverId] VARCHAR(64) NOT NULL, 
    [Amount] BIGINT NOT NULL DEFAULT 0, 
	[Size] int NOT NULL DEFAULT 0,
    [LockScript] VARCHAR(MAX) NULL, 
    [Spent] BIT NOT NULL DEFAULT 0,
    [IsDiscarded] BIT NOT NULL DEFAULT 0
)

GO
