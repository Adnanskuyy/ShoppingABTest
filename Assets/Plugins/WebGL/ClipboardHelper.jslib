mergeInto(LibraryManager.library, {

  CopyToClipboard: function (str) {
    var text = UTF8ToString(str);

    // 1. Define the "Old School" Fallback function
    var useFallback = function(textToCopy) {
      var textArea = document.createElement("textarea");
      textArea.value = textToCopy;
      
      // Ensure it's not visible but part of the DOM
      textArea.style.position = "fixed";
      textArea.style.left = "-9999px";
      textArea.style.top = "0";
      
      document.body.appendChild(textArea);
      textArea.focus();
      textArea.select();
      
      try {
        var successful = document.execCommand('copy');
        if (successful) {
            console.log("Fallback Copy Successful!");
        } else {
            console.error("Fallback Copy Failed.");
        }
      } catch (err) {
        console.error("Fallback Error: ", err);
      }
      
      document.body.removeChild(textArea);
    };

    // 2. Try the Modern API first
    if (navigator.clipboard && navigator.clipboard.writeText) {
      navigator.clipboard.writeText(text).then(
        function () {
          console.log("Modern API Copy Successful!");
        },
        function (err) {
          // --- THIS IS THE FIX ---
          // If the Modern API is blocked (Permission Error), run the fallback!
          console.warn("Modern API blocked. Attempting fallback...", err);
          useFallback(text);
        }
      );
    } else {
      // If Modern API doesn't exist, run fallback immediately
      useFallback(text);
    }
  }

});