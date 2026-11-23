using HRZone.UI;
using Zenject;

namespace HRZone.Installers;

public class MenuInstaller : Installer<MenuInstaller>
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<MainConfigMenu>().FromNewComponentAsViewController().AsSingle();
        Container.BindInterfacesAndSelfTo<ConfigViewFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
    }
}
