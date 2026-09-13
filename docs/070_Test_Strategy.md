# SASD Workbench – Test Strategy

> **Status:** Working Strategy  
> **Stand:** 2026-09-13  
> **Scope:** gemeinsamer Workbench Core, Infrastructure und erster WinForms-Host

## 1. Ziel

Die Teststrategie soll verhindern, dass die gemeinsame Workbench-Codebasis zwar kompiliert, aber bei Domain-Regeln, Use Cases, Migration, Persistenz, Recovery oder Wiederverwendung auseinanderläuft.

Für den Core sind insbesondere wichtig:

- schnelle Domain-/Application-Regeltests,
- reale SQLite-Roundtrips,
- Migrationen,
- Concurrency,
- kontrollierter File Storage,
- Backup/Restore,
- transaktionale Activity History,
- profile-neutrale Domain-/Application-Regeln,
- identische Dependency-Injection-Konfiguration in Host und Integrationstests.

## 2. Aktueller Teststand

Die Testlandschaft ist in vier ausführbare Projekte aufgeteilt:

```text
tests/
  SASD.Workbench.Domain.Tests/
  SASD.Workbench.Application.Tests/
  SASD.Workbench.Infrastructure.Tests/
  SASD.Workbench.SmokeTests/
```

### Domain Tests

Schnelle xUnit-v3-Tests ohne Datenbank oder Dateisystem. Sie sichern Validierung, Lifecycle, Versionierung und offene Core-Vokabulare ab.

### Application Tests

Schnelle xUnit-v3-Tests mit kleinen In-Memory-Fakes. Sie prüfen Use-Case-Regeln unabhängig vom konkreten Persistence Adapter, insbesondere Profil-/Projektgrenzen und normale Content-Management-Regeln.

### Infrastructure Tests

xUnit-v3-Integrationstests gegen echte temporäre SQLite-Datenbanken und echtes temporäres File-I/O. Sie verwenden die produktive `AddSasdWorkbenchCore(...)` Registrierung und konzentrieren sich auf technische Verträge und Fehlerpfade.

### Smoke Test

Der bestehende ausführbare End-to-End-Smoke-Test bleibt das breite Abschlussgate. Er verwendet:

- die produktive `AddSasdWorkbenchCore(...)` Registrierung,
- eine temporäre reale SQLite-Datenbank,
- einen temporären kontrollierten Datenordner,
- einen deterministischen Test-`IClock`,
- echte File-I/O für Attachments, Export und Backup/Restore.

Der Smoke-Test ist bewusst kein Mock-Test.

## 3. Testpyramide

### Ebene A – schnelle Domain Tests

Für reine fachneutrale Objektregeln ohne Application, SQLite oder Dateisystem.

Aktuelle Beispiele:

- Project-/Entry-Normalisierung,
- Version startet bei 1,
- logisches Update erhöht Version genau einmal,
- Archive/Unarchive,
- idempotentes Soft Delete,
- gelöschte Objekte blockieren normale Mutation,
- Relation-Key-Normalisierung,
- Built-in-Vokabulare sind eindeutig,
- profildefinierte Entry-/Relation-Keys bleiben möglich,
- Self-Links werden abgelehnt.

Ziel: sehr schnell, viele Randfälle.

### Ebene B – schnelle Application Tests

Für Use-Case-Regeln mit In-Memory-Fakes.

Aktuelle Beispiele:

- `general` Template ist in einem Spezialprofil nutzbar,
- profilspezifisches Template ist nicht in einem fremden Profil nutzbar,
- projektlokales Template bleibt im Projekt,
- System-Templates können nicht über normalen User-Delete entfernt werden,
- erstellte Entries bleiben unabhängig von später gelöschten Benutzertemplates,
- Relations dürfen in V1 nicht projektübergreifend angelegt werden,
- gelöschte/missing Entries können nicht als Relation-Endpunkt verwendet werden,
- Relation-Delete ist idempotent.

Ziel: Use-Case-Regeln ohne technische Adapter präzise lokalisieren.

### Ebene C – Infrastructure Integration Tests

Für SQLite, Migrationen und File Storage.

Aktuelle Beispiele:

- Migration von leerer DB bis aktuelle Version,
- idempotenter zweiter Migration-Lauf,
- LIKE-Suche escaped `%` und `_` als Benutzertext,
- Activity + Primärmutation rollen gemeinsam zurück,
- Attachment-Datei + SQLite-Metadaten bleiben auch bei Activity-Fehler konsistent: DB-Rollback plus kompensierende Dateilöschung.

Weitere Zieltests:

