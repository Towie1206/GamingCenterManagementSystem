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
        CitizenId VARCHAR(12) NULL, -- Khai báo CCCD 12 số (hoặc CMND 9 số) theo Nghị định 72/2013/NĐ-CP & 27/2018/NĐ-CP
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        IsActive BIT NOT NULL DEFAULT 1
    );
END
GO

IF COL_LENGTH('dbo.AppUser', 'IsActive') IS NULL
BEGIN
    ALTER TABLE dbo.AppUser
    ADD IsActive BIT NOT NULL CONSTRAINT DF_AppUser_IsActive DEFAULT 1;
END
GO

IF COL_LENGTH('dbo.AppUser', 'CitizenId') IS NULL
BEGIN
    ALTER TABLE dbo.AppUser
    ADD CitizenId VARCHAR(12) NULL;
END
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_AppUser_CitizenId' AND object_id = OBJECT_ID('dbo.AppUser'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_AppUser_CitizenId
    ON dbo.AppUser (CitizenId)
    WHERE CitizenId IS NOT NULL;
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

-- 8. Bảng Danh mục dịch vụ F&B & Phụ kiện (ServiceItem)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ServiceItem')
BEGIN
    CREATE TABLE ServiceItem (
        ServiceId INT IDENTITY(1,1) PRIMARY KEY,
        ServiceName NVARCHAR(100) NOT NULL UNIQUE,
        Category NVARCHAR(50) NOT NULL DEFAULT N'Đồ uống',
        UnitPrice DECIMAL(18,2) NOT NULL CHECK (UnitPrice >= 0),
        StockQuantity INT NOT NULL DEFAULT 0 CHECK (StockQuantity >= 0),
        IsActive BIT NOT NULL DEFAULT 1
    );
END
GO

-- 9. Bảng Chi tiết dịch vụ theo phiên chơi (SessionService)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SessionService')
BEGIN
    CREATE TABLE SessionService (
        SessionServiceId INT IDENTITY(1,1) PRIMARY KEY,
        SessionId INT NOT NULL,
        ServiceId INT NOT NULL,
        Quantity INT NOT NULL CHECK (Quantity > 0),
        UnitPrice DECIMAL(18,2) NOT NULL CHECK (UnitPrice >= 0),
        TotalPrice DECIMAL(18,2) NOT NULL CHECK (TotalPrice >= 0),
        OrderedAt DATETIME NOT NULL DEFAULT GETDATE(),
        Note NVARCHAR(255) NULL,
        CONSTRAINT FK_SessionService_Session FOREIGN KEY (SessionId) REFERENCES GamingSession(SessionId),
        CONSTRAINT FK_SessionService_Item FOREIGN KEY (ServiceId) REFERENCES ServiceItem(ServiceId)
    );
END
GO

