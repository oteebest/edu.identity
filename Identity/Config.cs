// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> Ids =>
            new IdentityResource[]
            { 
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };

        public static IEnumerable<ApiResource> Apis =>
            new ApiResource[] 
            { 
                new ApiResource("houseoftzapi","house of tz api")
                {
                    ApiSecrets = { new Secret("EF61AF64-D3ED-4F70-AEF9-5E45BB3AA6F6".Sha256()) }
                }
            };
        
        public static IEnumerable<Client> Clients =>
            new Client[] 
            {
                new Client
                {
                     AccessTokenLifetime = 600,
                    AllowOfflineAccess = true,
                    UpdateAccessTokenClaimsOnRefresh = true,
                      AllowedScopes =  new List<string> {

                          "houseoftzapi",
                          IdentityServerConstants.StandardScopes.OpenId,
                          IdentityServerConstants.StandardScopes.Profile,
                      },
                      ClientName = "House of TZ Client",
                      ClientId = "houseoftz",
                      AllowedGrantTypes = GrantTypes.Code,
                      ClientSecrets = new List<Secret>{
                           new Secret("AE240F15-9CB5-4633-8798-93DEC792B368".Sha512()),
                      },
                      RequirePkce = true,
                      RedirectUris = new List<string>()
                      {
                        "https://localhost:44389/signin-oidc"
                      },
                      PostLogoutRedirectUris = new List<string>()
                      {
                        "https://localhost:44389/signout-callback-oidc"
                      },


                }
            };
        
    }
}