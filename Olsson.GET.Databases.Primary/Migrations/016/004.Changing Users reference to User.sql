alter table dbo.FileResourceInfo drop constraint FK_FileResourceInfo_Users_UserId_UserId
GO

exec sp_rename 'dbo.FileResourceInfo.UserId', 'UserID', 'COLUMN'
GO

alter table dbo.FileResourceInfo add constraint FK_FileResourceInfo_User_UserID foreign key (UserID) references dbo.[User](UserID)