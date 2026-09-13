# SASD Workbench – V1 Internal Acceptance Test

> **Status:** Prepared / not yet executed  
> **Stand:** 2026-09-13  
> **Zweck:** strukturierter interner Nutzertest vor weiterer V2-Entwicklung

## 1. Ziel

Dieser Test prüft die V1 Workbench aus Anwendersicht auf einem realen Windows-Desktop. Er ergänzt CI und den automatischen Core-Smoke-Test; er ersetzt sie nicht.

Schwerpunkte:

- verständlicher Grundworkflow,
- Daten bleiben nach Neustart erhalten,
- Templates, Tags und Attachments sind praktisch nutzbar,
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

### AT-013 – stale Editor darf neueren Stand nicht überschreiben

Dieser Test verwendet bewusst zwei gleichzeitig gestartete Workbench-Instanzen mit demselben **Test-Datenbestand**.

1. Workbench A und Workbench B starten.
2. In beiden Instanzen denselben `First acceptance note` auswählen, sodass beide denselben Ausgangsstand geladen haben.
3. In Workbench A Titel oder Markdown deutlich ändern und speichern.
4. In Workbench B **ohne vorherigen Reload** einen anderen Text eingeben und speichern.

Erwartung:

- Save in Workbench A funktioniert.
- Workbench B überschreibt den neueren Stand aus A **nicht**.
- Workbench B zeigt einen verständlichen Concurrency-/Reload-Hinweis statt einer SQLite-spezifischen Fehlermeldung.
- der ungespeicherte Text aus Workbench B bleibt im Editor sichtbar und kann kopiert bzw. manuell mit dem neueren Stand zusammengeführt werden.
- erneutes Laden zeigt den zuletzt erfolgreich persistierten Stand aus Workbench A.
- erst nach bewusster Übernahme/Merge darf ein neuer Save gegen die aktuelle Version erfolgen.

Wenn parallele Instanzen auf der Testmaschine aus einem anderen Grund nicht möglich sind, Test als `BLOCKED` dokumentieren; die Core-Garantie wird zusätzlich automatisiert auf Application- und SQLite-Ebene geprüft.

## 6. Templates

### AT-020 – aktuellen Entry als projektlokales Template speichern

1. `First acceptance note` auswählen.
2. `Templates` öffnen.
3. `Save current entry as template...` verwenden.
4. Name `Acceptance local template` eingeben.
5. Profilweite Option **nicht** aktivieren.

Erwartung:

- Template erscheint in der Liste.
- Scope ist als aktuelles Projekt erkennbar.
- Entry Type, Default Status und Beschreibung/Inhalt entsprechen dem Ausgangs-Entry.

### AT-021 – Entry aus Template erzeugen

1. `Acceptance local template` auswählen.
2. Titel `Created from acceptance template` eingeben.
3. Entry erzeugen.

Erwartung:

- Dialog schließt erfolgreich.
- neuer Entry ist ausgewählt.
- Type/Status/Inhalt wurden aus dem Template kopiert.
- spätere Änderungen am Entry verändern das Template nicht automatisch.

### AT-022 – profilweites Template

1. einen geeigneten Entry auswählen.
2. erneut als Template speichern.
3. Option `Available to all projects in profile ...` aktivieren.
4. zweites Projekt mit demselben Profil anlegen.
5. Templates dort öffnen.

Erwartung:

- profilweites Template ist auch im zweiten Projekt sichtbar.
- projektlokales Template aus AT-020 ist dort **nicht** sichtbar.

### AT-023 – Template löschen

Ein selbst angelegtes Template löschen.

Erwartung:

- Löschwarnung beschreibt den Scope.
- Template verschwindet aus der Auswahl.
- bereits daraus erzeugte Entries bleiben unverändert erhalten.

## 7. Tags

### AT-030 – Tag anlegen und zuweisen

Am `First acceptance note`:

```text
Tag: acceptance
```

über `Create + assign` anlegen.

Erwartung:

- Tag erscheint und ist angehakt.
- erneutes Öffnen zeigt die Zuordnung weiterhin.

### AT-031 – vorhandenen Tag einem zweiten Entry zuordnen

1. `Created from acceptance template` auswählen.
2. Tags öffnen.
3. vorhandenen Tag `acceptance` anhaken.
4. Assignments übernehmen.

Erwartung:

- derselbe Tag wird wiederverwendet; kein zweiter gleichnamiger Tag nötig.

### AT-032 – Tag-Zuordnung entfernen

`acceptance` beim zweiten Entry abwählen und übernehmen.

Erwartung:

