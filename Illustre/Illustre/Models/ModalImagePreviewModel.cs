namespace Illustre.Models;

public class ModalImagePreviewModel
{
    private string _modalId;

    public string ModalId
    {
        get => "illustre-modal-" + _modalId;
        set => _modalId = value;
    }

    public string Title { get; set; }

    public string Image { get; set; }

    public string Hashtag
    {
        get => "#" + ModalId;
    }
}
