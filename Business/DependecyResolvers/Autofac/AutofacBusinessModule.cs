using Autofac;
using Autofac.Extras.DynamicProxy;
using Business.Abstract;
using Business.Concrete;
using Castle.DynamicProxy;
using Core.Utilities.Interceptors;
using Core.Utilities.Security.JWT;
using DataAccess.Abstract;
using DataAccess.Concrete;
using DataAccess.Concrete.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.DependecyResolvers.Autofac
{
    public class AutofacBusinessModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BolumManager>().As<IBolumService>();
            builder.RegisterType<EfBolumDal>().As<IBolumDal>();

            builder.RegisterType<DersManager>().As<IDersService>();
            builder.RegisterType<DersDal>().As<IDersDal>();

            builder.RegisterType<AkademikPersonelManager>().As<IAkademikPersonelService>();
            builder.RegisterType<EfAkademikPersonelDal>().As<IAkademikPersonelDal>();

            builder.RegisterType<SinavDetayManager>().As<ISinavDetayService>();
            builder.RegisterType<EfSinavDetayDal>().As<ISinavDetayDal>();

            builder.RegisterType<DBAPManager>().As<IDBAPService>();
            builder.RegisterType<DBAPDal>().As<IDBAPDal>();

            builder.RegisterType<DerslikManager>().As<IDerslikService>();
            builder.RegisterType<DerslikDal>().As<IDerslikDal>();

            builder.RegisterType<OgrenciManager>().As<IOgrenciService>();
            builder.RegisterType<EfOgrenciDal>().As<IOgrenciDal>();

            builder.RegisterType<EfUserDal>().As<IUserDal>();
            builder.RegisterType<UserManager>().As<IUserService>();

            builder.RegisterType<SinavDerslikManager>().As<ISinavDerslikService>();
            builder.RegisterType<EfSinavDerslikDal>().As<ISinavDerslikDal>();

            builder.RegisterType<AuthManager>().As<IAuthService>();
            builder.RegisterType<JwtHelper>().As<ITokenHelper>();

            // Operation Claims servisleri
            builder.RegisterType<OperationClaimManager>().As<IOperationClaimService>();
            builder.RegisterType<EfOperationClaimDal>().As<IOperationClaimDal>();
            // User Operation Claims servisleri
            builder.RegisterType<UserOperationClaimManager>().As<IUserOperationClaimService>();
            builder.RegisterType<EfUserOperationClaimDal>().As<IUserOperationClaimDal>();

            builder.RegisterType<BolumAkademikPersonelManager>().As<IBolumAkademikPersonellerService>();
            builder.RegisterType<EfBolumAkademikPersonellerDal>().As<IBolumAkademikPersonellerDal>();

            builder.RegisterType<DersBolumManager>().As<IDersBolumService>();
            builder.RegisterType<EfDersBolumDal>().As<IDersBolumDal>();

            builder.RegisterType<EfDerslikBolumDal>().As<IDerslikBolumDal>();

            // Notification ve Announcement servisleri
            builder.RegisterType<NotificationManager>().As<INotificationService>();
            builder.RegisterType<EfNotificationDal>().As<INotificationDal>();
            
            builder.RegisterType<AnnouncementManager>().As<IAnnouncementService>();
            builder.RegisterType<EfAnnouncementDal>().As<IAnnouncementDal>();
            builder.RegisterType<EfAnnouncementReadStatusDal>().As<IAnnouncementReadStatusDal>();

            // YasirSharp AI - Assistant System (26 Ekim 2025)
            builder.RegisterType<AssistantManager>().As<IAssistantService>();
            builder.RegisterType<EfAssistantInteractionDal>().As<IAssistantInteractionDal>();
            
            builder.RegisterType<UserAssistantPreferenceManager>().As<IUserAssistantPreferenceService>();
            builder.RegisterType<EfUserAssistantPreferenceDal>().As<IUserAssistantPreferenceDal>();


            var assembly = System.Reflection.Assembly.GetExecutingAssembly();

            builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces()
                .EnableInterfaceInterceptors(new ProxyGenerationOptions()
                {
                    Selector = new AspectInterceptorSelector()
                }).SingleInstance();

        }
    }
}
