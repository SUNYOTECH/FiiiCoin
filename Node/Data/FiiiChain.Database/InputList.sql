CREATE TABLE [dbo].[InputList]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY,
    [TransactionHash] VARCHAR(64) NOT NULL, 
    [OutputTransactionHash] VARCHAR(64) NOT NULL, 
    [OutputIndex] INT NULL,
    [Size] INT NOT NULL, 
    [Amount] BIGINT NOT NULL DEFAULT 0, 
    [UnlockScript] VARCHAR(MAX) NULL, 
    [AccountId] VARCHAR(64) NULL,
    [IsDiscarded] BIT NOT NULL DEFAULT 0
)

GO