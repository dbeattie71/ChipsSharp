using System.Collections.Generic;
using System.Reflection;
using Android.Content;
using com.android.ex.chips;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Droid.Platform;
using Cirrious.MvvmCross.ViewModels;
using Test.Core.ViewModels;
using Test.Droid.Helpers;

namespace Test.Droid
{
    public class Setup : MvxAndroidSetup
    {
        public Setup(Context applicationContext) : base(applicationContext)
        {
        }

        protected override IMvxApplication CreateApp()
        {
            return new Core.App();
        }
		
        protected override IMvxTrace CreateDebugTrace()
        {
            return new DebugTrace();
        }

		protected override List<Assembly> ValueConverterAssemblies
		{
			get
			{
				var toReturn = base.ValueConverterAssemblies;
				toReturn.Add(typeof(FirstViewModel).Assembly);
				return toReturn;
			}
		}

		//protected override void FillValueConverters(IMvxValueConverterRegistry registry)
		//{
		//	base.FillValueConverters(registry);
		//	registry.AddOrOverwrite("RecipientsConverter", new RecipientsConverter());
		//	//            registry.AddOrOverwrite("FormatPhoneNumberConverter", new FormatNameConverter());
		//}

		protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
		{
			registry.RegisterPropertyInfoBindingFactory((typeof(MvxAutoCompleteChipsEditTextViewPartialTextTargetBinding)),
												typeof(ChipsEditTextView), "PartialText");

			//registry.RegisterPropertyInfoBindingFactory(
			//									  typeof(MvxAutoCompleteRecipientEditTextViewSelectedObjectTargetBinding),
			//									  typeof(RecipientEditTextView),
			//									  "SelectedObject");



			base.FillTargetFactories(registry);
		}

		//protected override void FillBindingNames(IMvxBindingNameRegistry bindingNameRegistry)
		//{
		//	//bindingNameRegistry.AddOrOverwrite(typeof(TextViewImeActionBinding), "Command");
		//}
    }
}