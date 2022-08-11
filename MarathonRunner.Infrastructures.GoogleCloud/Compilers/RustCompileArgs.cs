namespace TerryU16.MarathonRunner.Infrastructures.GoogleCloud.Compilers;

public record RustCompileArgs(string BucketName, string ContestName, string ExeName, CompileContent[] Files);

public record CompileContent(string FilePath, string Content);