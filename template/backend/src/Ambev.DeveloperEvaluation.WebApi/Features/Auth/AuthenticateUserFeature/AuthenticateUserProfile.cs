using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Auth.AuthenticateUserFeature;

/// <summary>
/// AutoMapper profile for authentication-related mappings
/// </summary>
public sealed class AuthenticateUserProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticateUserProfile"/> class
    /// </summary>
    public AuthenticateUserProfile()
    {
        // AuthController.AuthenticateUser maps the incoming request straight to the
        // Application-layer command, then maps the returned AuthenticateUserResult to
        // this WebApi-layer response — both of those maps were missing, which made the
        // endpoint throw an unhandled AutoMapperMappingException on every call.
        CreateMap<AuthenticateUserRequest, Application.Auth.AuthenticateUser.AuthenticateUserCommand>();
        CreateMap<Application.Auth.AuthenticateUser.AuthenticateUserResult, AuthenticateUserResponse>();

        CreateMap<User, AuthenticateUserResponse>()
            .ForMember(dest => dest.Token, opt => opt.Ignore())
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));
    }
}
