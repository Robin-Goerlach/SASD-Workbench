# SASD Workbench – V1 Internal Acceptance Test

> **Status:** Prepared / not yet executed  
> **Stand:** 2026-09-13  
> **Zweck:** strukturierter interner Nutzertest vor weiterer V2-Entwicklung

## 1. Ziel

Dieser Test prüft die V1 Workbench aus Anwendersicht auf einem realen Windows-Desktop. Er ergänzt CI und den automatischen Core-Smoke-Test; er ersetzt sie nicht.

Schwerpunkte:

- verständlicher Grundworkflow,
- Daten bleiben nach Neustart erhalten,
- Search / Collections / Relations sind praktisch bedienbar,
- Activity History ist nachvollziehbar,
- Export ist außerhalb der Anwendung lesbar,
- Backup/Restore ist für den Anwender kontrollierbar,
- Fehler und Bestätigungen sind verständlich.

## 2. Sicherheitsregel

Für diesen Test ausschließlich einen **separaten Test-Datenbestand** verwenden.

Nicht auf einem produktiv genutzten Workbench-Datenbestand testen, insbesondere nicht beim Restore-Szenario.

Vor dem Test dokumentieren:

```text
Testdatum:
Commit / Version:
Windows-Version:
.NET-Version:
Workbench-Datenpfad:
Tester:
```

## 3. Ergebnis-Kategorien

Jeder Testpunkt erhält genau einen Status:

```text
PASS    Erwartung erfüllt
FAIL    Erwartung nicht erfüllt
BLOCKED Test kann wegen eines anderen Fehlers nicht sinnvoll durchgeführt werden
NOTE    kein Fehler, aber UX-/Verbesserungshinweis
```

Findings sollen enthalten:

```text
ID:
Testfall:
Status:
Beobachtung:
Erwartung:
Reproduzierbar:
Schweregrad: Critical / High / Medium / Low / UX
Screenshot/Log optional:
```

## 4. Start und leerer Datenbestand

### AT-001 – Erststart

1. Test-Datenbestand leer bereitstellen.
2. Workbench starten.

Erwartung:

- Anwendung startet ohne Ausnahme.
- leere Projekt-/Entry-Ansicht ist verständlich.
- Datenpfad wird sichtbar bzw. nachvollziehbar angezeigt.
- keine technischen SQLite-Fehler werden gezeigt.

### AT-002 – Neustart

1. Anwendung schließen.
2. erneut starten.

Erwartung:

- Start weiterhin fehlerfrei.
- keine doppelten Migrationen oder Startartefakte sichtbar.

## 5. Projects und Entries

### AT-010 – Projekt anlegen

Projekt:

```text
Name: V1 Acceptance Project
Description: Internal Workbench acceptance test
```

Erwartung:

- Projekt erscheint in der Liste.
- Auswahl funktioniert.

### AT-011 – Entry anlegen und speichern

Entry:

```text
Title: First acceptance note
Type: note
Status: draft
Summary: Acceptance smoke note
Content: # Acceptance\n\nSearch phrase: BlueOrchid-4711
```

Erwartung:

- Entry erscheint.
- Speichern funktioniert.
- erneute Auswahl zeigt denselben Inhalt.

### AT-012 – Persistenz nach Neustart

1. Anwendung schließen.
2. erneut starten.
3. Projekt und Entry auswählen.

Erwartung:

- Projekt und Entry vollständig vorhanden.
- Text `BlueOrchid-4711` unverändert vorhanden.

## 6. Search

### AT-020 – Textsuche

Suche nach:

```text
BlueOrchid-4711
```

Erwartung:

- `First acceptance note` wird gefunden.
- irrelevante Entries werden nicht als Treffer benötigt.

### AT-021 – Type-/Status-Filter

Filter nacheinander auf vorhandenen Type und Status setzen.

Erwartung:

- Ergebnis passt zum ausgewählten Projekt und Filter.
- Dialog bleibt verständlich, wenn kein Treffer existiert.

## 7. Collections

### AT-030 – Collection-Hierarchie

Anlegen:

```text
Research
  Sources
```

Erwartung:

- beide Collections erscheinen.
- Parent/Child-Zuordnung ist erkennbar.

### AT-031 – Mehrfachzuordnung

1. aktuellen Entry `Research` zuordnen.
2. denselben Entry zusätzlich `Sources` zuordnen.

Erwartung:

- Entry kann beiden Collections gleichzeitig angehören.
- keine implizite Verschiebe-Semantik.

### AT-032 – Zuordnung entfernen

Eine Membership entfernen.

Erwartung:

- nur die gewählte Membership verschwindet.
- andere Membership bleibt bestehen.

## 8. Relations

### AT-040 – Zweiten Entry anlegen

```text
Title: Supporting note
Type: finding
Status: draft
```

### AT-041 – Relation anlegen

Von `First acceptance note` zu `Supporting note`:

```text
Relation: supports
Comment: Acceptance relation
```

Erwartung:

- Relation erscheint in der Relations-Ansicht.
- Richtung Quelle → Ziel ist verständlich.

