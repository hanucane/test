using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Test.Models;

namespace Test.Controllers
{
    public class HomeController : Controller
    {
        private Belt _context;
        public HomeController(Belt context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("registration")]
        public IActionResult Registration()
        {
            return View("Forms/Register");
        }

        [HttpPost("register")]
        public IActionResult Process(Users NewUser, string confirm_password)
        {
            if(ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(x => x.email == NewUser.email);
                if (user == null){
                    if (NewUser.password == confirm_password){
                        PasswordHasher<Users> Hasher = new PasswordHasher<Users>();
                        NewUser.password = Hasher.HashPassword(NewUser, NewUser.password);
                        _context.Users.Add(NewUser);
                        _context.SaveChanges();
                        HttpContext.Session.SetInt32("user", NewUser.id);
                        HttpContext.Session.SetString("user_name", NewUser.first_name+" "+NewUser.last_name);
                        return Redirect("/home");
                    }
                    ModelState.AddModelError("password", "Passwords must match.");
                }
                ModelState.AddModelError("email", "User with email already exists.");
            }
            return View("Forms/Register");
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            return View("Forms/Login");
        }

        [HttpPost("login")]
        public IActionResult Login(string email, string Password)
        {
            var user = _context.Users.FirstOrDefault(x => x.email == email);
            if(user != null && Password != null)
            {
                var Hasher = new PasswordHasher<Users>();
                if(0 != Hasher.VerifyHashedPassword(user, user.password, Password))
                {
                    HttpContext.Session.SetInt32("user", user.id);
                    HttpContext.Session.SetString("user_name", user.first_name+" "+user.last_name);
                    ViewBag.login="You successfully logged in.";
                    return Redirect("/home");
                }
            }
            ViewBag.login="Please try logging in again.";
            return View("Forms/Login");
        }

        [HttpGet("/logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Redirect("/");
        }

        [HttpGet("/home")]
        public IActionResult Dashboard()
        {
            if(HttpContext.Session.GetInt32("user") != null){
                ViewBag.activity = _context.Activities.OrderBy(x=>x.event_date)
                    .Include(y => y.Creator)
                    .Include(y => y.Participant).ThenInclude(z => z.User).ToList();
                return View("Dashboard");
            }
            @ViewBag.login = "Please log in to view that page.";
            return View("Index");
        }

        [HttpGet("/new")]
        public IActionResult NewActivity()
        {
            if(HttpContext.Session.GetInt32("user") != null){
                return View("Forms/ActivityCreate");
            }
            @ViewBag.login = "Please log in to view that page.";
            return View("Index");
        }

        [HttpPost("createactivity")]
        public IActionResult CreateActivity(Activities newActivity, int duration, string duration_units)
        {
            if(ModelState.IsValid)
            {
                _context.Activities.Add(newActivity);
                _context.SaveChanges();
                if(duration_units == "Minutes")
                {
                    int event_duration = duration;
                    newActivity.duration = event_duration;
                }
                if(duration_units == "Hours")
                {
                    int event_duration = duration*60;
                    newActivity.duration = event_duration;
                }
                if(duration_units == "Days")
                {
                    int event_duration = duration*60*24;
                    newActivity.duration = event_duration;
                }
                _context.SaveChanges();
                TempData["event_id"] = newActivity.id;
                TempData["user_id"] = newActivity.CreatorId;
                return RedirectToAction("AddParticipant");
            }
            return View("Forms/ActivityCreate");
        }

        [HttpGet("addParticipant")]
        public IActionResult AddParticipant()
        {
            Participants newParticipant = new Participants(){
                    ActivitiesId = (int)TempData["event_id"],
                    UsersId = (int)TempData["user_id"]
                };
            _context.Participants.Add(newParticipant);
            _context.SaveChanges();
            return Redirect("/activity/"+TempData["event_id"]);
        }

        [HttpGet("add_rsvp/{id}")]
        public IActionResult Rsvp(int id)
        {
            if(ModelState.IsValid){
                // var conflicts = _context.Participants.Where(x=>x.id == HttpContext.Session.GetInt32("user")).Include(y => y.Activities).ToList();
                // foreach(var conflict in conflicts){
                //     string start_date = conflict.Activities.event_date.ToShortDateString();
                //     string start_time = conflict.Activities.event_time.ToShortTimeString(); 
                //     DateTime duration = new DateTime(0,0,0,0,conflict.Activities.duration,0);
                //     string end_time = start_time + duration.ToLongTimeString();
                    
                // }
                int? user = HttpContext.Session.GetInt32("user");
                int user_id = _context.Users.FirstOrDefault(x => x.id == user).id;
                Participants newParticipant = new Participants(){
                    ActivitiesId = id,
                    UsersId = user_id
                };
                _context.Participants.Add(newParticipant);
                _context.SaveChanges();
            }
            return Redirect("/activity/"+id);
        }

        [HttpGet("delete_rsvp/{id}")]
        public IActionResult DeleteRsvp(int id)
        {
            var parent = _context.Activities.Include(x => x.Participant).SingleOrDefault(m => m.id == id);
            foreach (var Participant in parent.Participant.ToList()){
                if (HttpContext.Session.GetInt32("user") == Participant.UsersId){
                    _context.Participants.Remove(Participant);
                }
            }
            _context.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet("/activity/{id}")]
        public IActionResult ViewActivity(int id)
        {
            if(HttpContext.Session.GetInt32("user") != null){
            ViewBag.activity = _context.Activities.Where(x => x.id == id)
                .Include(y => y.Creator)
                .Include(y => y.Participant).ThenInclude(z => z.User).ToList();
            return View("ActivityView");
            }
            @ViewBag.login = "Please log in to view that page.";
            return View("Index");
        }
        
        [HttpGet("activity_delete/{id}")]
        public IActionResult DeleteActivity(int id)
        {
            Activities dactivity = _context.Activities.SingleOrDefault(m => m.id == id);
            var parent = _context.Activities.Include(x => x.Participant).SingleOrDefault(m => m.id == id);
            foreach (var participant in parent.Participant.ToList()){
                _context.Participants.Remove(participant);
            }
            _context.Activities.Remove(dactivity);
            _context.SaveChanges();
            return RedirectToAction("Dashboard");
        }
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
