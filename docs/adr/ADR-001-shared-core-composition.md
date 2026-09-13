# ADR-001 – Gemeinsame Core-Composition für alle Workbench-Hosts

> **Status:** Accepted  
> **Datum:** 2026-09-13  
> **Entscheidungsebene:** gemeinsame SASD Workbench Codebasis

## Kontext

Die SASD Workbench soll künftig mehrere Hosts bzw. Fachprodukte tragen, unter anderem General, Research, Biblical Research, Health Research, Linux/Admin und Software/Engineering.

Die bisherigen Application- und Infrastructure-Bausteine waren zwar sauber geschichtet, der WinForms-Host verdrahtete Repositories und Services jedoch manuell in `Program.cs`. Würde dieses Muster für jeden späteren Fach-Host kopiert, entstünden mehrere fast identische Composition Roots. Neue Core-Funktionen könnten dann leicht nur in einzelnen Hosts registriert werden; Tests würden diese Abweichung möglicherweise erst spät entdecken.

Gleichzeitig soll der Core kein Wissen über fachliche Profile erhalten und es soll kein vorzeitiges Plugin-System eingeführt werden.

## Entscheidung

Die Infrastructure-Schicht stellt mit

```text
AddSasdWorkbenchCore(WorkbenchDataPaths paths)
```

eine kanonische Dependency-Injection-Registrierung für den gemeinsamen lokalen Core bereit.

Diese Registrierung umfasst ausschließlich profile-neutrale technische Adapter und Application Services, insbesondere:

- SQLite Connection Factory und Migration Runner,
- Clock-Abstraktion,
- kontrollierten File Storage,
- Core-Repositories,
- Project/Entry/Template/Tag/Attachment-Services,
- Collection/Relation/Search/Activity-Services,
- Markdown Export,
- Backup/Restore.

Ein Host bleibt verantwortlich für:

1. die Wahl seines lokalen Workbench-Datenpfads,
2. das Erzeugen der Verzeichnisse,
3. das Ausführen der Migrationen vor Start der UI,
4. die Registrierung eigener UI-, Profil- oder Modul-Dienste.

Profile-spezifische Services werden **nicht** in `AddSasdWorkbenchCore(...)` aufgenommen.

Für austauschbare technische Abhängigkeiten wird soweit sinnvoll `TryAdd` verwendet. Dadurch können Tests oder spätere Hosts beispielsweise eine alternative `IClock`-Implementierung registrieren, ohne die komplette Core-Verdrahtung zu duplizieren.

Der End-to-End-Smoke-Test verwendet dieselbe Core-Registrierung wie produktive Hosts. Damit wird fehlende oder inkonsistente Registrierung Bestandteil der CI-Prüfung.

## Alternativen

### A – Manuelle Verdrahtung in jedem Host

Verworfen. Sie ist zunächst einfach, führt bei mehreren Workbench-Produkten aber zu Kopien und schleichender Konfigurationsdrift.

### B – Vollständiger Generic Host / Plugin-Container bereits in V1

Zurückgestellt. Ein `Microsoft.Extensions.Hosting`-basiertes Host- und Pluginmodell wäre leistungsfähig, erhöht für den aktuellen lokalen Desktop-Core aber Komplexität ohne ausreichenden V1-Nutzen.

### C – Service Locator / globale statische Services

Verworfen. Versteckte Abhängigkeiten erschweren Tests, Wartung und spätere alternative Hosts.

## Auswirkungen

### Positiv

- Eine gemeinsame Core-Verdrahtung für alle zukünftigen Hosts.
- Neue profile-neutrale Services werden zentral ergänzt.
- Composition-Fehler werden im Smoke-Test und bei `ValidateOnBuild` früh sichtbar.
- Fach-Hosts können eigene Services ergänzen, ohne SQLite-Details kennen zu müssen.
- Constructor Injection bleibt sichtbar und testbar.

### Negativ / Trade-offs

- Infrastructure erhält eine kleine Abhängigkeit auf die DI-Abstraktionen von `Microsoft.Extensions.DependencyInjection`.
- Hosts benötigen einen DI-Container oder eine kompatible `IServiceCollection`-Implementierung.
- Die zentrale Registrierung darf nicht zu einem Sammelplatz für Profile-spezifische Funktionen werden.

## Folgeentscheidungen

- WinForms-Features werden schrittweise in kleinere Controls/Dialoge zerlegt statt `MainForm` unbegrenzt zu vergrößern.
- Ein vollständiges Plugin-System bleibt bis zu einem realen Bedarf zurückgestellt.
- Falls später CLI, API oder weitere Desktop-Hosts entstehen, sollen sie dieselbe Core-Registrierung wiederverwenden.
