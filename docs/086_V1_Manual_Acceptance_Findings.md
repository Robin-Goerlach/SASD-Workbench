# SASD Workbench – V1 Manual Acceptance Findings

> **Status:** Empty findings log / ready for first execution  
> **Stand:** 2026-09-13  
> **Basis:** `080_V1_Internal_Acceptance_Test.md`

## Zweck

Dieses Dokument ist die dauerhafte Ablage für Findings aus dem strukturierten V1-Desktop-Test. Die Testcheckliste beschreibt, **was** geprüft wird; dieses Dokument hält fest, **was tatsächlich beobachtet wurde**.

Die Datei bleibt vor der ersten manuellen Abnahme bewusst ohne erfundene PASS/FAIL-Ergebnisse. Automatisierte CI-Ergebnisse dürfen nicht als Ersatz für die reale Desktop-Beobachtung eingetragen werden.

## Testlauf

```text
Testdatum:
Tester:
Commit / Version:
Windows-Version:
.NET-Version:
Isolierter Workbench-Datenpfad:
Startbefehl:
CI-Status des getesteten Commits:
```

## Summary

```text
PASS:
FAIL:
BLOCKED:
NOTE:
Critical offen:
High offen:
Medium offen:
Low offen:
UX offen:
```

## Findings

Für jedes Finding einen eigenen Abschnitt nach diesem Muster anlegen:

```text
### V1-AT-XXX-F01 – Kurztitel

Testfall: AT-XXX
Status: FAIL / BLOCKED / NOTE
Schweregrad: Critical / High / Medium / Low / UX
Reproduzierbar: Ja / Nein / Unklar

Beobachtung:

Erwartung:

Reproduktionsschritte:

Betroffene Daten / Recovery-Risiko:

Screenshot / Log / Backup-Pfad optional:

Entscheidung:

Fix / PR / Commit:

Retest:
```

## Priorisierungsregel

- **Critical:** akuter Datenverlust, falscher Restore oder Sicherheitsproblem; V1-Abnahme stoppen.
- **High:** wesentlicher Kernworkflow unzuverlässig oder Lost-Update-/Recovery-Risiko; vor interner V1-Abnahme schließen.
- **Medium:** funktionaler Defekt mit Workaround, ohne unmittelbares Datenintegritätsrisiko.
- **Low:** kleiner funktionaler Defekt mit begrenzter Auswirkung.
- **UX:** Bedienbarkeit, Benennung, Layout oder Rückmeldung; priorisieren, wenn Missverständnisse Datenverlust begünstigen könnten.

## Exit-Regel

V1 wird erst als intern abgenommen bezeichnet, wenn die Checkliste tatsächlich auf einem realen Windows-Desktop ausgeführt wurde, alle Findings hier erfasst/priorisiert sind und kein Critical-/High-Finding mehr offen ist.
