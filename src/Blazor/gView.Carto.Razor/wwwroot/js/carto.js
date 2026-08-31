(function () {
    let isTocResizing = false, isDataResizing = false;
    //document.querySelector('.toc-content-splitter').addEventListener('mousedown', function (e) {
    //    isResizing = true;
    //});

    document.onkeydown = function (e) {  
        if (e.keyCode === 116) {  // F5
            return false;
        }
    };

    window.addEventListener('mousedown', function (e) {
        isTocResizing = isDataResizing = false;

        if (e.target.classList.contains('toc-content-splitter')) {
            isTocResizing = true;
        } else if (e.target.classList.contains('data-content-splitter')) {
            isDataResizing = true;
        }
    });

    window.addEventListener('mousemove', function (e) {
        if (isTocResizing) {
            let container = document.querySelector('.carto-main');

            let leftWidth = e.clientX - container.offsetLeft;

            leftWidth = Math.max(leftWidth, 10);
            leftWidth = Math.min(leftWidth, container.clientWidth - 10);

            document.querySelector('.toc').style.width = leftWidth + 'px';
            document.querySelector('.toc-content-splitter').style.left = (leftWidth) + 'px';
            document.querySelector('.content').style.left = (leftWidth + 5) + 'px';
        }
        else if (isDataResizing) {
            let container = document.querySelector('.carto-main');
            let bottomHeight = (container.clientHeight - 30) - (e.clientY - container.offsetTop);

            window.cartoInterops.setDataFrameSize(bottomHeight);
        }
    });

    window.addEventListener('mouseup', function (e) {
        if (isTocResizing === true || isDataResizing === true) {
            isTocResizing = isDataResizing = false;

            window.cartoInterops.refreshMapFrame();
        }
    });
}());

window.cartoInterops = {

    refreshMapFrame: function () {
        if (window.gViewLeaflet) {
            window.gViewLeaflet.resizeAllMaps();
        }
    },
    setDataFrameSize: function (size) {
        let container = document.querySelector('.carto-main');

        size = Math.max(size, 0);
        size = Math.min(size, container.clientHeight - 10 - 25 /*statusbar*/ - 100 /*toolbar*/);

        //console.logsizedataFrameSize', size);

        document.querySelector('.toc').style.bottom = (32 + size) + 'px';
        document.querySelector('.content').style.bottom = (32 + size) + 'px';
        document.querySelector('.data-content-splitter').style.bottom = (27 + size) + 'px';
        document.querySelector('.data').style.height = size + 'px';
    },
    showDataFrame: function (minSize) {
        minSize = minSize || 400;

        var size = parseInt(document.querySelector('.data').style.height) || 0;
        //console.log(size, minSize);

        if (isNaN(size) || size < minSize) {
            this.setDataFrameSize(minSize);
            this.refreshMapFrame();
        }
    },

    // --- Expression editor helpers (LabelExpressionDialog) ---

    // Insert text at the caret of a <textarea> (replacing any selection), keep the caret
    // after the inserted text and raise 'input' so Blazor's @oninput binding picks it up.
    expressionEditorInsert: function (el, text) {
        if (!el) {
            return;
        }

        var start = typeof el.selectionStart === 'number' ? el.selectionStart : el.value.length;
        var end = typeof el.selectionEnd === 'number' ? el.selectionEnd : el.value.length;

        el.value = el.value.substring(0, start) + text + el.value.substring(end);

        var caret = start + text.length;
        el.selectionStart = el.selectionEnd = caret;
        el.focus();
        el.dispatchEvent(new Event('input', { bubbles: true }));
    },

    // Make the Tab key insert two spaces instead of moving focus out of the editor.
    expressionEditorEnableTab: function (el) {
        if (!el || el._gvTabHandler) {
            return;
        }
        el._gvTabHandler = true;

        el.addEventListener('keydown', function (e) {
            if (e.key !== 'Tab') {
                return;
            }
            e.preventDefault();

            var start = el.selectionStart, end = el.selectionEnd;
            el.value = el.value.substring(0, start) + '  ' + el.value.substring(end);
            el.selectionStart = el.selectionEnd = start + 2;
            el.dispatchEvent(new Event('input', { bubbles: true }));
        });
    },

    expressionEditorFocus: function (el) {
        if (el) {
            el.focus();
        }
    }
};