// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

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

        };

        public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope("AnAPI", "An API"),
            new ApiScope("scope2"),
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
                    "AnAPI"
                }
            },

            //JS Client
            new Client
            {
                ClientId = "JSClient",
                ClientName = "JavaScript Client",
                AllowedGrantTypes = GrantTypes.Code,
                RequireClientSecret = false,

                RedirectUris =           { "https://localhost:5003/callback.html" },
                PostLogoutRedirectUris = { "https://localhost:5003/index.html" },
                AllowedCorsOrigins =     { "https://localhost:5003" },

                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "AnAPI"
                }
            },

            new Client
            {
                ClientId = "IdentityManagement",
                ClientSecrets = { new Secret("AnotherSecret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = { "https://localhost:5005/signin-oidc" },
                FrontChannelLogoutUri = "https://localhost:5005/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:5005/signout-callback-oidc" },

                AllowOfflineAccess = true,
                AllowedScopes = {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Address,
                    "AnAPI"
                }
            }
        };
    }
}