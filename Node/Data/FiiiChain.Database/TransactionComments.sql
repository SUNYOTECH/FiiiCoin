CREATE TABLE [dbo].[TransactionComments]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [TransactionHash] VARCHAR(64) NOT NULL, 
    [OutputIndex] INT NOT NULL DEFAULT 0, 
    [Comment] VARCHAR(200) NOT NULL, 
    [Timestamp] BIGINT NOT NULL DEFAULT 0
)
