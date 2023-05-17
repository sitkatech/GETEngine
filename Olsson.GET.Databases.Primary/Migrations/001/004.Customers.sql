CREATE TABLE [dbo].[Customers](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [Name] [nvarchar](256) NOT NULL,
    CONSTRAINT [PK_dbo.Customers] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
)
GO

INSERT INTO Customers (Name) VALUES ('Admin')
GO

UPDATE Users SET CustomerId = 1
GO

Alter TABLE [dbo].[Users]
	add	CONSTRAINT FK_Users_Customers FOREIGN KEY (CustomerId) REFERENCES dbo.Customers (Id);
GO