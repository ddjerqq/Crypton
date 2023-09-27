using Crypton.Application.Auth.Commands;

namespace Crypton.WebUIOld.Services.Interfaces;

public interface IAuthService
{
    public Task LoginAsync(UserLoginCommand command, CancellationToken ct = default);

    public Task RecoverAsync(UserRecoverCommand command, CancellationToken ct = default);

    public Task RegisterAsync(UserRegisterCommand command, CancellationToken ct = default);

    public Task LogoutAsync(CancellationToken ct = default);
}