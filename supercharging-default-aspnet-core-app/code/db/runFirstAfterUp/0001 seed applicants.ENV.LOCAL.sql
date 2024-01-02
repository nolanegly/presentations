DECLARE @JoeCoolId uniqueidentifier = 'FDF749CF-BD6E-EA11-87F3-00090FAA0001';
DECLARE @WoodstockByrdId uniqueidentifier = 'FDF749CF-BD6E-EA11-87F3-00090FAA0002';

MERGE INTO Applicant as Target
USING (VALUES
	(@JoeCoolId, 'Joe', 'Cool', 'joe@example.com', null),
	(@WoodstockByrdId, 'Woodstock', 'Byrd', 'woodstock@example.com', null)
) AS Source (Id, FirstName, LastName, Email, Essay)
ON Target.Id = Source.Id
WHEN NOT MATCHED BY TARGET THEN
	INSERT (Id, FirstName, LastName, Email, Essay)
	VALUES (Id, FirstName, LastName, Email, Essay);