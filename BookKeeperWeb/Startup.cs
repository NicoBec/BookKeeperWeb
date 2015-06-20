using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BookKeeperWeb.Startup))]
namespace BookKeeperWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
