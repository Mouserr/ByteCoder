namespace ByteCoder
{
    public class Command
    {
        public string FilePath { get; set; }
        public MethodType? Method { get; set; }
        public string KeyString { get; set; }
        public bool ShowHelp { get; set; }
    }
}