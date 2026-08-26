Configuration
=============

The gView Server can be configured from the _config/mapserver.json file:

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

        // Service names (wildcards "*"/"?" allowed) that should be loaded into memory
        // right after server startup, instead of on the first incoming request.
        // Loading happens in the background and does not delay the server startup.
        // Examples: "*" (all services), "myfolder/*" (all services in a folder), "myfolder/myservice"
        "preload-services": [ "myfolder/*" ]
    }

```

[Next..](inst/installation.md)