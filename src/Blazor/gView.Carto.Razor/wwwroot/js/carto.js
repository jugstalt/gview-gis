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

        bottomHeight = Math.max(size, 0);
        bottomHeight = Math.min(size, container.clientHeight - 10 - 25 /*statusbar*/ - 100 /*toolbar*/);

        //console.log('dataFrameSize', size);

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
    }
};