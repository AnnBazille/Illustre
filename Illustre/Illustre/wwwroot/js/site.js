// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(function () {
    $("#illustre-image-file").change(function () {
        if (typeof (FileReader) != "undefined") {
            var reader = new FileReader();
            reader.onload = function (e) {
                $("#illustre-image-preview").css('background-image', 'url(' + e.target.result + ')');
            }
            reader.readAsDataURL($(this)[0].files[0]);
        }
    });
})();
