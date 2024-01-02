/***** without projection **********************************************************************************************************************************************************************/
SELECT 
	-- core fields on Volunteer
	t.Id, t.Email, t.FirstName, t.JoinDate, t.LastName, t.RowVersion, 
	-- fields added from the .Include() expressions
	t0.Id, t0.NumHours, t0.OrganizationId, t0.RowVersion, t0.VolunteerId, t0.Id0, t0.Description, t0.Name, t0.OrganizationTypeId, t0.RowVersion0, t0.Id1, t0.Description0, t0.Name0, t0.RowVersion1
FROM (
	-- .SingleOrDefault() asks for top 2 so it can determine if 0, 1, or more than 1 row exist
    SELECT TOP(2) v.Id, v.Email, v.FirstName, v.JoinDate, v.LastName, v.RowVersion
    FROM Volunteer AS v
    WHERE v.Id = 'FDF749CF-BD6E-EA11-87F3-00090FAD0001'
) AS t
LEFT JOIN (
    SELECT 
		vh.Id, vh.NumHours, vh.OrganizationId, vh.RowVersion, vh.VolunteerId, 
		org.Id AS Id0, org.Description, org.Name, org.OrganizationTypeId, org.RowVersion AS RowVersion0,
		orgType.Id AS Id1, orgType.Description AS Description0, orgType.Name AS Name0, orgType.RowVersion AS RowVersion1
    FROM VolunteerHour AS vh
    INNER JOIN Organization AS org ON vh.OrganizationId = org.Id
    INNER JOIN OrganizationType AS orgType ON org.OrganizationTypeId = orgType.Id
) AS t0 ON t.Id = t0.VolunteerId
ORDER BY t.Id, t0.Id, t0.Id0, t0.Id1



/***** with projection **********************************************************************************************************************************************************************/
SELECT 
	-- core fields on Volunteer
	t.Email, t.FirstName, t.JoinDate, t.LastName, t.Id,
	-- fields added from the .Include() expressions
	t0.Id, t0.NumHours, t0.Name, t0.Name0, t0.Id0, t0.Id1
FROM (
	-- .SingleOrDefault() asks for top 2 so it can determine if 0, 1, or more than 1 row exist
    SELECT TOP(2) v.Email, v.FirstName, v.JoinDate, v.LastName, v.Id
    FROM Volunteer AS v
    WHERE v.Id = 'FDF749CF-BD6E-EA11-87F3-00090FAD0001'
) AS t
LEFT JOIN (
    SELECT
		vh.Id, vh.NumHours, vh.VolunteerId,
		org.Name, orgType.Name AS Name0, org.Id AS Id0,
		orgType.Id AS Id1
    FROM VolunteerHour AS vh
    INNER JOIN Organization AS org ON vh.OrganizationId = org.Id
    INNER JOIN OrganizationType AS orgType ON org.OrganizationTypeId = orgType.Id
) AS t0 ON t.Id = t0.VolunteerId
ORDER BY t.Id, t0.Id, t0.Id0, t0.Id1