### AT-042 – Eingehende Relation

`Supporting note` auswählen und Relations öffnen.

Erwartung:

- dieselbe Relation ist als eingehende Relation erkennbar.

### AT-043 – Relation löschen

Relation löschen.

Erwartung:

- Relation verschwindet aus beiden Ansichten.
- Entries selbst bleiben unverändert.

## 9. Activity History

### AT-050 – automatische Activities

Activity History für das Testprojekt öffnen.

Erwartung:

- Projekt-/Entry-Erstellung ist nachvollziehbar.
- Collection-/Membership-/Relation-Aktionen, die tatsächlich durchgeführt wurden, sind nachvollziehbar.
- gelöschte Relation erscheint als entsprechende History, sofern Aktion nach aktuellem Implementierungsstand protokolliert wird.
- History wird nicht als „Audit Trail“ oder manipulationssicher dargestellt.

### AT-051 – Idempotenz beobachten

Eine bereits bestehende Membership/Zuordnung ohne echte Änderung erneut ausführen, soweit die UI dies erlaubt.

Erwartung:

- kein irreführender zweiter automatischer History-Eintrag für dieselbe No-op-Mutation.

## 10. Markdown Export

### AT-060 – Projekt exportieren

Projekt in einen neuen leeren Zielordner exportieren.

Erwartung:

- eigener Exportordner wird erzeugt.
- `README.md` vorhanden.
- Entry-Dateien vorhanden.
- Markdown mit normalem Texteditor lesbar.
- Metadaten sind nachvollziehbar.

### AT-061 – Export außerhalb der Workbench lesen

Workbench schließen und Exportdateien separat öffnen.

Erwartung:

- Kerninformationen bleiben ohne die Anwendung verständlich.

## 11. Backup und Restore

### AT-070 – Full Backup

Backup über die Desktop-Funktion erzeugen.

Erwartung:

- ZIP-Datei wird erzeugt.
- Anwendung meldet Erfolg verständlich.

Pfad des Backups notieren.

### AT-071 – sichtbare Änderung nach Backup

Nach dem Backup:

- einen Entry deutlich umbenennen oder löschen,
- optional einen zusätzlichen Entry anlegen.

Erwartung:

- Änderung ist sichtbar und nach normalem Refresh/Neustart vorhanden.

### AT-072 – Restore

Das zuvor erzeugte Backup auswählen.

Erwartung:

- Anwendung warnt vor dem Zustandstausch.
- Restore wird nur nach bewusster Bestätigung ausgeführt.
- Safety-Backup des ersetzten Zustands wird gemeldet/erzeugt.
- Projektzustand entspricht danach dem Zeitpunkt von AT-070.
- Änderungen aus AT-071 sind nicht mehr im restaurierten Live-Zustand.
- Activity History entspricht ebenfalls dem Backup-Zeitpunkt.

### AT-073 – Neustart nach Restore

Anwendung schließen und erneut starten.

Erwartung:

- restaurierter Zustand bleibt stabil.
- keine Migration-/SQLite-Fehler.

## 12. Fehler- und UX-Prüfung

### AT-080 – ungültige Pflichtangabe

Versuchen, ein Objekt mit leerem Pflichtnamen/-titel anzulegen, soweit UI dies zulässt.

Erwartung:

- verständliche Meldung.
- keine halbfertige persistente Zeile.

### AT-081 – Dialog-Abbruch

Create-/Restore-/Export-Dialoge jeweils abbrechen.

Erwartung:

- keine Mutation.
- keine Fehlermeldung für einen normalen Benutzerabbruch.

### AT-082 – Fenstergrößen / Bedienbarkeit

Bei normaler Desktop-Auflösung prüfen:

- Texte abgeschnitten?
- Buttons erreichbar?
- Listen sinnvoll scrollbar?
- Dialoge verständlich beschriftet?
- kritische Aktionen ausreichend deutlich?

## 13. Explorative Phase

Nach den festen Fällen 15–30 Minuten frei mit der Anwendung arbeiten:

- mehrere Projects,
- viele Entries,
- längere Markdown-Texte,
- schnelle Selektionswechsel,
- wiederholtes Öffnen/Schließen der Dialoge,
- Kombination Search → Entry → Relation/Collection.

Jede Überraschung als Finding erfassen, auch wenn sie kein technischer Fehler ist.

## 14. Exit-Kriterien für V1 intern

V1 kann als intern belastbar betrachtet werden, wenn:

- kein Critical-/High-Finding offen ist,
- Backup/Restore einschließlich Neustart bestanden hat,
- Datenpersistenz nach Neustart bestanden hat,
- Search/Collections/Relations praktisch nutzbar sind,
- automatische Activity History plausibel ist,
- alle Findings priorisiert und im Repository nachvollziehbar festgehalten sind,
- CI des getesteten Commits grün ist.

Medium-/Low-/UX-Findings dürfen bewusst in V1.1 verschoben werden, wenn sie dokumentiert sind und weder Datenverlust noch Architekturbruch verursachen.
