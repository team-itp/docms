using IdentityServer4.Models;
using System;
using System.Collections.Generic;

namespace Docms.Web.Config
{
    public class IdentityConfig
    {
        public static IEnumerable<Client> GetAllClients()
        {
            return new List<Client>()
            {
                new Client()
                {
                    ClientId = "docms-client",
                    ClientSecrets = new List<Secret>()
                    {
                        new Secret("docms-client-secret".Sha256())
                    },
                    AllowedScopes = { "docmsapi" },
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    AllowAccessTokensViaBrowser = true,
                }
            };
        }

        public static IEnumerable<ApiResource> GetAllResources()
        {
            return new List<ApiResource>()
            {
                new ApiResource("docmsapi", "文書管理システム API")
                {
                    Scopes = new List<Scope>() {
                        new Scope("docmsapi", "文書管理システム API")
                    },
                    ApiSecrets = new  List<Secret>() {
                        new Secret("docmsapi-secret".Sha256())
                    }
                }
            };
        }
    }
}
