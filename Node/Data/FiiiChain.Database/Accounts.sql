CREATE TABLE [dbo].[Accounts]
(
	[Id] VARCHAR(64) NOT NULL PRIMARY KEY, 
    [PrivateKey] VARCHAR(64) NULL, 
    [PublicKey] VARCHAR(64) NULL, 
    [Balance] BIGINT NOT NULL DEFAULT 0.0, 
    [IsDefault] BIT NOT NULL DEFAULT 0, 
    [WatchedOnly] BIT NOT NULL DEFAULT 0, 
    [Timestamp] NCHAR(10) NOT NULL, 
    [Tag] VARCHAR(64) NULL
)

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Wallet',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Accounts',
    @level2type = N'COLUMN',
    @level2name = N'Id'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Wallet address',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Accounts',
    @level2type = NULL,
    @level2name = NULL