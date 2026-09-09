# CivicConnect — Community Service Request Management Platform

[![Course](https://img.shields.io/badge/Course-SEN381%20Software%20Engineering-blue)](https://github.com/BernardW88/SEN381-Software-engineering-)
[![Milestone](https://img.shields.io/badge/Milestone-M1%20Baseline-orange)](./docs/PED-CC-v1.0.md)
[![Status](https://img.shields.io/badge/Baseline-Proposed--Pending%20Gate-yellow)](#-project-status--governance)

> **Software Engineering 381 (SEN381)** | Project Engineering Document Baseline (PED-CC-v1.0)  
> **Authors:** Christoffel Lombard (602390) & Petrus Barend Wentzel (601201)

---

## 📌 Problem & Scope

CivicConnect addresses the operational issues of fragmented service-request channels (email, phone, WhatsApp, paper records). These disparate channels lead to duplicate, unassigned, or lost requests, poor visibility for requesters, and a lack of accountability for management.

**CivicConnect creates a single, traceable lifecycle record for every service request.**

### Key Capabilities
* **For Requesters:** Simple submission of service requests with real-time status feedback and history tracking.
* **For Service Staff:** Filterable operational queues, ownership assignment, controlled status updates, and audit notes.
* **For Service Managers:** Oversight access to monitor workload, category distribution, open, overdue, and resolved tasks.

---

## 👥 Project Team

| Name | Student ID | Role |
| :--- | :--- | :--- |
| **Christoffel Lombard** | 602390 | Author / Contributor |
| **Petrus Barend Wentzel** | 601201 | Author / Contributor |

---

## 📄 Key Engineering Documents

All formal project baselines and registers are version-controlled under the [`/docs`](./docs) directory:

* 📄 **[Project Engineering Document (PED v1.0)](./docs/PED-CC-v1.0.md)** — Complete M1 commitment baseline.
* 📋 **[Requirements Traceability Matrix (RTM)](./docs/PED-CC-v1.0.md#6-requirements-traceability-matrix)** — Source-to-acceptance mapping for FRs and NFRs.
* ⚠️ **[Initial Risk Register](./docs/PED-CC-v1.0.md#7-initial-risk-register)** — Risk evaluation and mitigation controls (RISK-001 through RISK-009).
* 🪵 **[Engineering Decision Log](./docs/PED-CC-v1.0.md#9-engineering-decision-log)** — Architectural & baseline decisions (DEC-001 to DEC-003).

---

## 🛠️ Tech Stack & Architecture

> **Note:** The concrete tech stack, database schema, and deployment targets are explicitly deferred to **Milestone 2 (M2)** as per baseline decision `DEC-003`. 

* **Documentation:** Markdown / IEEE Referencing Standard (Pending Confirmation)
* **Version Control:** Git & GitHub Workflow

---

## 🛡️ Project Status & Governance

* **Baseline Status:** `REVISION REQUIRED` (Pending complete peer-review evidence and lecturer clarification on 2-person team review controls).
* **Branch Protection:** All commits to `main` must pass through tracked GitHub Issues, Pull Requests, and a minimum of **1 peer approval** from the co-author.
* **Secrets Policy:** Environment variables and secrets must never be committed to this repository (`.gitignore` enforced).

---

## 📂 Repository Layout

```text
├── .github/              # Issue and Pull Request templates
├── docs/                 # Project documentation and PED baselines
│   └── PED-CC-v1.0.md    # Master Engineering Baseline Document
├── src/                  # Application Source Code (Planned for M2/M3)
├── .gitignore            # Git exclusion rules
├── LICENSE               # License information
└── README.md             # Repository landing page
