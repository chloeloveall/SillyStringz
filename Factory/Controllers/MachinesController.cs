using Factory.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Factory.Controllers
{
  public class MachinesController : Controller
  {
    private readonly FactoryContext _db;

    public MachinesController(FactoryContext db)
    {
      _db = db;
    }

    public ActionResult Index()
    {
      List<Machine> model = _db.Machines.ToList();
      return View(_db.Machines.OrderBy(m=>m.MachineName).ToList());
    }

    public ActionResult Create()
    {
      return View();
    }

    [HttpPost]
    public ActionResult Create(Machine machine)
    {
      if (ModelState.IsValid)
      {  
        DateTime now = DateTime.Now;
        machine.InstallationDate = now;
        _db.Machines.Add(machine);
        _db.SaveChanges();
        return RedirectToAction("Index");
      }
      return View(machine);
    }

    public ActionResult Details(int id)
    {
      var thisMachine = _db.Machines
          .Include(machine => machine.JoinEntities)
          .ThenInclude(join => join.Engineer)
          .FirstOrDefault(machine => machine.MachineId == id);
      return View(thisMachine);
    }

    public ActionResult Edit(int id)
    {
      var thisMachine = _db.Machines.FirstOrDefault(machine => machine.MachineId == id);
      ViewBag.EngineerId = new SelectList(_db.Engineers, "EngineerId", "EngineerName");
      return View(thisMachine);
    }

    [HttpPost]
    public ActionResult Edit(Machine machine, int engineerId)
    {
      bool matches = _db.EngineerMachine.Any(x => x.MachineId == machine.MachineId && x.EngineerId == engineerId);
      if (!matches && ModelState.IsValid)
      {
        if (engineerId != 0)
        {
          _db.EngineerMachine.Add(new EngineerMachine() { EngineerId = engineerId, MachineId = machine.MachineId });
        }
      }
      _db.Entry(machine).State = EntityState.Modified;
      _db.SaveChanges();
      return RedirectToAction("Index");
    }

    public ActionResult Delete(int id)
    {
      var thisMachine = _db.Machines.FirstOrDefault(machine => machine.MachineId == id);
      return View(thisMachine);
    }

    [HttpPost, ActionName("Delete")]
    public ActionResult DeleteConfirmed(int id)
    {
      var thisMachine = _db.Machines.FirstOrDefault(machine => machine.MachineId == id);
      _db.Machines.Remove(thisMachine);
      _db.SaveChanges();
      return RedirectToAction("Index");
    }

    // the AddEngineer POST/GET routes have been merged into the Edit routes
    // this code is left here for instructor review in case the separate add route was required 

    // public ActionResult AddEngineer(int id)
    // {
    //   var thisMachine = _db.Machines.FirstOrDefault(machine => machine.MachineId == id);
    //   ViewBag.EngineerId = new SelectList(_db.Engineers, "EngineerId", "EngineerName");
    //   return View(thisMachine);
    // }

    // [HttpPost]
    // public ActionResult AddEngineer(Machine machine, int engineerId)
    // {
    //   bool matches = _db.EngineerMachine.Any(x => x.MachineId == machine.MachineId && x.EngineerId == engineerId);
    //   if(!matches)
    //   {
    //     if (engineerId != 0)
    //     {
    //       _db.EngineerMachine.Add(new EngineerMachine() { EngineerId = engineerId, MachineId = machine.MachineId });
    //     }
    //   }
    //   _db.SaveChanges();
    //   return RedirectToAction("Index");
    // }

    [HttpPost]
    public ActionResult DeleteEngineer(int joinId)
    {
      var joinEntry = _db.EngineerMachine.FirstOrDefault(entry => entry.EngineerMachineId == joinId);
      _db.EngineerMachine.Remove(joinEntry);
      _db.SaveChanges();
      return RedirectToAction("Index");
    }

    public async Task<IActionResult> Search(string searchString)
    {
      ViewBag.MachineId = new SelectList(_db.Machines, "MachineId", "MachineName");
      var search = from m in _db.Machines
        select m;

      if (!String.IsNullOrEmpty(searchString))
      {
        search = search.Where(s => s.MachineName.Contains(searchString));
        return View(await Task.FromResult(search.ToList()));
      }
      else
      {
        List<Machine> model = _db.Machines.ToList();
        return View(_db.Machines.OrderBy(m=>m.MachineName).ToList());
      }
    }

  }
}