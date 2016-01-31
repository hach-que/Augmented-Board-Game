using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AugmentedBoardGame.Webcam;
using Microsoft.Xna.Framework.Input;
using Protogame;

namespace AugmentedBoardGame
{
    public class DesktopEventBinder : StaticEventBinder<IGameContext>
    {
        public override void Configure()
        {
            //Bind<KeyPressEvent>(x => x.Key == Keys.Space).On<DetectorEntity>().To<SwitchColorAction>();
        }
    }

    public class SwitchColorAction : IEventEntityAction<DetectorEntity>
    {
        public void Handle(IGameContext context, DetectorEntity entity, Event @event)
        {
            //entity.NextColor();
        }
    }
}
