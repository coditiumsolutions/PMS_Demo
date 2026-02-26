// For Without Sharing Folder

Working Backup:
BACKUP DATABASE [BMSBT] TO DISK = 'C:\SQLBackups\BTMSBT_07August2025.bak'

Working Restore:
RESTORE DATABASE BMSBT
FROM DISK = 'C:\SQL Backups\BMSBT.bak'
WITH 
    MOVE 'BMSBT' TO 'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\BMSBT.mdf',
    MOVE 'BMSBT_log' TO 'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\BMSBT_log.ldf',
    REPLACE;


// To Check if the exist in Physical Location
RESTORE FILELISTONLY 
FROM DISK = 'C:\SQL Backups\Payroll_15August2025.bak';


RESTORE DATABASE Payroll
FROM DISK = 'C:\SQL Backups\Payroll_15August2025.bak'
WITH 
    MOVE 'Payroll' TO 'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\Payroll.mdf',
    MOVE 'Payroll_Log'  TO 'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\Payroll_log.ldf',
    REPLACE;

//If Database is in Use THEN
USE master;
GO
-- Set the database to SINGLE_USER mode and rollback any existing connections
ALTER DATABASE BMSBT SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO
-- Perform the restore
RESTORE DATABASE BMSBT
FROM DISK = 'C:\SQL Backups\BMSBT.bak'
WITH 
    MOVE 'BMSBT' TO 'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\BMSBT.mdf',
    MOVE 'BMSBT_log' TO 'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\BMSBT_log.ldf',
    REPLACE;
GO
-- Set the database back to MULTI_USER mode
ALTER DATABASE BMSBT SET MULTI_USER;
GO




BACKUP DATABASE [BMSBT] 
TO DISK = N'C:\SQLBackups\BMSBT26July.bak'  -- Must exist on SQL Server's C: drive
WITH INIT, NAME = N'Full Backup of BMSBT', STATS = 10;


//For Sharing Folder Must Required. Share for Everyone
BACKUP DATABASE [BMSBT] 
TO DISK = N'\\172.20.229.2\SQLBackups\BMSBT27July.bak'
WITH INIT, NAME = N'Full Backup of BMSBT', STATS = 10;

// Following is to find the default folder
    EXEC master.dbo.xp_instance_regread 
    N'HKEY_LOCAL_MACHINE', 
    N'Software\Microsoft\MSSQLServer\MSSQLServer', 
    N'BackupDirectory';
	
//	Save Backup in Default Location
	BACKUP DATABASE BMSBT TO DISK = 'BMSBTDB.bak
