using System.Security.Claims;

namespace SwissWebApplicationFactory.Authentication;

// Inspired by: https://github.com/jabbera/aspnetcore-testing-role-handler
public class ClaimHeaderConfig
{
    private readonly Dictionary<string, List<string>> claims = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ClaimHeaderConfig"/> class.
    /// </summary>
    public ClaimHeaderConfig() => Reset();

    /// <summary>
    /// Gets or sets a value indicating whether true makes the client generate an anonymous request, false causes a client
    /// with all claims added to be generated.
    /// </summary>
    public bool AnonymousRequest { get; set; }

    /// <summary>
    /// Gets or sets the name claim.
    /// </summary>
    public string Name
    {
        get
        {
            return claims.Where(c => c.Key.Equals(ClaimTypes.Name, StringComparison.Ordinal)).SelectMany(t => t.Value).Single();
        }

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            claims.Remove(ClaimTypes.Name);
            claims.Add(ClaimTypes.Name, new List<string> { value });
        }
    }

    /// <summary>
    /// Gets or sets clears existing roles and sets the current roles to the array passed in.
    /// </summary>
    public string[] Roles
    {
        get
        {
            return claims.Where(c => c.Key.Equals(ClaimTypes.Role, StringComparison.Ordinal)).SelectMany(t => t.Value).ToArray();
        }

        set
        {
            if (value == null || Array.Exists(value, x => x == null))
            {
                throw new ArgumentNullException(nameof(value));
            }

            claims.Remove(ClaimTypes.Role);

            foreach (var role in value)
            {
                AddClaim(ClaimTypes.Role, role);
            }
        }
    }

    /// <summary>
    /// Adds a claim to the collection of claims.
    /// </summary>
    /// <param name="claimType">The type of claim. <see cref="ClaimTypes"/> for standard values.</param>
    /// <param name="value">The value of the claim.</param>
    public void AddClaim(string claimType, string value)
    {
        if (!claims.TryGetValue(claimType, out var values))
        {
            values = new List<string>();
            claims.Add(claimType, values);
        }

        values.Add(value);
    }

    /// <summary>
    /// Adds a claim to the collection of claims.
    /// </summary>
    /// <param name="claim">The claim to add.</param>
    public void AddClaim(Claim claim)
    {
        ArgumentNullException.ThrowIfNull(claim);
        AddClaim(claim.Type, claim.Value);
    }

    internal IEnumerable<Claim> Claims => claims.SelectMany(pair => pair.Value.Select(value => new Claim(pair.Key, value)));

    /// <summary>
    /// Resets the claim collection to it's default state.
    /// </summary>
    public void Reset()
    {
        claims.Clear();
        AnonymousRequest = false;
        Name = string.Empty;
        Roles = Array.Empty<string>();
    }
}