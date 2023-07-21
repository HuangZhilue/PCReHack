using System.Collections.Concurrent;

namespace CV_ViewTool.Contracts.Services;

public interface IAppState
{
    string Status { get; set; }
    /// <summary>
    /// List《string》 Monitor_page_id_List = CameraMonitorState[Name]
    /// </summary>
    ConcurrentDictionary<string, List<string>> CameraMonitorState { get; set; }
    /// <summary>
    /// string id = CameraState[Name]
    /// </summary>
    ConcurrentDictionary<string, string> CameraState { get; set; }
    event EventHandler StatusChanged;
}