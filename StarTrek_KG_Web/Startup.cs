using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(StarTrek_KG_Web.Startup))]
namespace StarTrek_KG_Web
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
           
        }
    }
}
