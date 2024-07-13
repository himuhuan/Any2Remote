using HimuRdp.Core;

var sessions = HimuRdpServices.GetTermsrvSessions();
foreach (var session in sessions)
{
    Console.WriteLine(session);
}