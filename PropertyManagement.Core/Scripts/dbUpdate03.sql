if (not exists(select * from sys.systable where lcase(table_name) = lcase('Currency'))) then
  CREATE TABLE Currency
  (
	"Id" bigint primary key not null default autoincrement,
	"Name" nvarchar(10),
	"Symbol" nvarchar(10),
	"IsDefault" bit not null default 0,
	"ExchangeRate" decimal (7,5) not null default 1
  )
end if

GO

if (not exists(select * from Currency where Name = 'BGN')) then
  INSERT INTO Currency (Name, Symbol, IsDefault, ExchangeRate)
  VALUES ('BGN', 'лв.', 1, 1.95583)
end if

GO

if (not exists(select * from Currency where Name = 'EUR')) then
  INSERT INTO Currency (Name, Symbol, IsDefault, ExchangeRate)
  VALUES ('EUR', '€', 0, 1)
end if

alter table Expenses 
add CurrencyId bigint not null default 1

GO

alter table IncomesPayment 
add CurrencyId bigint not null default 1

GO

alter table ResMoney 
add CurrencyId bigint not null default 1

GO

alter table UnitsCharges 
add CurrencyId bigint not null default 1

GO

if (not exists(select role from sys.sysforeignkey where role='FK_Expenses_Currency')) then
	alter table Expenses
	add constraint FK_Expenses_Currency
	foreign key (CurrencyId)
	references Currency(Id)
end if

GO

if (not exists(select role from sys.sysforeignkey where role='FK_IncomesPayment_Currency')) then
	alter table IncomesPayment
	add constraint FK_IncomesPayment_Currency
	foreign key (CurrencyId)
	references Currency(Id)
end if

GO

if (not exists(select role from sys.sysforeignkey where role='FK_ResMoney_Currency')) then
	alter table ResMoney
	add constraint FK_ResMoney_Currency
	foreign key (CurrencyId)
	references Currency(Id)
end if

GO

if (not exists(select role from sys.sysforeignkey where role='FK_UnitsCharges_Currency')) then
	alter table UnitsCharges
	add constraint FK_UnitsCharges_Currency
	foreign key (CurrencyId)
	references Currency(Id)
end if

GO

-- ////////////////////////////////////////////////////
-- ////////////////////////////////////////////////////
-- IMPORTANT !!!
-- EXECUTE THIS WHEN MOVE FROM BGN TO EUR

-- BGN TO EUR
UPDATE Currency 
SET IsDefault = 0
WHERE Id = 1

GO

UPDATE Currency 
SET IsDefault = 1
WHERE Id = 2


UPDATE Units
SET Balance = ROUND(Balance / 1.99583, 2)

-- ////////////////////////////////////////////////////
-- ////////////////////////////////////////////////////