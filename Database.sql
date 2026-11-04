-- =======================================================
-- KỊCH BẢN TẠO CƠ SỞ DỮ LIỆU: GamingCenterDB
-- Đồ án: Development of a Gaming Center Management System
-- Thư viện kết nối: Microsoft.Data.SqlClient
-- =======================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'GamingCenterDB')
BEGIN
    CREATE DATABASE GamingCenterDB;
END
GO

USE GamingCenterDB;
GO

-- 1. Bảng Khu vực phòng máy (Zone)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'StationZone')
BEGIN
    CREATE TABLE StationZone (
        ZoneId INT IDENTITY(1,1) PRIMARY KEY,
        ZoneName NVARCHAR(50) NOT NULL,
        BasePricePerHour DECIMAL(18,2) NOT NULL CHECK (BasePricePerHour > 0),
        Description NVARCHAR(255) NULL
    );
END
GO

-- 2. Bảng Máy trạm (ComputerStation)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ComputerStation')
BEGIN
    CREATE TABLE ComputerStation (
        StationId INT IDENTITY(1,1) PRIMARY KEY,
        StationName NVARCHAR(50) NOT NULL UNIQUE,
        ZoneId INT NOT NULL,
        IpAddress VARCHAR(45) NOT NULL,
        Status NVARCHAR(30) NOT NULL DEFAULT 'Available', -- 'Available', 'InUse', 'Maintenance', 'TournamentLocked'
        CONSTRAINT FK_Station_Zone FOREIGN KEY (ZoneId) REFERENCES StationZone(ZoneId)
    );
END
GO

-- 3. Bảng Người dùng / Hội viên (AppUser)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AppUser')
BEGIN
    CREATE TABLE AppUser (
        UserId INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(50) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(255) NOT NULL,
        FullName NVARCHAR(100) NOT NULL,
        Role NVARCHAR(20) NOT NULL DEFAULT 'Member', -- 'Admin', 'Staff', 'Member'
        Balance DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (Balance >= 0),
        MemberTier NVARCHAR(20) NOT NULL DEFAULT 'Standard', -- 'Standard', 'Silver', 'Gold'
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- 4. Bảng Gói cước (TariffPlan)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TariffPlan')
BEGIN
    CREATE TABLE TariffPlan (
        PlanId INT IDENTITY(1,1) PRIMARY KEY,
        PlanName NVARCHAR(100) NOT NULL,
        TariffType NVARCHAR(50) NOT NULL, -- 'Hourly', 'NightCombo', 'Tournament', 'MemberTier'
        FixedPrice DECIMAL(18,2) NULL,
        DiscountRate FLOAT NOT NULL DEFAULT 0,
        StartHour INT NULL,
        EndHour INT NULL
    );
END
GO

-- 5. Bảng Phiên chơi (GamingSession)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GamingSession')
BEGIN
    CREATE TABLE GamingSession (
        SessionId INT IDENTITY(1,1) PRIMARY KEY,
        StationId INT NOT NULL,
        UserId INT NULL, -- NULL nếu là khách vãng lai
        StartTime DATETIME NOT NULL DEFAULT GETDATE(),
        EndTime DATETIME NULL,
        TariffType NVARCHAR(50) NOT NULL,
        PlayCost DECIMAL(18,2) NOT NULL DEFAULT 0,
        ServiceCost DECIMAL(18,2) NOT NULL DEFAULT 0,
        TotalCost DECIMAL(18,2) NOT NULL DEFAULT 0,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Active', -- 'Active', 'Completed'
        CONSTRAINT FK_Session_Station FOREIGN KEY (StationId) REFERENCES ComputerStation(StationId),
        CONSTRAINT FK_Session_User FOREIGN KEY (UserId) REFERENCES AppUser(UserId)
    );
END
GO

-- 6. Bảng Giải đấu LAN Party (Tournament)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tournament')
BEGIN
    CREATE TABLE Tournament (
        TournamentId INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(150) NOT NULL,
        GameTitle NVARCHAR(100) NOT NULL,
        FormatType NVARCHAR(50) NOT NULL DEFAULT 'SingleElimination',
        PrizePool DECIMAL(18,2) NOT NULL DEFAULT 0,
        Status NVARCHAR(30) NOT NULL DEFAULT 'Upcoming', -- 'Upcoming', 'Ongoing', 'Finished'
        StartDate DATETIME NOT NULL
    );
END
GO

-- 7. Bảng Trận đấu giải (TournamentMatch)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TournamentMatch')
BEGIN
    CREATE TABLE TournamentMatch (
        MatchId INT IDENTITY(1,1) PRIMARY KEY,
        TournamentId INT NOT NULL,
        RoundNumber INT NOT NULL,
        Team1Name NVARCHAR(100) NOT NULL,
        Team2Name NVARCHAR(100) NOT NULL,
        WinnerTeamName NVARCHAR(100) NULL,
        Score NVARCHAR(20) NULL,
        AssignedStationIds NVARCHAR(100) NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        CONSTRAINT FK_Match_Tournament FOREIGN KEY (TournamentId) REFERENCES Tournament(TournamentId)
    );
END
GO

-- =======================================================
-- CHÈN DỮ LIỆU MẪU BAN ĐẦU ĐỂ TEST
-- =======================================================
IF NOT EXISTS (SELECT * FROM StationZone)
BEGIN
    INSERT INTO StationZone (ZoneName, BasePricePerHour, Description) VALUES 
    (N'Khu Thường', 8000, N'Cấu hình i5, GTX 1660, Màn 144Hz'),
    (N'Khu VIP', 12000, N'Cấu hình i7, RTX 3060, Màn 240Hz'),
    (N'Đấu Trường LAN', 18000, N'Dàn thi đấu 10 máy khép kín, RTX 4070, Màn 360Hz');
END

IF NOT EXISTS (SELECT * FROM ComputerStation)
BEGIN
    INSERT INTO ComputerStation (StationName, ZoneId, IpAddress, Status) VALUES 
    ('PC-01', 1, '192.168.1.101', 'Available'),
    ('PC-02', 1, '192.168.1.102', 'Available'),
    ('PC-03', 1, '192.168.1.103', 'Available'),
    ('PC-04', 2, '192.168.1.104', 'Available'),
    ('PC-05', 2, '192.168.1.105', 'Available'),
    ('PC-06', 3, '192.168.1.106', 'Available'),
    ('PC-07', 3, '192.168.1.107', 'Available'),
    ('PC-08', 3, '192.168.1.108', 'Available'),
    ('PC-09', 3, '192.168.1.109', 'Available'),
    ('PC-10', 3, '192.168.1.110', 'Available');
END

IF NOT EXISTS (SELECT * FROM AppUser)
BEGIN
    INSERT INTO AppUser (Username, PasswordHash, FullName, Role, Balance, MemberTier) VALUES 
    ('admin', '123456', N'Quản Lý Cyber', 'Admin', 0, 'Standard'),
    ('gamer01', '123456', N'Nguyễn Văn A', 'Member', 50000, 'Standard'),
    ('gamer02', '123456', N'Trần Thị B', 'Member', 120000, 'Silver'),
    ('gamer03', '123456', N'Lê Hoàng C', 'Member', 300000, 'Gold');
END
GO
