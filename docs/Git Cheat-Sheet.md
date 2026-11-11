# Git Cheat-Sheet

Nov 11, 2026 · @Elias

## Der normale Ablauf

So sieht ein typischer Arbeitstag aus, alles direkt auf `main`:

```bash
git pull                      # neuste Version von GitHub holen
# ... Code ändern ...
git status                    # schauen, was sich geändert hat
git add .                     # alle Änderungen vormerken
git commit -m "was ich gemacht hab"
git push                      # auf GitHub hochladen
```

Faustregel: **Immer zuerst pullen, dann arbeiten, dann pushen.** So gibt es am wenigsten Konflikte.

## Projekt starten

| Befehl | Was es macht |
| --- | --- |
| `git clone <url>` | Repo von GitHub herunterladen |
| `git init` | Neues Repo im aktuellen Ordner anlegen |
| `git remote add origin <url>` | Lokales Repo mit GitHub verbinden |
| `git push -u origin main` | Erster Push, danach reicht `git push` |

## Änderungen speichern

| Befehl | Was es macht |
| --- | --- |
| `git status` | Zeigt geänderte, neue und vorgemerkte Dateien |
| `git add <datei>` | Eine Datei vormerken |
| `git add .` | Alles im Ordner vormerken |
| `git commit -m "Text"` | Vorgemerkte Änderungen als Commit speichern |
| `git commit --amend -m "Text"` | Letzten Commit ändern (nur wenn noch nicht gepusht) |
| `git diff` | Zeigt, was sich genau geändert hat (noch nicht vorgemerkt) |
| `git diff --staged` | Zeigt, was vorgemerkt ist |

## Pushen und Pullen

| Befehl | Was es macht |
| --- | --- |
| `git push` | Deine Commits auf GitHub hochladen |
| `git pull` | Neuste Commits von GitHub holen und einbauen |
| `git fetch` | Nur herunterladen, noch nichts einbauen |
| `git pull --no-rebase -X theirs origin main` | Pullen, bei Konflikten gewinnt GitHub |
| `git pull --no-rebase -X ours origin main` | Pullen, bei Konflikten gewinnt deine lokale Version |

**Push wird abgelehnt ("rejected")?** Dann hat GitHub Commits, die du lokal nicht hast. Zuerst `git pull`, dann nochmals `git push`. Nicht einfach `--force` nehmen, das überschreibt die Arbeit von anderen.

## Konflikte lösen

Wenn Git meldet `CONFLICT`, stehen in der Datei solche Markierungen:

```
<<<<<<< HEAD
deine Version
=======
die andere Version
>>>>>>> origin/main
```

1. Datei öffnen und entscheiden, was bleiben soll (Markierungen löschen)
2. `git add <datei>`
3. `git commit`
4. `git push`

Abbrechen, wenn's schiefgeht: `git merge --abort`

## Verlauf anschauen

| Befehl | Was es macht |
| --- | --- |
| `git log --oneline` | Kurze Liste aller Commits |
| `git log --oneline --graph` | Mit Verzweigungen als Grafik |
| `git show <commit>` | Details zu einem Commit |

## Rückgängig machen

| Befehl | Was es macht |
| --- | --- |
| `git restore <datei>` | Änderungen an einer Datei verwerfen (noch nicht committet) |
| `git restore --staged <datei>` | Datei wieder aus dem Vormerken nehmen |
| `git reset --soft HEAD~1` | Letzten Commit aufheben, Änderungen bleiben |
| `git revert <commit>` | Neuer Commit, der einen alten rückgängig macht (sicher, auch nach Push) |
| `git stash` | Änderungen kurz zur Seite legen |
| `git stash pop` | Zur Seite gelegte Änderungen zurückholen |

Vorsicht: `git reset --hard` löscht ungespeicherte Änderungen endgültig.

## Branches (falls du doch mal einen brauchst)

| Befehl | Was es macht |
| --- | --- |
| `git branch` | Alle lokalen Branches anzeigen |
| `git switch -c <name>` | Neuen Branch erstellen und wechseln |
| `git switch main` | Zurück auf main |
| `git merge <name>` | Branch in den aktuellen Branch mergen |
| `git branch -d <name>` | Branch löschen |

## ours und theirs merken

- **Merge / Pull:** `ours` = wo du gerade bist (lokal), `theirs` = was reinkommt (GitHub)
- **Rebase:** genau umgekehrt, darum bei `pull` immer `--no-rebase` dazuschreiben, wenn du `-X` brauchst
- `-X ours` / `-X theirs` = nur bei Konflikten eine Seite bevorzugen
- `-s ours` = die andere Seite **komplett** ignorieren (fast nie das, was man will)

## Einmalig einrichten

```bash
git config --global user.name "Elias Wyss"
git config --global user.email "deine@mail.ch"
git config --global pull.rebase false   # pull macht immer einen Merge
```
