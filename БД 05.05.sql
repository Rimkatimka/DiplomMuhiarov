USE [master]
GO
/****** Object:  Database [EnergyMeteringSystem]    Script Date: 05.05.2026 18:29:08 ******/
CREATE DATABASE [EnergyMeteringSystem]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'EnergyMeteringSystem', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\EnergyMeteringSystem.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'EnergyMeteringSystem_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\EnergyMeteringSystem_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT
GO
ALTER DATABASE [EnergyMeteringSystem] SET COMPATIBILITY_LEVEL = 150
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [EnergyMeteringSystem].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [EnergyMeteringSystem] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [EnergyMeteringSystem] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [EnergyMeteringSystem] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [EnergyMeteringSystem] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [EnergyMeteringSystem] SET ARITHABORT OFF 
GO
ALTER DATABASE [EnergyMeteringSystem] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [EnergyMeteringSystem] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [EnergyMeteringSystem] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [EnergyMeteringSystem] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [EnergyMeteringSystem] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [EnergyMeteringSystem] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [EnergyMeteringSystem] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [EnergyMeteringSystem] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [EnergyMeteringSystem] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [EnergyMeteringSystem] SET  DISABLE_BROKER 
GO
ALTER DATABASE [EnergyMeteringSystem] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [EnergyMeteringSystem] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [EnergyMeteringSystem] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [EnergyMeteringSystem] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [EnergyMeteringSystem] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [EnergyMeteringSystem] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [EnergyMeteringSystem] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [EnergyMeteringSystem] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [EnergyMeteringSystem] SET  MULTI_USER 
GO
ALTER DATABASE [EnergyMeteringSystem] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [EnergyMeteringSystem] SET DB_CHAINING OFF 
GO
ALTER DATABASE [EnergyMeteringSystem] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [EnergyMeteringSystem] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [EnergyMeteringSystem] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [EnergyMeteringSystem] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [EnergyMeteringSystem] SET QUERY_STORE = OFF
GO
USE [EnergyMeteringSystem]
GO
/****** Object:  Table [dbo].[ConsumptionObject]    Script Date: 05.05.2026 18:29:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ConsumptionObject](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[ObjectTypeId] [int] NOT NULL,
	[StreetId] [int] NOT NULL,
	[HouseNumber] [nvarchar](10) NOT NULL,
	[ApartmentNumber] [nvarchar](10) NULL,
	[TotalArea] [decimal](8, 2) NULL,
	[ResidentCount] [int] NULL,
 CONSTRAINT [PK_ConsumptionObject] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Meter]    Script Date: 05.05.2026 18:29:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Meter](
	[Id] [int] NOT NULL,
	[SerialNumber] [nvarchar](50) NOT NULL,
	[MeterTypeId] [int] NOT NULL,
	[ConsumptionObjectId] [int] NOT NULL,
	[InstallationDate] [date] NOT NULL,
	[InitialReading] [decimal](15, 4) NOT NULL,
	[MeterStatusId] [int] NOT NULL,
	[VerificationDate] [date] NULL,
	[NextVerificationDate] [date] NULL,
 CONSTRAINT [PK_Meter] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MeterReading]    Script Date: 05.05.2026 18:29:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MeterReading](
	[Id] [int] NOT NULL,
	[MeterId] [int] NOT NULL,
	[ReadingDate] [date] NOT NULL,
	[Value] [decimal](15, 4) NOT NULL,
	[EnteredAt] [datetime] NOT NULL,
	[EnteredByUserId] [int] NOT NULL,
	[ReadingStatusId] [int] NOT NULL,
	[RejectionReasonId] [int] NULL,
	[Comment] [nvarchar](500) NULL,
	[TariffZone] [int] NOT NULL,
 CONSTRAINT [PK_MeterReading] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ReadingStatus]    Script Date: 05.05.2026 18:29:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReadingStatus](
	[Id] [int] NOT NULL,
	[Code] [nvarchar](20) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](255) NULL,
	[ColorHex] [nvarchar](7) NULL,
 CONSTRAINT [PK_ReadingStatus] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[User]    Script Date: 05.05.2026 18:29:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[User](
	[Id] [int] NOT NULL,
	[Username] [nvarchar](50) NOT NULL,
	[PasswordHash] [nvarchar](256) NOT NULL,
	[FullName] [nvarchar](100) NOT NULL,
	[Email] [nvarchar](100) NULL,
	[RoleId] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[ViewCurrentMeterReadings]    Script Date: 05.05.2026 18:29:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- 6. AEAU (VIEWS) aey io?aoiinoe
-- =============================================

CREATE VIEW [dbo].[ViewCurrentMeterReadings] AS
SELECT 
    mr.Id,
    m.SerialNumber,
    co.Name AS ObjectName,
    co.HouseNumber,
    co.ApartmentNumber,
    mr.ReadingDate,
    mr.Value,
    rs.Name AS StatusName,
    rs.ColorHex,
    u.FullName AS EnteredBy,
    mr.EnteredAt
FROM MeterReading mr
INNER JOIN Meter m ON mr.MeterId = m.Id
INNER JOIN ConsumptionObject co ON m.ConsumptionObjectId = co.Id
INNER JOIN ReadingStatus rs ON mr.ReadingStatusId = rs.Id
INNER JOIN [User] u ON mr.EnteredByUserId = u.Id
WHERE mr.ReadingDate >= DATEADD(MONTH, -1, GETDATE());
GO
/****** Object:  Table [dbo].[Payment]    Script Date: 05.05.2026 18:29:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Payment](
	[Id] [int] NOT NULL,
	[ConsumptionObjectId] [int] NOT NULL,
	[PaymentDate] [datetime] NOT NULL,
	[Amount] [decimal](15, 2) NOT NULL,
	[PaymentMethodId] [int] NOT NULL,
	[ReceivedByUserId] [int] NOT NULL,
	[ReceiptNumber] [nvarchar](50) NULL,
	[PeriodMonth] [int] NOT NULL,
	[PeriodYear] [int] NOT NULL,
 CONSTRAINT [PK_Payment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Accrual]    Script Date: 05.05.2026 18:29:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Accrual](
	[Id] [int] NOT NULL,
	[ConsumptionObjectId] [int] NOT NULL,
	[PeriodMonth] [int] NOT NULL,
	[PeriodYear] [int] NOT NULL,
	[ConsumptionValue] [decimal](15, 4) NOT NULL,
	[TariffId] [int] NOT NULL,
	[Amount] [decimal](15, 2) NOT NULL,
	[IsPaid] [bit] NOT NULL,
 CONSTRAINT [PK_Accrual] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Street]    Script Date: 05.05.2026 18:29:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Street](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[CityId] [int] NOT NULL,
	[PostalCode] [nvarchar](20) NULL,
 CONSTRAINT [PK_Street] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[ViewUnpaidAccruals]    Script Date: 05.05.2026 18:29:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[ViewUnpaidAccruals] AS
SELECT 
    co.Name AS ObjectName,
    s.Name AS StreetName,
    co.HouseNumber,
    co.ApartmentNumber,
    a.PeriodYear,
    a.PeriodMonth,
    a.ConsumptionValue,
    a.Amount,
    a.IsPaid,
    (SELECT SUM(p.Amount) 
     FROM Payment p 
     WHERE p.ConsumptionObjectId = a.ConsumptionObjectId
       AND p.PeriodYear = a.PeriodYear
       AND p.PeriodMonth = a.PeriodMonth) AS PaidAmount
FROM Accrual a
INNER JOIN ConsumptionObject co ON a.ConsumptionObjectId = co.Id
INNER JOIN Street s ON co.StreetId = s.Id
WHERE a.IsPaid = 0;
GO
/****** Object:  Table [dbo].[AuditLog]    Script Date: 05.05.2026 18:29:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AuditLog](
	[Id] [int] NOT NULL,
	[UserId] [int] NULL,
	[ActionTime] [datetime] NOT NULL,
	[ActionType] [nvarchar](50) NOT NULL,
	[TableName] [nvarchar](100) NOT NULL,
	[RecordId] [int] NOT NULL,
	[OldValuesJson] [nvarchar](max) NULL,
	[NewValuesJson] [nvarchar](max) NULL,
	[IpAddress] [nvarchar](45) NULL,
 CONSTRAINT [PK_AuditLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[City]    Script Date: 05.05.2026 18:29:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[City](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[RegionId] [int] NOT NULL,
 CONSTRAINT [PK_City] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Contract]    Script Date: 05.05.2026 18:29:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Contract](
	[Id] [int] NOT NULL,
	[ContractNumber] [nvarchar](50) NOT NULL,
	[ConsumptionObjectId] [int] NOT NULL,
	[ContractDate] [date] NOT NULL,
	[StartDate] [date] NOT NULL,
	[EndDate] [date] NULL,
	[ContractStatusId] [int] NOT NULL,
	[TariffId] [int] NOT NULL,
 CONSTRAINT [PK_Contract] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ContractStatus]    Script Date: 05.05.2026 18:29:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContractStatus](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[AllowsBilling] [bit] NOT NULL,
 CONSTRAINT [PK_ContractStatus] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EnergySource]    Script Date: 05.05.2026 18:29:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EnergySource](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Code] [nvarchar](20) NOT NULL,
	[CapacityMW] [decimal](10, 2) NULL,
 CONSTRAINT [PK_EnergySource] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MeterReplacementHistory]    Script Date: 05.05.2026 18:29:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MeterReplacementHistory](
	[Id] [int] NOT NULL,
	[OldMeterId] [int] NOT NULL,
	[NewMeterId] [int] NOT NULL,
	[ConsumptionObjectId] [int] NOT NULL,
	[ReplacementDate] [date] NOT NULL,
	[Reason] [nvarchar](200) NULL,
	[LastReadingOld] [decimal](15, 4) NULL,
	[FirstReadingNew] [decimal](15, 4) NULL,
 CONSTRAINT [PK_MeterReplacementHistory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MeterStatus]    Script Date: 05.05.2026 18:29:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MeterStatus](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[CanAcceptReadings] [bit] NOT NULL,
 CONSTRAINT [PK_MeterStatus] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MeterType]    Script Date: 05.05.2026 18:29:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MeterType](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Voltage] [int] NOT NULL,
	[MaxCurrent] [int] NOT NULL,
	[AccuracyClass] [nvarchar](10) NOT NULL,
	[DigitCount] [int] NOT NULL,
	[DecimalPlaces] [int] NOT NULL,
 CONSTRAINT [PK_MeterType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ObjectType]    Script Date: 05.05.2026 18:29:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ObjectType](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[NormConsumption] [decimal](10, 2) NULL,
 CONSTRAINT [PK_ObjectType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PaymentMethod]    Script Date: 05.05.2026 18:29:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PaymentMethod](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[RequiresCashier] [bit] NOT NULL,
 CONSTRAINT [PK_PaymentMethod] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Region]    Script Date: 05.05.2026 18:29:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Region](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Code] [nvarchar](10) NULL,
 CONSTRAINT [PK_Region] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RejectionReason]    Script Date: 05.05.2026 18:29:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RejectionReason](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[RequiresComment] [bit] NOT NULL,
 CONSTRAINT [PK_RejectionReason] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SupplyPoint]    Script Date: 05.05.2026 18:29:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SupplyPoint](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[EnergySourceId] [int] NOT NULL,
	[VoltageLevel] [int] NOT NULL,
	[MaxPower] [decimal](10, 2) NULL,
 CONSTRAINT [PK_SupplyPoint] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SupplyPointConsumptionObject]    Script Date: 05.05.2026 18:29:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SupplyPointConsumptionObject](
	[SupplyPointId] [int] NOT NULL,
	[ConsumptionObjectId] [int] NOT NULL,
	[ConnectionDate] [date] NOT NULL,
	[DisconnectionDate] [date] NULL,
 CONSTRAINT [PK_SupplyPointConsumptionObject] PRIMARY KEY CLUSTERED 
(
	[SupplyPointId] ASC,
	[ConsumptionObjectId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tariff]    Script Date: 05.05.2026 18:29:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tariff](
	[Id] [int] NOT NULL,
	[TariffTypeId] [int] NOT NULL,
	[UnitId] [int] NOT NULL,
	[PricePerUnit] [decimal](10, 4) NOT NULL,
	[ZoneNumber] [int] NOT NULL,
	[StartDate] [date] NOT NULL,
	[EndDate] [date] NULL,
 CONSTRAINT [PK_Tariff] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TariffType]    Script Date: 05.05.2026 18:29:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TariffType](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ZoneCount] [int] NOT NULL,
	[Description] [nvarchar](255) NULL,
 CONSTRAINT [PK_TariffType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UnitOfMeasure]    Script Date: 05.05.2026 18:29:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UnitOfMeasure](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](20) NOT NULL,
	[Symbol] [nvarchar](10) NOT NULL,
	[IsDefault] [bit] NOT NULL,
 CONSTRAINT [PK_UnitOfMeasure] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserRole]    Script Date: 05.05.2026 18:29:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserRole](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](255) NULL,
	[PermissionsJson] [nvarchar](max) NULL,
 CONSTRAINT [PK_UserRole] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VerificationInterval]    Script Date: 05.05.2026 18:29:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VerificationInterval](
	[Id] [int] NOT NULL,
	[MeterTypeId] [int] NOT NULL,
	[Years] [int] NOT NULL,
 CONSTRAINT [PK_VerificationInterval] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[Accrual] ([Id], [ConsumptionObjectId], [PeriodMonth], [PeriodYear], [ConsumptionValue], [TariffId], [Amount], [IsPaid]) VALUES (1, 1, 1, 2024, CAST(605.0000 AS Decimal(15, 4)), 1, CAST(3146.00 AS Decimal(15, 2)), 1)
INSERT [dbo].[Accrual] ([Id], [ConsumptionObjectId], [PeriodMonth], [PeriodYear], [ConsumptionValue], [TariffId], [Amount], [IsPaid]) VALUES (2, 1, 2, 2024, CAST(100.0000 AS Decimal(15, 4)), 1, CAST(520.00 AS Decimal(15, 2)), 0)
INSERT [dbo].[Accrual] ([Id], [ConsumptionObjectId], [PeriodMonth], [PeriodYear], [ConsumptionValue], [TariffId], [Amount], [IsPaid]) VALUES (3, 2, 1, 2024, CAST(650.0000 AS Decimal(15, 4)), 1, CAST(3380.00 AS Decimal(15, 2)), 1)
INSERT [dbo].[Accrual] ([Id], [ConsumptionObjectId], [PeriodMonth], [PeriodYear], [ConsumptionValue], [TariffId], [Amount], [IsPaid]) VALUES (4, 2, 2, 2024, CAST(170.0000 AS Decimal(15, 4)), 1, CAST(884.00 AS Decimal(15, 2)), 0)
INSERT [dbo].[Accrual] ([Id], [ConsumptionObjectId], [PeriodMonth], [PeriodYear], [ConsumptionValue], [TariffId], [Amount], [IsPaid]) VALUES (5, 3, 1, 2024, CAST(500.0000 AS Decimal(15, 4)), 1, CAST(2600.00 AS Decimal(15, 2)), 0)
INSERT [dbo].[Accrual] ([Id], [ConsumptionObjectId], [PeriodMonth], [PeriodYear], [ConsumptionValue], [TariffId], [Amount], [IsPaid]) VALUES (6, 3, 2, 2024, CAST(200.0000 AS Decimal(15, 4)), 1, CAST(1040.00 AS Decimal(15, 2)), 0)
INSERT [dbo].[Accrual] ([Id], [ConsumptionObjectId], [PeriodMonth], [PeriodYear], [ConsumptionValue], [TariffId], [Amount], [IsPaid]) VALUES (7, 1, 1, 2025, CAST(1395.2000 AS Decimal(15, 4)), 2, CAST(7673.60 AS Decimal(15, 2)), 1)
INSERT [dbo].[Accrual] ([Id], [ConsumptionObjectId], [PeriodMonth], [PeriodYear], [ConsumptionValue], [TariffId], [Amount], [IsPaid]) VALUES (8, 1, 2, 2025, CAST(80.6000 AS Decimal(15, 4)), 2, CAST(443.30 AS Decimal(15, 2)), 0)
INSERT [dbo].[Accrual] ([Id], [ConsumptionObjectId], [PeriodMonth], [PeriodYear], [ConsumptionValue], [TariffId], [Amount], [IsPaid]) VALUES (9, 2, 1, 2025, CAST(1150.2500 AS Decimal(15, 4)), 2, CAST(6326.38 AS Decimal(15, 2)), 1)
INSERT [dbo].[Accrual] ([Id], [ConsumptionObjectId], [PeriodMonth], [PeriodYear], [ConsumptionValue], [TariffId], [Amount], [IsPaid]) VALUES (10, 2, 2, 2025, CAST(70.2500 AS Decimal(15, 4)), 2, CAST(386.38 AS Decimal(15, 2)), 0)
INSERT [dbo].[Accrual] ([Id], [ConsumptionObjectId], [PeriodMonth], [PeriodYear], [ConsumptionValue], [TariffId], [Amount], [IsPaid]) VALUES (11, 3, 1, 2025, CAST(2350.0000 AS Decimal(15, 4)), 2, CAST(12925.00 AS Decimal(15, 2)), 0)
INSERT [dbo].[Accrual] ([Id], [ConsumptionObjectId], [PeriodMonth], [PeriodYear], [ConsumptionValue], [TariffId], [Amount], [IsPaid]) VALUES (12, 1, 1, 2026, CAST(1750.0000 AS Decimal(15, 4)), 5, CAST(10150.00 AS Decimal(15, 2)), 1)
INSERT [dbo].[Accrual] ([Id], [ConsumptionObjectId], [PeriodMonth], [PeriodYear], [ConsumptionValue], [TariffId], [Amount], [IsPaid]) VALUES (13, 1, 2, 2026, CAST(70.5000 AS Decimal(15, 4)), 5, CAST(408.90 AS Decimal(15, 2)), 0)
INSERT [dbo].[Accrual] ([Id], [ConsumptionObjectId], [PeriodMonth], [PeriodYear], [ConsumptionValue], [TariffId], [Amount], [IsPaid]) VALUES (14, 2, 1, 2026, CAST(1520.0000 AS Decimal(15, 4)), 5, CAST(8816.00 AS Decimal(15, 2)), 0)
INSERT [dbo].[Accrual] ([Id], [ConsumptionObjectId], [PeriodMonth], [PeriodYear], [ConsumptionValue], [TariffId], [Amount], [IsPaid]) VALUES (15, 3, 1, 2026, CAST(2650.0000 AS Decimal(15, 4)), 5, CAST(15370.00 AS Decimal(15, 2)), 0)
INSERT [dbo].[Accrual] ([Id], [ConsumptionObjectId], [PeriodMonth], [PeriodYear], [ConsumptionValue], [TariffId], [Amount], [IsPaid]) VALUES (16, 4, 1, 2026, CAST(2250.0000 AS Decimal(15, 4)), 5, CAST(13050.00 AS Decimal(15, 2)), 0)
GO
INSERT [dbo].[AuditLog] ([Id], [UserId], [ActionTime], [ActionType], [TableName], [RecordId], [OldValuesJson], [NewValuesJson], [IpAddress]) VALUES (1, 1, CAST(N'2026-05-05T16:17:37.233' AS DateTime), N'LOGIN', N'User', 1, NULL, N'{"login":"operator"}', N'127.0.0.1')
INSERT [dbo].[AuditLog] ([Id], [UserId], [ActionTime], [ActionType], [TableName], [RecordId], [OldValuesJson], [NewValuesJson], [IpAddress]) VALUES (2, 2, CAST(N'2026-05-05T16:17:37.233' AS DateTime), N'VERIFY', N'MeterReading', 1, N'{"status":1}', N'{"status":2}', N'127.0.0.1')
INSERT [dbo].[AuditLog] ([Id], [UserId], [ActionTime], [ActionType], [TableName], [RecordId], [OldValuesJson], [NewValuesJson], [IpAddress]) VALUES (3, 3, CAST(N'2026-05-05T16:17:37.233' AS DateTime), N'CALCULATE', N'Accrual', 1, NULL, N'{"period":"01.2024"}', N'127.0.0.1')
INSERT [dbo].[AuditLog] ([Id], [UserId], [ActionTime], [ActionType], [TableName], [RecordId], [OldValuesJson], [NewValuesJson], [IpAddress]) VALUES (4, 4, CAST(N'2026-05-05T16:17:37.233' AS DateTime), N'CREATE', N'User', 4, NULL, N'{"username":"admin"}', N'127.0.0.1')
GO
INSERT [dbo].[City] ([Id], [Name], [RegionId]) VALUES (2, N'Агидель', 1)
INSERT [dbo].[City] ([Id], [Name], [RegionId]) VALUES (1, N'Нефтекамск', 1)
GO
INSERT [dbo].[ConsumptionObject] ([Id], [Name], [ObjectTypeId], [StreetId], [HouseNumber], [ApartmentNumber], [TotalArea], [ResidentCount]) VALUES (0, N', 12', 1, 3, N'12', N'42', CAST(0.00 AS Decimal(8, 2)), -1)
INSERT [dbo].[ConsumptionObject] ([Id], [Name], [ObjectTypeId], [StreetId], [HouseNumber], [ApartmentNumber], [TotalArea], [ResidentCount]) VALUES (1, N'Квартира 46', 1, 1, N'25', N'46', CAST(65.50 AS Decimal(8, 2)), 3)
INSERT [dbo].[ConsumptionObject] ([Id], [Name], [ObjectTypeId], [StreetId], [HouseNumber], [ApartmentNumber], [TotalArea], [ResidentCount]) VALUES (2, N'Квартира 47', 1, 1, N'25', N'47', CAST(70.20 AS Decimal(8, 2)), 4)
INSERT [dbo].[ConsumptionObject] ([Id], [Name], [ObjectTypeId], [StreetId], [HouseNumber], [ApartmentNumber], [TotalArea], [ResidentCount]) VALUES (3, N'Частный дом', 2, 2, N'7', NULL, CAST(120.00 AS Decimal(8, 2)), 5)
INSERT [dbo].[ConsumptionObject] ([Id], [Name], [ObjectTypeId], [StreetId], [HouseNumber], [ApartmentNumber], [TotalArea], [ResidentCount]) VALUES (4, N'Магазин "Продукты"', 3, 1, N'10', NULL, CAST(150.50 AS Decimal(8, 2)), NULL)
GO
INSERT [dbo].[Contract] ([Id], [ContractNumber], [ConsumptionObjectId], [ContractDate], [StartDate], [EndDate], [ContractStatusId], [TariffId]) VALUES (1, N'Д-001/23', 1, CAST(N'2023-01-01' AS Date), CAST(N'2023-01-01' AS Date), NULL, 1, 5)
INSERT [dbo].[Contract] ([Id], [ContractNumber], [ConsumptionObjectId], [ContractDate], [StartDate], [EndDate], [ContractStatusId], [TariffId]) VALUES (2, N'Д-002/23', 2, CAST(N'2023-01-01' AS Date), CAST(N'2023-01-01' AS Date), NULL, 1, 5)
INSERT [dbo].[Contract] ([Id], [ContractNumber], [ConsumptionObjectId], [ContractDate], [StartDate], [EndDate], [ContractStatusId], [TariffId]) VALUES (3, N'Д-003/22', 3, CAST(N'2022-01-01' AS Date), CAST(N'2022-01-01' AS Date), NULL, 1, 5)
INSERT [dbo].[Contract] ([Id], [ContractNumber], [ConsumptionObjectId], [ContractDate], [StartDate], [EndDate], [ContractStatusId], [TariffId]) VALUES (4, N'Д-004/23', 4, CAST(N'2023-01-01' AS Date), CAST(N'2023-01-01' AS Date), NULL, 1, 5)
GO
INSERT [dbo].[ContractStatus] ([Id], [Name], [AllowsBilling]) VALUES (1, N'Активен', 1)
INSERT [dbo].[ContractStatus] ([Id], [Name], [AllowsBilling]) VALUES (2, N'Расторгнут', 0)
GO
INSERT [dbo].[EnergySource] ([Id], [Name], [Code], [CapacityMW]) VALUES (1, N'Нефтекамская ТЭЦ', N'TEC-01', CAST(250.00 AS Decimal(10, 2)))
INSERT [dbo].[EnergySource] ([Id], [Name], [Code], [CapacityMW]) VALUES (2, N'Агидельская ГЭС', N'GES-02', CAST(120.50 AS Decimal(10, 2)))
GO
INSERT [dbo].[Meter] ([Id], [SerialNumber], [MeterTypeId], [ConsumptionObjectId], [InstallationDate], [InitialReading], [MeterStatusId], [VerificationDate], [NextVerificationDate]) VALUES (1, N'SN00123456', 1, 1, CAST(N'2023-01-15' AS Date), CAST(0.0000 AS Decimal(15, 4)), 1, CAST(N'2023-01-15' AS Date), CAST(N'2029-01-15' AS Date))
INSERT [dbo].[Meter] ([Id], [SerialNumber], [MeterTypeId], [ConsumptionObjectId], [InstallationDate], [InitialReading], [MeterStatusId], [VerificationDate], [NextVerificationDate]) VALUES (2, N'SN00123457', 1, 2, CAST(N'2023-03-10' AS Date), CAST(0.0000 AS Decimal(15, 4)), 1, CAST(N'2023-03-10' AS Date), CAST(N'2029-03-10' AS Date))
INSERT [dbo].[Meter] ([Id], [SerialNumber], [MeterTypeId], [ConsumptionObjectId], [InstallationDate], [InitialReading], [MeterStatusId], [VerificationDate], [NextVerificationDate]) VALUES (3, N'SN00234567', 2, 3, CAST(N'2022-06-20' AS Date), CAST(0.0000 AS Decimal(15, 4)), 1, CAST(N'2022-06-20' AS Date), CAST(N'2028-06-20' AS Date))
INSERT [dbo].[Meter] ([Id], [SerialNumber], [MeterTypeId], [ConsumptionObjectId], [InstallationDate], [InitialReading], [MeterStatusId], [VerificationDate], [NextVerificationDate]) VALUES (4, N'SN00345678', 3, 4, CAST(N'2023-01-01' AS Date), CAST(0.0000 AS Decimal(15, 4)), 1, CAST(N'2023-01-01' AS Date), CAST(N'2029-01-01' AS Date))
INSERT [dbo].[Meter] ([Id], [SerialNumber], [MeterTypeId], [ConsumptionObjectId], [InstallationDate], [InitialReading], [MeterStatusId], [VerificationDate], [NextVerificationDate]) VALUES (5, N'номер счетчика', 1, 1, CAST(N'2026-05-05' AS Date), CAST(0.0000 AS Decimal(15, 4)), 1, CAST(N'2026-05-06' AS Date), NULL)
INSERT [dbo].[Meter] ([Id], [SerialNumber], [MeterTypeId], [ConsumptionObjectId], [InstallationDate], [InitialReading], [MeterStatusId], [VerificationDate], [NextVerificationDate]) VALUES (6, N'вапм', 1, 4, CAST(N'2026-05-05' AS Date), CAST(0.0000 AS Decimal(15, 4)), 1, CAST(N'2026-05-05' AS Date), NULL)
INSERT [dbo].[Meter] ([Id], [SerialNumber], [MeterTypeId], [ConsumptionObjectId], [InstallationDate], [InitialReading], [MeterStatusId], [VerificationDate], [NextVerificationDate]) VALUES (7, N'и', 2, 2, CAST(N'2026-05-05' AS Date), CAST(0.0000 AS Decimal(15, 4)), 1, CAST(N'2026-05-16' AS Date), NULL)
INSERT [dbo].[Meter] ([Id], [SerialNumber], [MeterTypeId], [ConsumptionObjectId], [InstallationDate], [InitialReading], [MeterStatusId], [VerificationDate], [NextVerificationDate]) VALUES (8, N'эюжбджбдбд', 2, 0, CAST(N'2027-05-05' AS Date), CAST(-5555.0000 AS Decimal(15, 4)), 1, CAST(N'2026-06-04' AS Date), NULL)
GO
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (1, 1, CAST(N'2023-10-25' AS Date), CAST(350.2500 AS Decimal(15, 4)), CAST(N'2023-10-25T10:00:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (2, 1, CAST(N'2023-11-25' AS Date), CAST(425.5000 AS Decimal(15, 4)), CAST(N'2023-11-25T11:00:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (3, 1, CAST(N'2023-12-25' AS Date), CAST(510.7500 AS Decimal(15, 4)), CAST(N'2023-12-25T09:30:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (4, 1, CAST(N'2024-01-25' AS Date), CAST(605.0000 AS Decimal(15, 4)), CAST(N'2024-01-25T14:20:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (5, 1, CAST(N'2024-02-25' AS Date), CAST(705.0000 AS Decimal(15, 4)), CAST(N'2024-02-25T10:15:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (6, 1, CAST(N'2024-03-25' AS Date), CAST(810.0000 AS Decimal(15, 4)), CAST(N'2024-03-25T11:00:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (7, 1, CAST(N'2024-11-25' AS Date), CAST(1250.0000 AS Decimal(15, 4)), CAST(N'2024-11-25T09:00:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (8, 1, CAST(N'2024-12-25' AS Date), CAST(1320.5000 AS Decimal(15, 4)), CAST(N'2024-12-25T10:30:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (9, 1, CAST(N'2025-01-25' AS Date), CAST(1395.2000 AS Decimal(15, 4)), CAST(N'2025-01-25T14:00:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (10, 1, CAST(N'2025-02-25' AS Date), CAST(1475.8000 AS Decimal(15, 4)), CAST(N'2025-02-25T09:45:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (11, 1, CAST(N'2026-01-25' AS Date), CAST(1750.0000 AS Decimal(15, 4)), CAST(N'2026-01-25T11:00:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (12, 1, CAST(N'2026-02-25' AS Date), CAST(1820.5000 AS Decimal(15, 4)), CAST(N'2026-02-25T10:00:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (13, 2, CAST(N'2023-10-26' AS Date), CAST(280.0000 AS Decimal(15, 4)), CAST(N'2023-10-26T10:00:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (14, 2, CAST(N'2023-12-26' AS Date), CAST(450.0000 AS Decimal(15, 4)), CAST(N'2023-12-26T11:00:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (15, 2, CAST(N'2024-02-26' AS Date), CAST(650.0000 AS Decimal(15, 4)), CAST(N'2024-02-26T09:30:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (16, 2, CAST(N'2024-04-26' AS Date), CAST(820.0000 AS Decimal(15, 4)), CAST(N'2024-04-26T14:00:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (17, 2, CAST(N'2025-01-26' AS Date), CAST(1150.2500 AS Decimal(15, 4)), CAST(N'2025-01-26T10:15:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (18, 2, CAST(N'2025-02-26' AS Date), CAST(1220.5000 AS Decimal(15, 4)), CAST(N'2025-02-26T11:30:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (19, 2, CAST(N'2026-01-26' AS Date), CAST(1520.0000 AS Decimal(15, 4)), CAST(N'2026-01-26T09:00:00.000' AS DateTime), 1, 1, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (20, 2, CAST(N'2026-02-26' AS Date), CAST(1590.7500 AS Decimal(15, 4)), CAST(N'2026-02-26T10:20:00.000' AS DateTime), 1, 1, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (21, 3, CAST(N'2023-08-20' AS Date), CAST(1200.0000 AS Decimal(15, 4)), CAST(N'2023-08-20T10:00:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (22, 3, CAST(N'2023-12-20' AS Date), CAST(1950.0000 AS Decimal(15, 4)), CAST(N'2023-12-20T11:00:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (23, 3, CAST(N'2024-02-20' AS Date), CAST(2150.0000 AS Decimal(15, 4)), CAST(N'2024-02-20T09:30:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (24, 3, CAST(N'2024-08-20' AS Date), CAST(1350.0000 AS Decimal(15, 4)), CAST(N'2024-08-20T14:00:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (25, 3, CAST(N'2025-02-20' AS Date), CAST(2350.0000 AS Decimal(15, 4)), CAST(N'2025-02-20T10:15:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (26, 3, CAST(N'2025-08-20' AS Date), CAST(1450.0000 AS Decimal(15, 4)), CAST(N'2025-08-20T11:30:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (27, 3, CAST(N'2026-02-20' AS Date), CAST(2650.0000 AS Decimal(15, 4)), CAST(N'2026-02-20T09:00:00.000' AS DateTime), 1, 1, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (28, 4, CAST(N'2023-01-10' AS Date), CAST(150.0000 AS Decimal(15, 4)), CAST(N'2023-01-10T10:00:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (29, 4, CAST(N'2023-06-10' AS Date), CAST(890.0000 AS Decimal(15, 4)), CAST(N'2023-06-10T11:00:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (30, 4, CAST(N'2023-12-10' AS Date), CAST(1850.0000 AS Decimal(15, 4)), CAST(N'2023-12-10T09:30:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (31, 4, CAST(N'2024-06-10' AS Date), CAST(950.0000 AS Decimal(15, 4)), CAST(N'2024-06-10T14:00:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (32, 4, CAST(N'2024-12-10' AS Date), CAST(1950.0000 AS Decimal(15, 4)), CAST(N'2024-12-10T10:15:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (33, 4, CAST(N'2025-06-10' AS Date), CAST(1050.0000 AS Decimal(15, 4)), CAST(N'2025-06-10T11:30:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (34, 4, CAST(N'2025-12-10' AS Date), CAST(2100.0000 AS Decimal(15, 4)), CAST(N'2025-12-10T09:00:00.000' AS DateTime), 1, 2, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (35, 4, CAST(N'2026-02-10' AS Date), CAST(2250.0000 AS Decimal(15, 4)), CAST(N'2026-02-10T10:00:00.000' AS DateTime), 1, 1, NULL, NULL, 1)
INSERT [dbo].[MeterReading] ([Id], [MeterId], [ReadingDate], [Value], [EnteredAt], [EnteredByUserId], [ReadingStatusId], [RejectionReasonId], [Comment], [TariffZone]) VALUES (36, 8, CAST(N'2026-05-05' AS Date), CAST(1234525.0000 AS Decimal(15, 4)), CAST(N'2026-05-05T17:43:11.750' AS DateTime), 4, 1, NULL, NULL, 1)
GO
INSERT [dbo].[MeterStatus] ([Id], [Name], [CanAcceptReadings]) VALUES (1, N'Исправен', 1)
INSERT [dbo].[MeterStatus] ([Id], [Name], [CanAcceptReadings]) VALUES (2, N'Неисправен', 0)
INSERT [dbo].[MeterStatus] ([Id], [Name], [CanAcceptReadings]) VALUES (3, N'На поверке', 0)
GO
INSERT [dbo].[MeterType] ([Id], [Name], [Voltage], [MaxCurrent], [AccuracyClass], [DigitCount], [DecimalPlaces]) VALUES (1, N'Однофазный', 220, 40, N'1.0', 6, 0)
INSERT [dbo].[MeterType] ([Id], [Name], [Voltage], [MaxCurrent], [AccuracyClass], [DigitCount], [DecimalPlaces]) VALUES (2, N'Трехфазный', 380, 100, N'0.5', 8, 0)
INSERT [dbo].[MeterType] ([Id], [Name], [Voltage], [MaxCurrent], [AccuracyClass], [DigitCount], [DecimalPlaces]) VALUES (3, N'Многотарифный', 220, 60, N'1.0', 7, 1)
GO
INSERT [dbo].[ObjectType] ([Id], [Name], [NormConsumption]) VALUES (1, N'Квартира', CAST(150.00 AS Decimal(10, 2)))
INSERT [dbo].[ObjectType] ([Id], [Name], [NormConsumption]) VALUES (2, N'Частный дом', CAST(300.00 AS Decimal(10, 2)))
INSERT [dbo].[ObjectType] ([Id], [Name], [NormConsumption]) VALUES (3, N'Магазин', CAST(500.00 AS Decimal(10, 2)))
GO
INSERT [dbo].[Payment] ([Id], [ConsumptionObjectId], [PaymentDate], [Amount], [PaymentMethodId], [ReceivedByUserId], [ReceiptNumber], [PeriodMonth], [PeriodYear]) VALUES (0, 1, CAST(N'2026-05-05T16:40:08.887' AS DateTime), CAST(89.00 AS Decimal(15, 2)), 1, 4, N'ПЛ-20260505-164008', 5, 2026)
INSERT [dbo].[Payment] ([Id], [ConsumptionObjectId], [PaymentDate], [Amount], [PaymentMethodId], [ReceivedByUserId], [ReceiptNumber], [PeriodMonth], [PeriodYear]) VALUES (1, 1, CAST(N'2024-02-10T10:00:00.000' AS DateTime), CAST(3146.00 AS Decimal(15, 2)), 1, 3, N'КВ-001/24', 1, 2024)
INSERT [dbo].[Payment] ([Id], [ConsumptionObjectId], [PaymentDate], [Amount], [PaymentMethodId], [ReceivedByUserId], [ReceiptNumber], [PeriodMonth], [PeriodYear]) VALUES (2, 2, CAST(N'2024-02-15T11:30:00.000' AS DateTime), CAST(3380.00 AS Decimal(15, 2)), 2, 3, N'КВ-002/24', 1, 2024)
INSERT [dbo].[Payment] ([Id], [ConsumptionObjectId], [PaymentDate], [Amount], [PaymentMethodId], [ReceivedByUserId], [ReceiptNumber], [PeriodMonth], [PeriodYear]) VALUES (3, 1, CAST(N'2025-02-10T09:45:00.000' AS DateTime), CAST(7673.60 AS Decimal(15, 2)), 1, 3, N'КВ-003/25', 1, 2025)
INSERT [dbo].[Payment] ([Id], [ConsumptionObjectId], [PaymentDate], [Amount], [PaymentMethodId], [ReceivedByUserId], [ReceiptNumber], [PeriodMonth], [PeriodYear]) VALUES (4, 2, CAST(N'2025-02-12T14:00:00.000' AS DateTime), CAST(6326.38 AS Decimal(15, 2)), 3, 3, N'КВ-004/25', 1, 2025)
INSERT [dbo].[Payment] ([Id], [ConsumptionObjectId], [PaymentDate], [Amount], [PaymentMethodId], [ReceivedByUserId], [ReceiptNumber], [PeriodMonth], [PeriodYear]) VALUES (5, 1, CAST(N'2026-02-20T10:00:00.000' AS DateTime), CAST(10150.00 AS Decimal(15, 2)), 1, 3, N'КВ-005/26', 1, 2026)
GO
INSERT [dbo].[PaymentMethod] ([Id], [Name], [RequiresCashier]) VALUES (1, N'Наличные', 1)
INSERT [dbo].[PaymentMethod] ([Id], [Name], [RequiresCashier]) VALUES (2, N'Банковская карта', 1)
INSERT [dbo].[PaymentMethod] ([Id], [Name], [RequiresCashier]) VALUES (3, N'Онлайн-банк', 0)
GO
INSERT [dbo].[ReadingStatus] ([Id], [Code], [Name], [Description], [ColorHex]) VALUES (1, N'Entered', N'Введено', N'Показания введены оператором', N'#FFA500')
INSERT [dbo].[ReadingStatus] ([Id], [Code], [Name], [Description], [ColorHex]) VALUES (2, N'Verified', N'Подтверждено', N'Показания проверены', N'#008000')
INSERT [dbo].[ReadingStatus] ([Id], [Code], [Name], [Description], [ColorHex]) VALUES (3, N'Rejected', N'Отклонено', N'Показания отклонены', N'#FF0000')
GO
INSERT [dbo].[Region] ([Id], [Name], [Code]) VALUES (1, N'Республика Башкортостан', N'02')
GO
INSERT [dbo].[RejectionReason] ([Id], [Name], [RequiresComment]) VALUES (1, N'Некорректные данные', 1)
INSERT [dbo].[RejectionReason] ([Id], [Name], [RequiresComment]) VALUES (2, N'Скачок потребления', 1)
INSERT [dbo].[RejectionReason] ([Id], [Name], [RequiresComment]) VALUES (3, N'Отсутствие доступа', 0)
GO
INSERT [dbo].[Street] ([Id], [Name], [CityId], [PostalCode]) VALUES (1, N'Ленина', 1, N'452680')
INSERT [dbo].[Street] ([Id], [Name], [CityId], [PostalCode]) VALUES (2, N'Социалистическая', 1, N'452683')
INSERT [dbo].[Street] ([Id], [Name], [CityId], [PostalCode]) VALUES (3, N'Мира', 2, N'452690')
GO
INSERT [dbo].[Tariff] ([Id], [TariffTypeId], [UnitId], [PricePerUnit], [ZoneNumber], [StartDate], [EndDate]) VALUES (1, 1, 1, CAST(5.2000 AS Decimal(10, 4)), 1, CAST(N'2024-01-01' AS Date), CAST(N'2024-12-31' AS Date))
INSERT [dbo].[Tariff] ([Id], [TariffTypeId], [UnitId], [PricePerUnit], [ZoneNumber], [StartDate], [EndDate]) VALUES (2, 1, 1, CAST(5.5000 AS Decimal(10, 4)), 1, CAST(N'2025-01-01' AS Date), CAST(N'2025-12-31' AS Date))
INSERT [dbo].[Tariff] ([Id], [TariffTypeId], [UnitId], [PricePerUnit], [ZoneNumber], [StartDate], [EndDate]) VALUES (3, 2, 1, CAST(6.0000 AS Decimal(10, 4)), 1, CAST(N'2025-01-01' AS Date), CAST(N'2025-12-31' AS Date))
INSERT [dbo].[Tariff] ([Id], [TariffTypeId], [UnitId], [PricePerUnit], [ZoneNumber], [StartDate], [EndDate]) VALUES (4, 2, 1, CAST(3.5000 AS Decimal(10, 4)), 2, CAST(N'2025-01-01' AS Date), CAST(N'2025-12-31' AS Date))
INSERT [dbo].[Tariff] ([Id], [TariffTypeId], [UnitId], [PricePerUnit], [ZoneNumber], [StartDate], [EndDate]) VALUES (5, 1, 1, CAST(5.8000 AS Decimal(10, 4)), 1, CAST(N'2026-01-01' AS Date), NULL)
GO
INSERT [dbo].[TariffType] ([Id], [Name], [ZoneCount], [Description]) VALUES (1, N'Однотарифный', 1, N'Единая цена в любое время суток')
INSERT [dbo].[TariffType] ([Id], [Name], [ZoneCount], [Description]) VALUES (2, N'Двухтарифный', 2, N'День/Ночь')
GO
INSERT [dbo].[UnitOfMeasure] ([Id], [Name], [Symbol], [IsDefault]) VALUES (1, N'киловатт-час', N'кВт·ч', 1)
GO
INSERT [dbo].[User] ([Id], [Username], [PasswordHash], [FullName], [Email], [RoleId], [IsActive], [CreatedAt]) VALUES (1, N'operator', N'12345', N'Иванов Иван Иванович', N'ivanov@mail.ru', 1, 1, CAST(N'2025-01-01T00:00:00.000' AS DateTime))
INSERT [dbo].[User] ([Id], [Username], [PasswordHash], [FullName], [Email], [RoleId], [IsActive], [CreatedAt]) VALUES (2, N'inspector', N'12345', N'Петров Петр Петрович', N'petrov@mail.ru', 2, 1, CAST(N'2025-01-01T00:00:00.000' AS DateTime))
INSERT [dbo].[User] ([Id], [Username], [PasswordHash], [FullName], [Email], [RoleId], [IsActive], [CreatedAt]) VALUES (3, N'accountant', N'12345', N'Сидорова Анна Сергеевна', N'sidorova@mail.ru', 3, 1, CAST(N'2025-01-01T00:00:00.000' AS DateTime))
INSERT [dbo].[User] ([Id], [Username], [PasswordHash], [FullName], [Email], [RoleId], [IsActive], [CreatedAt]) VALUES (4, N'admin', N'12345', N'Админов Алексей Алексеевич', N'admin@system.ru', 4, 1, CAST(N'2025-01-01T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[UserRole] ([Id], [Name], [Description], [PermissionsJson]) VALUES (1, N'Оператор', N'Ввод показаний', N'["Reading.Enter","Reading.ViewOwn"]')
INSERT [dbo].[UserRole] ([Id], [Name], [Description], [PermissionsJson]) VALUES (2, N'Инспектор', N'Верификация показаний', N'["Reading.Verify","Reading.ViewAll"]')
INSERT [dbo].[UserRole] ([Id], [Name], [Description], [PermissionsJson]) VALUES (3, N'Бухгалтер', N'Расчеты и платежи', N'["Accrual.Calculate","Payment.Register"]')
INSERT [dbo].[UserRole] ([Id], [Name], [Description], [PermissionsJson]) VALUES (4, N'Администратор', N'Управление системой', N'["All"]')
GO
INSERT [dbo].[VerificationInterval] ([Id], [MeterTypeId], [Years]) VALUES (1, 1, 16)
INSERT [dbo].[VerificationInterval] ([Id], [MeterTypeId], [Years]) VALUES (2, 2, 8)
INSERT [dbo].[VerificationInterval] ([Id], [MeterTypeId], [Years]) VALUES (3, 3, 10)
GO
/****** Object:  Index [UQ_Accrual_Object_Period]    Script Date: 05.05.2026 18:29:08 ******/
ALTER TABLE [dbo].[Accrual] ADD  CONSTRAINT [UQ_Accrual_Object_Period] UNIQUE NONCLUSTERED 
(
	[ConsumptionObjectId] ASC,
	[PeriodMonth] ASC,
	[PeriodYear] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Accrual_ConsumptionObjectId]    Script Date: 05.05.2026 18:29:08 ******/
