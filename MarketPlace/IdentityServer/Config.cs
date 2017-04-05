using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityModel;
using IdentityServer4;
using IdentityServer.Models;

namespace IdentityServer
{
    public class Config
    {
        ///requires using IdentityServer4.Models;
        public static IEnumerable<ApiResource> GetApiResources() {
            return new List<ApiResource>{
                //requires using IdentityModel;
                new ApiResource("MarketplaceService", "Marketplace Service" ,new [] { JwtClaimTypes.Name})
            };
        }

        public static IEnumerable<Client> GetClients() {
            return new List<Client> {
                new Client {
                    ClientId = "js",
                    ClientName = "JavaScript Client",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,

                    RedirectUris =           { "http://localhost:5001/callback.html" },
                    PostLogoutRedirectUris = { "http://localhost:5001/index.html" },
                    AllowedCorsOrigins =     { "http://localhost:5001" },

                    //requires using IdentityServer4;
                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "MarketplaceService"
                    }
                }
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources() {
            return new List<IdentityResource> {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
        }

        //requires using IdentityServer.Models;
        public static List<ApplicationUser> GetUsers() {
            return new List<ApplicationUser> {
                new ApplicationUser {
                    UserName = "alice@gmail.com",
                    Email = "alice@gmail.com"
                },
                new ApplicationUser {
                    UserName = "bob@gmail.com",
                    Email = "bob@gmail.com"
                }
            };
                }
    }
}
