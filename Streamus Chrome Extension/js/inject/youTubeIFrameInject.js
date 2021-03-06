﻿//  This code runs inside of the MusicHolder iframe in the Streamus background -- hax!
$(function () {

    //  Only run against our intended iFrame -- not embedded YouTube iframes on other pages.
    if (window.name === 'MusicHolder') {
        
        //  Monitor the video for change of src so that background can mimic player.
        var videoStream = $('.video-stream');

        //  HTML5 player loaded
        if (videoStream.length > 0) {

            var observer = new window.WebKitMutationObserver(function (mutations) {
                mutations.forEach(function (mutation) {
                    var attributeName = mutation.attributeName;

                    if (attributeName === 'src') {

                        var videoStreamSrc = mutation.target.getAttribute(attributeName);

                        //  Don't send a blank src across, I think?
                        if (videoStreamSrc != null && $.trim(videoStreamSrc) != '') {
                            chrome.runtime.sendMessage({
                                method: "videoStreamSrcChange", videoStreamSrc: videoStreamSrc
                            });

                            if (videoStreamSrc.indexOf('blob') > -1) {
                                videoStream.removeAttr('src');
                            }
                            else {
                                videoStream.removeAttr('src');

                                chrome.runtime.sendMessage({
                                    method: "needSeekTo"
                                });

                            }

                        }

                    }
                });
            });

            observer.observe(videoStream[0], {
                attributes: true,
                subtree: false
            });
        }
        
    }

});