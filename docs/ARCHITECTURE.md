# AurumWealth — Solution Architecture

## Overview

**AurumWealth** is a production-ready, luxury-branded investment platform for the Ghana market (with expansion to Nigeria, Kenya, and South Africa). It follows **Clean Architecture** with clear separation of concerns, enterprise security patterns, and a mobile-first premium UI.

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    MyFreelance.Web (Presentation)            │
│  Razor Pages │ SignalR │ JWT API │ Bootstrap 5 │ Serilog    │
└──────────────────────────┬──────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────┐
│                 MyFreelance.Application                      │
│  Service Interfaces │ DTOs │ FluentValidation │ Use Cases   │
└──────────────────────────┬──────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────┐
│                MyFreelance.Infrastructure                    │
│  EF Core │ Repositories │ Unit of Work │ External Services  │
└──────────────────────────┬──────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────┐
│                   MyFreelance.Domain                         │
│  Entities │ Enums │ Repository Interfaces │ Constants       │
└─────────────────────────────────────────────────────────────┘
                           │
                    ┌──────▼──────┐
                    │  SQL Server │
                    └─────────────┘
```

## Default Credentials (Development Seed)

- **Admin:** admin@aurumwealth.gh / Admin@123!

## Running the Application

```bash
dotnet run --project src/MyFreelance.Web
```
