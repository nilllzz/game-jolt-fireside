namespace GameJoltFireside.Phoenix;

internal class Binding
{
    internal string Event { get; set; }
    internal Action<dynamic, string> Callback { get; set; }
}
