namespace CDriveCleanupMaster.App.ViewModels;

public sealed class CountLimitOptionViewModel
{
    public CountLimitOptionViewModel(int count, string label)
    {
        Count = count;
        Label = label;
    }

    public int Count { get; }
    public string Label { get; }
}
