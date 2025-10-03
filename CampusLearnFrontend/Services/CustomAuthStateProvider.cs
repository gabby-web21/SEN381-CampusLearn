using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CampusLearnFrontend.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly AuthService _authService;

        public CustomAuthStateProvider(AuthService authService)
        {
            _authService = authService;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var user = _authService.GetCurrentUser();

            ClaimsIdentity identity;

            if (user != null)
            {
                identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, string.IsNullOrWhiteSpace(user.Name) ? user.Email : user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                }, "FakeAuth");
            }
            else
            {
                identity = new ClaimsIdentity();
            }

            var principal = new ClaimsPrincipal(identity);
            return Task.FromResult(new AuthenticationState(principal));
        }

        // 👇 This was missing
        public void NotifyAuthStateChanged() =>
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