- optimistic concurrency,
- Foreign Keys,
- Upgrade-Fixtures veröffentlichter Schemastände,
- Backup-Archivvalidierung,
- Restore und Safety Backup,
- Schutz gegen Path Traversal als gezielter Regressionstest.

### Ebene D – End-to-End Smoke Test

Wenige breite Kernabläufe durch echte Core-Komposition.

Ziel: beweisen, dass die Anwendungsschichten gemeinsam funktionieren und Recovery nicht durch lokale Änderungen gebrochen wurde.

## 4. Was der V1-Core-Smoke-Test mindestens abdecken muss

### Composition

- `AddSasdWorkbenchCore(...)` kann mit `ValidateOnBuild` aufgebaut werden.
- alle V1-Core-Services sind auflösbar.

### Migration

- neue temporäre DB startet leer,
- alle Migrationen werden angewendet,
- zweiter Migrationslauf verändert die Anzahl nicht,
- erwartete Migration Count stimmt.

### Project / Entry

- Create/Read/Update,
- Version startet bei 1,
- logisches Update erhöht Version genau einmal,
- Soft Delete / Restore-Roundtrip.

### Templates

- persistentes Template,
- Entry-Erzeugung aus Template,
- allgemeines Template bleibt in Spezialprofilen sichtbar/nutzbar,
- Benutzertemplate kann soft-deleted werden,
- Core-Template-Katalog bleibt konsistent.

### Tags / Collections

- normalisierte Tag-Wiederverwendung,
- idempotente Tag-Zuordnung,
- Mehrfachzuordnung zu Collections,
- hierarchische Collections.

### Relations

- Built-in Relation,
- zusätzlicher valider profildefinierter Relation Key,
- Normalisierung vor Persistenz.

### Activity History

- automatische Activities für persistierte Core-Mutationen,
- explizite Activity über `ActivityLogService`,
- idempotente No-op-Mutation erzeugt keinen doppelten Logeintrag,
- künstlich erzwungener Activity-Insert-Fehler rollt die Primärmutation zurück,
- Restore stellt Activity History auf denselben Backup-Zeitpunkt zurück.

### Attachments

- kontrollierte Dateikopie,
- SHA-256 stimmt,
- Metadaten persistieren,
- Kommentar-Update persistiert und erzeugt Activity,
- Backup enthält Datei und Metadaten,
- Restore stellt Metadaten, Kommentar und Datei wieder her.

### Search

- Textsuche in Inhalt,
- Tag Filter,
- Collection Filter.

### Export

- README/Projektmetadaten,
- Entry-Markdown-Dateien,
- Attachment-Kopien.

### Backup / Restore

- Backup-Datei wird erzeugt,
- Live-State wird nach Backup absichtlich verändert,
- Restore stellt alten Zustand wieder her,
- vor Restore wird ein Safety Backup erzeugt.

## 5. Transaktions- und Fehlerpfadtests

Happy-Path-Tests reichen für den Core nicht aus.

Besonders wichtig sind Fehler, bei denen ein Teil einer Operation bereits erfolgreich war.

Beispiele:

- Activity Insert schlägt nach Primärmutation fehl,
- Attachment-Datei wurde kopiert, Metadateninsert bzw. nachfolgender Activity-Insert schlägt fehl,
- Restore-Archiv enthält unerwartete Pfade,
- Concurrency-Version stimmt nicht,
- referenziertes Entry/Project existiert nicht.

Erwartung: Der resultierende Zustand muss dokumentiert und kontrolliert sein; keine stillen Teil-Erfolge.

Der gezielte Attachment-Integrationstest erzwingt deshalb einen Fehler **nach** der Dateikopie und nach dem Metadata-INSERT beim Activity-Write. Erwartet werden SQLite-Rollback und kompensierende Löschung der bereits kopierten Datei.

## 6. Deterministische Zeit

Tests verwenden einen ersetzbaren `IClock` statt direkten Zugriff auf `DateTime.UtcNow` in Core-Abläufen.

Vorteile:

- stabile Erwartungen,
- reproduzierbare Reihenfolge,
- kontrollierte Backup-/Activity-Zeitpunkte,
- keine flakey Zeitvergleiche.

Neue Core-Komponenten sollen bestehendes `IClock` wiederverwenden.

## 7. Temporäre Testdaten

Integration-/Smoke-Tests verwenden pro Lauf einen eindeutigen temporären Root-Ordner.

Regeln:

