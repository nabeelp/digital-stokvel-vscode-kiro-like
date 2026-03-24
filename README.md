# Digital Stokvel Banking Platform

A bank-native platform that brings South Africa's R50B informal savings economy into the formal banking system.

## Overview

Digital Stokvel Banking is a microservices-based platform supporting 11M+ potential users across Android, iOS, USSD, and Web platforms. The platform enables traditional stokvel groups to digitize their operations while maintaining their cultural governance structures.

## Architecture

The system follows a microservices architecture with the following key components:

### Services

- **Group Service** - Group creation, member management, and constitution handling
- **Contribution Service** - Payment processing, recurring payments, and ledger management
- **Payout Service** - Rotating payouts, dual approvals, and disbursement orchestration
- **Governance Service** - Voting, dispute resolution, and constitution enforcement
- **Notification Service** - Multi-channel notifications (Push, SMS, USSD)
- **Credit Profile Service** - Stokvel Score calculation and credit bureau integration (Phase 2)

### Gateways

- **USSD Gateway** - Feature phone integration with session management
- **API Gateway** - Request routing, authentication, and rate limiting

### Shared Libraries

- **Domain** - Domain models and business entities
- **Common** - Cross-cutting concerns, utilities, and helpers
- **Infrastructure** - Data access, external service integrations, and infrastructure concerns

## Technology Stack

- **Backend**: .NET 10 (C#)
- **Database**: PostgreSQL 15+ (primary & ledger), Redis 7+ (cache)
- **Message Queue**: RabbitMQ / Azure Service Bus
- **Container Orchestration**: Kubernetes (AKS/EKS)
- **Frontend**: 
  - Android: Kotlin + Jetpack Compose
  - iOS: Swift + SwiftUI
  - Web: React + TypeScript

## Prerequisites

- .NET 10 SDK
- Docker Desktop
- PostgreSQL 15+
- Redis 7+
- Visual Studio 2024 or JetBrains Rider

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/nabeelp/digital-stokvel-vscode-kiro-like.git
cd digital-stokvel-vscode-kiro-like
```

### 2. Build the solution

```bash
dotnet restore
dotnet build
```

### 3. Run tests

```bash
dotnet test
```

### 4. Run services locally

```bash
# Start infrastructure services
docker-compose up -d

# Run a specific service
cd src/services/DigitalStokvel.GroupService
dotnet run
```

## Project Structure

```
.
├── src/
│   ├── services/           # Microservices
│   │   ├── DigitalStokvel.GroupService/
│   │   ├── DigitalStokvel.ContributionService/
│   │   ├── DigitalStokvel.PayoutService/
│   │   ├── DigitalStokvel.GovernanceService/
│   │   ├── DigitalStokvel.NotificationService/
│   │   └── DigitalStokvel.CreditProfileService/
│   ├── gateways/           # API and USSD gateways
│   │   ├── DigitalStokvel.ApiGateway/
│   │   └── DigitalStokvel.UssdGateway/
│   └── shared/             # Shared libraries
│       ├── DigitalStokvel.Domain/
│       ├── DigitalStokvel.Common/
│       └── DigitalStokvel.Infrastructure/
├── tests/
│   ├── unit/               # Unit tests
│   └── integration/        # Integration tests
├── docs/                   # Documentation
├── scripts/                # Deployment and automation scripts
├── infrastructure/         # Infrastructure as Code (Terraform/Bicep)
└── .kiro-like/
    └── specs/              # Requirements and design documents
        ├── requirements.md
        ├── design.md
        └── tasks.md
```

## Documentation

- [Requirements Specification](.kiro-like/specs/requirements.md) - Functional and non-functional requirements in EARS notation
- [Technical Design](.kiro-like/specs/design.md) - Architecture, data models, APIs, and sequence diagrams
- [Implementation Plan](.kiro-like/specs/tasks.md) - Sequenced task breakdown for development

## Development Workflow

1. **Foundation** (Weeks 1-2): Dev environment setup, CI/CD, project standards
2. **Infrastructure** (Weeks 3-5): Database schema, migrations, infrastructure provisioning
3. **Backend Services** (Weeks 6-10): Microservices and API implementation
4. **Client Apps** (Weeks 8-13): Android, iOS, USSD, and Web applications
5. **Integration** (Weeks 11-13): External service integration
6. **Security & Compliance** (Weeks 12-14): POPIA, AML/CFT, encryption
7. **Testing** (Weeks 14-16): Unit, integration, UAT, penetration testing
8. **Performance** (Weeks 15-16): Optimization and monitoring
9. **Launch** (Weeks 17-18): Production deployment and go-live

## Contributing

Please refer to the [Implementation Plan](.kiro-like/specs/tasks.md) for the current development priorities and task assignments.

## Compliance

This platform is designed with compliance to:
- **POPIA** (Protection of Personal Information Act)
- **FICA** (Financial Intelligence Centre Act)
- **SARB** (South African Reserve Bank) regulations
- **AML/CFT** (Anti-Money Laundering / Combating the Financing of Terrorism)

## License

Proprietary - All rights reserved

## Support

For questions or issues, please contact the development team.

---

**Status**: MVP Development (Phase 0 - Foundation)  
**Target Launch**: Q2 2026  
**Version**: 1.0.0-alpha
