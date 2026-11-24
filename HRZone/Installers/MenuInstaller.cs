using HRZone.UI;
using Zenject;

namespace HRZone.Installers;

public class MenuInstaller : Installer<MenuInstaller>
{
    // The menu display and its poller are bound regardless of ModEnable: the Menu container is
    // installed once per game session, so an enable check here could only ever apply on the next
    // game start. Both classes read ModEnable while running, which makes the settings toggle live.
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<MainConfigMenu>().FromNewComponentAsViewController().AsSingle();
        Container.BindInterfacesAndSelfTo<ConfigViewFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

        Container.BindInterfacesAndSelfTo<MenuHRFeed>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<MenuHRZone>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
    }
}
