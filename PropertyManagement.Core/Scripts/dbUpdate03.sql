if (not exists(select * from sys.systable where lcase(table_name) = lcase('Statements'))) then
  CREATE TABLE Statements
  (
	"Id" bigint primary key not null default autoincrement,
	"DetectionId" bigint not null,
	"Data" text not null,
	"InsertDate" datetime not null
  )
end if

GO

if (not exists(select role from sys.sysforeignkey where role='FK_Statements_Detection')) then
	alter table Statements
	add constraint FK_Statements_Detection
	foreign key (DetectionId)
	references Detections(Id)
end if