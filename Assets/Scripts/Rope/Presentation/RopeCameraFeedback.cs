using System;
using Cinemachine;

public sealed class RopeCameraFeedback : IDisposable
{
    private readonly Rope rope;
    private readonly CinemachineVirtualCamera virtualCamera;
    private readonly int activePriority;
    private readonly int inactivePriority;

    public RopeCameraFeedback(
        Rope rope,
        CinemachineVirtualCamera virtualCamera,
        int activePriority,
        int inactivePriority)
    {
        this.rope = rope;
        this.virtualCamera = virtualCamera;
        this.activePriority = activePriority;
        this.inactivePriority = inactivePriority;

        rope.RopeAttached += Activate;
        rope.RopeReset += Deactivate;
    }

    public void Dispose()
    {
        rope.RopeAttached -= Activate;
        rope.RopeReset -= Deactivate;
    }

    private void Activate()
    {
        if (virtualCamera != null)
            virtualCamera.Priority = activePriority;
    }

    private void Deactivate()
    {
        if (virtualCamera != null)
            virtualCamera.Priority = inactivePriority;
    }
}
