using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PeopleApp.Infrastructure.Repository;
using PeopleApp.Models;
using PeopleApp.ViewModels;

namespace PeopleApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPersonRepository _personRepository;

        public HomeController(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        public async Task<ActionResult<List<ReadPerson>>> Index()
        {
            var result = (await _personRepository.GetAll())
                .Select(x => new ReadPerson()
                 {
                     PersonId = x.PersonId,
                     Name = x.Name,
                     Email = x.Email,
                     IsActive = x.IsActive
                 })
                .OrderBy(x => x.Name).ToList();

            if (TempData["Message"] != null)
            {
                ViewData["Message"] = TempData["Message"];
            }
            if (TempData["Error"] != null)
            {
                ViewData["Error"] = TempData["Error"];
            }

            return View(result);
        }

        public IActionResult Add()
        {
            ViewData["Title"] = "Add Person";
            return View("AddEdit", new AddEditDelete { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddEditDelete aevm)
        {
            var person = new Person()
            {
                Name = aevm.Name,
                Email = aevm.Email,
                IsActive = aevm.IsActive
            };

            Result res = await _personRepository.Add(person);

            if (res.Success)
            {
                TempData["Message"] = "Person added successfully.";
            }
            else
            {
                TempData["Error"] = res.Error;
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(Guid personId)
        {
            ViewData["Title"] = "Edit Person";

            var person = await _personRepository.Get(personId);

            // copy to viewmodel
            var aevm = new AddEditDelete
            {
                PersonId = person.PersonId,
                Name = person.Name,
                Email = person.Email,
                IsActive = person.IsActive,
                LastUpdatedDttm = person.LastUpdatedDttm
            };

            return View("AddEdit", aevm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AddEditDelete aevm)
        {
            var person = new Person()
            {
                PersonId = aevm.PersonId,
                Name = aevm.Name,
                Email = aevm.Email,
                IsActive = aevm.IsActive,
                LastUpdatedDttm = aevm.LastUpdatedDttm
            };

            Result res = await _personRepository.Update(person);

            if (res.Success)
            {
                TempData["Message"] = "Person edited successfully.";
            }
            else
            {
                TempData["Error"] = res.Error;
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(Guid personId)
        {
            var person = await _personRepository.Get(personId);

            if (person == null)
            {
                TempData["Error"] = "Error retrieving person details.";
                return RedirectToAction("Index");
            }

            // Copy to view model
            var aevm = new AddEditDelete
            {
                PersonId = person.PersonId,
                Name = person.Name,
                Email = person.Email,
                IsActive = person.IsActive
            };
            return View(aevm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid personId)
        {
            var res = await _personRepository.Delete(personId);

            if (res.Success)
            {
                TempData["Message"] = "Person deleted successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = res.Error;
            }

            return RedirectToAction("Index");
        }

        [AcceptVerbs("GET", "POST")]
        public IActionResult ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Json($"Email is required."); ;

            var error = $"Email { email} is invalid.";

            try
            {
                if (!Regex.IsMatch(email,
                   @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                   RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)))
                {
                    return Json(error);
                }
            }
            catch (RegexMatchTimeoutException)
            {
                return Json(error);
            }

            return Json(true);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
