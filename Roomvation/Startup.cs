using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Roomvation.Startup))]
namespace Roomvation
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
