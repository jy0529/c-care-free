using CDriveCleanupMaster.Domain.Models;

namespace CDriveCleanupMaster.App.ViewModels;

public sealed class LargeFileItemViewModel
{
    public LargeFileItemViewModel(LargeFileItem model)
    {
        Model = model;
    }

    public LargeFileItem Model { get; }

    public string Name => Model.Name;
    public string FilePath => Model.Path;
    public string Extension => string.IsNullOrWhiteSpace(Model.Extension) ? "(无扩展名)" : Model.Extension;
    public long Bytes => Model.Bytes;
    public string SizeText => ByteSizeFormatter.Format(Model.Bytes);
    public string ModifiedText => Model.ModifiedAt.LocalDateTime.ToString("yyyy-MM-dd HH:mm");
    public string RelocationHintText => string.IsNullOrWhiteSpace(Model.RelocationHint) ? "—" : Model.RelocationHint;
}
