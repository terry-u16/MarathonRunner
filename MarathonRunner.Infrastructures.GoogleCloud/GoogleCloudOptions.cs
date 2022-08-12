namespace TerryU16.MarathonRunner.Infrastructures.GoogleCloud;

public class GoogleCloudOptions
{
    // User Secretsから取得する想定
    public Uri? RunEndpoint { get; set; }

    public Uri? RustCompilerEndpoint { get; set; }

    public string BucketName { get; set; } = "";
}