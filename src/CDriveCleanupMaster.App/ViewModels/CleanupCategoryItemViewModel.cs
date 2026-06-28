using System.Windows.Media;
using CDriveCleanupMaster.App.Helpers;
using CDriveCleanupMaster.Domain.Enums;
using CDriveCleanupMaster.Domain.Models;

namespace CDriveCleanupMaster.App.ViewModels;

public sealed class CleanupCategoryItemViewModel : ObservableObject
{
    private readonly Action _selectionChanged;
    private bool _isSelected;

    public CleanupCategoryItemViewModel(CleanupCategoryResult model, Action selectionChanged)
    {
        Model = model;
        _selectionChanged = selectionChanged;
        _isSelected = model.IsSelected;
    }

    public CleanupCategoryResult Model { get; }

    public string DisplayName => Model.Definition.DisplayName;
    public string Description => Model.Definition.Description;
    public string IconKey => Model.Definition.IconKey;
    public System.Windows.Media.Brush IconBackground => CleanupIconCatalog.GetBackgroundBrush(IconKey);
    public Geometry IconGeometry => CleanupIconCatalog.GetIconGeometry(IconKey);
    public string PathsText => string.Join(Environment.NewLine, Model.ResolvedPaths);
    public string RiskLevelText => Model.Definition.RiskLevel switch
    {
        RiskLevel.Low => "低",
        RiskLevel.Medium => "中",
        RiskLevel.High => "高",
        _ => Model.Definition.RiskLevel.ToString()
    };
    public string SizeText => ByteSizeFormatter.Format(Model.TotalBytes);
    public string FileCountText => $"{Model.FileCount:N0} 个文件";
    public long TotalBytes => Model.TotalBytes;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (SetProperty(ref _isSelected, value))
            {
                _selectionChanged();
            }
        }
    }

    public CleanupCategoryResult ToModel()
    {
        return new CleanupCategoryResult
        {
            Definition = Model.Definition,
            TotalBytes = Model.TotalBytes,
            FileCount = Model.FileCount,
            ResolvedPaths = Model.ResolvedPaths,
            IsSelected = IsSelected
        };
    }
}
