
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server Compact Edition
-- --------------------------------------------------
-- Date Created: 09/05/2021 01:25:16
-- Generated from EDMX file: D:\swork\xdiscovery\Code\NV.DetectionPlatform.Entity\Model.edmx
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- NOTE: if the constraint does not exist, an ignorable error will be reported.
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- NOTE: if the table does not exist, an ignorable error will be reported.
-- --------------------------------------------------


-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Product'
CREATE TABLE [Product] (
    [GUID] nvarchar(254)  NOT NULL,
    [ProductName] nvarchar(254)  NULL,
    [ProductTypeID] nvarchar(254)  NULL,
    [ProductSpecification] nvarchar(254)  NULL,
    [ProductKeywords] nvarchar(254)  NULL,
    [StartTime] nvarchar(254)  NULL,
    [EndTime] nvarchar(254)  NULL,
    [ImageFolder] nvarchar(254)  NULL
);
GO

-- Creating table 'ExamParam'
CREATE TABLE [ExamParam] (
    [KV] float  NULL,
    [UA] int  NULL,
    [Time] float  NULL,
    [Fps] int  NULL,
    [ProductType] nvarchar(254)  NULL,
    [FILA] int  NULL,
    [PREH] int  NULL,
    [Name] nvarchar(254)  NULL,
    [GUID] nvarchar(254)  NOT NULL
);
GO

-- Creating table 'ImageParam'
CREATE TABLE [ImageParam] (
    [GUID] nvarchar(254)  NOT NULL,
    [Name] nvarchar(254)  NULL,
    [WindowWidth] int  NULL,
    [WindowLevel] int  NULL,
    [Gamma] float  NULL,
    [SharpLevel] int  NULL,
    [ReduceNoiseLevel] int  NULL,
    [FalseColor] bit  NULL,
    [EqualHist] bit  NULL,
    [ProductType] nvarchar(254)  NULL
);
GO

-- Creating table 'Overlay'
CREATE TABLE [Overlay] (
    [OverlayDesc] nvarchar(254)  NULL,
    [TagGroup] nvarchar(254)  NULL,
    [TagElement] nvarchar(254)  NULL,
    [DisplayFormat] nvarchar(254)  NULL,
    [OverlayID] int  NOT NULL,
    [IsUseful] bit  NULL
);
GO

-- Creating table 'AOIParam'
CREATE TABLE [AOIParam] (
    [GUID] nvarchar(254)  NOT NULL,
    [UpperlimitofBubble] int  NULL,
    [LowerlimitofBubble] int  NULL,
    [PercentofBubblePass] int  NULL,
    [Name] nvarchar(254)  NULL
);
GO

-- Creating table 'USMParamSet'
CREATE TABLE [USMParamSet] (
    [GUID] nvarchar(254)  NOT NULL,
    [Amount] float  NULL,
    [Radius] int  NULL,
    [Threshold] int  NULL,
    [Name] nvarchar(254)  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [GUID] in table 'Product'
ALTER TABLE [Product]
ADD CONSTRAINT [PK_Product]
    PRIMARY KEY ([GUID] );
GO

-- Creating primary key on [GUID] in table 'ExamParam'
ALTER TABLE [ExamParam]
ADD CONSTRAINT [PK_ExamParam]
    PRIMARY KEY ([GUID] );
GO

-- Creating primary key on [GUID] in table 'ImageParam'
ALTER TABLE [ImageParam]
ADD CONSTRAINT [PK_ImageParam]
    PRIMARY KEY ([GUID] );
GO

-- Creating primary key on [OverlayID] in table 'Overlay'
ALTER TABLE [Overlay]
ADD CONSTRAINT [PK_Overlay]
    PRIMARY KEY ([OverlayID] );
GO

-- Creating primary key on [GUID] in table 'AOIParam'
ALTER TABLE [AOIParam]
ADD CONSTRAINT [PK_AOIParam]
    PRIMARY KEY ([GUID] );
GO

-- Creating primary key on [GUID] in table 'USMParamSet'
ALTER TABLE [USMParamSet]
ADD CONSTRAINT [PK_USMParamSet]
    PRIMARY KEY ([GUID] );
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------