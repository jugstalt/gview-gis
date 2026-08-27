Konfiguration
=============

Der gView Server kann über die Datei `_config/mapserver.json` konfiguriert werden:

```javascript
   {
        // Folder, where gView Server stores Services, Logins, etc
        "services-folder": "C:\\gView5\\Server\\Services",

        // Path, where gView Server stores Map Request Images
        "output-path": "C:\\IIS\\Output",
        // Url to the Path, where gView Server stores Map Request Images
        "output-url": "http://my.server.com/output",

        // Url with which gView Server can be reached via Internet
        "onlineresource-url": "https://my.server.com/gview5-server",

        // Path, where gView Server stores Tiles
        "tilecache-root": "C:\\data\\tilecache",

        // The Task Queue
        "task-queue": {
            // Indicates how many requests can be processed at the same time
            "max-parallel-tasks": 20,
            // The maximum length of the queue
            "max-queue-length": 1000
        },

        // Whether clients are allowed to log in through the web interface
        "allowFormsLogin": true,
        // It can be assumed that all calls are made over HTTPS
        "force-https":  false,

        // Schriftdateien (*.ttf, *.otf, *.ttc) in diesen Verzeichnissen werden beim
        // Serverstart für das Rendering verfügbar gemacht - die Schriften müssen
        // NICHT im Betriebssystem installiert sein. Funktioniert unter Windows und
        // Linux gleich. Neue Schriftdateien erfordern einen Server-Neustart.
        "fonts": {
            "directories": [ "C:\\gView5\\Server\\Fonts" ],
            // optional, Standard false: Schriften zusätzlich in den benutzerbezogenen
            // Schriftspeicher des Betriebssystems kopieren (best effort)
            "install-to-system": false
        },

        // Dienstnamen (Wildcards "*"/"?" erlaubt), die direkt nach dem Serverstart in den
        // Speicher geladen werden sollen, statt erst beim ersten eingehenden Request.
        // Das Laden erfolgt im Hintergrund und verzögert den Serverstart nicht.
        // Beispiele: "*" (alle Dienste), "myfolder/*" (alle Dienste in einem Ordner), "myfolder/myservice"
        "preload-services": [ "myfolder/*" ]
```

[Weiter..](inst/installation.md)