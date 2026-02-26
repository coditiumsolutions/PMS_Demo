# Generate Customers Script

## Overview
This script generates test customers for all projects in the database. It creates 100 customers per project (configurable) with proper CustomerID based on Project Prefix.

## How to Use

### Option 1: Via Browser URL (Admin Only)
1. Make sure you're logged in as Admin
2. Navigate to: `http://172.20.228.2:84/Customer/GenerateTestCustomers?customersPerProject=100`
3. Or use POST request with form data

### Option 2: Via PowerShell/curl
```powershell
# Login first to get authentication cookie, then:
Invoke-WebRequest -Uri "http://172.20.228.2:84/Customer/GenerateTestCustomers?customersPerProject=100" -Method POST -Headers @{"Cookie"="your-auth-cookie"}
```

### Option 3: Direct Database Access (if needed)
You can also create a simple console app using the GenerateCustomersScript.cs file in the Scripts folder.

## What It Does

1. **Gets All Projects**: Retrieves all projects that have a Prefix defined
2. **Gets Configuration**: Reads sizes and subprojects from Configuration table
3. **Gets Payment Plans**: Retrieves payment plans for each project
4. **Generates CustomerIDs**: Creates sequential CustomerIDs based on Project Prefix
   - Format: `{Prefix}{Number}` (e.g., "1C1500001", "1C1500002" for project with prefix "1C15")
5. **Creates Customers**: Generates 100 customers per project with:
   - Random names (from predefined lists)
   - Random CNIC numbers
   - Random phone numbers
   - Random addresses
   - Random sizes (from Configuration)
   - Random subprojects (from Configuration)
   - Assigned to a random payment plan for that project

## Current Projects
- **JUBA SMART CITY** (Prefix: "1C15") - Will generate: 1C1500001, 1C1500002, etc.
- **Zahid Heights** (Prefix: "ZH 1") - Will generate: ZH 1000001, ZH 1000002, etc.

## Notes
- The script automatically finds the next available CustomerID number for each project
- It saves in batches of 50 for performance
- Duplicate CustomerIDs are skipped
- Only Admin users can run this script

## Example Output
After running, you'll see a success message:
"Successfully created 200 test customers." (100 per project × 2 projects)