-- 10. Bảng Lịch sử giao dịch ví hội viên (WalletTransaction)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WalletTransaction')
BEGIN
    CREATE TABLE WalletTransaction (
        TransactionId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        SessionId INT NULL,
        TransactionType NVARCHAR(30) NOT NULL, -- 'TopUp', 'SessionPayment', 'Refund', 'Adjustment'
        Amount DECIMAL(18,2) NOT NULL CHECK (Amount > 0),
        BalanceBefore DECIMAL(18,2) NOT NULL CHECK (BalanceBefore >= 0),
        BalanceAfter DECIMAL(18,2) NOT NULL CHECK (BalanceAfter >= 0),
        PaymentMethod NVARCHAR(50) NULL,
        Description NVARCHAR(255) NULL,
        CreatedByUserId INT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_WalletTx_User FOREIGN KEY (UserId) REFERENCES AppUser(UserId) ON DELETE CASCADE,
        CONSTRAINT FK_WalletTx_Session FOREIGN KEY (SessionId) REFERENCES GamingSession(SessionId),
        CONSTRAINT FK_WalletTx_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES AppUser(UserId)
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
    ('PC-02', 1, '192.168.1.102', 'InUse'),
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
    INSERT INTO AppUser (Username, PasswordHash, FullName, Role, Balance, MemberTier, CitizenId, IsActive) VALUES 
    ('admin', '123456', N'Quản Lý Cyber', 'Admin', 0, 'Standard', '001099000001', 1),
    ('gamer01', '123456', N'Nguyễn Văn A', 'Member', 50000, 'Standard', '001099000002', 1),
    ('gamer02', '123456', N'Trần Thị B', 'Member', 120000, 'Silver', '001099000003', 1),
    ('gamer03', '123456', N'Lê Hoàng C', 'Member', 300000, 'Gold', '001099000004', 1);
END

IF NOT EXISTS (SELECT 1 FROM AppUser WHERE Username = 'staff01')
BEGIN
    INSERT INTO AppUser (Username, PasswordHash, FullName, Role, Balance, MemberTier, CitizenId, IsActive)
    VALUES ('staff01', '123456', N'Nhân Viên Quầy', 'Staff', 0, 'Standard', '001099000005', 1);
END

-- Đảm bảo dữ liệu mẫu đã có CCCD nếu cơ sở dữ liệu đã tồn tại từ trước
IF COL_LENGTH('dbo.AppUser', 'CitizenId') IS NOT NULL
BEGIN
    UPDATE AppUser SET CitizenId = '001099000001' WHERE Username = 'admin' AND CitizenId IS NULL;
    UPDATE AppUser SET CitizenId = '001099000002' WHERE Username = 'gamer01' AND CitizenId IS NULL;
    UPDATE AppUser SET CitizenId = '001099000003' WHERE Username = 'gamer02' AND CitizenId IS NULL;
    UPDATE AppUser SET CitizenId = '001099000004' WHERE Username = 'gamer03' AND CitizenId IS NULL;
    UPDATE AppUser SET CitizenId = '001099000005' WHERE Username = 'staff01' AND CitizenId IS NULL;
END

IF NOT EXISTS (SELECT * FROM TariffPlan)
BEGIN
    INSERT INTO TariffPlan (PlanName, TariffType, FixedPrice, DiscountRate, StartHour, EndHour) VALUES 
    (N'Cước Theo Giờ Thường', 'Hourly', NULL, 0, NULL, NULL),
    (N'Gói Combo Đêm Trọn Gói', 'NightCombo', 45000, 0, 22, 6),
    (N'Gói Cước Ưu Đãi Giải Đấu LAN', 'Tournament', NULL, 0.5, NULL, NULL),
    (N'Gói Ưu Đãi Hội Viên Vàng', 'MemberTier', NULL, 0.15, NULL, NULL);
END
ELSE IF NOT EXISTS (SELECT 1 FROM TariffPlan WHERE TariffType = 'MemberTier')
BEGIN
    INSERT INTO TariffPlan (PlanName, TariffType, FixedPrice, DiscountRate, StartHour, EndHour)
    VALUES (N'Gói Ưu Đãi Hội Viên Vàng', 'MemberTier', NULL, 0.15, NULL, NULL);
END

IF NOT EXISTS (SELECT * FROM Tournament)
BEGIN
    INSERT INTO Tournament (Title, GameTitle, FormatType, PrizePool, Status, StartDate) VALUES 
    (N'EAUT Valorant Championship 2026', 'Valorant', 'SingleElimination', 10000000, 'Ongoing', GETDATE()),
    (N'Đông Á League of Legends LAN Cup', 'League of Legends', 'DoubleElimination', 15000000, 'Upcoming', DATEADD(day, 3, GETDATE()));
END

IF NOT EXISTS (SELECT * FROM TournamentMatch)
BEGIN
    INSERT INTO TournamentMatch (TournamentId, RoundNumber, Team1Name, Team2Name, WinnerTeamName, Score, AssignedStationIds, Status) VALUES 
    (1, 1, 'Dragon Slayer', 'Cyber Hunters', 'Dragon Slayer', '2 - 1', 'PC-06,PC-07,PC-08,PC-09,PC-10', 'Finished'),
    (1, 2, 'Dragon Slayer', 'Phoenix Gaming', NULL, '0 - 0', 'PC-06,PC-07,PC-08,PC-09,PC-10', 'Playing');
END

IF NOT EXISTS (SELECT * FROM GamingSession)
BEGIN
    INSERT INTO GamingSession (StationId, UserId, StartTime, EndTime, TariffType, PlayCost, ServiceCost, TotalCost, Status) VALUES 
    (1, 2, DATEADD(hour, -4, GETDATE()), DATEADD(hour, -1, GETDATE()), 'Hourly', 24000, 15000, 39000, 'Completed'),
    (4, 3, DATEADD(hour, -3, GETDATE()), DATEADD(hour, -1, GETDATE()), 'Hourly', 24000, 20000, 44000, 'Completed'),
    (6, 4, DATEADD(hour, -8, GETDATE()), DATEADD(hour, -1, GETDATE()), 'NightCombo', 45000, 25000, 70000, 'Completed'),
    (2, 2, DATEADD(minute, -45, GETDATE()), NULL, 'Hourly', 8000, 10000, 18000, 'Active');
END

IF EXISTS (SELECT 1 FROM GamingSession WHERE StationId = 2 AND Status = 'Active')
BEGIN
    UPDATE ComputerStation
    SET Status = 'InUse'
    WHERE StationId = 2 AND Status = 'Available';
END

IF NOT EXISTS (SELECT * FROM ServiceItem)
BEGIN
    INSERT INTO ServiceItem (ServiceName, Category, UnitPrice, StockQuantity, IsActive) VALUES
    (N'Sting Dâu', N'Đồ uống', 15000, 50, 1),
    (N'Red Bull', N'Đồ uống', 20000, 40, 1),
    (N'Coca-Cola Lon', N'Đồ uống', 15000, 60, 1),
    (N'Mì Tôm Trứng Xúc Xích', N'Đồ ăn', 25000, 30, 1),
    (N'Cơm Gà Xối Mỡ', N'Đồ ăn', 45000, 20, 1),
    (N'Bánh Mì Trứng Ốp La', N'Đồ ăn', 20000, 25, 1);
END

IF NOT EXISTS (SELECT * FROM SessionService)
BEGIN
    IF EXISTS (SELECT 1 FROM GamingSession WHERE SessionId = 1)
       AND EXISTS (SELECT 1 FROM ServiceItem WHERE ServiceId = 1)
    BEGIN
        INSERT INTO SessionService (SessionId, ServiceId, Quantity, UnitPrice, TotalPrice, Note)
        VALUES (1, 1, 1, 15000, 15000, N'Dịch vụ mẫu phiên 1');
    END
END

IF NOT EXISTS (SELECT * FROM WalletTransaction)
BEGIN
    IF EXISTS (SELECT 1 FROM AppUser WHERE UserId = 2)
    BEGIN
        INSERT INTO WalletTransaction (UserId, SessionId, TransactionType, Amount, BalanceBefore, BalanceAfter, PaymentMethod, Description, CreatedByUserId)
        VALUES (2, NULL, 'TopUp', 50000, 0, 50000, N'Tiền mặt tại quầy', N'Nạp khởi tạo tài khoản hội viên', 1);
    END
END
GO

