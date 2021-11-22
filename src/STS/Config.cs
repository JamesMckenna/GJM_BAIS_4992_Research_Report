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
            //Will be returned from UserInfo Endpoint
            //and/or included in the ID_Token
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResources.Address(),
            new IdentityResource {
                Name = "admin",
                DisplayName = "Identity Management Adminsitrator",
                UserClaims = new [] { "admin" },
                Description = "Identity Management Adminsitrator",
                ShowInDiscoveryDocument = true,
            },
            new IdentityResource
            {
                Name = "customClaim",
                DisplayName = "Your custom Identity Info added to tokens",
                UserClaims = new[] {"customClaim" },
                Description = "Your custom Identity Info added to tokens",
                ShowInDiscoveryDocument = true,
            }
        };

        public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            //Flat scopes, no grouping based on logical API design
            //What the API is looking for when allowing access to an endpoint/resource
            new ApiScope(
                name: "AnAPI",
                displayName: "Administrator Access",
                userClaims: new [] { "admin", "name", "sub" }),
            //Read and rewite access to resource/endpoints
            new ApiScope(
                name: "invoiceManage", 
                displayName: "Can read and write invoices", 
                userClaims: new [] { "invoiceManage" }),
            new ApiScope(
                name: "identityManagementAdmin", 
                displayName: "Can manage others identity", 
                userClaims: new [] { "sub", "name", "email", "admin"}),
            //Read only access to resource/endpoints
            new ApiScope(
                name: "invoiceRead", 
                displayName: "Can read invoices", 
                userClaims: new [] { "invoiceRead" }),
            new ApiScope(
                name: "identityManagement", 
                displayName : "Can manage your identity", 
                userClaims : new[] { "sub, name, email" }),
        };

        public static IEnumerable<ApiResource> ApiResources =>
        new List<ApiResource>
        {
            new ApiResource
            {
                Name = "identityManagementAdmin",
                DisplayName = "Identity Management",
                //the user claim included in access token
                UserClaims = new [] { "sub", "name", "email", "admin" },
                //the endpoints user/client can access
                Scopes = new [] { "identityManagementAdmin" },
            },

            new ApiResource()
            {
                Name = "identityManagement",
                DisplayName = "Identity Management",
                Description = "Administrator Access",
                UserClaims = { "sub, name, email" },
                Scopes = { "identityManagement" }
            },
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

                RequirePkce = false,
                AllowPlainTextPkce = false,
                RequireConsent = false,

                RedirectUris = { "https://localhost:5002/signin-oidc" },
                FrontChannelLogoutSessionRequired = true,
                FrontChannelLogoutUri = "https://localhost:5002/signout-oidc",
                BackChannelLogoutSessionRequired = true,
                BackChannelLogoutUri = "https://localhost:5002/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },

                AllowOfflineAccess = true,
                AllowedScopes = {
                    //Identity Resouces to include in access token the client app can request
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Address,
                    "customClaim", "identityManagementAdmin", "identityManagement", "admin",
                    //simple APIScopes
                    "AnAPI", "offline_access", "invoiceManage", "invoiceRead"
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
            },

            //JS Client
            new Client
            {
                ClientId = "JSClient",
                ClientName = "JavaScript Client",
                Enabled = true,
                RequireClientSecret = false,

                AllowedGrantTypes = GrantTypes.Code,


                RedirectUris = 
                {
                    "https://localhost:5003/callback.html", 
                    "https://localhost:5003/silent-refresh.html"
                },

                PostLogoutRedirectUris = 
                {
                    "https://localhost:5003/index.html"
                },

                AllowedCorsOrigins = 
                {
                    "https://localhost:6001", 
                    "https://localhost:5001", 
                    "https://localhost:5003"
                },

                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "AnAPI", "offline_access"
                },


                //The Token settings
                AlwaysIncludeUserClaimsInIdToken = true,
                AlwaysSendClientClaims = true,
                //Requies user consent to include claims in token
                RequireConsent = false,
                RequirePkce = true,
                AllowPlainTextPkce = false,

                AllowAccessTokensViaBrowser = true,

                //The refresh token should be long lived (at least longer than the access token).
                //Once the refresh token expires, the user has to login again.
                //Without sliding expiration the refresh token will expire in an absolute time,
                //having the user to login again.


                //A self contained token - verification with STS not required on every request
                AccessTokenType = AccessTokenType.Jwt, 
                AllowOfflineAccess = true, //Allows Refresh Token
                    
                //Token lifetime - NOT COOKIE LIFETIME, NOT AUTHENTICATION LIFETIME.
                //Just how long an access token can be used against an API (Resource registered with IS4) 
                //Default 300 seconds
                IdentityTokenLifetime = 300,

                //Default 3600 seconds, 1 hour
                AccessTokenLifetime = 300,

                //Default 300 seconds: Once User consents,
                //this token should no longer be needed until re-authorization.
                //This AuthorizationCode is used to prove to IS4 that an access token and
                //id token have been constented too
                //and from there the refresh token takes over.
                //So if using refresh tokens, AuthorizationCode shouldn't need a long lifetime.
                AuthorizationCodeLifetime = 300,
                
                //Defaults to 2592000 seconds / 30 days
                //NOT GOOD FOR SPA's - 36000 = 10 hours
                AbsoluteRefreshTokenLifetime = 36000,

                //Each time a refresh token is requested, give a new, dispose the old
                RefreshTokenUsage = TokenUsage.OneTimeOnly,

                //token will be refreshed only if this value has 50% elasped.
                //If 50% elapsed, refresh will happen.
                //Setting the accessTokenExpiringNotificationTime of the oidc-client/IdentityModel
                //client to the same inactive timeout,
                //will allow refresh on page navigation (assuming access and id tokens haven't already expired)
                SlidingRefreshTokenLifetime = 150,
                RefreshTokenExpiration = TokenExpiration.Sliding,

                //Gets or sets a value indicating whether the access token (and its claims)
                //should be updated on a refresh token request.
                UpdateAccessTokenClaimsOnRefresh = true,

                //The maximum duration (in seconds) since the last time the user authenticated. 
                //Defaults to null. 
                //You can adjust the lifetime of a session token to control when and how often a user is required
                //to reenter credentials
                //instead of being silently authenticated, when using a web application.                       
                UserSsoLifetime = 300,

            },

            new Client
            {
                ClientId = "IdentityManagement",
                ClientName = "Identity Management",
                ClientSecrets = { new Secret("AnotherSecret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                 RequirePkce = true,
                 AllowPlainTextPkce = false,
                 RequireConsent = false,

                RedirectUris = { "https://localhost:5005/signin-oidc" },
                FrontChannelLogoutSessionRequired = true,
                FrontChannelLogoutUri = "https://localhost:5005/signout-oidc",
                BackChannelLogoutSessionRequired = true,
                BackChannelLogoutUri = "https://localhost:5005/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:5005/signout-callback-oidc" },

                AllowOfflineAccess = true,//Allow the use of refresh tokens
                AllowedScopes = {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "offline_access", 
                    //Uses APIResource instead of APIScope
                    "identityManagement", "identityManagementAdmin", "admin"
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
                //More secure than a Jwt, but requires API to verify with STS on every interaction 
                AccessTokenType = AccessTokenType.Reference, 
                AlwaysIncludeUserClaimsInIdToken = true,
                AlwaysSendClientClaims = true,
            }
        };
    }
}