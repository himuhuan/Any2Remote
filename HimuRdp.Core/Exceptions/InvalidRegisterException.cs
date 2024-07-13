using Microsoft.Win32;

namespace HimuRdp.Core.Exceptions
{
    public class InvalidRegisterException : Exception
    {
        public string Position { get; }
        public RegistryHive Top { get; }

        public InvalidRegisterException(RegistryHive top, string position, Exception innerException)
            : base($"Invalid register key \"{position}\" in {top.ToString()}", innerException)
        {
            Position = position;
            Top = top;
        }

        public InvalidRegisterException(RegistryHive top, string position) 
            : base($"Invalid register key \"{position}\" in {top.ToString()}")
        {
            Position = position;
            Top = top;
        }
    }
}
