using System;
using System.Security.Claims;

namespace LibraryAPI.Helpers
{
    public interface ITokenHelper
    {
        string Sign(int userId);
        ClaimsPrincipal Decode(string token);
    }
}
