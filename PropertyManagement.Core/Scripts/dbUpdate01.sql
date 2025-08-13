if (not exists(select * from sys.systable where lcase(table_name) = lcase('Users'))) then
  CREATE TABLE Users
  (
	"Id" bigint primary key not null default autoincrement,
	"UserName" nvarchar(255) unique,
	"ApplicationName" nvarchar(255),
	"Email" nvarchar(255),
	"Comment" nvarchar(255),
	"Password" nvarchar(128),
	"PasswordQuestion" nvarchar(255),
	"PasswordAnswer" nvarchar(128),
	"IsApproved" bit,
	"LastLoginDate" datetime,
	"LastPasswordChangeDate" datetime,
	"CreationDate" datetime,
	"IsOnline" bit null,
	"IsLockedOut" bit null,
	"FailedPswrdAttemptCount" integer,
	"FailedPswrdAttempt_WinStr" datetime,
	"FailedPswrdAnswer_AtmptCount" integer,
	"FailedPswrdAnswer_AtmptWinStr" datetime,
	"IsDeleted" bit,
	"ProviderUserKey" nvarchar(255),
	"LastActivityDate" datetime,
	"LastLockedOutDate" datetime
  )
end if

GO

if(not exists(select * from sys.systable where lcase(table_name) = lcase('Roles'))) then
  CREATE TABLE Roles
  (
	"Id" bigint primary key not null default autoincrement,
	"RoleName" varchar(64),
	"Description" varchar(255)
  )
end if

GO

if(not exists(select * from sys.systable where lcase(table_name) = lcase('Rights'))) then
  CREATE TABLE Rights
  (
	"Id" bigint primary key not null default autoincrement,
	"R_Key" nvarchar(255),
	"Description" nvarchar(255)
  )
end if

GO

if (not exists(select * from sys.systable where lcase(table_name) = lcase('UsersInRoles'))) then
   CREATE TABLE UsersInRoles
   (
     "Id" bigint primary key not null default autoincrement,
 	 "UserId" bigint not null,
	 "RoleId" bigint not null
   )
end if

GO

if (not exists(select * from sys.systable where lcase(table_name) = lcase('RolesRights'))) then
   CREATE TABLE RolesRights
   (
     "Id" bigint primary key not null default autoincrement,
	 "RoleId" bigint not null,
	 "RightId" bigint not null
   )
end if

GO

if (not exists(select role from sys.sysforeignkey where role='FK_UIR_Rle')) then
	alter table UsersInRoles
	add constraint FK_UIR_Rle
	foreign key (RoleId)
	references Roles(Id)
end if

GO

if (not exists(select role from sys.sysforeignkey where role='FK_UIR_Usr')) then
	alter table UsersInRoles
	add constraint FK_UIR_Usr
	foreign key (Id)
	references Users(Id)
end if

GO

if(not exists(select role from sys.sysforeignkey where role='FK_RRights_Rights')) then
	alter table RolesRights
	add constraint FK_RRights_Rights
	foreign key(RightId)
	references Rights(Id)
end if

GO

INSERT INTO Roles(RoleName, Description)
VALUES ('GlobalAdmin', 'Global administrator')

GO

INSERT INTO Users(UserName, ApplicationName, Email, Password, IsApproved, IsDeleted, ProviderUserKey, IsLockedOut)
VALUES ('admin', 'PropertyManagement', 'ivan.gk@gmail.com', 'npOhR2eYjc3+mhHJ8eYu4xaEngM=', 
1, 0, '26cb73e0-abd9-4c1c-95db-d1d638c43538', 0)

GO

INSERT INTO UsersInRoles(UserId, RoleId) 
VALUES(1, 1)
