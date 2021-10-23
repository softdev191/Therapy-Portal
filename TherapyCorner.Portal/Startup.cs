using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TherapyCorner.Portal.Startup))]
namespace TherapyCorner.Portal
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
