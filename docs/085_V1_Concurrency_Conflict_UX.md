# SASD Workbench – V1 Concurrency Conflict UX

> **Status:** V1 host behavior  
> **Stand:** 2026-09-13  
> **Scope:** WinForms Entry editor

## Ziel

Die gemeinsame Optimistic-Concurrency-Garantie verhindert Lost Updates bereits in Application und SQLite. Der Desktop-Host muss denselben Konflikt zusätzlich so darstellen, dass der Benutzer seine noch nicht gespeicherten Änderungen nicht versehentlich verliert.

## Verhalten bei einem stale Entry

Wenn ein Entry im Editor auf einer älteren Version basiert und ein anderer Writer inzwischen eine neuere Version gespeichert hat:

1. der Save wird abgewiesen;
2. der neuere persistierte Stand wird nicht überschrieben;
3. der Desktop lädt den Entry **nicht automatisch neu**;
4. Titel, Summary, Type, Status und Markdown bleiben deshalb zunächst so im Editor stehen, wie der Benutzer sie eingegeben hat;
5. eine Warnung erklärt, dass die Änderungen nicht gespeichert wurden und der gespeicherte Entry inzwischen neuer ist;
6. der Benutzer soll benötigten Text sichern, anschließend bewusst Refresh/Neuauswahl ausführen, die Unterschiede vergleichen bzw. zusammenführen und erst gegen die aktuelle Version erneut speichern.

Ein automatischer Retry mit denselben stale Feldern ist ausdrücklich kein zulässiges Konfliktverhalten.

## Warum kein automatischer Reload?

Ein sofortiger Reload wäre technisch bequem, würde aber genau die lokalen Änderungen überschreiben, die durch die Concurrency-Sperre vor einem stillen Lost Update geschützt wurden. Die UI priorisiert deshalb Datenintegrität und die Möglichkeit zur manuellen Übernahme vor Komfort.

## Warum kein automatischer Merge in V1?

Ein sinnvoller Merge ist für freie Markdown-Inhalte, Titel und Metadaten nicht immer eindeutig. Ein vorschneller automatischer Merge könnte fachliche Änderungen falsch kombinieren. V1 zeigt deshalb den Konflikt klar an und lässt die Entscheidung beim Benutzer.

Eine spätere Version kann eine Compare-/Merge-Ansicht ergänzen, ohne den zugrunde liegenden Application-Vertrag zu ändern.

## Testabdeckung

Automatisierte Host-Tests prüfen die nichtvisuelle Text-/Interaktionsregel für:

- bekannte aktuelle Version (Application erkennt stale Caller bereits vor dem Write),
- unbekannte aktuelle Version (Konflikt entsteht erst beim atomaren SQLite-UPDATE),
- Null-Argument-Validierung des Presenters.

Die tatsächliche MessageBox und das Erhalten des sichtbaren Editorinhalts bleiben Teil von `docs/080_V1_Internal_Acceptance_Test.md`, da V1 bewusst noch keine WinForms-UI-Automation besitzt.
