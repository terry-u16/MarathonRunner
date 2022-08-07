namespace TerryU16.MarathonRunner.Core.Runners;

public interface IRunnerCallback
{
    void OnSingleTestEnd(int seed, TestCaseResult result);

    void OnAllTestEnd();
}