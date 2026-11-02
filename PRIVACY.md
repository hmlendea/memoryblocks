# Privacy and Personal Data

Memory Blocks is a local desktop tile-matching puzzle game with no personal data collection, network communication, or external integrations.

## 📑 Table of Contents

- [What This Document Covers](#what-this-document-covers)
- [Self-Hosted Deployments](#self-hosted-deployments)
- [Data We Handle](#data-we-handle)
  - [Data Provided to the Application](#data-provided-to-the-application)
  - [Data Generated or Collected by the Application](#data-generated-or-collected-by-the-application)
  - [Data Received from Integrations](#data-received-from-integrations)
- [Processing and Use](#processing-and-use)
- [Storage, Retention, and Deletion](#storage-retention-and-deletion)
- [External Processing and Integrations](#external-processing-and-integrations)
- [Data Protection and Security](#data-protection-and-security)
- [Document Changes](#document-changes)
- [Contact](#contact)

## 🔎 What This Document Covers

This document describes how Memory Blocks at https://github.com/hmlendea/memoryblocks handles personal data. It covers the application behaviour and verified integrations described below. Where the software is self-hosted, the instance operator may have separate responsibilities described below.

## 🏠 Self-Hosted Deployments

Memory Blocks is distributed as source code and compiled binaries that users build and run locally. The project maintainers do not operate a hosted service. Each operator controls their instance's configuration, local storage, logs, backups, access controls, retention, and request handling. The project does not control those functions.

No data is sent from a self-hosted instance to project maintainers or external services. There is no telemetry, update checks, crash reports, email, authentication, reverse-proxy, object-storage, or monitoring integrations.

## 📥 Data We Handle

### Data Provided to the Application

No personal data is requested, collected, or stored by the application. The game does not prompt for names, accounts, identifiers, or any user-provided information.

### Data Generated or Collected by the Application

No personal data is generated or collected automatically. The application does not create logs, telemetry, crash reports, usage analytics, or any persistent records of user activity. Game state (board layout, timer, flip count) exists only in memory during a play session and is discarded when the application closes.

### Data Received from Integrations

No personal data is received from integrations or third parties. The application has no network connectivity and no integration endpoints.

## 🧭 Processing and Use

The application processes no personal data. The only in-memory state consists of:
- Randomised tile positions and values for the current game board — session-only, no identifiers
- Game timer and flip counter — session-only, no identifiers

These are used solely to render the puzzle and determine win conditions during the active session.

## 🗄️ Storage, Retention, and Deletion

No personal data is stored. The application writes no databases, files, logs, caches, or backups containing personal data. Game assets (tile images) are embedded resources shipped with the application and contain no user data. For self-hosted deployments, the instance operator controls any local storage, deletion, and backups of their own system; the project does not manage these.

## 🔗 External Processing and Integrations

The application has no built-in external data transfer. It does not communicate with any external service, API, or recipient.

| Service or integration | Purpose | Data involved | Configuration or documentation |
|-----------------------|---------|---------------|--------------------------------|
| None | Not applicable | Not applicable | Not applicable |

## 🛡️ Data Protection and Security

The application handles no personal data, so no personal-data-specific safeguards are required. The codebase uses no network sockets, no external process invocation, and no persistent storage of user information. For self-hosted deployments, the operator is responsible for updates, secrets, access controls, backups, network exposure, and log protection on their own system. The original maintainers are not responsible for changes, modifications, or privacy issues introduced by downstream forks.

## 🔄 Document Changes

Update this document when application data flows, storage, integrations, or deployment responsibilities change. The current version is published at https://github.com/hmlendea/memoryblocks/blob/main/PRIVACY.md.

## 📬 Contact

For questions about application data handling, contact the project maintainers via GitHub issues at https://github.com/hmlendea/memoryblocks/issues. For a self-hosted instance, contact the instance operator, unless the project explicitly handles the request. Include the repository URL and deployment details if relevant; do not send passwords, access tokens, or other secrets.