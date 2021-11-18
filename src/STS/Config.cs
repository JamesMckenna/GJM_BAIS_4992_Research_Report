// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;
using System.Security.Claims;

namespace STS
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResources.Address(),
            new IdentityResource("Admin", new List<string>{"Admin" })

        };

        public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope(){
                Name = "AnAPI",
                DisplayName = "Admin",
            },
            new ApiScope("scope", "a scope"),
        };

        public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            // machine to machine client credentials flow client
            new Client
            {
                ClientId = "ANativeClientOrService",
                ClientName = "Client Credentials Client",

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("aSuperSecret".Sha256()) },

                AllowedScopes = { "AnAPI" }
            },

            // Interactive client using code flow + pkce. Server-side rendered, browser client
            new Client
            {
                ClientId = "mvcClient",
                ClientName = "MvcClient",
                ClientSecrets = { new Secret("aDifferentSecret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = { "https://localhost:5002/signin-oidc" },
                FrontChannelLogoutUri = "https://localhost:5002/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },

                AllowOfflineAccess = true,
                AllowedScopes = {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Address,
                    IdentityServerConstants.StandardScopes.Email,
                    "AnAPI", "offline_access"
                },

                //The Token settings
                IdentityTokenLifetime = 300,
                AccessTokenLifetime = 3600,
                AuthorizationCodeLifetime = 300,
                AbsoluteRefreshTokenLifetime = 3600,
                SlidingRefreshTokenLifetime = 1800,
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                UpdateAccessTokenClaimsOnRefresh = true,
                AccessTokenType = AccessTokenType.Jwt,
                AlwaysIncludeUserClaimsInIdToken = true,
                AlwaysSendClientClaims = true,

                RequireConsent = true,
            },

            //JS Client
            new Client
            {
                ClientId = "JSClient",
                ClientName = "JavaScript Client",
                AllowedGrantTypes = GrantTypes.Code,
                RequireClientSecret = false,

                RedirectUris = {"https://localhost:5003/callback.html", "https://localhost:5003/silent-refresh.html"},
                PostLogoutRedirectUris = {"https://localhost:5003/index.html"},
                AllowedCorsOrigins = {"https://localhost:5005", "https://localhost:6001", "https://localhost:5002", "https://localhost:5001", "https://localhost:5003"},

                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Address,
                    IdentityServerConstants.StandardScopes.Email,
                    "AnAPI", "offline_access"
                },


                //The Token settings
                AlwaysIncludeUserClaimsInIdToken = true,
                AlwaysSendClientClaims = true,
                RequireConsent = true,
                RequirePkce = true,
                AllowPlainTextPkce = false,

                AllowAccessTokensViaBrowser = true,
                //The refresh token should be long lived (at least longer than the access token).
                //Once the refresh token expires, the user has to login again. Without sliding expiration the refresh token will expire in an absolute time, having the user to login again.
                AccessTokenType = AccessTokenType.Jwt,//A self contained token - verification with STS not required on every request 
                AllowOfflineAccess = true, //Allows Refresh Token
                    
                //Token lifetime - NOT COOKIE LIFETIME, NOT AUTHENTICATION LIFETIME. Just how long an access token can be used against an API (Resource registered with IS4) 
                IdentityTokenLifetime = 300, //Default 300 seconds
                AccessTokenLifetime = 300, //Default 3600 seconds, 1 hour
                AuthorizationCodeLifetime = 300, //Default 300 seconds: Once User consents, this token should no longer be needed until re-authorization. This AuthorizationCode is used to prove to IS4 that an access token and id token have been constented too and from there the refresh token takes over. So if using refresh tokens, AuthorizationCode shouldn't need a long lifetime. 
                    
                AbsoluteRefreshTokenLifetime = 36000, //Defaults to 2592000 seconds / 30 days - NOT GOOD FOR SPA's - 36000 = 10 hours
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime = 150,//token will be refreshed only if this value has 50% elasped. Router Guard on Vue Router will ask for refresh on every page navigation. If 50% elapsed, refresh will happen. Setting the accessTokenExpiringNotificationTime of the oidc-client to the same timeout, will allow refresh on page navigation (assuming access and id tokens haven't already expired)
                UpdateAccessTokenClaimsOnRefresh = true, //Gets or sets a value indicating whether the access token (and its claims) should be updated on a refresh token request.
                                       
                UserSsoLifetime = 300,
                /* The maximum duration (in seconds) since the last time the user authenticated. 
                    * Defaults to null. 
                    * You can adjust the lifetime of a session token to control when and how often a user is required to reenter credentials
                    * instead of being silently authenticated, when using a web application.*/
            },

            new Client
            {
                ClientId = "IdentityManagement",
                ClientName = "Identity Management",
                ClientSecrets = { new Secret("AnotherSecret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = { "https://localhost:5005/signin-oidc" },
                FrontChannelLogoutUri = "https://localhost:5005/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:5005/signout-callback-oidc" },

                AllowOfflineAccess = true,//Allow the use of refresh tokens
                AllowedScopes = {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Address,
                    IdentityServerConstants.StandardScopes.Email,
                    "AnAPI", "offline_access", "Admin"
                },

                //The Token settings
                IdentityTokenLifetime = 300,
                AccessTokenLifetime = 3600,
                AuthorizationCodeLifetime = 300,
                AbsoluteRefreshTokenLifetime = 3600,
                SlidingRefreshTokenLifetime = 1800, 
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                UpdateAccessTokenClaimsOnRefresh = true,
                AccessTokenType = AccessTokenType.Reference, //More secure than a Jwt, but requires API to verify with STS on every interaction 
                AlwaysIncludeUserClaimsInIdToken = true,
                AlwaysSendClientClaims = true,

                RequireConsent = true,
            }
        };
    }
}