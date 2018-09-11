CREATE TABLE [dbo].[AddressBook]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [Address] VARCHAR(64) NOT NULL, 
    [Tag] VARCHAR(64) NULL, 
    [Timestamp] BIGINT NOT NULL
)
