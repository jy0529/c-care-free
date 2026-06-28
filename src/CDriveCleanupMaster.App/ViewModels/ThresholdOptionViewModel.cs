namespace CDriveCleanupMaster.App.ViewModels;

public sealed class ThresholdOptionViewModel
{
    public ThresholdOptionViewModel(int megabytes, string label)
    {
        Megabytes = megabytes;
        Label = label;
    }

    public int Megabytes { get; }
    public string Label { get; }
}
