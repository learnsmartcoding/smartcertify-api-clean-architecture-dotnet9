use [master]
go

IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'SmartCertify')
BEGIN
    -- Create the database
    CREATE DATABASE SmartCertify;
END
ELSE
BEGIN
   DROP DATABASE SmartCertify;
END

Go
use SmartCertify
go

-- User Profile Table
CREATE TABLE UserProfile (
    UserId INT IDENTITY(1,1),
	DisplayName NVARCHAR(100) NOT NULL CONSTRAINT DF_UserProfile_DisplayName DEFAULT 'Guest',
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
	AdObjId NVARCHAR(128) NOT NULL,
    ProfileImageUrl NVARCHAR(500) NULL,
    CreatedOn DATETIME2 NOT NULL CONSTRAINT DF_UserProfile_CreatedOn DEFAULT GETUTCDATE(),
    -- Add other user-related fields as needed
    CONSTRAINT PK_UserProfile_UserId PRIMARY KEY (UserId)
);

--Roles
CREATE TABLE Roles (
    RoleId INT IDENTITY(1,1),    
    RoleName NVARCHAR(50) NOT NULL, --Admin, ReadOnly, Support, Customer etc    
    CONSTRAINT PK_Roles_RoleId PRIMARY KEY (RoleId)    
);

-- SmartApp Table. We can use this databse as centralized and add more tables for future apps. that is why this table is added
CREATE TABLE SmartApp (
    SmartAppId INT IDENTITY(1,1),    
    AppName NVARCHAR(50) NOT NULL,    
    CONSTRAINT PK_SmartApp_SmartAppId PRIMARY KEY (SmartAppId)
);

-- UserRole Table
CREATE TABLE UserRole (
    UserRoleId INT IDENTITY(1,1),
    RoleId INT NOT NULL,
    UserId INT NOT NULL,    
	SmartAppId INT NOT NULL,
    CONSTRAINT PK_UserRole_UserRoleId PRIMARY KEY (UserRoleId),
    CONSTRAINT FK_UserRole_UserProfile FOREIGN KEY (UserId) REFERENCES UserProfile(UserId),
    CONSTRAINT FK_UserRole_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId),
	CONSTRAINT FK_UserRole_SmartApp FOREIGN KEY (SmartAppId) REFERENCES SmartApp(SmartAppId)
);

CREATE TABLE Courses (
    CourseId INT IDENTITY(1,1),
    Title NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    CreatedBy INT NOT NULL,
    CreatedOn DATETIME2 NOT NULL CONSTRAINT DF_Courses_CreatedOn DEFAULT GETDATE(),
    CONSTRAINT PK_Courses_CourseId PRIMARY KEY (CourseId),
    CONSTRAINT FK_Courses_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES UserProfile(UserId)
);

CREATE TABLE Questions (
    QuestionId INT IDENTITY(1,1),
    CourseId INT NOT NULL,
    QuestionText NVARCHAR(MAX) NOT NULL, -- Question content
    DifficultyLevel NVARCHAR(20) NOT NULL, -- Easy, Medium, Advance
    IsCode BIT NOT NULL DEFAULT 0, -- Marks if the question includes a code sample
	HasMultipleAnswers  BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_Questions_QuestionId PRIMARY KEY (QuestionId),
    CONSTRAINT FK_Questions_CourseId FOREIGN KEY (CourseId) REFERENCES Courses(CourseId)
);

CREATE TABLE Choices (
    ChoiceId INT IDENTITY(1,1),
    QuestionId INT NOT NULL,
    ChoiceText NVARCHAR(MAX) NOT NULL, -- Text for the choice
    IsCode BIT NOT NULL DEFAULT 0, -- Marks if the choice is a code snippet
    IsCorrect BIT NOT NULL, -- Indicates the correct answer
    CONSTRAINT PK_Choices_ChoiceId PRIMARY KEY (ChoiceId),
    CONSTRAINT FK_Choices_QuestionId FOREIGN KEY (QuestionId) REFERENCES Questions(QuestionId)
);

CREATE TABLE Exams (
    ExamId INT IDENTITY(1,1),
    CourseId INT NOT NULL,
    UserId INT NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'In Progress', -- e.g., In Progress, Completed
    StartedOn DATETIME2 NOT NULL CONSTRAINT DF_Exams_StartedOn DEFAULT GETDATE(),
    FinishedOn DATETIME2 NULL,
	Feedback NVARCHAR(2000) NULL,
    CONSTRAINT PK_Exams_ExamId PRIMARY KEY (ExamId),
    CONSTRAINT FK_Exams_CourseId FOREIGN KEY (CourseId) REFERENCES Courses(CourseId),
    CONSTRAINT FK_Exams_UserId FOREIGN KEY (UserId) REFERENCES UserProfile(UserId)
);

