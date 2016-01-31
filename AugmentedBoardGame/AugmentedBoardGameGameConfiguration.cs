using Microsoft.Xna.Framework;
using Protogame;
using Protoinject;

namespace AugmentedBoardGame
{
    public class AugmentedBoardGameGameConfiguration : IGameConfiguration
    {
        public void ConfigureKernel(IKernel kernel)
        {
            kernel.Load<ProtogameCoreModule>();
            kernel.Load<ProtogameAssetIoCModule>();
            kernel.Load<ProtogameEventsIoCModule>();
            kernel.Load<AugmentedBoardGameModule>();
        }

        public void InitializeAssetManagerProvider(IAssetManagerProviderInitializer initializer)
        {
            initializer.Initialize<GameAssetManagerProvider>();
        }

        public Game ConstructGame(IKernel kernel)
        {
            return new AugmentedBoardGameGame(kernel);
        }
    }
}