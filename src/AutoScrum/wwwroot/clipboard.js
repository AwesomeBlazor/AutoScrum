window.setClipboard = (text, html) => {
    const blobPlain = new Blob([text], { type: "text/plain" });
    const blobHtml = new Blob([html], { type: "text/html" });

    navigator.clipboard.write([
        new ClipboardItem({
            [blobHtml.type]: blobHtml,
            [blobPlain.type]: blobPlain
        }),
    ])
}
