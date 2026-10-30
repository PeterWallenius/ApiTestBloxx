# Context for Whistregnskab

Dette dokument beskriver krav, arkitektur, drift og udviklingsretningslinjer for udvikling af systemer. Filen fungerer som projektbaggrund og reference for udviklingsteamet, DevOps og interessenter.

---

### Overordnede krav
- Kørsel i **Azure** med fokus på at minimere omkostninger.  
- Data gemmes i **Azure Table Storage** i en Storage Account.  
- Backend og forretningslogik i **C# på .NET 10**.  
- Frontend i **Blazor Server** med komponenter fra **MudBlazor**.  
- Projektstruktur efter **Clean Architecture**.  
- Følg moderne Microsoft anbefalinger som kodestandard.  
- Kildekode i **GitHub**. **GitHub Actions** til CI/CD og automatisk deployment ved ændringer i `main` branch.

---

### Arkitekturoversigt
**Frontend**  
- Blazor Server for enkel state management, lav kompleksitet i autentifikation og realtidsopdateringer.  
- UI komponenter fra MudBlazor for konsistent design og hurtig udvikling.

**Backend og hosting**  
- .NET 10 applikationer hostet i Azure App Service eller Azure Container Apps afhængig af krav til autoscaling og isolation.  
- Overvej Azure App Service for enkel drift og lave faste omkostninger ved moderat trafik. Overvej Container Apps hvis containeriseret deployment og fleksibel autoscaling ønskes.

**Data**  
- Azure Table Storage til alle domæneentiteter.  
- Designprincipper: små entiteter, enkel partitionering, minimal rækkevidde.  

**Integrationer**  
- Key Vault til hemmeligheder.  
- Managed Identity til adgang fra app til Storage og Key Vault.  
- Application Insights til telemetri med sampling for omkostningskontrol.

### Projektstruktur og kodestandard
**Arkitektur**  
- Clean Architecture med lagene Presentation, Application, Domain og Infrastructure.  
- Tydelige grænseflader og afhængighedsinjektion.

**Kodestandard**  
- Brug .NET 10 best practices: async/await, dependency injection, IOptions, ILogger.  
- Små metoder, single responsibility, klare interfaces, DTOs til UI og persistence.  
- Pin NuGet pakkeversioner for deterministiske builds.  
- Skriv unit tests for domænelogik og integrationstests mod emulator eller test Storage Account.

---

### CI CD og GitHub Actions
- Repository i GitHub med branch strategi: feature branches → pull request → review → merge til `main`.  
- GitHub Actions workflow for build, test og deploy.  
- Workflow trin: checkout, restore, build, test, publish artifact, deploy til Azure.  
- Brug GitHub OIDC til kortvarig tokenudstedelse til Azure for sikker deployment uden permanente credentials.  
- Secrets gemmes i GitHub Secrets hvis nødvendigt.  
- Deployment flow: merge til `main` → CI → deploy til staging → validering → promotion til production.

---

### Sikkerhed og adgang
- Brug **Managed Identity** for adgang til Azure ressourcer.  
- Gem alle hemmeligheder i **Azure Key Vault**.  
- Anvend princip om mindst privilegium for servicekonti og ressourcer.  
- RBAC for Azure ressourcer og begræns adgang til GitHub Secrets.  
- Inputvalidering og output encoding i UI og API.

---

### Observability og drift
- Application Insights til telemetri. Konfigurer sampling og retention for at holde omkostninger nede.  
- Opsæt alerts for fejlrate, latency og kvoter.  
- Centraliseret logging i Azure Monitor.  
- Dokumenter runbooks for almindelige fejlscenarier og rollback procedure.

---

### Omkostningsoptimering
- Start med lavt forbrug og skaler efter behov.  
- Vælg App Service plan eller Container Apps SKU der matcher forventet trafik.  
- Table Storage som billig lagringsløsning for små datamængder. Vælg LRS som standard redundans for lavere pris.  
- Begræns telemetri sampling og retention.  
- Undgå unødvendige netværkskomplekser og dyre VNET peeringer.

---

### Deployment og rollback
- Gem artifacts og versionsnumre for hurtig rollback.  
- Automatiser deployment til staging og kræv manuel godkendelse før production promotion hvis ønsket.  
- Implementer health checks og readiness probes for hurtig detektion af fejl efter deploy.

---

### Test og kvalitetssikring
- Unit tests for domænelogik og pointberegning.  
- Integrationstests mod Storage emulator eller test Storage Account.  
- Automatiske tests i CI pipeline.  
- Code reviews og statisk analyse som del af PR flow.

---

### Dokumentation og onboarding
- README med projektstruktur, build og deploy instruktioner.  
- Context.md som projektbaggrund og kravspecifikation.  
- Runbooks for deployment, rollback og almindelige fejl.  
- Kort forklaring af meldestige og pointregler i UI som tooltips.

