function AppendLogoutIFrame(target, url) {
    var iframe = document.createElement('iframe');
    iframe.width = 0;
    iframe.height = 0;
    iframe.class = 'signout';
    iframe.src = url;
    target.appendChild(iframe);
}