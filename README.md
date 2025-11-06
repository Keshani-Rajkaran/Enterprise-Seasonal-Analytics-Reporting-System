# 📊Enterprise-Seasonal-Analytics-Reporting-System 
High-performance Seasonal Report dashboard for ASP.NET ERP systems.

## 📋 Executive Summary
This system was architected as a high-impact Business Intelligence  upgrade for the McKaylan Motor Engineering ERP system.

Previously, management relied on static, single-dimensional reports to gauge seasonal performance. This resulted in missed opportunities for optimizing labor allocation and marketing spend.

**The Solution:** A dynamic, multi-vector analytics system that transforms raw operational data into actionable strategic insights in real-time. It allows leadership to simultaneously track operational demand, workforce efficiency, and client acquisition trends through a secure, unified dashboard.

---

## 🎥 System Demonstration

> **DEMO VIDEO**

https://github.com/user-attachments/assets/9429d6fe-fa2c-4a63-a1c9-f3ea1c0619dc
> *Demonstration of the multi-select criteria, two-stage validation security, and real-time chart rendering.*
>
> ## 📂 Module Source Artifacts
This repository hosts the two core files that constitute the Seasonal Analytics Reporting System, decoupled from the main ERP project for demonstration purposes.

### [Seasonal_Report.aspx](Seasonal_Report.aspx) (Presentation Layer)
This file handles the complete user interface and client-side presentation.
* **Dynamic UI Controls:** Manages the multi-select dropdowns and standardizes user inputs.
* **Print/PDF Styling:** Contains specialized CSS media queries to format the output for executive-level physical reports.
* **Chart.js Integration:** Bridges the backend data with the frontend JavaScript library to render reactive charts.

### Seasonal_Report.aspx.cs (Business Logic Layer)
*  The C# "code-behind" that serves as the intelligence brain of the module.

*  Validation Engine: Enforces business rules (e.g., preventing future date selection) and manages the two-stage confirmation modal.

*  Intelligence Generators: Contains the dedicated engines that execute complex SQL queries and process raw data into business metrics.

*  Natural Language Processing: Dynamically constructs the "Executive Summary" paragraphs by interpreting the data trends in real-time.

---
## 🚀 Key Functional Capabilities

### 1. Smart Seasonal Selection & Validation
The system moves uses predefined, industry-relevant business seasons.
* **Temporal Integrity Logic:** Includes advanced validation that prevents users from generating reports for seasons that have not yet occurred, ensuring data integrity and preventing empty-state errors.

### 2.  Comparative Logic (YoY)
With a single click, managers can enable **Year-over-Year (YoY) comparisons**. The system automatically calculates historical baselines for the exact same period in the previous year, instantly visualizing growth or decline with color-coded performance badges (e.g., "📈 12% UP").

### 3. Multi-Vector Intelligence 
Allows for simultaneous, "stacked" analysis of critical business metrics:
* **Operational Volume:** analyzing demand versus shop throughput capacity.
* **Workforce Efficiency:** measuring active technician utilization and identifying top performers.
* **Client Retention:** distinguishing between new acquisition trends and loyal returning client baselines.

### 4. Dynamic Data Visualization
Integrated **Chart.js** renders reactive, client-side visuals based on the selected data scope:
* **Line Charts** for daily demand trends.
* **Bar Charts** for individual technician billing performance.
* **Doughnut Charts** for client composition ratios.

### 5. Native Export Capabilities
Built with executive reporting in mind, the dashboard features a dedicated **Print/PDF view**. This strips away UI clutter (buttons, dropdowns) to generate a clean, branded document ready for stakeholder meetings immediately.

---

## 🛠️ Technical Architecture

* **Core Framework:** ASP.NET Web Forms (.NET Framework 4.8)
* **Language:** C# (incorporating modern pattern matching and Tuples for date handling)
* **Database Layer:** MS SQL Server (accessed via secure ADO.NET with parameterized `SqlCommand` to prevent SQL injection)
* **Frontend:** Responsive HTML5/CSS3, JavaScript, and Chart.js library
* **UI/UX:** Responsive HTML5/CSS3 with extensive JavaScript interop for modal control

---

## 📄 Sample Output
> **DEMO SEASONAL PERFORMANCE REPORT**[Seasonal Performance Report .pdf](https://github.com/user-attachments/files/23381796/Seasonal.Performance.Report.pdf)
---

## ⚖️ Disclaimer & Usage
*This repository contains the intellectual property (frontend views and backend logic engines) of the analytics module. For security reasons, proprietary database connection strings, `web.config` files, and actual client data have been excluded.*
