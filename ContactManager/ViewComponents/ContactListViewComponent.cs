using ContactManager.Models;
using Microsoft.AspNetCore.Mvc;

namespace ContactManager.ViewComponents;

public class ContactListViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(ContactListViewModel model)
    {
        return View(model);
    }
}