namespace TerryU16.MarathonRunner.Infrastructures.GoogleCloud;

public record RustCompileArgs(string BucketName, string ContestName, string ExeName, Files[] Files);

public record Files(string FilePath, string Content);