namespace Plantitask.Web.Interfaces;

public interface ISettingsUIService
{
    event Action<bool>? OnDirtyStateChanged;
    void NotifyDirtyStateChanged(bool hasChanges);

    event Func<Task<(bool Success, string? Error)>>? OnSaveRequested;
    event Action? OnResetRequested;

    Task<(bool Success, string? Error)> RequestSave();
    void RequestReset();
}
