using System.Threading.Tasks;

namespace NovusOne.Web.Features.Navigation;

public interface INavigationService
{
    Task<NavigationItemViewModel> GetNavigationItemViewModel(NavigationItem navigationItem);

    Task<NavigationMenuViewModel> GetNavigationMenuViewModel(NavigationMenu navigationMenu);
}
