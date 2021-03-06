﻿using Microsoft.AspNet.Identity.Owin;
using SdojWeb.Models;
using StructureMap;
using System.Security.Principal;
using System.Web;

namespace SdojWeb.Infrastructure.IoC
{
    public class MvcRegistry : Registry
    {
        public MvcRegistry()
        {
            For<IIdentity>().Use(() => HttpContext.Current.User.Identity);
            For<IPrincipal>().Use(() => HttpContext.Current.User);
            For<HttpContextBase>()
                .Use(() => new HttpContextWrapper(HttpContext.Current));
            For<ApplicationDbContext>().Use(() => 
                ApplicationDbContext.Create());
            For<ApplicationUserManager>().Use(() =>
                HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>());
            For<ApplicationRoleManager>().Use(() =>
                HttpContext.Current.GetOwinContext().Get<ApplicationRoleManager>());
        }
    }
}