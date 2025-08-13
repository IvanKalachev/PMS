if (not exists(select * from sys.systable where lcase(table_name) = lcase('Units'))) then
  CREATE TABLE Units
  (
	"Id" bigint primary key not null default autoincrement,
	"Number" nvarchar(10),
	"FamilyName" nvarchar(100),
	"MembersCount" integer,
	"PercentIdealParts" decimal,
	"Floor" int,
	"IsDeleted" bit
  )
end if

GO

if (not exists(select * from sys.systable where lcase(table_name) = lcase('Detections'))) then
  CREATE TABLE Detections
  (
	"Id" bigint primary key not null default autoincrement,
	"Year" integer,
	"Month" integer,
  )
end if

GO

if (not exists(select * from sys.systable where lcase(table_name) = lcase('ChargeTypes'))) then
  CREATE TABLE ChargeTypes
  (
	"Id" bigint primary key not null default autoincrement,
	"Name" nvarchar(100),
  )
end if

GO

INSERT INTO ChargeTypes(Name)
VALUES("Според брой живущи в апартамент")

GO

INSERT INTO ChargeTypes(Name)
VALUES("Според % идеални части")

GO

INSERT INTO ChargeTypes(Name)
VALUES("Поравно на апартаментите")

GO

if (not exists(select * from sys.systable where lcase(table_name) = lcase('Services'))) then
  CREATE TABLE Services
  (
	"Id" bigint primary key not null default autoincrement,
	"Name" nvarchar(100),
	"DefaultChargeTypeId" bigint
  )
end if

GO

if (not exists(select role from sys.sysforeignkey where role='FK_Servc_DefType')) then
	alter table Services
	add constraint FK_Servc_DefType
	foreign key (DefaultChargeTypeId)
	references ChargeTypes(Id)
end if

GO

INSERT INTO Services(Name, DefaultChargeTypeId)
VALUES('Ел. енергия асансьор', 1)

GO

INSERT INTO Services(Name, DefaultChargeTypeId)
VALUES('Ел. енергия обща', 1)

GO

if (not exists(select * from sys.systable where lcase(table_name) = lcase('UnitNoChargeService'))) then
  CREATE TABLE UnitNoChargeService
  (
	"Id" bigint primary key not null default autoincrement,
	"UnitId" bigint,
	"ServiceId" bigint
  )
end if

GO

if (not exists(select role from sys.sysforeignkey where role='FK_UnitNoChargeService_Unit')) then
	alter table UnitNoChargeService
	add constraint FK_UnitNoChargeService_Unit
	foreign key (UnitId)
	references Units(Id)
end if

GO

if (not exists(select role from sys.sysforeignkey where role='FK_UnitNoChargeService_Service')) then
	alter table UnitNoChargeService
	add constraint FK_UnitNoChargeService_Service
	foreign key (ServiceId)
	references Services(Id)
end if

GO

if (not exists(select * from sys.systable where lcase(table_name) = lcase('Expenses'))) then
  CREATE TABLE Expenses
  (
	"Id" bigint primary key not null default autoincrement,
	"ServiceId" bigint,
	"DetectionId" bigint,
	"Sum" numeric(12,2),
	"IsDistributed" bit,
	"ExpenseType" int
  )
end if

GO

if (not exists(select role from sys.sysforeignkey where role='FK_Expense_Service')) then
	alter table Expenses
	add constraint FK_Expense_Service
	foreign key (ServiceId)
	references Services(Id)
end if

GO

if (not exists(select role from sys.sysforeignkey where role='FK_Expense_Detection')) then
	alter table Expenses
	add constraint FK_Expense_Detection
	foreign key (DetectionId)
	references Detections(Id)
end if

GO

if (not exists(select * from sys.systable where lcase(table_name) = lcase('UnitsCharges'))) then
  CREATE TABLE UnitsCharges
  (
	"Id" bigint primary key not null default autoincrement,
	"UnitId" bigint,
	"ExpenseId" bigint,
	"SumToPay" numeric(12,2),
	"PayedSum" numeric(12,2)
  )
end if

GO

if (not exists(select role from sys.sysforeignkey where role='FK_UnitCharges_Unit')) then
	alter table UnitsCharges
	add constraint FK_UnitCharges_Unit
	foreign key (UnitId)
	references Units(Id)
end if

GO

