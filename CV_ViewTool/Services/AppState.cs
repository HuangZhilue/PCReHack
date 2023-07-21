using CV_ViewTool.Contracts.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CV_ViewTool.Services;

public class AppState : IAppState
{
    private string _status;
    private ConcurrentDictionary<string, List<string>> _cameraMonitorState = new();
    private ConcurrentDictionary<string, string> _cameraState = new();

    public string Status
    {
        get => _status;
        set
        {
            _status = value;
            StatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public ConcurrentDictionary<string, List<string>> CameraMonitorState
    {
        get => _cameraMonitorState;
        set
        {
            _cameraMonitorState = value;
            StatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public ConcurrentDictionary<string, string> CameraState
    {
        get => _cameraState;
        set
        {
            _cameraState = value;
            StatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler StatusChanged;
}