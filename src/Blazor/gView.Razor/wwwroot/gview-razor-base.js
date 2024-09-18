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
    }
};