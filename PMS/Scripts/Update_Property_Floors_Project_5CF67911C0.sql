-- Update Floor for ProjectID 5CF67911C0 (102 properties)
-- G+10 floors: 2 units on G, 10 units on each of floors 1-10
-- Run: sqlcmd -S localhost -U sa -P Pakistan@786 -d PMSAbbas -C -i "Scripts\Update_Property_Floors_Project_5CF67911C0.sql"

WITH Ordered AS (
  SELECT PropertyID,
    ROW_NUMBER() OVER (ORDER BY PropertyID) AS rn
  FROM Property
  WHERE ProjectID = '5CF67911C0'
),
Floors AS (
  SELECT PropertyID,
    CASE
      WHEN rn <= 2 THEN N'G'
      WHEN rn <= 12 THEN N'1'
      WHEN rn <= 22 THEN N'2'
      WHEN rn <= 32 THEN N'3'
      WHEN rn <= 42 THEN N'4'
      WHEN rn <= 52 THEN N'5'
      WHEN rn <= 62 THEN N'6'
      WHEN rn <= 72 THEN N'7'
      WHEN rn <= 82 THEN N'8'
      WHEN rn <= 92 THEN N'9'
      WHEN rn <= 102 THEN N'10'
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
WHERE ProjectID = '5CF67911C0'
GROUP BY Floor
ORDER BY CASE WHEN Floor = 'G' THEN 0 ELSE CAST(Floor AS INT) END;
