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

            bottomHeight = Math.max(bottomHeight, 0);
            bottomHeight = Math.min(bottomHeight, container.clientHeight - 10 - 25 /*statusbar*/ - 100 /*toolbar*/);

            console.log('bottomHeight', bottomHeight);

            document.querySelector('.toc').style.bottom = (32 + bottomHeight) + 'px';
            document.querySelector('.content').style.bottom = (32 + bottomHeight) + 'px';
            document.querySelector('.data-content-splitter').style.bottom = (27 + bottomHeight) + 'px';
            document.querySelector('.data').style.height = bottomHeight + 'px';
        }
    });

    window.addEventListener('mouseup', function (e) {
        if (isTocResizing === true || isDataResizing === true) {
            isTocResizing = isDataResizing = false;

            if (window.gViewLeaflet) {
                window.gViewLeaflet.resizeAllMaps();
            }
        }
    });
}());