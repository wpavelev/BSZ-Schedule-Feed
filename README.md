# BszScheduleFeed

Das Berufliche Schulzentrum für Elektrotechnik Dresden bietet einen Online-Zugang zum Vertretungsplan an. Dieser liegt im PDF-Format vor und ist nur nutzerberechtigt mit einem vorgegebenen Nutzernamen und Passwort erreichbar. Da es keine Nutzerbenachrichtigungen gibt und der Zugriff auf die PDF-Ressource über Smartphones erschwert ist, habe ich diese Azure-Funktion erstellt. Diese fragt alle 5 Minuten den Vertretungsplan für IT-Klassen ab und sendet bei Neuerungen die Vertretungsliste an einen Telegrambot.

Benötigte Ressourcen:
- Telegram Bot
- Azure Function (Isolated) benötigt
- Azure Table Storage

Die Konfiguration muss im Root Order der Azure Function als **"appsettings.json"** bereitgestellt werden.

Beispiel appsettings.json:

```
{
  "Telegram": {
    "Token": "",
    "ChannelId": "",
    "DebugChatId": ""
  },
  "TableStorage": {
    "ConnectionString": "",
    "TableName": "",
    "DebugTableName": ""

  },
  "Schedule": {
    "URL": "http://geschuetzt.bszet.de/s-lk-vw/Vertretungsplaene/vertretungsplan-bs-it.pdf",
    "Username": "",
    "Password": ""
  }
}
```
