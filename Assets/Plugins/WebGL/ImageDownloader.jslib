mergeInto(LibraryManager.library, {
    DownloadImageFromBrowser: function (filenamePtr, base64Ptr) {
        // Converte os dados da Unity para texto do Javascript
        var filename = UTF8ToString(filenamePtr);
        var base64 = UTF8ToString(base64Ptr);

        // Cria um link invisível com a imagem e clica nele para baixar
        var element = document.createElement('a');
        element.setAttribute('href', 'data:image/png;base64,' + base64);
        element.setAttribute('download', filename);

        element.style.display = 'none';
        document.body.appendChild(element);
        element.click();
        document.body.removeChild(element);
    }
});