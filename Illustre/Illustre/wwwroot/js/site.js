(function () {
    $("#illustre-image-file").change(function () {
        if (typeof (FileReader) != "undefined") {
            var reader = new FileReader();
            reader.onload = function (e) {
                $("#illustre-image-preview")
                    .css('background-image', 'url(' + e.target.result + ')');
                $("#illustre-add-image-preview").attr("src", e.target.result);
            }
            reader.readAsDataURL($(this)[0].files[0]);
        }
    });
})();
