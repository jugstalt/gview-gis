window.gview_base = {
    setCursor: function (cursorStyle) {
        if (cursorStyle === 'wait') {
            document.body.classList.add('gview-razor-wait-cursor');
        } else {
            document.body.classList.remove('gview-razor-wait-cursor');
        }
    },

    copyToClipboard: function(elementId) {
        var text = document.getElementById(elementId).innerText;
        navigator.clipboard.writeText(text).then(function () {
            console.log('Text copied to clipboard');
            return true;
        }).catch(function (error) {
            console.error('Error copying text: ', error);
            return false;
        });
    },

    // Theme: 'light' | 'dark' | 'system' in localStorage; the old bool key 'gview_darkmode' (v8) is taken over once
    themeStorageKey: 'gview-webapps-theme',

    getThemeMode: function () {
        try {
            var mode = localStorage.getItem(this.themeStorageKey);
            if (!mode) {
                var old = (localStorage.getItem('gview_darkmode') || '').toLowerCase();
                mode = old === 'true' ? 'dark' : old === 'false' ? 'light' : null;
                if (mode) {
                    localStorage.setItem(this.themeStorageKey, mode);
                }
                localStorage.removeItem('gview_darkmode');
            }
            return mode === 'light' || mode === 'dark' ? mode : 'system';
        } catch (e) {
            return 'system';
        }
    },

    setThemeMode: function (mode) {
        try { localStorage.setItem(this.themeStorageKey, mode); } catch (e) { }
    },

    applyTheme: function (isDark) {
        document.documentElement.setAttribute('data-theme', isDark ? 'dark' : 'light');
    }
};