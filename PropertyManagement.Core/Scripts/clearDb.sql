DELETE FROM IncomesPayments_PayedExpenses

GO

DELETE FROM IncomesPayment

GO

DELETE FROM UnitsCharges

GO

DELETE FROM ResMoney

GO

DELETE FROM Expenses

GO

DELETE FROM Detections

GO

UPDATE Units SET Balance = 0 