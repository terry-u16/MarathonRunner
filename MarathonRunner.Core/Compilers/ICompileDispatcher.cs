namespace TerryU16.MarathonRunner.Core.Compilers;

public interface ICompileDispatcher
{
    Task CompileAsync(CancellationToken ct = default);
}