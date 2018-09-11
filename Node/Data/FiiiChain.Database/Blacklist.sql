CREATE TABLE [dbo].[Blacklist]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [Address] VARCHAR(50) NOT NULL, 
    [Timestamp] BIGINT NOT NULL, 
    [Expired] BIGINT NULL 
)