CREATE NONCLUSTERED INDEX [IX_Accrual_ConsumptionObjectId] ON [dbo].[Accrual]
(
	[ConsumptionObjectId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Accrual_IsPaid]    Script Date: 05.05.2026 18:29:08 ******/
CREATE NONCLUSTERED INDEX [IX_Accrual_IsPaid] ON [dbo].[Accrual]
(
	[IsPaid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Accrual_PeriodYear_PeriodMonth]    Script Date: 05.05.2026 18:29:08 ******/
CREATE NONCLUSTERED INDEX [IX_Accrual_PeriodYear_PeriodMonth] ON [dbo].[Accrual]
(
	[PeriodYear] ASC,
	[PeriodMonth] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_AuditLog_ActionTime]    Script Date: 05.05.2026 18:29:08 ******/
CREATE NONCLUSTERED INDEX [IX_AuditLog_ActionTime] ON [dbo].[AuditLog]
(
	[ActionTime] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AuditLog_TableName_RecordId]    Script Date: 05.05.2026 18:29:08 ******/
CREATE NONCLUSTERED INDEX [IX_AuditLog_TableName_RecordId] ON [dbo].[AuditLog]
(
	[TableName] ASC,
	[RecordId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_AuditLog_UserId]    Script Date: 05.05.2026 18:29:08 ******/
CREATE NONCLUSTERED INDEX [IX_AuditLog_UserId] ON [dbo].[AuditLog]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_City_Region_Name]    Script Date: 05.05.2026 18:29:08 ******/
ALTER TABLE [dbo].[City] ADD  CONSTRAINT [UQ_City_Region_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC,
	[RegionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_Contract_Number]    Script Date: 05.05.2026 18:29:08 ******/
ALTER TABLE [dbo].[Contract] ADD  CONSTRAINT [UQ_Contract_Number] UNIQUE NONCLUSTERED 
(
	[ContractNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Contract_ConsumptionObjectId]    Script Date: 05.05.2026 18:29:08 ******/
CREATE NONCLUSTERED INDEX [IX_Contract_ConsumptionObjectId] ON [dbo].[Contract]
(
	[ConsumptionObjectId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Contract_ContractStatusId]    Script Date: 05.05.2026 18:29:08 ******/
CREATE NONCLUSTERED INDEX [IX_Contract_ContractStatusId] ON [dbo].[Contract]
(
	[ContractStatusId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_ContractStatus_Name]    Script Date: 05.05.2026 18:29:08 ******/
ALTER TABLE [dbo].[ContractStatus] ADD  CONSTRAINT [UQ_ContractStatus_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_EnergySource_Code]    Script Date: 05.05.2026 18:29:08 ******/
ALTER TABLE [dbo].[EnergySource] ADD  CONSTRAINT [UQ_EnergySource_Code] UNIQUE NONCLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_EnergySource_Name]    Script Date: 05.05.2026 18:29:08 ******/
ALTER TABLE [dbo].[EnergySource] ADD  CONSTRAINT [UQ_EnergySource_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_Meter_SerialNumber]    Script Date: 05.05.2026 18:29:08 ******/
ALTER TABLE [dbo].[Meter] ADD  CONSTRAINT [UQ_Meter_SerialNumber] UNIQUE NONCLUSTERED 
(
	[SerialNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UQ_MeterReading_Meter_Date]    Script Date: 05.05.2026 18:29:08 ******/
ALTER TABLE [dbo].[MeterReading] ADD  CONSTRAINT [UQ_MeterReading_Meter_Date] UNIQUE NONCLUSTERED 
(
	[MeterId] ASC,
	[ReadingDate] ASC,
	[TariffZone] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_MeterReading_EnteredByUserId]    Script Date: 05.05.2026 18:29:08 ******/
CREATE NONCLUSTERED INDEX [IX_MeterReading_EnteredByUserId] ON [dbo].[MeterReading]
(
	[EnteredByUserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_MeterReading_MeterId]    Script Date: 05.05.2026 18:29:08 ******/
CREATE NONCLUSTERED INDEX [IX_MeterReading_MeterId] ON [dbo].[MeterReading]
(
	[MeterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_MeterReading_ReadingDate]    Script Date: 05.05.2026 18:29:08 ******/
CREATE NONCLUSTERED INDEX [IX_MeterReading_ReadingDate] ON [dbo].[MeterReading]
(
	[ReadingDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_MeterReading_ReadingStatusId]    Script Date: 05.05.2026 18:29:08 ******/
CREATE NONCLUSTERED INDEX [IX_MeterReading_ReadingStatusId] ON [dbo].[MeterReading]
(
	[ReadingStatusId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_MeterStatus_Name]    Script Date: 05.05.2026 18:29:08 ******/
ALTER TABLE [dbo].[MeterStatus] ADD  CONSTRAINT [UQ_MeterStatus_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_MeterType_Name]    Script Date: 05.05.2026 18:29:08 ******/
ALTER TABLE [dbo].[MeterType] ADD  CONSTRAINT [UQ_MeterType_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_ObjectType_Name]    Script Date: 05.05.2026 18:29:08 ******/
ALTER TABLE [dbo].[ObjectType] ADD  CONSTRAINT [UQ_ObjectType_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_Payment_ReceiptNumber]    Script Date: 05.05.2026 18:29:08 ******/
ALTER TABLE [dbo].[Payment] ADD  CONSTRAINT [UQ_Payment_ReceiptNumber] UNIQUE NONCLUSTERED 
(
	[ReceiptNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Payment_ConsumptionObjectId]    Script Date: 05.05.2026 18:29:08 ******/
CREATE NONCLUSTERED INDEX [IX_Payment_ConsumptionObjectId] ON [dbo].[Payment]
(
	[ConsumptionObjectId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Payment_PaymentDate]    Script Date: 05.05.2026 18:29:08 ******/
CREATE NONCLUSTERED INDEX [IX_Payment_PaymentDate] ON [dbo].[Payment]
(
	[PaymentDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Payment_PeriodYear_PeriodMonth]    Script Date: 05.05.2026 18:29:08 ******/
CREATE NONCLUSTERED INDEX [IX_Payment_PeriodYear_PeriodMonth] ON [dbo].[Payment]
(
	[PeriodYear] ASC,
	[PeriodMonth] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_PaymentMethod_Name]    Script Date: 05.05.2026 18:29:08 ******/
ALTER TABLE [dbo].[PaymentMethod] ADD  CONSTRAINT [UQ_PaymentMethod_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_ReadingStatus_Code]    Script Date: 05.05.2026 18:29:08 ******/
ALTER TABLE [dbo].[ReadingStatus] ADD  CONSTRAINT [UQ_ReadingStatus_Code] UNIQUE NONCLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_ReadingStatus_Name]    Script Date: 05.05.2026 18:29:08 ******/
ALTER TABLE [dbo].[ReadingStatus] ADD  CONSTRAINT [UQ_ReadingStatus_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_Region_Name]    Script Date: 05.05.2026 18:29:08 ******/
ALTER TABLE [dbo].[Region] ADD  CONSTRAINT [UQ_Region_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_RejectionReason_Name]    Script Date: 05.05.2026 18:29:08 ******/
ALTER TABLE [dbo].[RejectionReason] ADD  CONSTRAINT [UQ_RejectionReason_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_Street_City_Name]    Script Date: 05.05.2026 18:29:08 ******/
ALTER TABLE [dbo].[Street] ADD  CONSTRAINT [UQ_Street_City_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC,
	[CityId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_TariffType_Name]    Script Date: 05.05.2026 18:29:08 ******/
ALTER TABLE [dbo].[TariffType] ADD  CONSTRAINT [UQ_TariffType_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_UnitOfMeasure_Name]    Script Date: 05.05.2026 18:29:08 ******/
ALTER TABLE [dbo].[UnitOfMeasure] ADD  CONSTRAINT [UQ_UnitOfMeasure_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_UnitOfMeasure_Symbol]    Script Date: 05.05.2026 18:29:08 ******/
ALTER TABLE [dbo].[UnitOfMeasure] ADD  CONSTRAINT [UQ_UnitOfMeasure_Symbol] UNIQUE NONCLUSTERED 
(
	[Symbol] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_User_Email]    Script Date: 05.05.2026 18:29:08 ******/
ALTER TABLE [dbo].[User] ADD  CONSTRAINT [UQ_User_Email] UNIQUE NONCLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_User_Username]    Script Date: 05.05.2026 18:29:08 ******/
ALTER TABLE [dbo].[User] ADD  CONSTRAINT [UQ_User_Username] UNIQUE NONCLUSTERED 
(
	[Username] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_User_RoleId]    Script Date: 05.05.2026 18:29:08 ******/
CREATE NONCLUSTERED INDEX [IX_User_RoleId] ON [dbo].[User]
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_UserRole_Name]    Script Date: 05.05.2026 18:29:08 ******/
ALTER TABLE [dbo].[UserRole] ADD  CONSTRAINT [UQ_UserRole_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Accrual] ADD  DEFAULT ((0)) FOR [IsPaid]
GO
ALTER TABLE [dbo].[AuditLog] ADD  DEFAULT (getdate()) FOR [ActionTime]
GO
ALTER TABLE [dbo].[ContractStatus] ADD  DEFAULT ((1)) FOR [AllowsBilling]
GO
ALTER TABLE [dbo].[Meter] ADD  DEFAULT ((0)) FOR [InitialReading]
GO
ALTER TABLE [dbo].[MeterReading] ADD  DEFAULT (getdate()) FOR [EnteredAt]
GO
ALTER TABLE [dbo].[MeterReading] ADD  DEFAULT ((1)) FOR [TariffZone]
GO
ALTER TABLE [dbo].[MeterStatus] ADD  DEFAULT ((1)) FOR [CanAcceptReadings]
GO
ALTER TABLE [dbo].[MeterType] ADD  DEFAULT ((0)) FOR [DecimalPlaces]
GO
ALTER TABLE [dbo].[Payment] ADD  DEFAULT (getdate()) FOR [PaymentDate]
GO
ALTER TABLE [dbo].[PaymentMethod] ADD  DEFAULT ((1)) FOR [RequiresCashier]
GO
ALTER TABLE [dbo].[RejectionReason] ADD  DEFAULT ((0)) FOR [RequiresComment]
GO
ALTER TABLE [dbo].[SupplyPointConsumptionObject] ADD  DEFAULT (getdate()) FOR [ConnectionDate]
GO
ALTER TABLE [dbo].[Tariff] ADD  DEFAULT ((1)) FOR [ZoneNumber]
GO
ALTER TABLE [dbo].[TariffType] ADD  DEFAULT ((1)) FOR [ZoneCount]
GO
ALTER TABLE [dbo].[UnitOfMeasure] ADD  DEFAULT ((0)) FOR [IsDefault]
GO
ALTER TABLE [dbo].[User] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[User] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Accrual]  WITH CHECK ADD  CONSTRAINT [FK_Accrual_ConsumptionObject] FOREIGN KEY([ConsumptionObjectId])
REFERENCES [dbo].[ConsumptionObject] ([Id])
GO
ALTER TABLE [dbo].[Accrual] CHECK CONSTRAINT [FK_Accrual_ConsumptionObject]
GO
ALTER TABLE [dbo].[Accrual]  WITH CHECK ADD  CONSTRAINT [FK_Accrual_Tariff] FOREIGN KEY([TariffId])
REFERENCES [dbo].[Tariff] ([Id])
GO
ALTER TABLE [dbo].[Accrual] CHECK CONSTRAINT [FK_Accrual_Tariff]
GO
ALTER TABLE [dbo].[AuditLog]  WITH CHECK ADD  CONSTRAINT [FK_AuditLog_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([Id])
GO
ALTER TABLE [dbo].[AuditLog] CHECK CONSTRAINT [FK_AuditLog_User]
GO
ALTER TABLE [dbo].[City]  WITH CHECK ADD  CONSTRAINT [FK_City_Region] FOREIGN KEY([RegionId])
REFERENCES [dbo].[Region] ([Id])
GO
ALTER TABLE [dbo].[City] CHECK CONSTRAINT [FK_City_Region]
GO
ALTER TABLE [dbo].[ConsumptionObject]  WITH CHECK ADD  CONSTRAINT [FK_ConsumptionObject_ObjectType] FOREIGN KEY([ObjectTypeId])
REFERENCES [dbo].[ObjectType] ([Id])
GO
ALTER TABLE [dbo].[ConsumptionObject] CHECK CONSTRAINT [FK_ConsumptionObject_ObjectType]
GO
ALTER TABLE [dbo].[ConsumptionObject]  WITH CHECK ADD  CONSTRAINT [FK_ConsumptionObject_Street] FOREIGN KEY([StreetId])
REFERENCES [dbo].[Street] ([Id])
GO
ALTER TABLE [dbo].[ConsumptionObject] CHECK CONSTRAINT [FK_ConsumptionObject_Street]
GO
ALTER TABLE [dbo].[Contract]  WITH CHECK ADD  CONSTRAINT [FK_Contract_ConsumptionObject] FOREIGN KEY([ConsumptionObjectId])
REFERENCES [dbo].[ConsumptionObject] ([Id])
GO
ALTER TABLE [dbo].[Contract] CHECK CONSTRAINT [FK_Contract_ConsumptionObject]
GO
ALTER TABLE [dbo].[Contract]  WITH CHECK ADD  CONSTRAINT [FK_Contract_ContractStatus] FOREIGN KEY([ContractStatusId])
REFERENCES [dbo].[ContractStatus] ([Id])
GO
ALTER TABLE [dbo].[Contract] CHECK CONSTRAINT [FK_Contract_ContractStatus]
GO
ALTER TABLE [dbo].[Contract]  WITH CHECK ADD  CONSTRAINT [FK_Contract_Tariff] FOREIGN KEY([TariffId])
REFERENCES [dbo].[Tariff] ([Id])
GO
ALTER TABLE [dbo].[Contract] CHECK CONSTRAINT [FK_Contract_Tariff]
GO
ALTER TABLE [dbo].[Meter]  WITH CHECK ADD  CONSTRAINT [FK_Meter_ConsumptionObject] FOREIGN KEY([ConsumptionObjectId])
REFERENCES [dbo].[ConsumptionObject] ([Id])
GO
ALTER TABLE [dbo].[Meter] CHECK CONSTRAINT [FK_Meter_ConsumptionObject]
GO
ALTER TABLE [dbo].[Meter]  WITH CHECK ADD  CONSTRAINT [FK_Meter_MeterStatus] FOREIGN KEY([MeterStatusId])
REFERENCES [dbo].[MeterStatus] ([Id])
GO
ALTER TABLE [dbo].[Meter] CHECK CONSTRAINT [FK_Meter_MeterStatus]
GO
ALTER TABLE [dbo].[Meter]  WITH CHECK ADD  CONSTRAINT [FK_Meter_MeterType] FOREIGN KEY([MeterTypeId])
REFERENCES [dbo].[MeterType] ([Id])
GO
ALTER TABLE [dbo].[Meter] CHECK CONSTRAINT [FK_Meter_MeterType]
GO
ALTER TABLE [dbo].[MeterReading]  WITH CHECK ADD  CONSTRAINT [FK_MeterReading_Meter] FOREIGN KEY([MeterId])
REFERENCES [dbo].[Meter] ([Id])
GO
ALTER TABLE [dbo].[MeterReading] CHECK CONSTRAINT [FK_MeterReading_Meter]
GO
ALTER TABLE [dbo].[MeterReading]  WITH CHECK ADD  CONSTRAINT [FK_MeterReading_ReadingStatus] FOREIGN KEY([ReadingStatusId])
REFERENCES [dbo].[ReadingStatus] ([Id])
GO
ALTER TABLE [dbo].[MeterReading] CHECK CONSTRAINT [FK_MeterReading_ReadingStatus]
GO
ALTER TABLE [dbo].[MeterReading]  WITH CHECK ADD  CONSTRAINT [FK_MeterReading_RejectionReason] FOREIGN KEY([RejectionReasonId])
REFERENCES [dbo].[RejectionReason] ([Id])
GO
ALTER TABLE [dbo].[MeterReading] CHECK CONSTRAINT [FK_MeterReading_RejectionReason]
GO
ALTER TABLE [dbo].[MeterReading]  WITH CHECK ADD  CONSTRAINT [FK_MeterReading_User] FOREIGN KEY([EnteredByUserId])
REFERENCES [dbo].[User] ([Id])
GO
ALTER TABLE [dbo].[MeterReading] CHECK CONSTRAINT [FK_MeterReading_User]
GO
ALTER TABLE [dbo].[MeterReplacementHistory]  WITH CHECK ADD  CONSTRAINT [FK_MeterReplacementHistory_ConsumptionObject] FOREIGN KEY([ConsumptionObjectId])
REFERENCES [dbo].[ConsumptionObject] ([Id])
GO
ALTER TABLE [dbo].[MeterReplacementHistory] CHECK CONSTRAINT [FK_MeterReplacementHistory_ConsumptionObject]
GO
ALTER TABLE [dbo].[MeterReplacementHistory]  WITH CHECK ADD  CONSTRAINT [FK_MeterReplacementHistory_NewMeter] FOREIGN KEY([NewMeterId])
REFERENCES [dbo].[Meter] ([Id])
GO
ALTER TABLE [dbo].[MeterReplacementHistory] CHECK CONSTRAINT [FK_MeterReplacementHistory_NewMeter]
GO
ALTER TABLE [dbo].[MeterReplacementHistory]  WITH CHECK ADD  CONSTRAINT [FK_MeterReplacementHistory_OldMeter] FOREIGN KEY([OldMeterId])
REFERENCES [dbo].[Meter] ([Id])
GO
ALTER TABLE [dbo].[MeterReplacementHistory] CHECK CONSTRAINT [FK_MeterReplacementHistory_OldMeter]
GO
ALTER TABLE [dbo].[Payment]  WITH CHECK ADD  CONSTRAINT [FK_Payment_ConsumptionObject] FOREIGN KEY([ConsumptionObjectId])
REFERENCES [dbo].[ConsumptionObject] ([Id])
GO
ALTER TABLE [dbo].[Payment] CHECK CONSTRAINT [FK_Payment_ConsumptionObject]
GO
ALTER TABLE [dbo].[Payment]  WITH CHECK ADD  CONSTRAINT [FK_Payment_PaymentMethod] FOREIGN KEY([PaymentMethodId])
REFERENCES [dbo].[PaymentMethod] ([Id])
GO
ALTER TABLE [dbo].[Payment] CHECK CONSTRAINT [FK_Payment_PaymentMethod]
GO
ALTER TABLE [dbo].[Payment]  WITH CHECK ADD  CONSTRAINT [FK_Payment_User] FOREIGN KEY([ReceivedByUserId])
REFERENCES [dbo].[User] ([Id])
GO
ALTER TABLE [dbo].[Payment] CHECK CONSTRAINT [FK_Payment_User]
GO
ALTER TABLE [dbo].[Street]  WITH CHECK ADD  CONSTRAINT [FK_Street_City] FOREIGN KEY([CityId])
REFERENCES [dbo].[City] ([Id])
GO
ALTER TABLE [dbo].[Street] CHECK CONSTRAINT [FK_Street_City]
GO
ALTER TABLE [dbo].[SupplyPoint]  WITH CHECK ADD  CONSTRAINT [FK_SupplyPoint_EnergySource] FOREIGN KEY([EnergySourceId])
REFERENCES [dbo].[EnergySource] ([Id])
GO
ALTER TABLE [dbo].[SupplyPoint] CHECK CONSTRAINT [FK_SupplyPoint_EnergySource]
GO
ALTER TABLE [dbo].[SupplyPointConsumptionObject]  WITH CHECK ADD  CONSTRAINT [FK_SupplyPointConsumptionObject_ConsumptionObject] FOREIGN KEY([ConsumptionObjectId])
REFERENCES [dbo].[ConsumptionObject] ([Id])
GO
ALTER TABLE [dbo].[SupplyPointConsumptionObject] CHECK CONSTRAINT [FK_SupplyPointConsumptionObject_ConsumptionObject]
GO
ALTER TABLE [dbo].[SupplyPointConsumptionObject]  WITH CHECK ADD  CONSTRAINT [FK_SupplyPointConsumptionObject_SupplyPoint] FOREIGN KEY([SupplyPointId])
REFERENCES [dbo].[SupplyPoint] ([Id])
GO
ALTER TABLE [dbo].[SupplyPointConsumptionObject] CHECK CONSTRAINT [FK_SupplyPointConsumptionObject_SupplyPoint]
GO
ALTER TABLE [dbo].[Tariff]  WITH CHECK ADD  CONSTRAINT [FK_Tariff_TariffType] FOREIGN KEY([TariffTypeId])
REFERENCES [dbo].[TariffType] ([Id])
GO
ALTER TABLE [dbo].[Tariff] CHECK CONSTRAINT [FK_Tariff_TariffType]
GO
ALTER TABLE [dbo].[Tariff]  WITH CHECK ADD  CONSTRAINT [FK_Tariff_UnitOfMeasure] FOREIGN KEY([UnitId])
REFERENCES [dbo].[UnitOfMeasure] ([Id])
GO
ALTER TABLE [dbo].[Tariff] CHECK CONSTRAINT [FK_Tariff_UnitOfMeasure]
GO
ALTER TABLE [dbo].[User]  WITH CHECK ADD  CONSTRAINT [FK_User_UserRole] FOREIGN KEY([RoleId])
REFERENCES [dbo].[UserRole] ([Id])
GO
ALTER TABLE [dbo].[User] CHECK CONSTRAINT [FK_User_UserRole]
GO
ALTER TABLE [dbo].[VerificationInterval]  WITH CHECK ADD  CONSTRAINT [FK_VerificationInterval_MeterType] FOREIGN KEY([MeterTypeId])
REFERENCES [dbo].[MeterType] ([Id])
GO
ALTER TABLE [dbo].[VerificationInterval] CHECK CONSTRAINT [FK_VerificationInterval_MeterType]
GO
ALTER TABLE [dbo].[Accrual]  WITH CHECK ADD  CONSTRAINT [CHK_Accrual_Amount] CHECK  (([Amount]>=(0)))
GO
ALTER TABLE [dbo].[Accrual] CHECK CONSTRAINT [CHK_Accrual_Amount]
GO
ALTER TABLE [dbo].[Accrual]  WITH CHECK ADD  CONSTRAINT [CHK_Accrual_Consumption] CHECK  (([ConsumptionValue]>=(0)))
GO
ALTER TABLE [dbo].[Accrual] CHECK CONSTRAINT [CHK_Accrual_Consumption]
GO
ALTER TABLE [dbo].[Accrual]  WITH CHECK ADD  CONSTRAINT [CHK_Accrual_PeriodMonth] CHECK  (([PeriodMonth]>=(1) AND [PeriodMonth]<=(12)))
GO
ALTER TABLE [dbo].[Accrual] CHECK CONSTRAINT [CHK_Accrual_PeriodMonth]
GO
ALTER TABLE [dbo].[Accrual]  WITH CHECK ADD  CONSTRAINT [CHK_Accrual_PeriodYear] CHECK  (([PeriodYear]>=(2000) AND [PeriodYear]<=(2100)))
GO
ALTER TABLE [dbo].[Accrual] CHECK CONSTRAINT [CHK_Accrual_PeriodYear]
GO
ALTER TABLE [dbo].[Contract]  WITH CHECK ADD  CONSTRAINT [CHK_Contract_Dates] CHECK  (([StartDate]>=[ContractDate] AND ([EndDate] IS NULL OR [EndDate]>[StartDate])))
GO
ALTER TABLE [dbo].[Contract] CHECK CONSTRAINT [CHK_Contract_Dates]
GO
ALTER TABLE [dbo].[Meter]  WITH CHECK ADD  CONSTRAINT [CHK_Meter_Dates] CHECK  (([VerificationDate]<=[NextVerificationDate]))
GO
ALTER TABLE [dbo].[Meter] CHECK CONSTRAINT [CHK_Meter_Dates]
GO
ALTER TABLE [dbo].[MeterReading]  WITH CHECK ADD  CONSTRAINT [CHK_MeterReading_TariffZone] CHECK  (([TariffZone]>=(1) AND [TariffZone]<=(3)))
GO
ALTER TABLE [dbo].[MeterReading] CHECK CONSTRAINT [CHK_MeterReading_TariffZone]
GO
ALTER TABLE [dbo].[MeterReading]  WITH CHECK ADD  CONSTRAINT [CHK_MeterReading_Value] CHECK  (([Value]>=(0)))
GO
ALTER TABLE [dbo].[MeterReading] CHECK CONSTRAINT [CHK_MeterReading_Value]
GO
ALTER TABLE [dbo].[MeterReplacementHistory]  WITH CHECK ADD  CONSTRAINT [CHK_MeterReplacementHistory_Meters] CHECK  (([OldMeterId]<>[NewMeterId]))
GO
ALTER TABLE [dbo].[MeterReplacementHistory] CHECK CONSTRAINT [CHK_MeterReplacementHistory_Meters]
GO
ALTER TABLE [dbo].[MeterType]  WITH CHECK ADD  CONSTRAINT [CHK_MeterType_DecimalPlaces] CHECK  (([DecimalPlaces]>=(0) AND [DecimalPlaces]<=(4)))
GO
ALTER TABLE [dbo].[MeterType] CHECK CONSTRAINT [CHK_MeterType_DecimalPlaces]
GO
ALTER TABLE [dbo].[MeterType]  WITH CHECK ADD  CONSTRAINT [CHK_MeterType_DigitCount] CHECK  (([DigitCount]>=(1) AND [DigitCount]<=(10)))
GO
ALTER TABLE [dbo].[MeterType] CHECK CONSTRAINT [CHK_MeterType_DigitCount]
GO
ALTER TABLE [dbo].[MeterType]  WITH CHECK ADD  CONSTRAINT [CHK_MeterType_Voltage] CHECK  (([Voltage]=(380) OR [Voltage]=(220)))
GO
ALTER TABLE [dbo].[MeterType] CHECK CONSTRAINT [CHK_MeterType_Voltage]
GO
ALTER TABLE [dbo].[Payment]  WITH CHECK ADD  CONSTRAINT [CHK_Payment_Amount] CHECK  (([Amount]>(0)))
GO
ALTER TABLE [dbo].[Payment] CHECK CONSTRAINT [CHK_Payment_Amount]
GO
ALTER TABLE [dbo].[Payment]  WITH CHECK ADD  CONSTRAINT [CHK_Payment_PeriodMonth] CHECK  (([PeriodMonth]>=(1) AND [PeriodMonth]<=(12)))
GO
ALTER TABLE [dbo].[Payment] CHECK CONSTRAINT [CHK_Payment_PeriodMonth]
GO
ALTER TABLE [dbo].[Payment]  WITH CHECK ADD  CONSTRAINT [CHK_Payment_PeriodYear] CHECK  (([PeriodYear]>=(2000) AND [PeriodYear]<=(2100)))
GO
ALTER TABLE [dbo].[Payment] CHECK CONSTRAINT [CHK_Payment_PeriodYear]
GO
ALTER TABLE [dbo].[SupplyPointConsumptionObject]  WITH CHECK ADD  CONSTRAINT [CHK_SupplyPointConsumptionObject_Dates] CHECK  (([DisconnectionDate] IS NULL OR [DisconnectionDate]>[ConnectionDate]))
GO
ALTER TABLE [dbo].[SupplyPointConsumptionObject] CHECK CONSTRAINT [CHK_SupplyPointConsumptionObject_Dates]
GO
ALTER TABLE [dbo].[Tariff]  WITH CHECK ADD  CONSTRAINT [CHK_Tariff_Dates] CHECK  (([EndDate] IS NULL OR [EndDate]>[StartDate]))
GO
ALTER TABLE [dbo].[Tariff] CHECK CONSTRAINT [CHK_Tariff_Dates]
GO
ALTER TABLE [dbo].[Tariff]  WITH CHECK ADD  CONSTRAINT [CHK_Tariff_Price] CHECK  (([PricePerUnit]>(0)))
GO
ALTER TABLE [dbo].[Tariff] CHECK CONSTRAINT [CHK_Tariff_Price]
GO
ALTER TABLE [dbo].[Tariff]  WITH CHECK ADD  CONSTRAINT [CHK_Tariff_ZoneNumber] CHECK  (([ZoneNumber]>=(1) AND [ZoneNumber]<=(3)))
GO
ALTER TABLE [dbo].[Tariff] CHECK CONSTRAINT [CHK_Tariff_ZoneNumber]
GO
ALTER TABLE [dbo].[TariffType]  WITH CHECK ADD  CONSTRAINT [CHK_TariffType_ZoneCount] CHECK  (([ZoneCount]=(3) OR [ZoneCount]=(2) OR [ZoneCount]=(1)))
GO
ALTER TABLE [dbo].[TariffType] CHECK CONSTRAINT [CHK_TariffType_ZoneCount]
GO
ALTER TABLE [dbo].[VerificationInterval]  WITH CHECK ADD  CONSTRAINT [CHK_VerificationInterval_Years] CHECK  (([Years]>(0)))
GO
ALTER TABLE [dbo].[VerificationInterval] CHECK CONSTRAINT [CHK_VerificationInterval_Years]
GO
USE [master]
GO
ALTER DATABASE [EnergyMeteringSystem] SET  READ_WRITE 
GO
