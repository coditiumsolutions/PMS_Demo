# Refund Module

## Overview

The Refund module allows staff to initiate a refund against one or more payments made by a customer. Refunds go through an approval workflow; once approved the selected payments are permanently deleted from the system.

**Controller:** `RefundController.cs`
**Model:** `Refund.cs`
**Views:** `Views/Refund/`

---

## Database Table: `Refund`

| Column | Type | Notes |
|--------|------|-------|
| RefundID | CHAR(10) PK | Auto-generated: `RFD` + 7-digit sequence (e.g. `RFD0000001`) |
| CustomerID | CHAR(10) FK | Customer being refunded |
| RefundType | NVARCHAR(50) | `Full` or `Partial` |
| PaidAmount | DECIMAL(18,2) | Sum of the selected payment amounts |
| DeductionAmount | DECIMAL(18,2) | Processing fee or other deductions (default 0) |
| RefundedAmount | DECIMAL(18,2) | `PaidAmount - DeductionAmount` (auto-calculated) |
| Reason | NVARCHAR(255) | Why the refund is requested |
| WorkflowStatus | NVARCHAR(100) | `Initiated`, `Approved`, or `Declined` |
| SelectedPaymentIDs | NVARCHAR(MAX) | JSON array of PaymentIDs e.g. `["PAY0000001","PAY0000002"]` |
| CreatedBy | CHAR(10) FK | User who initiated |
| CreatedAt | DATETIME | Auto-set |
| ApprovedBy | CHAR(10) FK | User who approved or declined |
| ApprovedAt | DATETIME | Timestamp of approval/decline |
| Notes | NVARCHAR(500) | Approver's notes |

---

## Workflow

```
Initiated  -->  Approved   (payments deleted, refund finalised)
           -->  Declined   (no changes to payments)
```

- Only refunds in `Initiated` status can be approved or declined.
- Once approved or declined, the status is final.

---

## Key Business Rules

1. **Payment Selection:** User searches a customer (AJAX), sees all their payments that have not already been refunded. They pick one or more payments to include in the refund.
2. **Already-Refunded Filter:** Payments linked to any previously `Approved` refund are excluded from the selectable list (tracked via `SelectedPaymentIDs` JSON).
3. **Amount Calculation:** `RefundedAmount = PaidAmount - DeductionAmount`. If negative, clamped to 0.
4. **Approval Deletes Payments:** When a refund is approved, all payments in `SelectedPaymentIDs` are permanently removed from the `Payments` table. Each deletion is individually logged in ActivityLog.
5. **Decline Is Safe:** Declining a refund only changes the status; no payments are affected.
6. **Approval Requires Admin:** Only users with `Admin` permission on the Refund module can approve. Declining requires `Edit`.

---

## Pages & Actions

| Action | Permission | Description |
|--------|-----------|-------------|
| `Index` | Read | List refunds with workflow and customer filters; shows Initiated/Approved/Declined counts |
| `Create` | Edit | Initiate a new refund: search customer, select payments, enter deduction and reason |
| `Details(id)` | Read | View refund details including the list of selected payments |
| `Approve(refundId)` | Admin | Approve refund and delete selected payments |
| `Decline(refundId)` | Edit | Decline refund with optional notes |
| `SearchCustomerForRefund` (AJAX) | - | Returns customer info and eligible (non-refunded) payments |

---

## Activity Logging

- **On initiation:** Logs refund ID, customer ID, and paid amount.
- **On approval:** Logs the approval plus one entry per deleted payment (amount and payment ID).
- **On decline:** Logs the decline.

All entries use `RefType = "Refund"` or `"Payment"` with the respective ID.

---

## Config-Driven Values

- **Workflow Statuses:** Read from `Configurations` table key `refundworkflow` (comma-separated). Fallback: `Initiated, Approved, Declined`.
