using Godot;
using Packinator3D.datastructure;

namespace Packinator3D.scenes.menus.main;

public partial class QuitHandler : Node {
    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest) {
            SaveManager.Save();
        }
    }
}