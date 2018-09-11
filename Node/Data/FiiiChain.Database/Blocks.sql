CREATE TABLE [dbo].[Blocks]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY,
	[Hash] VARCHAR(64) NOT NULL,
    [Version] INT NOT NULL DEFAULT 1, 
    [Height] BIGINT NOT NULL, 
    [PreviousBlockHash] VARCHAR(64) NOT NULL, 
	[Bits] BIGINT NOT NULL,
    [Nonce] BIGINT NOT NULL, 
    [Timestamp] BIGINT NOT NULL, 
    [NextBlockHash] VARCHAR(64) NULL, 
    [TotalAmount] BIGINT NOT NULL DEFAULT 0, 
    [TotalFee] BIGINT NOT NULL DEFAULT 0, 
    [GeneratorId] VARCHAR(64) NULL, 
	[BlockSignature] VARCHAR(MAX) NOT NULL,
	[PayloadHash] VARCHAR(64) NOT NULL,
    [IsDiscarded] BIT NOT NULL DEFAULT 0,
	[IsVerified] BIT NOT NULL DEFAULT 0 

)

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'The hash of current block',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Blocks',
    @level2type = N'COLUMN',
    @level2name = N'Id'