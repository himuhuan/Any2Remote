namespace HimuRdp.Core;

public class CheckResult
{
    public CheckResult(HimuRdpError errorCode = HimuRdpError.Success, string message = "")
    {
        ErrorCode = errorCode;
        Message = message;
    }

    public HimuRdpError ErrorCode { get; set; }
    public bool IsInstalled => ErrorCode == HimuRdpError.Success;
    public string Message { get; set; }
}