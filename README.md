# LoanFlow – Workflow & Rule-Driven Loan Approval System

LoanFlow is a **workflow-oriented loan approval system** built with **Camunda BPMN**, **DMN decision tables**, and **external task workers**.  
It demonstrates how to combine a **workflow engine** and a **rule engine** to build flexible, auditable, and scalable business processes.

---

## Architecture Overview

**Core concepts used:**

- **BPMN (Camunda 7)** – process orchestration
- **DMN** – risk evaluation rules (decision tables)
- **External Tasks** – microservice-style workers
- **User Tasks** – manual review by loan officers
- **REST APIs** – system integration and custom UI support

### High-level flow

1. Loan application is submitted
2. Application is registered
3. Risk is evaluated using DMN
4. Decision gateway routes to:
   - Automatic approval
   - Manual review
   - Rejection
5. Applicant is notified

---

## BPMN Process

**Process ID:** `loan_approval`

### Steps

1. Register Application (external task)
2. Automatic Risk Check (DMN)
3. Exclusive Gateway – Risk Acceptable?
4. Manual Review (user task)
5. Approve Loan (external task)
6. Reject Loan (external task)
7. Notify Applicant (external task)

---

## DMN – Risk Evaluation

### Decisions

#### dtiBand
Debt-to-Income ratio:

DTI = existingDebt / monthlyIncome

Outputs:
- LOW
- MEDIUM
- HIGH
- VERY_HIGH
- INVALID

---

#### creditBand

| Credit Score | Band |
|-------------|------|
| < 550 | BAD |
| 550–650 | FAIR |
| 650–750 | GOOD |
| ≥ 750 | EXCELLENT |
| otherwise | UNKNOWN |

---

#### FinalRisk

Decision output:

```json
{
  "riskScore": 20,
  "manualReview": false,
  "reason": "Low Risk"
}
```

---

## Process Variables

### Input Variables

- applicantName
- email
- amount
- monthlyIncome
- existingDebt
- creditScore
- currency

---

### Output Variable

`riskResult` (stored as object)

---

## External Workers

| Topic | Responsibility |
|----|----|
| loan_register | Register loan |
| loan_approve | Approve loan |
| loan_reject | Reject loan |
| notify_applicant | Notify applicant |

---

## Manual Review

- Candidate group: `loan_officers`
- Triggered when `manualReview = true`
- Can be handled via Tasklist or custom UI

---

## API Example

The application exposes a simple HTTP endpoint that accepts a loan application and then starts the workflow + evaluates the DMN behind the scenes.

### Create a Loan Application

**Endpoint**

```http
POST /api/loan
Content-Type: application/json
```

**Payload**

```json
{
  "applicantName": "Andro",
  "email": "andro@test.com",
  "amount": 10000,
  "monthlyIncome": 2500,
  "existingDebt": 500,
  "creditScore": 720,
  "currency": "EUR"
}
```

**cURL**

```bash
curl -X POST "http://localhost:8080/api/loan" \
  -H "Content-Type: application/json" \
  -d '{
    "applicantName": "Andro",
    "email": "andro@test.com",
    "amount": 10000,
    "monthlyIncome": 2500,
    "existingDebt": 500,
    "creditScore": 720,
    "currency": "EUR"
  }'
```

---

## Docker Setup

```yaml
version: "3.8"

services:
  camunda:
    image: camunda/camunda-bpm-platform:run-7.18.0
    ports:
      - "8080:8080"
    environment:
      CAMUNDA_BPM_ADMIN_USER_ID: demo
      CAMUNDA_BPM_ADMIN_USER_PASSWORD: demo
```

---

## Status

✅ Workflow engine  
✅ Rule engine  
✅ External workers  
✅ Manual review  

Production-ready foundation.
