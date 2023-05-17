create table dbo.GETPageType (
	GETPageTypeID int not null constraint PK_GETPageType_GETPageTypeID  primary key,
	GETPageTypeName varchar(100) not null  constraint AK_GETPageType_GETPageTypeName unique,
	GetPageTypeDisplayName varchar(100) not null constraint AK_GETPageType_GETPageTypeDisplayName unique
)

create table dbo.GETPage (
	GETPageID int not null identity(1,1) constraint PK_GETPage_GETPageID primary key,
	GETPageTypeID int not null constraint FK_GETPage_GETPageType_GETPageTypeID foreign key references dbo.GETPageType (GETPageTypeID),
	GETPageContent dbo.html null
)

create table dbo.GETPageImage (
	GETPageImageID int not null  identity(1,1) constraint PK_GETPageImage_GETPageImageID  primary key,
	GETPageID int not null  constraint FK_GETPageImage_GETPage_GETPageID foreign  key references dbo.GETPage (GETPageID),
	FileResourceInfoID int  not null constraint FK_GETPageImage_FileResourceInfo_FileResourceInfoID foreign key references dbo.FileResourceInfo (FileResourceInfoID)
)

insert into dbo.GETPageType (GETPageTypeID, GETPageTypeName, GETPageTypeDisplayName)
values (1, 'LaunchPad', 'Launch Pad')

insert into dbo.GETPage (GETPageTypeID, GETPageContent)
values(1, '<p>This release focuses on enhancements to the GET user experience:</p>
            <ul>
                <li>New Home page with a link to common tasks and your most recently completed Actions</li>
                <li>Dedicated pages to view your list of Models and Model details</li>
                <li>Dedicated pages to view your list of Scenarios and Scenario details</li>
                <li>Improved linking between Actions, Models, and Scenarios to help improve platform discoverability</li>
                <li>Improved layout of Action Outputs - You can now compare multiple model output charts and maps next to each other in a single page</li>
            </ul>
            <p>More new features are on the way; this page will be updated as they are rolled out.</p>')