using Sanalia.Core.Models;

namespace Sanalia.Api.Security;

public interface ITokenIssuer
{
    (string Token, DateTimeOffset ExpiresAt) Issue(UserIdentity identity);
}
