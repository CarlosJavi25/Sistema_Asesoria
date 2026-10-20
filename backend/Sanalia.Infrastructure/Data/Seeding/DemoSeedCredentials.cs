namespace Sanalia.Infrastructure.Data.Seeding;

public sealed class DemoSeedCredentials
{
    public const string SectionName = "DemoUsers";

    public string AdminPassword { get; set; } = string.Empty;
    public string AdvisorPassword { get; set; } = string.Empty;
    public string ClientPassword { get; set; } = string.Empty;

    public void ValidateAdminAndAdvisor()
    {
        Require(AdminPassword, nameof(AdminPassword));
        Require(AdvisorPassword, nameof(AdvisorPassword));
    }

    public void ValidateClient() => Require(ClientPassword, nameof(ClientPassword));

    private static void Require(string value, string key)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException(
                $"{SectionName}:{key} must be configured through User Secrets in Development " +
                "or an environment variable in other environments.");
    }
}
