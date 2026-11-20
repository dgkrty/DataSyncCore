# DataSync Core 🔄

**An offline, air-gapped database migration and replication engine built for high-security enterprise environments.**

---

## 📌 The Problem
Modern database tools (like AWS or Azure) assume computers are always connected to the internet. However, high-security environments—such as defense contractors, banks, and industrial edge facilities—operate on **"air-gapped"** networks (computers strictly disconnected from the internet for security). 

When software engineers in these restricted environments need to move, clone, or back up complex databases, standard internet-dependent tools fail, forcing them to use risky, manual processes.

## The Solution
**DataSync Core** is a local-first software utility that solves this problem. It securely reads a local database, automatically figures out the complex relationships between the data, and packages it into a highly compressed, secure offline file. This file can then be safely moved (e.g., via a secure USB drive) and applied to an isolated target computer without ever needing an internet connection.

## Technical Architecture & Features
This project is divided into a high-performance backend engine and a user-friendly desktop interface:

* **Schema Introspection:** Automatically scans database structures (Microsoft SQL Server, PostgreSQL) to map out tables, columns, and data rules.
* **Topological Dependency Resolver:** Uses advanced sorting algorithms (Directed Acyclic Graphs) to figure out the exact mathematical order data must be copied so that relational rules are never broken.
* **Smart Cycle-Breaking:** Detects infinite loops in database relationships and safely resolves them without crashing.
* **Secure Offline Packaging:** Compresses massive amounts of data into chunked binary files using cryptographic hashes (SHA-256) to ensure the data cannot be tampered with while being moved.
* **Modern Desktop UI:** A responsive, dark-themed dashboard that provides real-time telemetry and execution logs to the user.

## Technology Stack
* **Language:** C#
* **Framework:** .NET 8.0
* **User Interface:** WPF (Windows Presentation Foundation) / XAML
* **Database Drivers:** ADO.NET (SQL Server, PostgreSQL)
* **Architecture:** Multi-project solution (Core Library, CLI Runner, GUI Client)

## How to Run the Application
1. Ensure the [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download) is installed.
2. Open a terminal in the repository folder.
3. Run the desktop application using the following command:
   ```bash
   dotnet run --project DataSync.UI