- Zuordnung zum zweiten Entry verschwindet.
- Tag bleibt global vorhanden.
- Zuordnung zum ersten Entry bleibt unverändert.

## 8. Attachments

Vorbereitung: kleine lokale Testdatei, z. B. `acceptance-attachment.txt`, mit eindeutigem Inhalt anlegen.

### AT-040 – Attachment hinzufügen

1. `First acceptance note` auswählen.
2. `Attachments` öffnen.
3. Testdatei hinzufügen.
4. Kommentar `Initial acceptance attachment` eintragen.

Erwartung:

- Attachment erscheint mit Dateiname, Größe, Kommentar und SHA-256.
- Originaldatei bleibt an ihrem ursprünglichen Ort unverändert.

### AT-041 – Attachment-Kommentar bearbeiten

Kommentar ändern auf:

```text
Updated acceptance attachment comment
```

Erwartung:

- Änderung erscheint nach Reload/erneutem Öffnen weiterhin.
- Dateiname und SHA-256 ändern sich dadurch nicht.

### AT-042 – Attachment entfernen

Attachment über `Remove from entry` entfernen.

Erwartung:

- Warnung erklärt den Soft-Delete/Recovery-Charakter.
- Attachment verschwindet aus der aktiven Liste.
- UI behauptet nicht, dass die physische Datei sofort sicher gelöscht wurde.

Für den späteren Backup-/Restore-Test anschließend ein Attachment erneut hinzufügen und **nicht** entfernen.

## 9. Search

### AT-050 – Textsuche

Suche nach:

```text
BlueOrchid-4711
```

Erwartung:

- `First acceptance note` wird gefunden.
- irrelevante Entries werden nicht als Treffer benötigt.

### AT-051 – Type-/Status-Filter

Filter nacheinander auf vorhandenen Type und Status setzen.

Erwartung:

- Ergebnis passt zum ausgewählten Projekt und Filter.
- Dialog bleibt verständlich, wenn kein Treffer existiert.

## 10. Collections

### AT-060 – Collection-Hierarchie

Anlegen:

```text
Research
  Sources
```

Erwartung:

- beide Collections erscheinen.
- Parent/Child-Zuordnung ist erkennbar.

### AT-061 – Mehrfachzuordnung

1. aktuellen Entry `Research` zuordnen.
2. denselben Entry zusätzlich `Sources` zuordnen.

Erwartung:

- Entry kann beiden Collections gleichzeitig angehören.
- keine implizite Verschiebe-Semantik.

### AT-062 – Zuordnung entfernen

Eine Membership entfernen.

Erwartung:

- nur die gewählte Membership verschwindet.
- andere Membership bleibt bestehen.

## 11. Relations

### AT-070 – zweiten Entry als Relation-Ziel verwenden

Den vorhandenen zweiten Entry verwenden oder anlegen:

```text
Title: Supporting note
Type: finding
Status: draft
```

### AT-071 – Relation anlegen

Von `First acceptance note` zu `Supporting note`:

```text
Relation: supports
Comment: Acceptance relation
```

Erwartung:

- Relation erscheint in der Relations-Ansicht.
- Richtung Quelle → Ziel ist verständlich.

### AT-072 – eingehende Relation

`Supporting note` auswählen und Relations öffnen.

Erwartung:

- dieselbe Relation ist als eingehende Relation erkennbar.

### AT-073 – Relation löschen

Relation löschen.

Erwartung:

- Relation verschwindet aus beiden Ansichten.
- Entries selbst bleiben unverändert.

## 12. Activity History

### AT-080 – automatische Activities

Activity History für das Testprojekt öffnen.

Erwartung:

- Projekt-/Entry-Erstellung ist nachvollziehbar.
- projektlokale Template-Erstellung/-Löschung ist nachvollziehbar.
- Tag-Zuordnungen, Attachment-Metadatenänderungen, Collection-/Membership- und Relation-Aktionen sind nachvollziehbar.
- History wird nicht als „Audit Trail“ oder manipulationssicher dargestellt.

Hinweis: rein globale/profilweite Template- oder Tag-Erzeugung kann ohne konkrete Project-ID protokolliert sein und muss deshalb nicht in einer **projektgefilterten** Activity-Ansicht erscheinen. Die konkrete Zuordnung zu einem Entry besitzt dagegen Projektkontext.

### AT-081 – Idempotenz beobachten

Eine bereits bestehende Membership/Tag-Zuordnung ohne echte Änderung erneut ausführen, soweit die UI dies erlaubt.

Erwartung:

- kein irreführender zweiter automatischer History-Eintrag für dieselbe No-op-Mutation.

