--
-- 由SQLiteStudio v3.1.1 产生的文件 周二 7月 31 16:56:44 2018
--
-- 文本编码：System
--
PRAGMA foreign_keys = off;
BEGIN TRANSACTION;

-- 表：Accounts
CREATE TABLE IF NOT EXISTS Accounts (
    Id          VARCHAR (64) NOT NULL
                             PRIMARY KEY,
    PrivateKey  VARCHAR (64),
    PublicKey   VARCHAR (64),
    Balance     INTEGER      NOT NULL
                             DEFAULT 0,
    IsDefault   BOOLEAN      NOT NULL
                             DEFAULT 0,
    WatchedOnly BOOLEAN      NOT NULL
                             DEFAULT 0,
    Timestamp   INTEGER      NOT NULL,
    Tag         VARCHAR (64) 
);


-- 表：AddressBook
CREATE TABLE IF NOT EXISTS AddressBook (
    Id        INTEGER      NOT NULL
                           PRIMARY KEY AUTOINCREMENT,
    Address   VARCHAR (64) NOT NULL,
    Tag       VARCHAR (64),
    Timestamp INTEGER      NOT NULL
);


-- 表：Blacklist
CREATE TABLE IF NOT EXISTS Blacklist (
    Id        INTEGER      NOT NULL
                           PRIMARY KEY AUTOINCREMENT,
    Address   VARCHAR (50) NOT NULL,
    Timestamp INTEGER      NOT NULL,
    Expired   INTEGER
);


-- 表：Blocks
CREATE TABLE IF NOT EXISTS Blocks (
    Id                INTEGER      NOT NULL
                                   PRIMARY KEY AUTOINCREMENT,
    Hash              VARCHAR (64) NOT NULL,
    Version           INT          NOT NULL
                                   DEFAULT 1,
    Height            INTEGER      NOT NULL,
    PreviousBlockHash VARCHAR (64) NOT NULL,
    Bits              INTEGER      NOT NULL,
    Nonce             INTEGER      NOT NULL,
    Timestamp         INTEGER      NOT NULL,
    NextBlockHash     VARCHAR (64),
    TotalAmount       INTEGER      NOT NULL
                                   DEFAULT 0,
    TotalFee          INTEGER      NOT NULL
                                   DEFAULT 0,
    GeneratorId 	  VARCHAR(64)  NULL, 
	BlockSignature    TEXT         NOT NULL,
	PayloadHash       VARCHAR(64)  NOT NULL,
    IsDiscarded       BOOLEAN      NOT NULL
                                   DEFAULT 0,
    IsVerified        BOOLEAN      NOT NULL
                                   DEFAULT 0
);


-- 表：InputList
CREATE TABLE IF NOT EXISTS InputList (
    Id                    INTEGER      NOT NULL
                                       PRIMARY KEY AUTOINCREMENT,
    TransactionHash       VARCHAR (64) NOT NULL,
    OutputTransactionHash VARCHAR (64) NOT NULL,
    OutputIndex           INT,
    Size                  INT          NOT NULL,
    Amount                INTEGER      NOT NULL
                                       DEFAULT 0,
    UnlockScript          TEXT,
    AccountId             VARCHAR (64),
    IsDiscarded           BOOLEAN      NOT NULL
                                       DEFAULT 0
);


-- 表：OutputList
CREATE TABLE IF NOT EXISTS OutputList (
    Id              INTEGER      NOT NULL
                                 PRIMARY KEY AUTOINCREMENT,
    [Index]         INT          NOT NULL,
    TransactionHash VARCHAR (64) NOT NULL,
    ReceiverId      VARCHAR (64) NOT NULL,
    Amount          INTEGER      NOT NULL
                                 DEFAULT 0,
    Size            INT          NOT NULL
                                 DEFAULT 0,
    LockScript      TEXT,
    Spent           BOOLEAN      NOT NULL
                                 DEFAULT 0,
    IsDiscarded     BOOLEAN      NOT NULL
                                 DEFAULT 0
);


-- 表：PaymentRequests
CREATE TABLE IF NOT EXISTS PaymentRequests (
    Id        INTEGER       NOT NULL
                            PRIMARY KEY AUTOINCREMENT,
    AccountId VARCHAR (64)  NOT NULL,
    Tag       VARCHAR (64),
    Comment   VARCHAR (200),
    Amount    INTEGER       NOT NULL
                            DEFAULT 0,
    Timestamp INTEGER       NOT NULL
                            DEFAULT 0
);


-- 表：Peers
CREATE TABLE IF NOT EXISTS Peers (
    Id        INTEGER      NOT NULL
                           PRIMARY KEY AUTOINCREMENT,
    IP        VARCHAR (50) NOT NULL,
    Port      INT          NOT NULL,
    PingTime  INTEGER,
    Timestamp INTEGER      NOT NULL
);


-- 表：Settings
CREATE TABLE IF NOT EXISTS Settings (
    Id             INTEGER      NOT NULL
                                PRIMARY KEY AUTOINCREMENT,
    Confirmations  INTEGER      NOT NULL
                                DEFAULT 1,
    FeePerKB       INTEGER      NOT NULL
                                DEFAULT 1000,
    Encrypt        BOOLEAN      NOT NULL
                                DEFAULT 0,
    PassCiphertext VARCHAR (64) 
);


