CREATE TABLE OrganizationType (
  Id UNIQUEIDENTIFIER NOT NULL DEFAULT NewSequentialId(),
  CONSTRAINT [PK_OrganizationType] PRIMARY KEY CLUSTERED (Id ASC),
  RowVersion ROWVERSION,

  Name NVARCHAR(500) NOT NULL,
  Description NVARCHAR(500) NOT NULL
);

CREATE TABLE Organization (
  Id UNIQUEIDENTIFIER NOT NULL DEFAULT NewSequentialId(),
  CONSTRAINT [PK_Organization] PRIMARY KEY CLUSTERED (Id ASC),
  RowVersion ROWVERSION,

  OrganizationTypeId UNIQUEIDENTIFIER NOT NULL,
  CONSTRAINT [FK_Organization_OrganizationType]  FOREIGN KEY (OrganizationTypeId) REFERENCES OrganizationType(Id),
  Name NVARCHAR(500) NOT NULL,
  Description NVARCHAR(500) NOT NULL
);


CREATE TABLE [Volunteer] (
  Id UNIQUEIDENTIFIER NOT NULL DEFAULT NewSequentialId(),
  CONSTRAINT [PK_Volunteer] PRIMARY KEY CLUSTERED (Id ASC),
  RowVersion ROWVERSION,

  FirstName NVARCHAR(500) NOT NULL,
  LastName NVARCHAR(500) NOT NULL,
  Email NVARCHAR(500) NOT NULL,
  JoinDate date NOT NULL
);


CREATE TABLE VolunteerHour (
  Id UNIQUEIDENTIFIER NOT NULL DEFAULT NewSequentialId(),
  CONSTRAINT [PK_VolunteerHour] PRIMARY KEY CLUSTERED (Id ASC),
  RowVersion ROWVERSION,
  VolunteerId UNIQUEIDENTIFIER NOT NULL,
  CONSTRAINT [FK_VolunteerHour_Volunteer]  FOREIGN KEY (VolunteerId) REFERENCES Volunteer(Id),
  OrganizationId UNIQUEIDENTIFIER NOT NULL,
  CONSTRAINT [FK_VolunteerHour_Organization]  FOREIGN KEY (OrganizationId) REFERENCES Organization(Id),
  NumHours int NOT NULL
);