if (not exists(select role from sys.sysforeignkey where role='FK_UnitCharges_Expense')) then
	alter table UnitsCharges
	add constraint FK_UnitCharges_Expense
	foreign key (ExpenseId)
	references Expenses(Id)
end if

GO

ALTER TABLE Units
ADD Balance decimal(12,2) default 0

GO

if (not exists(select * from sys.systable where lcase(table_name) = lcase('IncomesPayment'))) then
  CREATE TABLE IncomesPayment
  (
	"Id" bigint primary key not null default autoincrement,
	"DetectionId" bigint,
	"UnitId" bigint,
	"PayedSum" numeric(12,2),
	"PayDate" datetime
  )
end if

GO

if (not exists(select role from sys.sysforeignkey where role='FK_IncomesPayment_Detection')) then
	alter table IncomesPayment
	add constraint FK_IncomesPayment_Detection
	foreign key (DetectionId)
	references Detections(Id)
end if

GO

if (not exists(select role from sys.sysforeignkey where role='FK_IncomesPayment_Unit')) then
	alter table IncomesPayment
	add constraint FK_IncomesPayment_Unit
	foreign key (UnitId)
	references Units(Id)
end if

------------ DELETE PAYMENTS

DELETE FROM IncomesPayments_PayedExpenses

GO

DELETE FROM IncomesPayment

GO

DELETE FROM UnitsCharges

GO

Update Expenses SET IsDistributed = 0


GO

Update Units SET Balance = 0

GO

DELETE FROM ResMoney

-------

if (not exists(select * from sys.systable where lcase(table_name) = lcase('IncomesPaymentsPayedCharges'))) then
  CREATE TABLE IncomesPayments_PayedExpenses
  (
	"Id" bigint primary key not null default autoincrement,
	"IncomePaymentId" bigint,
	"UnitChargeId" bigint,
  )
end if

GO

if (not exists(select role from sys.sysforeignkey where role='FK_IncomesPaymentsPayedCharges_IncomePayment')) then
	alter table IncomesPayments_PayedExpenses
	add constraint FK_IncomesPaymentsPayedCharges_IncomePayment
	foreign key (IncomePaymentId)
	references IncomesPayment(Id)
end if

GO

if (not exists(select role from sys.sysforeignkey where role='FK_IncomesPaymentsPayedCharges_UnitCharge')) then
	alter table IncomesPayments_PayedExpenses
	add constraint FK_IncomesPaymentsPayedCharges_UnitCharge
	foreign key (UnitChargeId)
	references UnitsCharges(Id)
end if

GO

UPDATE Expenses
SET ExpenseType = 1

GO

ALTER TABLE Expenses
ADD UnitId bigint

GO

if (not exists(select role from sys.sysforeignkey where role='FK_Expense_Unit')) then
	alter table Expenses
	add constraint FK_Expense_Unit
	foreign key (UnitId)
	references Units(Id)
end if

ALTER TABLE Units
ADD Deactivated bit default 0

ALTER TABLE UnitsCharges
MODIFY SumToPay numeric(12,3)

GO

ALTER TABLE UnitsCharges
MODIFY PayedSum numeric(12,3)

GO

ALTER TABLE Units
MODIFY Balance numeric(12,3)
 
ALTER TABLE UnitsCharges
MODIFY SumToPay numeric(12,2)

GO

ALTER TABLE UnitsCharges
MODIFY PayedSum numeric(12,2)

GO

ALTER TABLE Units
MODIFY Balance numeric(12,2)



if (not exists(select * from sys.systable where lcase(table_name) = lcase('ResMoney'))) then
  CREATE TABLE ResMoney
  (
	"Id" bigint primary key not null default autoincrement,
	"DetectionId" bigint,
	"UnitId" bigint,
	"PayedSum" numeric(12,2),
	"InsertDate" datetime DEFAULT CURRENT_TIMESTAMP
  )
end if

GO

if (not exists(select role from sys.sysforeignkey where role='FK_ResMoney_Detection')) then
	alter table ResMoney
	add constraint FK_ResMoney_Detection
	foreign key (DetectionId)
	references Detections(Id)
end if

GO

if (not exists(select role from sys.sysforeignkey where role='FK_ResMoney_Unit')) then
	alter table ResMoney
	add constraint FK_ResMoney_Unit
	foreign key (UnitId)
	references Units(Id)
end if

GO

ALTER TABLE Services
ADD IsCost bit default 0

GO

ALTER TABLE Services
ADD IsProfit bit default 0