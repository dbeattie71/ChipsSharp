using Cirrious.CrossCore;
using Cirrious.CrossCore.IoC;
using Cirrious.MvvmCross.Plugins.ModernHttpClient;
using EnjinMobile.Api;
using Test.Core.Helpers;
using Test.Core.ViewModels;

namespace Test.Core
{
    public class App : Cirrious.MvvmCross.ViewModels.MvxApplication
    {
        public override void Initialize()
        {
            CreatableTypes()
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();

			var testSessionPersistence = new TestSessionPersistence();

			var modernHttpClient = Mvx.Resolve<IModernHttpClient>();

			var enjinApiFacade = new EnjinApiFacade();
			enjinApiFacade = enjinApiFacade
				.WithDependency(testSessionPersistence)
				.WithDependency(modernHttpClient.GetNativeHandler());

			var sessionId = "i51iq5g5hpcpref9s45lbe3pt4";
			var sessionService = enjinApiFacade.SessionService;
			sessionService.UserSession.SessionId = sessionId;
			sessionService.SaveSession();

			Mvx.RegisterSingleton<IEnjinApiFacade>(() => enjinApiFacade);

			RegisterAppStart<FirstViewModel>();
        }
    }
}