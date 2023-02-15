CREATE DATABASE ElectivaIV;

USE ElectivaIV;

CREATE TABLE [dbo].[Huellas](
[Id] [int] IDENTITY(1,1) NOT NULL,
[Huella] [varbinary](max) NULL,
[Nombres] [varchar](150) NULL,
CONSTRAINT [PK_Huellas] PRIMARY KEY CLUSTERED
(
[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF,
ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]