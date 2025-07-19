namespace Gay.Silverbranch.Utilities.Security.Constants;

#pragma warning disable CS1591

public static class AuthConstants
{
    public const string AdminUserPolicyName = "Admin";
    public const string AdminUserClaimName = "admin";

    public const string TrustedMemberPolicyName = "Trusted";
    public const string TrustedMemberClaimName = "trusted_user";

    public const string MemberPolicyName = "User";
    public const string MemberClaimName = "user";

    public const string APIKeyHeaderName = "x-api-key";
}

#pragma warning restore CS1591s