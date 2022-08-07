namespace TerryU16.MarathonRunner.Core;

public class ProblemOption
{
    public string ProblemName { get; set; } = "problem";
    public TimeSpan TimeLimit { get; set; } = TimeSpan.FromSeconds(2);
    
}