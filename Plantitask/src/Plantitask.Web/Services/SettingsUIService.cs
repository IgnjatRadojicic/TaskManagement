using Plantitask.Web.Interfaces;

namespace Plantitask.Web.Services;

public class SettingsUIService : ISettingsUIService
{
    // ----- Dirty state (child → parent) -----
    public event Action<bool>? OnDirtyStateChanged;
    public void NotifyDirtyStateChanged(bool hasChanges) => OnDirtyStateChanged?.Invoke(hasChanges);

    // ----- Save / Reset commands (parent → child) -----
    public event Func<Task<(bool Success, string? Error)>>? OnSaveRequested;
    public event Action? OnResetRequested;

    public async Task<(bool Success, string? Error)> RequestSave()
    {
        if (OnSaveRequested is not null)
            return await OnSaveRequested.Invoke();

        return (false, "No active section to save");
    }

    public void RequestReset() => OnResetRequested?.Invoke();
}
