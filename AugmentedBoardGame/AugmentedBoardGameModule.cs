using System;
using Protogame;
using Protoinject;

namespace AugmentedBoardGame
{
    public class AugmentedBoardGameModule : IProtoinjectModule
    {
        public void Load(IKernel kernel)
        {
            kernel.Bind<IEntityFactory>().ToFactory();
            kernel.Bind<IAssetManager>().ToMethod(x => x.Kernel.Get<IAssetManagerProvider>().GetAssetManager());
            kernel.Bind<IEventBinder<IGameContext>>().To<DesktopEventBinder>();
            kernel.Bind<ISkin>().To<TransparentBasicSkin>();
            kernel.Bind<IBasicSkin>().To<DefaultBasicSkin>();
        }
    }
}

