using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

using Events_GSS.ViewModels;

namespace Events_GSS.Views;

public sealed partial class SubmitProofDialog : ContentDialog
{
    public SubmitProofArgs? Result { get; private set; }

    private readonly QuestItemViewModel _questItem;

    public SubmitProofDialog(QuestItemViewModel questItem)
    {
        InitializeComponent();
        _questItem = questItem;
        Title = $"Submit Proof — {questItem.Quest.Name}";
    }

    private void OnSubmitClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var url = ImageUrlBox.Text?.Trim();
        var text = DescriptionBox.Text?.Trim();

        if (string.IsNullOrEmpty(url) && string.IsNullOrEmpty(text))
        {
            args.Cancel = true;
            ValidationText.Text = "Provide at least an image URL or a description.";
            ValidationText.Visibility = Visibility.Visible;
            return;
        }

        Result = new SubmitProofArgs(_questItem, url, text);
    }
}