## 13. Markdown Export

### AT-090 – Projekt exportieren

Projekt in einen neuen leeren Zielordner exportieren.

Erwartung:

- eigener Exportordner wird erzeugt.
- `README.md` vorhanden.
- Entry-Dateien vorhanden.
- Markdown mit normalem Texteditor lesbar.
- Metadaten sind nachvollziehbar.
- aktives Attachment wird kopiert.
- Attachment-Kommentar/Hash-Metadaten sind nachvollziehbar, soweit im Exportformat vorgesehen.

### AT-091 – Export außerhalb der Workbench lesen

Workbench schließen und Exportdateien separat öffnen.

Erwartung:

- Kerninformationen bleiben ohne die Anwendung verständlich.

## 14. Backup und Restore

### AT-100 – Full Backup

Backup über die Desktop-Funktion erzeugen.

Erwartung:

- ZIP-Datei wird erzeugt.
- Anwendung meldet Erfolg verständlich.

Pfad des Backups notieren.

### AT-101 – sichtbare Änderungen nach Backup

Nach dem Backup mindestens zwei gut sichtbare Änderungen durchführen, z. B.:

- einen Entry deutlich umbenennen oder löschen,
- Attachment-Kommentar ändern oder Attachment entfernen,
- optional einen zusätzlichen Entry anlegen.

Erwartung:

- Änderungen sind sichtbar und nach normalem Refresh/Neustart vorhanden.

### AT-102 – Restore

Das zuvor erzeugte Backup auswählen.

Erwartung:

- Anwendung warnt vor dem Zustandstausch.
- Restore wird nur nach bewusster Bestätigung ausgeführt.
- Safety-Backup des ersetzten Zustands wird gemeldet/erzeugt.
- Projektzustand entspricht danach dem Zeitpunkt von AT-100.
- Änderungen aus AT-101 sind nicht mehr im restaurierten Live-Zustand.
- Attachment samt Kommentar/Datei entspricht wieder dem Backup-Zeitpunkt.
- Activity History entspricht ebenfalls dem Backup-Zeitpunkt.

### AT-103 – Neustart nach Restore

Anwendung schließen und erneut starten.

Erwartung:

- restaurierter Zustand bleibt stabil.
- keine Migration-/SQLite-Fehler.

## 15. Fehler- und UX-Prüfung

### AT-110 – ungültige Pflichtangabe

Versuchen, ein Objekt mit leerem Pflichtnamen/-titel anzulegen, soweit UI dies zulässt.

Erwartung:

- verständliche Meldung.
- keine halbfertige persistente Zeile.

### AT-111 – Dialog-Abbruch

Create-/Template-/Attachment-/Restore-/Export-Dialoge jeweils abbrechen.

Erwartung:

- keine unbeabsichtigte Mutation.
- keine Fehlermeldung für einen normalen Benutzerabbruch.

### AT-112 – Fenstergrößen / Bedienbarkeit

Bei normaler Desktop-Auflösung prüfen:

- Texte abgeschnitten?
- Buttons erreichbar?
- Listen sinnvoll scrollbar?
- Toolstrip auch bei kleinerem Fenster sinnvoll nutzbar?
- Dialoge verständlich beschriftet?
- kritische Aktionen ausreichend deutlich?

## 16. Explorative Phase

Nach den festen Fällen 15–30 Minuten frei mit der Anwendung arbeiten:

- mehrere Projects,
- viele Entries,
- mehrere Templates/Tags/Attachments,
- längere Markdown-Texte,
- schnelle Selektionswechsel,
- wiederholtes Öffnen/Schließen der Dialoge,
- Kombination Template → Entry → Tags/Attachment → Search → Relation/Collection.

Jede Überraschung als Finding erfassen, auch wenn sie kein technischer Fehler ist.

## 17. Exit-Kriterien für V1 intern

V1 kann als intern belastbar betrachtet werden, wenn:

- kein Critical-/High-Finding offen ist,
- Backup/Restore einschließlich Neustart bestanden hat,
- Datenpersistenz nach Neustart bestanden hat,
- stale Editoren keinen neueren Stand still überschreiben,
- Templates/Tags/Attachments praktisch nutzbar sind,
- Search/Collections/Relations praktisch nutzbar sind,
- automatische Activity History plausibel ist,
- alle Findings priorisiert und im Repository nachvollziehbar festgehalten sind,
- CI des getesteten Commits grün ist.

Medium-/Low-/UX-Findings dürfen bewusst in V1.1 verschoben werden, wenn sie dokumentiert sind und weder Datenverlust noch Architekturbruch verursachen.
