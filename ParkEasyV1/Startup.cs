using Microsoft.Owin;
using Owin;
using Stripe;
using System.Configuration;

[assembly: OwinStartupAttribute(typeof(ParkEasyV1.Startup))]
namespace ParkEasyV1
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
