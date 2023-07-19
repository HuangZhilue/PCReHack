using ColorPicker.Models;

namespace CV_ViewTool.Models;

public class CVProfile
{
    public string ProfileName { get ; set; }
    public int Threshold { get; set; }
    public int MaxBinary { get; set; }
    public bool UseInRange { get; set; }
    public ColorState ColorState { get; set; }
    public ColorState ColorState2 { get; set; }
}
