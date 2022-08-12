namespace TerryU16.MarathonRunner.Core.Compilers;

public class CompileOption
{
    public string ExeName { get; set; } = "main";

    public CompileFile[] Files { get; set; } = Array.Empty<CompileFile>();
}