- keine produktiven Benutzerpfade verwenden,
- keine vorhandene Workbench-Datenbank verändern,
- Cleanup in `Dispose`/`finally`,
- SQLite Connection Pools vor Windows-Datei-Cleanup leeren,
- Test darf bei parallelem Lauf keinen festen gemeinsamen DB-Dateinamen außerhalb seines Root verwenden.

## 8. Migrationstests

Bei jeder neuen Migration muss mindestens geprüft werden:

1. leere DB → aktuelle Version,
2. zweiter Lauf → keine Doppelanwendung,
3. neue Tabellen/Indizes/Constraints nutzbar,
4. bestehende Core-Roundtrips weiterhin grün,
5. Backup/Restore mit aktuellem Schema funktioniert.

Für spätere reale Upgrade-Pfade sollen zusätzlich Fixture-Datenbanken älterer veröffentlichter Versionen eingeführt werden.

## 9. Backup-/Restore-Tests als Pflichtgate

Jede neue persistente Datenquelle muss in die Recovery-Betrachtung aufgenommen werden.

Fragen:

- Liegt sie in SQLite und ist damit im Snapshot?
- Liegt sie im kontrollierten Dateispeicher?
- Muss das Backupformat erweitert werden?
- Erkennt Restore unerwartete Dateien?
- Ist Path Traversal weiterhin ausgeschlossen?

Eine Funktion, deren persistenter Zustand nicht vollständig recoverbar ist, gilt nicht als Core-fertig.

## 10. UI-Tests

V1 verlässt sich für WinForms weiterhin auf Build plus strukturierte manuelle Desktop-Prüfung; es gibt noch keine automatisierte WinForms-UI-Test-Suite.

Die aktuelle Abnahmecheckliste liegt in:

```text
docs/080_V1_Internal_Acceptance_Test.md
```

Sie umfasst insbesondere:

- Project/Entry,
- Templates,
- Tags,
- Attachments,
- Search,
- Collection Membership,
- Relations,
- Activity,
- Markdown Export,
- Backup/Restore,
- Fehlerdialoge und Bedienbarkeit.

Automatisierte UI-Tests werden erst eingeführt, wenn die UI-Struktur stabil genug ist, damit die Tests nicht hauptsächlich Layoutdetails testen.

## 11. CI-Gate

GitHub Actions führt auf Windows mindestens aus:

```powershell
dotnet restore SASD-Workbench.slnx
dotnet build SASD-Workbench.slnx --configuration Release --no-restore

dotnet run --project tests/SASD.Workbench.Domain.Tests/SASD.Workbench.Domain.Tests.csproj --configuration Release --no-build
dotnet run --project tests/SASD.Workbench.Application.Tests/SASD.Workbench.Application.Tests.csproj --configuration Release --no-build
dotnet run --project tests/SASD.Workbench.Infrastructure.Tests/SASD.Workbench.Infrastructure.Tests.csproj --configuration Release --no-build
dotnet run --project tests/SASD.Workbench.SmokeTests/SASD.Workbench.SmokeTests.csproj --configuration Release --no-build
```

Ein PR mit fehlgeschlagenem Release-Build, Layer-Test oder Smoke-Test wird nicht gemergt.

## 12. Regression-Regel

Jeder gefundene reproduzierbare Defekt soll nach Möglichkeit den **kleinsten passenden** Test erhalten, der vor dem Fix fehlschlägt und danach grün ist.

Beispiele aus dem bisherigen Projekt:

- fehlender Separator beim zusammengesetzten Entry-Search-SQL → Infrastructure-Regressionsbereich,
- LIKE-Wildcards aus Benutzereingaben → Infrastructure Search Test,
- SQLite Connection Pooling blockiert temporäre Backup-Datei unter Windows → Infrastructure/Recovery,
- Attachment-Datei wurde kopiert und späterer DB-/Activity-Schritt schlägt fehl → Infrastructure Compensation Test,
- Cross-Project-Relation → Application Test,
- Entry-Versionierung → Domain Test.

Der breite Smoke-Test bleibt wichtig, soll aber nicht zur einzigen Stelle werden, an der jede kleine Regel getestet wird.

## 13. Definition of Done aus Testsicht

Eine persistente Core-Funktion gilt als testseitig fertig, wenn:

- die reine Regel auf der schnellsten passenden Ebene abgesichert ist,
- mindestens der relevante reale Roundtrip geprüft ist,
- Fehler-/Teilfehlerpfad betrachtet wurde,
- Migrationen berücksichtigt wurden,
- Activity History konsistent bleibt,
- Backup/Restore konsistent bleibt,
- Release-Build ohne Warnungen erfolgreich ist,
- CI grün ist.
