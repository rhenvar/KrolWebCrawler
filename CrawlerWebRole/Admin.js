window.onload = function () {
    setup();
}

function setup() {
    document.getElementById("resume").onclick = resume;
    document.getElementById("stop").onclick = stop;
    document.getElementById("search").onclick = searchTitle;

    var timeout = setInterval(function () {
        var dataAjax = new XMLHttpRequest();
        ajax.open('GET');
    }, 2000);
}

function resume() {
    var ajax = new XMLHttpRequest();
    ajax.open('GET', '/admin.asmx/StartCrawling', true);
    ajax.send();
    ajax.onload = function () { };
}

function stop() {
    var ajax = new XMLHttpRequest();
    ajax.open('GET', '/admin.asmx/StopCrawling', true);
    ajax.send();
}

function searchTitle() {
    var url = document.getElementById("url").value;
    if (url != null && url.Length > 0) {
        var urlAjax = new XMLHttpRequest();
        urlAjax.open('GET', '/admin.asmx/GetPageTitle?url=' + url);
        urlAjax.onload = parseTitle;
        urlAjax.send();
    }
}

function parseTitle() {

}