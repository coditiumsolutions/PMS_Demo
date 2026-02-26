-- Update Floor for ProjectID 9FB177CB92 (161 properties)
-- G+16: 1 unit on G, 10 units on each of floors 1-16
-- Run: sqlcmd -S localhost -U sa -P Pakistan@786 -d PMSAbbas -C -i "Scripts\Update_Property_Floors_Project_9FB177CB92.sql"

WITH Ordered AS (
  SELECT PropertyID,
    ROW_NUMBER() OVER (ORDER BY PropertyID) AS rn
  FROM Property
  WHERE ProjectID = '9FB177CB92'
),
Floors AS (
  SELECT PropertyID,
    CASE
      WHEN rn <= 1 THEN N'G'
      WHEN rn <= 11 THEN N'1'
      WHEN rn <= 21 THEN N'2'
      WHEN rn <= 31 THEN N'3'
      WHEN rn <= 41 THEN N'4'
      WHEN rn <= 51 THEN N'5'
      WHEN rn <= 61 THEN N'6'
      WHEN rn <= 71 THEN N'7'
      WHEN rn <= 81 THEN N'8'
      WHEN rn <= 91 THEN N'9'
      WHEN rn <= 101 THEN N'10'
      WHEN rn <= 111 THEN N'11'
      WHEN rn <= 121 THEN N'12'
      WHEN rn <= 131 THEN N'13'
      WHEN rn <= 141 THEN N'14'
      WHEN rn <= 151 THEN N'15'
      WHEN rn <= 161 THEN N'16'
      ELSE NULL
    END AS Floor
  FROM Ordered
)
UPDATE p
SET p.Floor = f.Floor
FROM Property p
INNER JOIN Floors f ON p.PropertyID = f.PropertyID;

-- Verify counts per floor
SELECT Floor, COUNT(*) AS [Count]
FROM Property
WHERE ProjectID = '9FB177CB92'
GROUP BY Floor
ORDER BY CASE WHEN Floor = 'G' THEN 0 ELSE CAST(Floor AS INT) END;
