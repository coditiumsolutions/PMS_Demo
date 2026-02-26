# Payment Plan & Payment Schedule Module

## Overview

A **Payment Plan** is a billing template tied to a project. It defines the total price and duration. Each plan contains one or more **Payment Schedules** (installments) that customers pay against. Plans are shared across all customers assigned to the same plan.

**Controller:** `PaymentController.cs`
**Models:** `PaymentPlan.cs`, `PaymentSchedule.cs`, `PaymentPlanCreateViewModel.cs`
**Views:** `Views/Payment/` (PaymentPlans, PaymentSchedule, CreatePaymentPlan, CreatePaymentSchedule)

---

## Database Tables

### PaymentPlan

| Column | Type | Notes |
|--------|------|-------|
| PlanID | CHAR(10) PK | Random 10-char ID |
| ProjectID | CHAR(10) FK | Which project this plan belongs to |
| PlanName | NVARCHAR(150) | Display name |
| TotalAmount | DECIMAL(18,2) | Total price in local currency (PKR) |
| TotalAmountUSD | DECIMAL(18,2) | Equivalent in USD |
| ExchangeRate | DECIMAL(18,4) | USD-to-PKR rate at plan creation |
| Currency | NVARCHAR(10) | `PKR` or `SSP` |
| DurationMonths | INT | Plan duration |
| Frequency | NVARCHAR(50) | Monthly, Quarterly, Half Yearly, Yearly |
| Description | NVARCHAR(255) | Optional notes |
| CreatedAt | DATETIME | Auto-set |

**Relationships:** One plan has many Customers and many PaymentSchedules.

### PaymentSchedule

| Column | Type | Notes |
|--------|------|-------|
| ScheduleID | CHAR(10) PK | Random 10-char ID |
| PlanID | CHAR(10) FK | Parent plan |
| PaymentDescription | NVARCHAR(250) | e.g. "Installment", "Token" |
| InstallmentNo | INT | 0 = Token, 1+ = regular installments |
| DueDate | DATETIME | When this installment is due |
| Amount | DECIMAL(18,2) | Installment amount (PKR) |
| AmountUSD | DECIMAL(18,2) | Installment amount (USD) |
| SurchargeApplied | BIT | Whether late surcharge applies |
| SurchargeRate | DECIMAL(18,2) | Stored as decimal (e.g. 0.05 = 5%). Form sends percentage, controller divides by 100 |
| Description | NVARCHAR(255) | Optional notes |

**Relationships:** One schedule has many Payments.

---

## Creating a Payment Plan (with auto-generated schedules)

The creation form sends a single JSON object containing both plan data and schedule parameters. The controller then:

1. Validates project, total amount (PKR or USD), and exchange rate.
2. **Auto-calculates** if only one currency is provided (converts using exchange rate).
3. Computes installment amounts:
   - `distributableAmount = TotalAmount - TokenAmount`
   - `baseInstallmentAmount = distributableAmount / totalInstallments` (rounded)
   - Last installment absorbs any rounding remainder.
4. **Validates** that Token + all installments do not exceed total plan amount.
5. Creates the PaymentPlan record.
6. Creates PaymentSchedule records:
   - **Token (Installment #0):** Optional, due on the first due date, surcharge disabled.
   - **Regular installments (#1 .. N):** Due dates spaced by frequency.

### Due Date Calculation

| Frequency | Interval |
|-----------|----------|
| Monthly | +1 month per installment |
| Quarterly | +3 months |
| Half Yearly | +6 months |
| Yearly | +12 months |

---

## Adding / Editing / Deleting Individual Schedules

After a plan is created, individual installments can be managed:

- **Create:** Add a new installment to an existing plan. Amount must not cause the sum of all installments to exceed `TotalAmount`.
- **Edit:** Update installment details. Same total-amount guard applies. Surcharge rate is sent as a percentage from the form and divided by 100 before storage.
- **Delete:** Requires Admin permission. Removes the installment and logs the action.

---

## Pages & Actions

| Action | Permission | Description |
|--------|-----------|-------------|
| `PaymentPlans` | Read | List all plans with project, customer count, schedule count |
| `CreatePaymentPlan` | Edit | JSON-based plan + schedule creation |
| `PaymentSchedule(planId)` | Read | View all installments for a plan |
| `CreatePaymentSchedule(planId)` | Edit | Add a single installment to a plan |
| `EditPaymentSchedule` | Edit | Update an installment |
| `DeletePaymentSchedule` | Admin | Remove an installment |
| `Schedules` | Read | Global list of all schedules across all plans |

---

## Exchange Rate

Pulled from `Configurations` table key `Currency:USDToPKR`. If missing or invalid, defaults to `1`. Used for PKR/USD auto-conversion on both plan and schedule levels.
