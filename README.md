# Al-Muhtaref CRM — Customer Relationship Management System

> Semester project — Al-Sham Private University, Faculty of Informatics Engineering — Academic Year 2025–2026
> And real project in the same time it's for Almohtaref startup 

A web-based Customer Relationship Management (CRM) system built specifically for **"Al-Muhtaref"**, a startup in the digital education sector for children. The system provides a single, unified record for every customer (parent / student) and connects the Marketing, Sales, Customer Service, and Educational Guidance departments on one platform, replacing the scattered manual tools currently in use (Google Sheets, Excel, paper notebooks, and unstructured WhatsApp conversations).

---

## Table of Contents

- [Problem Statement](#problem-statement)
- [Proposed Solution](#proposed-solution)
- [Why a Custom Solution?](#why-a-custom-solution)
- [Users and Stakeholders](#users-and-stakeholders)
- [Features and Functional Requirements](#features-and-functional-requirements)
- [Non-Functional Requirements](#non-functional-requirements)
- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
- [Project Management](#project-management)
- [Team](#team)
- [References](#references)

---

## Problem Statement

As the platform has grown and the number of parents using it has increased, managing customer data and coordinating between departments has become difficult. An internal survey and an interview with management revealed the following recurring problems:

| Problem | Impact |
|---|---|
| Customer data scattered across multiple sources | Hard to know the latest update or contact (100% of surveyed staff use Google Sheets) |
| Multiple touchpoints between the customer and departments | Several employees contacting the same customer (66.7% of staff observed this) |
| No unified customer status classification | Unclear which stage each customer is actually at |
| No clear owner responsible for follow-up | Lost subscription opportunities and delayed replies |
| Reliance on manual tools for reminders and follow-up | Forgotten follow-ups (83.4% of staff must manually contact other departments to check a customer's status) |
| No objective performance metrics | Difficult to measure campaign effectiveness and sales team performance |
| No clear access permissions | Risks to data integrity and conflicting edits between staff |
| No unified interaction history | Hard to understand customer behavior; the same questions are asked repeatedly |
| Missing essential data at first contact | Staff time wasted searching and verifying instead of following up |
| No central system for complaints | Complaints scattered across departments and resolved late |

---

## Proposed Solution

A unified CRM system that provides:

- **Unified customer data:** a single trusted record (basic info, communication history, payments, enrolled courses).
- **Faster response:** less time wasted searching for customer data and the last contact.
- **No lost opportunities:** a clear owner for each customer and automatic follow-up reminders.
- **Better customer service:** log issues (tickets) and track them through to resolution.
- **Performance measurement:** customers per stage, conversion rate, and customer source.
- **Data protection:** role-based permissions controlling what each user can view or edit.

---

## Why a Custom Solution?

Although ready-made systems exist (Salesforce, HubSpot, Zoho), management decided to build a custom solution because generic systems do not fit the company's specific workflow, are costly (priced in USD), and have complex interfaces for non-technical staff.

| Feature | Al-Muhtaref CRM | Ready-made Cloud CRMs | Excel / Google Sheets |
|---|:---:|:---:|:---:|
| Unified 360° customer view | ✅ | ✅ | ❌ |
| Automatic data transfer between departments | ✅ | ✅ | ❌ |
| WhatsApp integration and conversation logging | ✅ | ❌ (extra subscription) | ❌ |
| Automated reminders and follow-up scheduling | ✅ | ✅ | ❌ |
| Assign a direct owner to each student | ✅ | ✅ | ❌ |
| Interfaces tailored to non-technical staff | ✅ | ❌ (complex) | ✅ |
| Roles and permissions system | ✅ | ✅ | ❌ |
| Dedicated module for educational readiness and placement | ✅ | ❌ (requires custom development) | ❌ |
| Affordable for a startup (no subscription) | ✅ | ❌ | ✅ |

---

## Users and Stakeholders

**Users**

- **Customer Service staff:** need a unified view of student data and past communication.
- **Sales / Lead follow-up staff:** need follow-up alerts and automatic lead distribution to avoid duplication.
- **Marketing staff:** need reports on each customer's source and each campaign's effectiveness.
- **Educational Guidance team:** need to see the courses a student is enrolled in and their learning history.
- **Senior Management:** needs a comprehensive view of department performance and conversion indicators.

**Stakeholders**

- End users (staff of the four departments above)
- Al-Muhtaref senior management
- Parents (end customers, indirectly affected by system quality and response speed)
- Project team / developers

---

## Features and Functional Requirements

### Customer and Data Management
- Record basic customer data (name, phone number, subject/course of interest) at first contact.
- Maintain one unified file per customer covering contact info, calls, messages, payments, and enrolled courses.
- Warn when attempting to register a duplicate customer (same phone number or name).
- Distinguish between a parent account and linked student(s) (one parent → multiple students).
- Provide a free-text notes field for each customer.

### Customer Status and Lifecycle
- Unified statuses: `Interested` → `Contacted` → `Registered` → `Subscribed` → `Withdrawn`, with the same meaning across all departments.
- Log the date of every status change and the user who made it.
- Allow reactivating a customer from `Withdrawn` to `Interested` when contact is re-established.

### Workflow and Lead Assignment
- Transfer customer data between relevant departments without manual re-entry.
- Assign each customer to a single responsible employee (automatically or manually), with the ability to reassign.
- Show the responsible employee the customer's latest update and status before contact.

### Follow-up and Reminders
- Schedule a follow-up appointment for each customer.
- Automatically notify the responsible employee before the follow-up date.
- Alert employees when a customer has had no update or contact for a defined period.

### Customer Service and Issue Management
- Create a Ticket / Issue linked to the customer's file.
- Set issue type, priority, status, and assigned employee.
- Record issue details, actions taken, and the final resolution.
- Escalate issues to the Technical Team or a Manager when needed.
- Show previous issues within the customer's file.

### Unified Customer Journey (Timeline)
- Display the customer journey on a single screen in chronological order: source, first contact, calls, messages, status changes, payments, and enrolled courses.

### Reports and Metrics
- Number of customers in each lifecycle stage.
- Conversion rate from lead to subscriber.
- Link each customer to their source (campaign / ad) to measure campaign effectiveness.
- Customer service metrics: open and closed issues, response and resolution time (when data is available).

### Users and Permissions
- Individual login for each employee, tied to their job role.
- Fine-grained permissions per role (view / edit access).

### External Integrations
- **WhatsApp:** document and track customer conversations and link them to the customer record; log the delivery status of sent messages.
- **Google Sheets:** import leads in an organized way, validating the data and checking for duplicates before adding it to the database.

### Future Scope
- A visual Kanban pipeline board showing customers by status.
- Bulk messaging to a selected segment of customers.

---

## Non-Functional Requirements

| Requirement | Description |
|---|---|
| Usability | Simple, clear interface for non-technical staff |
| Performance | Fast search and customer-file loading under normal use |
| Security | Protect sensitive customer data and prevent unauthorized access through permissions and authentication |
| Reliability | Stable operation during working hours without frequent interruptions |
| Scalability | Support adding users, departments, or features without rebuilding the system |
| Compatibility | Works on modern browsers; responsive across screen sizes |
| Backup | Regular database backups to reduce the risk of data loss |
| Maintainability | Organized structure that makes future changes and additions easy |

---

## Tech Stack

| Category | Details |
|---|---|
| Platform | Web System / Web Application |
| Backend | ASP.NET Core (C#) |
| Frontend | React (TypeScript) |
| Database | Microsoft SQL Server |
| Tools | Visual Studio, VS Code, SQL Server Management Studio (SSMS), GitHub, Postman, Jira |

---

## Getting Started

> ⚠️ The steps below are a generic template — adjust them to match the actual repository structure.

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (version used by the project)
- [Node.js](https://nodejs.org/) and npm
- Microsoft SQL Server (or SQL Server Express)

### Setup

```bash
# 1) Clone the repository
git clone <repository-url>
cd <repository-folder>

# 2) Run the backend
cd backend
dotnet restore
# Update the connection string in appsettings.json
dotnet ef database update
dotnet run

# 3) Run the frontend (in a separate terminal)
cd frontend
npm install
npm run dev
```

---

## Project Management

- **Methodology:** Agile (Scrum).
- **Meetings:** Daily, one hour, via Google Meet or on-site with the client at the company's office.
- **Communication:** WhatsApp for official day-to-day communication.
- **Task tracking:** Jira.
- **Source code and documentation:** GitHub.
- **Project manager:** Mohammad Al-Sheikh.
- **Supervisor:** Eng. Khaled Ismail.

---

## Team

| # | Name | Role |
|:---:|---|---|
| 1 | Ahmad Fares Oyoun | Backend Developer |
| 2 | Mohammad Al-Sheikh | Backend Developer (Project Manager) |
| 3 | Qusai Al-Madi | Frontend Developer + Product Owner |
| 4 | Mohammad Jawad Al-Sous | Frontend Developer |

---

## References

- Ronald S. Swift, *Accelerating Customer Relationships*.
- Microsoft Dynamics 365 and Zoho reports on CRM systems.
- Salesforce case studies via Cuspera and Ascendix.
