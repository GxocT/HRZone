using BeatSaberMarkupLanguage.MenuButtons;
using BGLib.Polyglot;
using HMUI;
using Zenject;

namespace HRZone.UI;

public class ConfigViewFlowCoordinator : FlowCoordinator
{
    [Inject]
    private readonly MainFlowCoordinator _mainFlowCoordinator = null!;

    [Inject]
    private readonly MenuButtons _menuButtons = null!;

    [Inject]
    private readonly MainConfigMenu _mainPanel = null!;

    private readonly MenuButton _menuButton;

    public ConfigViewFlowCoordinator() => _menuButton =
        new MenuButton(Localization.Get("HRZONE_HRZONE"), Localization.Get("HRZONE_MENU_BUTTON_HINT"), OnMenuButtonClick);

    private void Start()
    {
        _menuButtons.RegisterButton(_menuButton);
    }

    private void OnMenuButtonClick()
    {
        _mainFlowCoordinator.PresentFlowCoordinator(this);
    }

    protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
    {
        if (firstActivation)
        {
            SetTitle(Localization.Get("HRZONE_CONFIG_MENU_TITLE"));
            showBackButton = true;
            ProvideInitialViewControllers(_mainPanel);
        }
    }

    protected override void BackButtonWasPressed(ViewController topController)
    {
        base.BackButtonWasPressed(topController);
        _mainFlowCoordinator.DismissFlowCoordinator(this);
    }
}