CREATE TABLE ExamQuestions (
    ExamQuestionId INT IDENTITY(1,1),
    ExamId INT NOT NULL,
    QuestionId INT NOT NULL,
    SelectedChoiceId INT NULL, -- User's selected answer
    IsCorrect BIT NULL, -- Indicates whether the user's answer is correct
	ReviewLater  BIT NULL DEFAULT 0,
    CONSTRAINT PK_ExamQuestions_ExamQuestionId PRIMARY KEY (ExamQuestionId),
    CONSTRAINT FK_ExamQuestions_ExamId FOREIGN KEY (ExamId) REFERENCES Exams(ExamId),
    CONSTRAINT FK_ExamQuestions_QuestionId FOREIGN KEY (QuestionId) REFERENCES Questions(QuestionId),
    CONSTRAINT FK_ExamQuestions_SelectedChoiceId FOREIGN KEY (SelectedChoiceId) REFERENCES Choices(ChoiceId)
);

CREATE TABLE Notification (
    NotificationId INT IDENTITY(1,1),
    Subject NVARCHAR(200) NOT NULL, -- Subject of the notification/email
    Content NVARCHAR(MAX) NOT NULL, -- Email body or notification content
    CreatedOn DATETIME2 NOT NULL CONSTRAINT DF_Notification_CreatedOn DEFAULT GETDATE(),
    ScheduledSendTime DATETIME2 NOT NULL, -- When the notification is scheduled to be sent
    IsActive BIT NOT NULL DEFAULT 1, -- If active, it will trigger user notifications
    CONSTRAINT PK_Notification_NotificationId PRIMARY KEY (NotificationId)
);

CREATE TABLE UserNotifications (
    UserNotificationId INT IDENTITY(1,1),
    NotificationId INT NOT NULL,
    UserId INT NOT NULL,
    EmailSubject NVARCHAR(200) NOT NULL, -- Personalized email subject
    EmailContent NVARCHAR(MAX) NOT NULL, -- Personalized email body
    NotificationSent BIT NOT NULL DEFAULT 0, -- Flag to indicate if the email was sent
    SentOn DATETIME2 NULL, -- Time when the email was sent
    CreatedOn DATETIME2 NOT NULL CONSTRAINT DF_UserNotifications_CreatedOn DEFAULT GETDATE(),
    CONSTRAINT PK_UserNotifications_UserNotificationId PRIMARY KEY (UserNotificationId),
    CONSTRAINT FK_UserNotifications_NotificationId FOREIGN KEY (NotificationId) REFERENCES Notification(NotificationId),
    CONSTRAINT FK_UserNotifications_UserId FOREIGN KEY (UserId) REFERENCES UserProfile(UserId)
);

CREATE TABLE BannerInfo (
    BannerId INT IDENTITY(1,1),
    Title NVARCHAR(100) NOT NULL, -- Banner title or heading
    Content NVARCHAR(MAX) NOT NULL, -- Banner content or description
    ImageUrl NVARCHAR(500) NULL, -- Optional URL for banner image
    IsActive BIT NOT NULL DEFAULT 1, -- Only active banners are displayed in the app
    DisplayFrom DATETIME2 NOT NULL, -- Start date for displaying the banner
    DisplayTo DATETIME2 NOT NULL, -- End date for displaying the banner
    CreatedOn DATETIME2 NOT NULL CONSTRAINT DF_BannerInfo_CreatedOn DEFAULT GETDATE(),
    CONSTRAINT PK_BannerInfo_BannerId PRIMARY KEY (BannerId)
);


-- UserActivityLog Table
CREATE TABLE UserActivityLog (
    LogId INT IDENTITY(1,1),
    UserId INT,
    ActivityType NVARCHAR(50) NOT NULL,
    ActivityDescription NVARCHAR(MAX),
    LogDate DATETIME NOT NULL,
    -- Add other log-related fields as needed
    CONSTRAINT PK_UserActivityLog_LogId PRIMARY KEY (LogId),
    CONSTRAINT FK_UserActivityLog_UserProfile FOREIGN KEY (UserId) REFERENCES UserProfile(UserId)
);

--Contact Us table
CREATE TABLE ContactUs (
    ContactUsId INT IDENTITY(1,1),
    UserName NVARCHAR(100) NOT NULL,
	UserEmail NVARCHAR(100) NOT NULL,
	MessageDetail NVARCHAR(2000) NOT NULL,    
    CONSTRAINT PK_ContactUs_ContactUsId PRIMARY KEY (ContactUsId)    
);


-- Insert default roles
USE [SmartCertify]
GO

INSERT INTO [dbo].[Roles]
           ([RoleName])
     VALUES
           ('Admin')
		   ,('Support')
		   ,('Customer')
		   ,('ReadOnly')
GO

INSERT INTO [dbo].[SmartApp]
           ([AppName])
     VALUES
           ('SmartCertify')
GO




