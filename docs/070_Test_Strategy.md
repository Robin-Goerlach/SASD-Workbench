# SASD Workbench – Test Strategy

> **Status:** Working Strategy  
> **Stand:** 2026-09-13  
> **Scope:** gemeinsamer Workbench Core, Infrastructure und erster WinForms-Host

## 1. Ziel

Die Teststrategie soll verhindern, dass die gemeinsame Workbench-Codebasis zwar kompiliert, aber bei Migration, Persistenz, Recovery oder Wiederverwendung auseinanderläuft.

Für den Core sind insbesondere wichtig:

- reale SQLite-Roundtrips,
- Migrationen,
- Concurrency,
- kontrollierter File Storage,
- Backup/Restore,
- transaktionale Activity History,
- profile-neutrale Domain-/Application-Regeln,
- identische Dependency-Injection-Konfiguration in Host und Tests.

## 2. Aktueller Teststand

Aktuell existiert ein ausführlicher ausführbarer End-to-End-Smoke-Test:

```text
tests/SASD.Workbench.SmokeTests
```

Er verwendet:

- die produktive `AddSasdWorkbenchCore(...)` Registrierung,
- eine temporäre reale SQLite-Datenbank,
- einen temporären kontrollierten Datenordner,
- einen deterministischen Test-`IClock`,
- echte File-I/O für Attachments, Export und Backup/Restore.

Der Smoke-Test ist bewusst kein Mock-Test.

## 3. Testpyramide – Zielbild

Die Testlandschaft soll schrittweise in drei Ebenen wachsen.

### Ebene A – schnelle Unit Tests

Für reine Domain-/Application-Regeln ohne SQLite oder Dateisystem.

Beispiele:

- Entry-/Project-Validierung,
- Relation-Key-Normalisierung,
- Core-Vokabulare,
- Template-Definitionen,
- Status-/Archivierungsregeln,
- profilneutrale Application-Validierung mit Fakes.

Ziel: sehr schnell, viele Randfälle.

### Ebene B – Infrastructure Integration Tests

Für SQLite, Migrationen und File Storage.

Beispiele:

- Repository-Roundtrips,
- optimistic concurrency,
- Foreign Keys,
- Migration von leerer DB bis aktuelle Version,
- idempotenter zweiter Migration-Lauf,
- Activity + Mutation in gemeinsamer Transaktion,
- Attachment Storage und SHA-256,
- Backup-Archivvalidierung,
- Restore und Safety Backup,
- Schutz gegen Path Traversal.

Ziel: technische Verträge der gemeinsamen Infrastructure absichern.

### Ebene C – End-to-End Smoke Tests

Wenige, breite Kernabläufe durch echte Core-Komposition.

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
- Backup enthält Datei,
- Restore stellt Metadaten und Datei wieder her.

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
- Attachment-Datei wurde kopiert, Metadateninsert schlägt fehl,
- Restore-Archiv enthält unerwartete Pfade,
- Concurrency-Version stimmt nicht,
- referenziertes Entry/Project existiert nicht.

Erwartung: Der resultierende Zustand muss dokumentiert und kontrolliert sein; keine stillen Teil-Erfolge.

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
- Cleanup im `finally`,
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

V1 verlässt sich derzeit auf Build plus manuelle Desktop-Prüfung; es gibt noch keine automatisierte WinForms-UI-Test-Suite.

Vor einem breiteren Produktiv-/Release-Einsatz sollen mindestens folgende manuelle Szenarien als Checkliste existieren:

- Project/Entry anlegen und bearbeiten,
- Search Dialog,
- Collection Membership,
- Relation anlegen/löschen,
- Activity anzeigen,
- Markdown Export,
- Backup,
- Restore nach sichtbarer Datenänderung,
- Fehlerdialog bei ungültiger Eingabe.

Automatisierte UI-Tests werden erst eingeführt, wenn die UI-Struktur stabil genug ist, damit die Tests nicht hauptsächlich Layoutdetails testen.

## 11. CI-Gate

GitHub Actions führt auf Windows mindestens aus:

```powershell
dotnet restore SASD-Workbench.slnx
dotnet build SASD-Workbench.slnx --configuration Release --no-restore
dotnet run --project tests/SASD.Workbench.SmokeTests/SASD.Workbench.SmokeTests.csproj --configuration Release --no-build
```

Ein PR mit fehlgeschlagenem Release-Build oder Smoke-Test wird nicht gemergt.

## 12. Ausbau der Testprojekte

Empfohlene nächste Struktur:

```text
tests/
  SASD.Workbench.Domain.Tests/
  SASD.Workbench.Application.Tests/
  SASD.Workbench.Infrastructure.Tests/
  SASD.Workbench.SmokeTests/
```

Die neuen Projekte sollen erst eingeführt werden, wenn sie echte Tests aufnehmen; leere Testprojekte bringen keinen Qualitätsgewinn.

## 13. Regression-Regel

Jeder gefundene reproduzierbare Defekt soll nach Möglichkeit einen Test erhalten, der vor dem Fix fehlschlägt und danach grün ist.

Beispiele aus dem bisherigen Projekt:

- fehlender Separator beim zusammengesetzten Entry-Search-SQL,
- SQLite Connection Pooling blockiert temporäre Backup-Datei unter Windows.

Solche Fehler sind besonders wertvoll als Regressionstests, weil sie plattform- oder implementationsspezifische Randbedingungen dokumentieren.

## 14. Definition of Done aus Testsicht

Eine persistente Core-Funktion gilt als testseitig fertig, wenn:

- mindestens der relevante reale Roundtrip geprüft ist,
- Fehler-/Teilfehlerpfad betrachtet wurde,
- Migrationen berücksichtigt wurden,
- Activity History konsistent bleibt,
- Backup/Restore konsistent bleibt,
- Release-Build ohne Warnungen erfolgreich ist,
- CI grün ist.
