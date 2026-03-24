# Digital Stokvel Banking - Requirements Specification

**Version:** 1.0  
**Status:** Draft  
**Date:** March 2026  
**Based on:** Product Requirements Document v1.0

---

## Table of Contents

1. [Introduction](#introduction)
2. [EARS Notation Reference](#ears-notation-reference)
3. [Functional Requirements](#functional-requirements)
4. [Non-Functional Requirements](#non-functional-requirements)
5. [User Stories](#user-stories)
6. [Acceptance Criteria](#acceptance-criteria)

---

## Introduction

This document provides structured requirements for the Digital Stokvel Banking platform using EARS (Easy Approach to Requirements Syntax) notation. The platform aims to bring South Africa's R50B informal savings economy into the formal banking system.

### System Context

- **Target Users:** 11M+ South Africans participating in stokvels
- **Platforms:** Android, iOS, USSD, Web
- **MVP Timeline:** 3 months
- **Revenue Model:** Interest margin on pooled deposits

---

## EARS Notation Reference

| Pattern | Structure | Example |
|---------|-----------|---------|
| **Ubiquitous** | The system shall [requirement] | The system shall encrypt all stored data |
| **Event-driven** | WHEN [trigger] the system shall [requirement] | WHEN a payment is received the system shall send a confirmation |
| **Unwanted behavior** | IF [condition] THEN the system shall [requirement] | IF authentication fails THEN the system shall lock the account |
| **State-driven** | WHILE [state] the system shall [requirement] | WHILE processing a transaction the system shall display a loading indicator |
| **Optional** | WHERE [feature] the system shall [requirement] | WHERE USSD is used the system shall limit menu depth to 3 levels |

---

## Functional Requirements

### FR-01: Group Creation & Management [P0]

#### FR-01.1: Create Stokvel Group
**EARS:** WHEN a bank customer initiates group creation, the system shall allow them to specify group name, description, group type (rotating payout, savings pot, investment club), and contribution rules.

**User Story:** US-01  
**Priority:** P0 (Must Have for MVP)

#### FR-01.2: Invite Members
**EARS:** WHEN a chairperson invites members, the system shall support invitation via phone number, share link, or QR code.

**User Story:** US-01  
**Priority:** P0

#### FR-01.3: Assign Group Roles
**EARS:** WHEN setting up a group, the system shall allow the creator to assign roles including Chairperson, Treasurer, and Secretary.

**User Story:** US-01  
**Priority:** P0

#### FR-01.4: Member Onboarding
**EARS:** IF an invited member does not have a bank account, THEN the system shall provide an embedded onboarding flow to open an account.

**User Story:** US-02  
**Priority:** P0

#### FR-01.5: Set Contribution Rules
**EARS:** WHEN creating a group, the system shall allow configuration of contribution amount, frequency (weekly/monthly), and payout schedule.

**User Story:** US-01  
**Priority:** P0

---

### FR-02: Digital Group Wallet [P0]

#### FR-02.1: Pooled Savings Account
**EARS:** The system shall hold all member contributions in a dedicated, bank-held group savings account earning tiered interest.

**User Story:** US-03  
**Priority:** P0

#### FR-02.2: Interest Calculation
**EARS:** The system shall calculate interest using daily compounding with monthly capitalisation to the group wallet.

**User Story:** US-03  
**Priority:** P0

#### FR-02.3: Real-Time Balance Display
**EARS:** The system shall display the real-time group balance to all members with role-based viewing permissions.

**User Story:** US-04  
**Priority:** P0

#### FR-02.4: Contribution History
**EARS:** The system shall maintain a full, immutable ledger showing who paid, when, and how much.

**User Story:** US-04  
**Priority:** P0

#### FR-02.5: Withdrawal Governance
**EARS:** IF a partial withdrawal is requested, THEN the system shall require quorum approval before execution.

**User Story:** US-05  
**Priority:** P0

#### FR-02.6: Tiered Interest Rates
**EARS:** The system shall apply interest rates based on balance tiers: R0-R10K at 3.5%, R10K-R50K at 4.5%, and R50K+ at 5.5% per annum.

**User Story:** US-03  
**Priority:** P0

---

### FR-03: Contribution Collection [P0]

#### FR-03.1: One-Tap Payment
**EARS:** WHERE the mobile app is used, the system shall enable one-tap payment from a linked bank account.

**User Story:** US-06  
**Priority:** P0

#### FR-03.2: Recurring Payments
**EARS:** The system shall support debit order/recurring payment options for automatic contributions.

**User Story:** US-07  
**Priority:** P0

#### FR-03.3: USSD Payment
**EARS:** WHERE USSD is used, the system shall support contribution payments via dial-in (e.g., *120*STOKVEL#).

**User Story:** US-08  
**Priority:** P0

#### FR-03.4: Payment Reminders
**EARS:** The system shall send payment reminders 3 days and 1 day before the due date via push notification, SMS, and USSD.

**User Story:** US-06  
**Priority:** P0

#### FR-03.5: Payment Confirmation
**EARS:** WHEN a contribution is received, the system shall issue a receipt to the member and log the transaction on the group ledger.

**User Story:** US-06  
**Priority:** P0

#### FR-03.6: PIN Authentication
**EARS:** WHERE USSD is used, the system shall authenticate all transactions using the customer's existing bank PIN.

**User Story:** US-08  
**Priority:** P0

---

### FR-04: Payout Engine [P0]

#### FR-04.1: Rotating Payout
**EARS:** WHERE rotating payout is configured, the system shall automatically disburse funds to the designated member each cycle.

**User Story:** US-09  
**Priority:** P0

#### FR-04.2: Year-End Pot Payout
**EARS:** WHERE year-end pot is configured, the system shall disburse the full balance proportionally to all members at year-end.

**User Story:** US-09  
**Priority:** P0

#### FR-04.3: Dual Approval Process
**EARS:** WHEN a payout is initiated, the system shall require the Chairperson to initiate and the Treasurer to confirm before execution.

**User Story:** US-10  
**Priority:** P0

#### FR-04.4: Instant EFT Disbursement
**EARS:** The system shall execute payouts via instant EFT to member bank accounts.

**User Story:** US-09  
**Priority:** P0

#### FR-04.5: Payout Notifications
**EARS:** WHEN a payout is executed, the system shall send notifications to all group members for transparency.

**User Story:** US-09  
**Priority:** P0

---

### FR-05: Group Governance & Dispute Resolution [P0]

#### FR-05.1: Group Constitution Builder
**EARS:** WHEN creating a group, the system shall provide a constitution builder to define rules for missed payments, late fees, and member removal.

**User Story:** US-11  
**Priority:** P0

#### FR-05.2: In-App Voting
**EARS:** WHERE major decisions are required (e.g., change contribution amount, remove member), the system shall facilitate in-app voting.

**User Story:** US-12  
**Priority:** P0

#### FR-05.3: Missed Payment Escalation
**EARS:** IF a payment is missed, THEN the system shall send an automated notice, apply a grace period, and notify the Chairperson.

**User Story:** US-13  
**Priority:** P0

#### FR-05.4: Dispute Flagging
**EARS:** WHEN a member raises a dispute, the system shall log the dispute and provide a mechanism for bank mediation if unresolved.

**User Story:** US-14  
**Priority:** P0

---

### FR-06: Multilingual Interface [P0]

#### FR-06.1: Language Support
**EARS:** The system shall provide full UI support in English, isiZulu, Sesotho, Xhosa, and Afrikaans.

**User Story:** US-15  
**Priority:** P0

#### FR-06.2: USSD Language Support
**EARS:** WHERE USSD is used, the system shall provide menu navigation in all 5 supported languages.

**User Story:** US-15  
**Priority:** P0

#### FR-06.3: Language Selection
**EARS:** The system shall allow language selection at onboarding and provide the ability to change language in settings.

**User Story:** US-15  
**Priority:** P0

---

### FR-07: Credit Profile Builder [P1]

#### FR-07.1: Credit Bureau Opt-In
**EARS:** WHERE credit profile building is enabled, the system shall require explicit member consent before sharing contribution history with credit bureaus.

**User Story:** US-16  
**Priority:** P1 (Phase 2)

#### FR-07.2: Stokvel Score Generation
**EARS:** IF a member opts in, THEN the system shall generate a 'Stokvel Score' based on consistent on-time payments.

**User Story:** US-16  
**Priority:** P1

#### FR-07.3: Score Visibility
**EARS:** The system shall display the Stokvel Score in the banking app and make it referenceable for loan applications.

**User Story:** US-16  
**Priority:** P1

#### FR-07.4: Pre-Qualification Offers
**EARS:** WHEN Stokvel Score thresholds are met, the system shall trigger bank pre-qualification offers.

**User Story:** US-17  
**Priority:** P1

---

### FR-08: Financial Wellness Nudges [P1]

#### FR-08.1: Post-Payout Education
**EARS:** WHEN a payout is completed, the system shall display contextual financial education messages showing interest earned and growth opportunities.

**User Story:** US-18  
**Priority:** P1 (Phase 2)

#### FR-08.2: Contribution Streak Milestones
**EARS:** WHEN a member achieves contribution milestones (e.g., 12 months of perfect payments), the system shall display recognition messages about credit profile building.

**User Story:** US-18  
**Priority:** P1

#### FR-08.3: Annual Summary Report
**EARS:** The system shall generate an annual stokvel summary report showing contributions made, interest earned, and payouts received.

**User Story:** US-18  
**Priority:** P1

---

### FR-09: Stokvel-to-Investment Bridge [P2]

#### FR-09.1: Investment Voting
**EARS:** WHERE investment features are enabled, the system shall allow groups to vote to move a portion of savings into bank-offered unit trusts or fixed deposits.

**User Story:** US-19  
**Priority:** P2 (Longer-term)

#### FR-09.2: Group Investment Holdings
**EARS:** The system shall hold investments in the group name with performance visibility to all members.

**User Story:** US-19  
**Priority:** P2

---

### FR-10: Insurance Integration [P2]

#### FR-10.1: Burial Society Product
**EARS:** WHERE insurance features are enabled, the system shall allow groups to opt into bank-facilitated burial society products.

**User Story:** US-20  
**Priority:** P2 (Longer-term)

#### FR-10.2: Premium Deduction
**EARS:** IF insurance is active, THEN the system shall deduct monthly premiums from the group wallet.

**User Story:** US-20  
**Priority:** P2

#### FR-10.3: Payout to Beneficiaries
**EARS:** WHEN a claim is approved, the system shall disburse the payout to the family of the deceased member.

**User Story:** US-20  
**Priority:** P2

---

## Non-Functional Requirements

### NFR-01: Platform Support

#### NFR-01.1: Android Support
**EARS:** The system shall provide full feature set (F-01 to F-06) on Android devices using Material Design 3.

**Priority:** P0

#### NFR-01.2: iOS Support
**EARS:** The system shall provide full feature set (F-01 to F-06) on iOS devices with feature parity to Android.

**Priority:** P0

#### NFR-01.3: USSD Support
**EARS:** WHERE USSD is used, the system shall support contribution, balance check, and payout notification on any mobile network and handset.

**Priority:** P0

#### NFR-01.4: USSD Menu Depth
**EARS:** WHERE USSD is used, the system shall limit menu depth to a maximum of 3 levels.

**Priority:** P0

#### NFR-01.5: USSD Session Timeout
**EARS:** WHERE USSD is used, the system shall maintain session state for 120 seconds.

**Priority:** P0

#### NFR-01.6: Web Dashboard
**EARS:** The system shall provide a web-based Chairperson admin dashboard for member management, contribution tracking, and payout approvals.

**Priority:** P0

#### NFR-01.7: Offline Tolerance
**EARS:** WHERE the mobile app is used, the system shall support offline-tolerant architecture for low-connectivity areas.

**Priority:** P0

---

### NFR-02: Performance

#### NFR-02.1: Response Time
**EARS:** The system shall respond to user interactions within 2 seconds under normal load conditions.

**Priority:** P0

#### NFR-02.2: Balance Display Latency
**EARS:** The system shall display real-time group balance with maximum latency of 5 seconds.

**Priority:** P0

#### NFR-02.3: Transaction Processing
**EARS:** The system shall process contributions and payouts within 30 seconds of confirmation.

**Priority:** P0

---

### NFR-03: Security & Compliance

#### NFR-03.1: POPIA Compliance
**EARS:** The system shall comply with the Protection of Personal Information Act (POPIA) for all personal data handling.

**Priority:** P0

#### NFR-03.2: Explicit Consent
**EARS:** WHERE credit bureau reporting or marketing contact is involved, the system shall obtain explicit consent from members.

**Priority:** P0

#### NFR-03.3: FICA/KYC Verification
**EARS:** The system shall verify that all group members are verified bank customers before allowing group participation.

**Priority:** P0

#### NFR-03.4: Simplified Onboarding
**EARS:** IF a non-customer joins a group, THEN the system shall provide simplified onboarding (ID + selfie) before participation.

**Priority:** P0

#### NFR-03.5: AML Monitoring
**EARS:** The system shall flag groups with single deposits >R20,000 or total monthly inflows >R100,000 for AML review.

**Priority:** P0

#### NFR-03.6: Data Encryption
**EARS:** The system shall encrypt all data in transit using TLS 1.3 and at rest using AES-256.

**Priority:** P0

#### NFR-03.7: Data Residency
**EARS:** The system shall store all data on South Africa-domiciled infrastructure in compliance with SARB requirements.

**Priority:** P0

#### NFR-03.8: Immutable Ledger
**EARS:** The system shall maintain an immutable digital ledger for all contributions and payouts.

**Priority:** P0

---

### NFR-04: Reliability & Availability

#### NFR-04.1: System Uptime
**EARS:** The system shall maintain 99.9% uptime during business hours (06:00-22:00 SAST).

**Priority:** P0

#### NFR-04.2: USSD Session Persistence
**EARS:** WHERE USSD is used, the system shall persist session state to handle network interruptions.

**Priority:** P0

#### NFR-04.3: Fallback SMS Confirmation
**EARS:** IF USSD session fails, THEN the system shall provide fallback SMS confirmation for all transactions.

**Priority:** P0

---

### NFR-05: Usability

#### NFR-05.1: Cultural Language
**EARS:** The system shall use warm, communal language including terms like 'stokvel', 'umuntu', 'ilungu' rather than clinical banking terms.

**Priority:** P0

#### NFR-05.2: Encouraging Error Messages
**EARS:** WHEN an error occurs, the system shall display encouraging messages (e.g., 'Your payment didn't go through this time — let's try again').

**Priority:** P0

#### NFR-05.3: Trust Signals
**EARS:** The system shall display bank logo and FSCA regulation badge on the group wallet screen.

**Priority:** P0

#### NFR-05.4: Money Protection Disclosure
**EARS:** WHEN a first deposit is made, the system shall show 'Your money is protected' disclosure on the wallet home screen.

**Priority:** P0

---

### NFR-06: Scalability

#### NFR-06.1: Concurrent Users
**EARS:** The system shall support 10,000 concurrent users in MVP with horizontal scaling capability.

**Priority:** P0

#### NFR-06.2: Group Scaling
**EARS:** The system shall support 10,000 active groups by 12-month target with performance degradation <10%.

**Priority:** P0

---

### NFR-07: Data Ownership & Export

#### NFR-07.1: Group Data Ownership
**EARS:** The system shall treat contribution history, member roster, and group ledger as owned by the group with the bank as custodian.

**Priority:** P0

#### NFR-07.2: Data Export
**EARS:** The system shall allow members to export their data at any time in PDF format.

**Priority:** P0

#### NFR-07.3: Ledger Export for AGM
**EARS:** WHEN requested, the system shall export the group ledger as a branded PDF for stokvel AGM records.

**Priority:** P0

---

## User Stories

### US-01: Create Stokvel Group (Chairperson)
**As a** Chairperson  
**I want to** create a digital stokvel group with custom rules  
**So that** I can manage my group's savings formally and earn interest  

**Linked Requirements:** FR-01.1, FR-01.2, FR-01.3, FR-01.5

**Acceptance Criteria:** See AC-01

---

### US-02: Join Stokvel Group (Member)
**As a** prospective member  
**I want to** join a stokvel group via invitation  
**So that** I can participate in group savings digitally  

**Linked Requirements:** FR-01.4

**Acceptance Criteria:** See AC-02

---

### US-03: Earn Interest on Group Savings
**As a** stokvel member  
**I want to** earn interest on our pooled savings  
**So that** our money grows instead of sitting idle  

**Linked Requirements:** FR-02.1, FR-02.2, FR-02.6

**Acceptance Criteria:** See AC-03

---

### US-04: View Group Finances
**As a** stokvel member  
**I want to** view the group balance and contribution history  
**So that** I have transparency into our collective savings  

**Linked Requirements:** FR-02.3, FR-02.4

**Acceptance Criteria:** See AC-04

---

### US-05: Approve Withdrawals (Chairperson)
**As a** Chairperson  
**I want to** require quorum approval for withdrawals  
**So that** no one can unilaterally withdraw group funds  

**Linked Requirements:** FR-02.5

**Acceptance Criteria:** See AC-05

---

### US-06: Make Contributions via Mobile App
**As a** stokvel member with a smartphone  
**I want to** contribute with one tap from my bank account  
**So that** I can easily fulfill my monthly obligation  

**Linked Requirements:** FR-03.1, FR-03.4, FR-03.5

**Acceptance Criteria:** See AC-06

---

### US-07: Set Up Automatic Contributions
**As a** stokvel member  
**I want to** set up recurring debit orders  
**So that** I never miss a payment  

**Linked Requirements:** FR-03.2

**Acceptance Criteria:** See AC-07

---

### US-08: Make Contributions via USSD (Feature Phone User)
**As a** feature phone user  
**I want to** contribute via USSD  
**So that** I can participate without a smartphone  

**Linked Requirements:** FR-03.3, FR-03.6, NFR-01.3, NFR-01.4, NFR-01.5

**Acceptance Criteria:** See AC-08

---

### US-09: Receive Payouts
**As a** stokvel member  
**I want to** receive my payout automatically via EFT  
**So that** I don't have to handle large amounts of cash  

**Linked Requirements:** FR-04.1, FR-04.2, FR-04.4, FR-04.5

**Acceptance Criteria:** See AC-09

---

### US-10: Approve Payouts (Treasurer)
**As a** Treasurer  
**I want to** confirm payouts initiated by the Chairperson  
**So that** there is dual control over disbursements  

**Linked Requirements:** FR-04.3

**Acceptance Criteria:** See AC-10

---

### US-11: Define Group Rules (Chairperson)
**As a** Chairperson  
**I want to** define rules for missed payments and member removal  
**So that** the group has a clear constitution  

**Linked Requirements:** FR-05.1

**Acceptance Criteria:** See AC-11

---

### US-12: Vote on Major Decisions
**As a** stokvel member  
**I want to** vote on major group decisions  
**So that** we make collective choices democratically  

**Linked Requirements:** FR-05.2

**Acceptance Criteria:** See AC-12

---

### US-13: Handle Missed Payments (Chairperson)
**As a** Chairperson  
**I want to** be notified when members miss payments  
**So that** I can follow up appropriately  

**Linked Requirements:** FR-05.3

**Acceptance Criteria:** See AC-13

---

### US-14: Raise a Dispute (Member)
**As a** stokvel member  
**I want to** flag a dispute in the system  
**So that** issues are formally recorded and can be mediated  

**Linked Requirements:** FR-05.4

**Acceptance Criteria:** See AC-14

---

### US-15: Use System in My Language
**As a** stokvel participant  
**I want to** use the system in my preferred language  
**So that** I can understand and trust the platform  

**Linked Requirements:** FR-06.1, FR-06.2, FR-06.3

**Acceptance Criteria:** See AC-15

---

### US-16: Build Credit Profile (Member) [P1]
**As a** stokvel member with no credit history  
**I want to** have my contributions reported to credit bureaus  
**So that** I can build a formal credit profile  

**Linked Requirements:** FR-07.1, FR-07.2, FR-07.3

**Acceptance Criteria:** See AC-16

---

### US-17: Receive Pre-Qualified Loan Offers (Member) [P1]
**As a** stokvel member with a good Stokvel Score  
**I want to** receive pre-qualified loan offers  
**So that** I can access credit based on my savings behavior  

**Linked Requirements:** FR-07.4

**Acceptance Criteria:** See AC-17

---

### US-18: Learn About Financial Growth (Member) [P1]
**As a** stokvel member  
**I want to** receive financial wellness tips  
**So that** I can make informed decisions about my money  

**Linked Requirements:** FR-08.1, FR-08.2, FR-08.3

**Acceptance Criteria:** See AC-18

---

### US-19: Invest Group Savings [P2]
**As a** stokvel group  
**I want to** invest a portion of our savings  
**So that** we can earn higher returns collectively  

**Linked Requirements:** FR-09.1, FR-09.2

**Acceptance Criteria:** See AC-19

---

### US-20: Get Burial Society Cover [P2]
**As a** stokvel group  
**I want to** add funeral/burial insurance  
**So that** we can support families of deceased members  

**Linked Requirements:** FR-10.1, FR-10.2, FR-10.3

**Acceptance Criteria:** See AC-20

---

## Acceptance Criteria

### AC-01: Create Stokvel Group
**Given** I am a verified bank customer  
**When** I create a new stokvel group  
**Then** I should be able to:
- Enter a group name (min 3, max 50 characters)
- Add a description (optional, max 200 characters)
- Select group type from: Rotating Payout, Savings Pot, Investment Club
- Set contribution amount (min R50, max R10,000)
- Set contribution frequency: Weekly or Monthly
- Set payout schedule based on group type
- Assign myself as Chairperson
- Invite at least 2 other members

**And** the system should create a dedicated group savings account  
**And** I should receive confirmation with group details and unique group ID

---

### AC-02: Join Stokvel Group
**Given** I have received an invitation via phone number, link, or QR code  
**When** I accept the invitation  
**Then** 
- **If** I am an existing bank customer, I should be added to the group immediately
- **If** I am not a bank customer, I should be directed to simplified onboarding (ID + selfie)
- **After** successful verification, I should see group details, contribution rules, and current balance
- I should receive a welcome message in my preferred language

---

### AC-03: Earn Interest on Group Savings
**Given** my stokvel group has pooled savings in the group wallet  
**When** interest is calculated  
**Then** 
- Interest should be calculated daily using the formula: (Balance × Rate) / 365
- Interest should be compounded daily
- Interest should be capitalised monthly to the group wallet
- The correct tier rate should apply:
  - R0-R10,000: 3.5% per annum
  - R10,001-R50,000: 4.5% per annum
  - R50,001+: 5.5% per annum
- Interest earned should be visible in the group wallet transaction history

---

### AC-04: View Group Finances
**Given** I am a member of a stokvel group  
**When** I access the group wallet  
**Then** I should see:
- Current group balance (updated in real-time, max 5-second latency)
- Total contributions month-to-date
- Interest earned month-to-date and year-to-date
- Contribution history with columns: Member Name, Date, Amount, Status
- My personal contribution status (paid/pending)
- Next payout date and recipient

**And** I should be able to filter history by date range and member  
**And** I should be able to export the ledger as PDF

---

### AC-05: Approve Withdrawals
**Given** I am a Chairperson  
**When** I request a partial withdrawal (not a scheduled payout)  
**Then** 
- The system should require quorum approval (default: 50% + 1 member)
- A notification should be sent to all members
- Members should have 48 hours to vote (approve/reject)
- **If** quorum is reached and majority approves, withdrawal should execute
- **If** quorum is not reached or majority rejects, withdrawal should be denied
- All members should receive notification of the outcome

---

### AC-06: Make Contributions via Mobile App
**Given** I am a stokvel member with the mobile app  
**When** my contribution is due  
**Then** 
- I should receive reminders at 3 days before and 1 day before due date via push notification
- I should see a one-tap "Pay Now" button on the home screen
- **When** I tap "Pay Now", I should see contribution amount, group name, and payment confirmation screen
- **After** confirming, payment should deduct from my linked bank account within 30 seconds
- I should receive a payment receipt showing: date, amount, group name, receipt number
- The payment should appear in the group ledger within 30 seconds
- All group members should see the updated contribution status

---

### AC-07: Set Up Automatic Contributions
**Given** I am a stokvel member  
**When** I set up a recurring payment  
**Then** 
- I should be able to choose recurring payment on my contribution due date
- I should see a mandate authorization screen with full disclosure
- I should be able to select my linked bank account
- **After** authorization, recurring payments should execute automatically 1 day before due date
- I should receive confirmation SMS/push notification after each automatic payment
- I should be able to view, pause, or cancel the recurring payment at any time

---

### AC-08: Make Contributions via USSD
**Given** I am a feature phone user  
**When** I dial the USSD shortcode (e.g., *120*STOKVEL#)  
**Then** 
- I should see a menu in my selected language (max 3 levels deep)
- I should be able to navigate: 1. Pay Contribution, 2. Check Balance, 3. Help
- **When** I select "Pay Contribution"
  - I should see my groups (if member of multiple)
  - I should see contribution amount and due date
  - I should confirm: "Press 1 for Yes, 2 for No"
  - I should enter my bank PIN for authentication
  - I should receive confirmation message with receipt number via SMS
- Session should persist for 120 seconds
- **If** session times out, I should receive SMS with transaction status

---

### AC-09: Receive Payouts
**Given** a payout is scheduled for my group  
**When** the Chairperson initiates and Treasurer confirms the payout  
**Then** 
- **For Rotating Payout:**
  - The designated member for this cycle should receive full payout amount via instant EFT
  - All members should receive notification: "R5,000 payout sent to [Member Name]"
- **For Year-End Pot:**
  - All members should receive proportional share based on contributions via instant EFT
  - Each member should receive notification: "Year-end payout of R[Amount] sent to your account"
- Payout should appear in bank account within 2 minutes
- Payout receipt should be generated showing: date, amount, group name, payout type

---

### AC-10: Approve Payouts
**Given** I am a Treasurer  
**When** the Chairperson initiates a payout  
**Then** 
- I should receive an immediate notification (push/SMS)
- I should see payout details: amount, recipient(s), payout type, current balance, remaining balance
- I should be able to approve or reject with optional comment
- **If** I approve, payout should execute immediately
- **If** I reject, Chairperson should be notified with my comment
- **If** I do not respond within 24 hours, Chairperson should be prompted to re-initiate

---

### AC-11: Define Group Rules
**Given** I am creating or editing a stokvel group as Chairperson  
**When** I access the constitution builder  
**Then** I should be able to define:
- **Missed Payment Rules:**
  - Grace period (0-7 days)
  - Late fee amount (R0-R200)
  - Number of missed payments before member review (1-3)
- **Member Removal Rules:**
  - Voting threshold (simple majority, 2/3, unanimous)
  - Notice period (1-30 days)
- **Dispute Resolution:**
  - Internal resolution period (3-14 days)
  - Bank mediation trigger (yes/no)

**And** all rules should be displayed to members at time of joining  
**And** changes to rules should require in-app voting

---

### AC-12: Vote on Major Decisions
**Given** a major decision requires group vote  
**When** the Chairperson initiates a vote  
**Then** 
- All members should receive notification with vote details
- Members should have 48 hours to vote
- Vote options should be clearly displayed (e.g., Approve/Reject or multiple choices)
- Members should be able to change their vote before deadline
- All members should see live vote tally (votes, not individual choices) if transparency is enabled
- **When** voting closes:
  - Results should be displayed to all members
  - **If** threshold is met, decision should be implemented automatically where applicable
  - Vote should be logged in group history as immutable record

---

### AC-13: Handle Missed Payments
**Given** a member's payment is overdue  
**When** the due date passes  
**Then** 
- **Day 0 (due date):**
  - Member receives automated notice: "Your payment of R[Amount] is now overdue"
- **Day 1 (grace period start):**
  - Chairperson receives notification: "[Member Name] has missed payment – grace period active"
- **Day X (grace period end per constitution):**
  - **If** still unpaid, late fee is applied (per constitution)
  - Member receives notice: "Late fee of R[Amount] applied"
  - Chairperson receives escalation: "[Member Name] still unpaid – late fee applied"
- **After 3 missed payments (or per constitution):**
  - Chairperson can initiate member review or removal vote

---

### AC-14: Raise a Dispute
**Given** I am a stokvel member with a concern  
**When** I raise a dispute  
**Then** 
- I should be able to select dispute type: Payment Issue, Payout Issue, Member Conduct, Other
- I should be able to provide a description (min 10, max 500 characters)
- I should be able to attach evidence (optional, image or PDF, max 5MB)
- Dispute should be logged with unique ID and timestamp
- Chairperson and Treasurer should be notified immediately
- **If** resolved by group, Chairperson can close dispute with resolution notes
- **If** unresolved after internal resolution period (per constitution), I should be able to escalate to bank mediation
- All dispute activity should be logged in immutable group history

---

### AC-15: Use System in My Language
**Given** I am a stokvel participant  
**When** I first access the system  
**Then** 
- I should be prompted to select my preferred language from: English, isiZulu, Sesotho, Xhosa, Afrikaans
- All UI elements should display in my selected language including:
  - Navigation menus
  - Button labels
  - Form fields and validation messages
  - Notifications and alerts
  - Error messages
  - Help content
- **For USSD:**
  - All menu options should display in my selected language
  - Menu depth should not exceed 3 levels
  - Confirmation prompts should be clear (e.g., "1=Yebo, 2=Cha" for isiZulu)
- I should be able to change my language at any time in settings
- Language preference should persist across sessions and devices

---

### AC-16: Build Credit Profile [P1]
**Given** I am a stokvel member  
**When** I access the Credit Profile Builder  
**Then** 
- I should see a clear explanation of what data will be shared with credit bureaus
- I should see a clear explanation of how this affects my credit profile
- I should be able to opt in with explicit consent checkbox
- **After** opting in:
  - My contribution history should be reported monthly to the credit bureau
  - My Stokvel Score should be calculated based on:
    - On-time payment percentage (0-100%)
    - Contribution consistency (months active)
    - Contribution amount relative to group average
  - My Stokvel Score should be displayed in the app (0-100 scale)
  - I should see my contribution streak (consecutive on-time payments)
- I should be able to opt out at any time
- **If** I opt out, no future data should be shared but historical data remains with bureau

---

### AC-17: Receive Pre-Qualified Loan Offers [P1]
**Given** I have a Stokvel Score of 75+ and 12+ months of contribution history  
**When** I access the app  
**Then** 
- I should see a banner: "You're pre-qualified for a R[Amount] personal loan"
- **When** I tap the banner:
  - I should see loan details: amount, interest rate, term, monthly payment
  - I should see that this is based on my Stokvel Score and contribution history
  - I should be able to apply with simplified process (no additional documentation required)
  - Application should be prioritized for fast-track approval

---

### AC-18: Learn About Financial Growth [P1]
**Given** I am a stokvel member  
**When** financial wellness events occur  
**Then** 
- **After a payout:**
  - I should see: "Your R3,200 payout earned R48 in interest this year — here's how to grow it more"
  - I should be able to tap to see savings product options
- **After reaching a milestone:**
  - I should see: "12 months of perfect payments — your credit profile is building!"
  - I should see impact on Stokvel Score
- **At year-end:**
  - I should receive annual summary report with:
    - Total contributions made
    - Interest earned
    - Payouts received
    - Stokvel Score (if opted in)
    - Contribution streak
  - Report should be shareable and exportable as PDF

---

### AC-19: Invest Group Savings [P2]
**Given** my stokvel group has savings of R50,000+  
**When** the group discusses investment  
**Then** 
- Chairperson should be able to initiate investment proposal
- Proposal should specify: investment product (unit trust/fixed deposit), amount, term
- All members should receive proposal with product details and risk disclosure
- Members should vote on proposal (48-hour window)
- **If** approved by majority:
  - Investment should be created in group name
  - Amount should be transferred from group wallet to investment account
  - All members should see investment performance in group dashboard
  - Investment returns should be added to group wallet at term end or monthly (depending on product)

---

### AC-20: Get Burial Society Cover [P2]
**Given** my stokvel group wants burial insurance  
**When** we access insurance options  
**Then** 
- Chairperson should see available burial society products
- Product details should show: cover amount, monthly premium, waiting period, benefits
- Chairperson should initiate insurance proposal
- All members should receive proposal and vote
- **If** approved:
  - Monthly premium should be deducted from group wallet on specified date
  - All members should be covered (policy in group name)
  - **When** a claim occurs:
    - Family should be able to submit claim with required documentation
    - **After** approval, payout should be made to nominated beneficiaries
    - All members should be notified of claim and payout

---

## Success Metrics & KPIs

### MVP (3-Month) Targets

| Metric | Target |
|--------|--------|
| Active Stokvel Groups Created | 500 groups |
| Total Members Onboarded | 5,000 members |
| Pooled Deposits Under Management | R5M |
| Interest Revenue Generated | R25K |
| USSD-Originated Groups | 30% of total |
| NPS Score (Stokvel Users) | >60 |

### 12-Month Targets

| Metric | Target |
|--------|--------|
| Active Stokvel Groups Created | 10,000 groups |
| Total Members Onboarded | 100,000 members |
| Pooled Deposits Under Management | R250M |
| Interest Revenue Generated | R3M+ |
| USSD-Originated Groups | 40% of total |
| Credit Profile Activations | 20,000 members |
| NPS Score (Stokvel Users) | >70 |

---

## Appendix: Glossary

| Term | Definition |
|------|------------|
| **Stokvel** | An informal rotating savings club or credit union where members contribute regularly and receive lump sum payouts |
| **Umseki / Chairperson** | The leader of a stokvel group, responsible for administration and management |
| **Ilungu / Member** | A participant in a stokvel group |
| **USSD** | Unstructured Supplementary Service Data — protocol for real-time mobile communication via short codes (e.g., *120*321#) |
| **POPIA** | Protection of Personal Information Act — South Africa's primary data privacy legislation |
| **FICA** | Financial Intelligence Centre Act — South Africa's KYC/AML regulatory framework |
| **FSCA** | Financial Sector Conduct Authority — the conduct regulator for financial institutions in South Africa |
| **EARS** | Easy Approach to Requirements Syntax — structured requirements notation |
| **NPS** | Net Promoter Score — customer satisfaction metric |
| **EFT** | Electronic Funds Transfer |
| **AML** | Anti-Money Laundering |
| **KYC** | Know Your Customer |
| **SARB** | South African Reserve Bank |

---

**Document Status:** Draft  
**Last Updated:** March 2026  
**Next Review:** Upon completion of MVP Phase 0-1