-- 表：TransactionComments
CREATE TABLE IF NOT EXISTS TransactionComments (
    Id              INTEGER       NOT NULL
                                  PRIMARY KEY AUTOINCREMENT,
    TransactionHash VARCHAR (64)  NOT NULL,
    OutputIndex     INT           NOT NULL
                                  DEFAULT 0,
    Comment         VARCHAR (200) NOT NULL,
    Timestamp       INTEGER       NOT NULL
                                  DEFAULT 0
);


-- 表：Transactions
CREATE TABLE IF NOT EXISTS Transactions (
    Id          INTEGER      NOT NULL
                             PRIMARY KEY AUTOINCREMENT,
    Hash        VARCHAR (64) NOT NULL,
    BlockHash   VARCHAR (64),
    Version     INT          NOT NULL
                             DEFAULT 1,
    Timestamp   INTEGER      NOT NULL,
    LockTime    INTEGER      NOT NULL
                             DEFAULT 0,
    TotalInput  INTEGER      NOT NULL
                             DEFAULT 0,
    TotalOutput INTEGER      NOT NULL
                             DEFAULT 0,
    Size        INTEGER      NOT NULL
                             DEFAULT 0,
    Fee         INTEGER      NOT NULL
                             DEFAULT 0,
    IsDiscarded BOOLEAN      NOT NULL
                             DEFAULT 0,
	[ExpiredTime] INTEGER	 NOT NULL 
							 DEFAULT 0
);

-- 表：MiningPool
CREATE TABLE IF NOT EXISTS MiningPool(
	[Id]			INTEGER		NOT NULL	PRIMARY KEY AUTOINCREMENT,
	[NAME]		VARCHAR	(64) NOT NULL,
	PublicKey	varchar (500) NOT NULL,
	[Signature] varchar(500) Not Null
);



-- 索引：Address
DROP INDEX IF EXISTS Address;

CREATE UNIQUE INDEX Address ON AddressBook (
    Address
);


-- 索引：BlockHash
DROP INDEX IF EXISTS BlockHash;

CREATE INDEX BlockHash ON Transactions (
    BlockHash
);


-- 索引：Hash
DROP INDEX IF EXISTS Hash;

CREATE UNIQUE INDEX Hash ON Blocks (
    Hash
);


-- 索引：Height
DROP INDEX IF EXISTS Height;

CREATE INDEX Height ON Blocks (
    Height
);


-- 索引：InputListAcountId
DROP INDEX IF EXISTS InputListAcountId;

CREATE INDEX InputListAcountId ON InputList (
    AccountId
);


-- 索引：InputListIsDiscarded
DROP INDEX IF EXISTS InputListIsDiscarded;

CREATE INDEX InputListIsDiscarded ON InputList (
    IsDiscarded
);


-- 索引：InputListOutputTxIndex
DROP INDEX IF EXISTS InputListOutputTxIndex;

CREATE INDEX InputListOutputTxIndex ON InputList (
    OutputTransactionHash,
    OutputIndex
);


-- 索引：InputListUniqueIndex
DROP INDEX IF EXISTS InputListUniqueIndex;

CREATE INDEX InputListUniqueIndex ON InputList (
    TransactionHash
);


-- 索引：IsDiscarded
DROP INDEX IF EXISTS IsDiscarded;

CREATE INDEX IsDiscarded ON Blocks (
    IsDiscarded
);


-- 索引：IsVerified
DROP INDEX IF EXISTS IsVerified;

CREATE INDEX IsVerified ON Blocks (
    IsVerified
);


-- 索引：OutputListIsDiscarded
DROP INDEX IF EXISTS OutputListIsDiscarded;

CREATE INDEX OutputListIsDiscarded ON OutputList (
    IsDiscarded
);


-- 索引：OutputListReceiverId
DROP INDEX IF EXISTS OutputListReceiverId;

CREATE INDEX OutputListReceiverId ON OutputList (
    ReceiverId
);


-- 索引：OutputListSpent
DROP INDEX IF EXISTS OutputListSpent;

CREATE INDEX OutputListSpent ON OutputList (
    Spent
);


-- 索引：OutputListTxHash
DROP INDEX IF EXISTS OutputListTxHash;

CREATE INDEX OutputListTxHash ON OutputList (
    TransactionHash
);


-- 索引：OutputListUniqueIndex
DROP INDEX IF EXISTS OutputListUniqueIndex;

CREATE INDEX OutputListUniqueIndex ON OutputList (
    "Index",
    TransactionHash
);


-- 索引：PrivateKey
DROP INDEX IF EXISTS PrivateKey;

CREATE UNIQUE INDEX PrivateKey ON Accounts (
    PrivateKey
);


-- 索引：Tag
DROP INDEX IF EXISTS Tag;

CREATE INDEX Tag ON AddressBook (
    Tag
);


-- 索引：TxHash
DROP INDEX IF EXISTS TxHash;

CREATE INDEX TxHash ON Transactions (
    Hash
);


-- 索引：TxIsDiscarded
DROP INDEX IF EXISTS TxIsDiscarded;

CREATE INDEX TxIsDiscarded ON Transactions (
    IsDiscarded
);


COMMIT TRANSACTION;
PRAGMA foreign_keys = on;