--Make a few updates to our Roles
update dbo.[Role]
set RoleDisplayName = 'Administrator'
where RoleID = 1

update dbo.[Role]
set RoleName = 'PowerUser', RoleDisplayName = 'Power User'
where RoleID = 2

insert into dbo.[Role](RoleID, RoleName, RoleDisplayName, RoleCategory)
values (3, 'Normal', 'Normal', 1)

--Remove the lower access of the multiple roles from anyone who has multiple roles
delete from dbo.UserRole
where UserRoleID in (
	select UserRoleID
	from dbo.UserRole ur
	join (
		select UserID, max(RoleID) roleID
		from dbo.UserRole
		group by UserID
		having count(*) > 1
		) moreThanOne on ur.UserID = moreThanOne.UserID and ur.RoleID = moreThanOne.roleID
	)

--Convert everyone with CustomerAdmin (previous name) access to Normal access
update dbo.UserRole
set RoleID = 3
where RoleID = 2

--Anyone who can currently manage reports will get bumped up to power user
update dbo.UserRole
set RoleID = 2
where UserID in (
	select u.UserID
	from dbo.UserRole ur
	join dbo.[User] u on ur.UserID = u.UserID
	where ur.RoleID = 2 and u.CanManageReports = 1
)

--Anyone who currently has no role, insert into the system as a 'Normal' role
insert into dbo.UserRole(UserID, RoleID)
select u.UserID, 3
from dbo.[User] u
left join dbo.UserRole ur on u.UserID = ur.UserID
where ur.UserID is null

--Remove CanManageReports from User table
alter table dbo.[User]
drop column CanManageReports