# APHA-FPS-Apps (Suite of FPS Applications)

#![Build status](#https://github.com/DEFRA/apha-vir/actions/workflows/dev-ci.yaml/badge.svg)[![Quality Gate Status](#https://sonarcloud.io/api/project_badges/measure?project=DEFRA_apha-vir&metric=alert_status)](#https://sonarcloud.io/dashboard?id=DEFRA_apha-vir)[![Coverage](#https://sonarcloud.io/api/project_badges/measure?project=DEFRA_apha-vir&metric=coverage)](#https://sonarcloud.io/dashboard?id=DEFRA_apha-vir)

**APHA-FPS-Apps** is the codebase for suite of FPS applications, built using ASP.NET Core MVC web and Apis, and hosted on AWS. 

---

## Table of Contents

- [Features](#features)  
- [Key technologies](#Keytechnologies)  
- [Deployment](#deployment)  
- [Versioning](#versioning)  
- [License](#license)

---

## Features

- Web application using ASP.NET Core MVC 
- Business functionalities expose as Rest APis using ASP.NET Core MVC  
- Secure user authentication & authorization  
- Role-based access control  
- Reporting / dashboards  
- Hosted on AWS for scalability and reliability  

---

## Key technologies

- ASP.NET Core MVC 
- AWS RDS PostgreSql 
- AWS Cloud infrastructure  
- ECR for CI
- Redis cache for session management
- ECS for hosting application 

---

## Deployment

- CI using GitHub Actions
- CD via Jenkins job

---

## Versioning  

This project uses [Semantic Versioning (SemVer)](https://semver.org/) for versioning.  
Image Versions are indicated by **Git tags** in the repository (e.g. `v1.2.3`).  

You can view all available versions directly in the GitHub UI:  

- **Releases page:**  
  #[https://github.com/DEFRA/apha-fps-apps/releases](https://github.com/DEFRA/apha-fps-apps/releases)

- **Tags page:**  
  #[https://github.com/DEFRA/apha-fps-apps/tags](https://github.com/DEFRA/apha-fps-apps/tags)

---

## License  

This project is licensed under the [MIT License](LICENSE).  
See the LICENSE file for full details.

