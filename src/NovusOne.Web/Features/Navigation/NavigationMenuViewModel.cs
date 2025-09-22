using System.Collections.Generic;

namespace NovusOne.Web.Features.Navigation;

public class NavigationMenuViewModel
{
    public string Name { get; set; }

    public IEnumerable<NavigationItemViewModel> Items { get; set; }
}
