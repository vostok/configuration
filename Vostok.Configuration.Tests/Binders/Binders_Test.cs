using NUnit.Framework;
using SimpleInjector;
using Vostok.Configuration.Binders;

namespace Vostok.Configuration.Tests.Binders
{
    public class Binders_Test
    {
        protected Container Container;

        [SetUp]
        public void SetUp()
        {
            Container = new Container();
            Container.RegisterConditional(typeof(ISettingsBinder<>), typeof(PrimitiveAndSimpleBinder<>),
                c => PrimitiveAndSimpleBinder<bool>.IsAvailableType(c.ServiceType.GetGenericArguments()[0]));
            Container.RegisterConditional(typeof(ISettingsBinder<>), typeof(NullableBinder<>),
                c => c.ServiceType.GetGenericArguments()[0].IsNullable());
            Container.RegisterConditional(typeof(ISettingsBinder<>), typeof(EnumBinder<>),
                c => c.ServiceType.GetGenericArguments()[0].IsEnum);
            Container.Register(typeof(ISettingsBinder<>), typeof(ListBinder<>));
            Container.Register(typeof(ISettingsBinder<>), typeof(DictionaryBinder<,>));
            Container.Register(typeof(ISettingsBinder<>), typeof(SetBinder<>));
            Container.RegisterConditional(typeof(ISettingsBinder<>), typeof(ArrayBinder<>),
                c => c.ServiceType.GetGenericArguments()[0].IsArray);
            Container.RegisterConditional(typeof(ISettingsBinder<>), typeof(ClassAndStructBinder<>),
                c =>
                {
                    var type = c.ServiceType.GetGenericArguments()[0];
                    return type.IsValueType && !type.IsPrimitive && !type.IsGenericType && !type.IsEnum && !PrimitiveAndSimpleBinder<bool>.IsAvailableType(type) || !c.Handled;
                });
            Container.Register<ISettingsBinderFactory>(() => new SettingsBinderFactory(Container));
        }
    }
}