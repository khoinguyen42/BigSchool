using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BigSchool.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace BigSchool.Controllers
{
    public class CoursesController : Controller
    {
        BigSchoolContext context = new BigSchoolContext();
        // GET: Courses
        public ActionResult Create()
        {
            Course objCourse = new Course();
            objCourse.ListCategory = context.Categories.ToList();
            return View(objCourse);
        }
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create (Course objCourse)
        {
            ModelState.Remove("LecturerId");
            if (!ModelState.IsValid)
            {
                objCourse.ListCategory = context.Categories.ToList();
                return View("Create", objCourse);
            }
            ApplicationUser user = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());
            objCourse.LecturerId = user.Id;
            context.Courses.Add(objCourse);
            context.SaveChanges();
            return RedirectToAction("Index", "Home");
        }
        public ActionResult Attending()
        {
            ApplicationUser currentUser = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());
            var listAttendances = context.Attendances.Where(p => p.Attendee == currentUser.Id).ToList();
            var courses = new List<Course>();
            foreach (Attendance temp in listAttendances)
            {
                Course objCourse = temp.Course;
                objCourse.LectureName = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(objCourse.LecturerId).Name;
                courses.Add(objCourse);
            }
            return View(courses);
        }
        public ActionResult Mine()
        {
            ApplicationUser currentUser = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());
            var courses = context.Courses.Where(c => c.LecturerId == currentUser.Id && c.DateTime > DateTime.Now).ToList();
            foreach (Course i in courses)
            {
                i.LectureName = currentUser.Name;
            }
            return View(courses);
        }
        public ActionResult Edit(int id)
        {
            Course course = context.Courses.SingleOrDefault(p => p.Id == id);
            course.ListCategory = context.Categories.ToList();
            if (course == null)
            {
                return HttpNotFound();
            }
            return View(course);
        }
        [HttpPost]
        [Authorize]
        public ActionResult Edit(Course c)
        {
            Course courseUpdate = context.Courses.SingleOrDefault(p => p.Id == c.Id);
            if (courseUpdate != null)
            {
                context.Courses.AddOrUpdate(c);
                context.SaveChanges();
            }
            return RedirectToAction("Mine");
        }
        [Authorize]
        public ActionResult Delete(int id)
        {
            BigSchoolContext context = new BigSchoolContext();
            Course couse = context.Courses.SingleOrDefault(p => p.Id == id);
            if (couse == null)
            {
                return HttpNotFound();
            }
            return View(couse);
        }
        [Authorize]
        public ActionResult DeleteMine(int id)
        {
            BigSchoolContext context = new BigSchoolContext();
            Course couse = context.Courses.SingleOrDefault(p => p.Id == id);
            if (couse != null)
            {
                var list = context.Attendances.Where(m => m.CourseId == id).ToList();
                if(list != null)
                {
                    foreach(var item in list)
                    {
                        context.Attendances.Remove(item);
                        context.SaveChanges();

                    }
                    context.Courses.Remove(couse);
                    context.SaveChanges();
                    return RedirectToAction("Attending", "Courses");
                }
                context.Courses.Remove(couse);
                context.SaveChanges();
                return RedirectToAction("Attending", "Courses");
            }
            return RedirectToAction("Mine");
        }
    }
}