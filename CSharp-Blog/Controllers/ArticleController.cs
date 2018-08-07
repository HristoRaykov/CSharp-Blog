using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CSharp_Blog.Data;
using CSharp_Blog.Models;
using Microsoft.AspNetCore.Authorization;

namespace CSharp_Blog.Controllers
{
    public class ArticleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ArticleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Article
        public ActionResult Index()
        {
            return RedirectToAction("List","Article");
        }

        // GET: Article/List
        public ActionResult List()
        {
            var articles = _context.Articles
                .Include(a => a.Author)
                .ToList();

            return View(articles);
        }

        // GET: Article/Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = _context.Articles
                .Include(a => a.Author)
                .SingleOrDefault(a => a.Id == id);

            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            
            return View();
        }

        
        [HttpPost]
        [Authorize]
        public IActionResult Create(Article article)
        {
            if (ModelState.IsValid)
            {
                var authorId = _context.Users
                    .FirstOrDefault(u => u.UserName == this.User.Identity.Name)
                    .Id;
                article.AuthorId = authorId;
                _context.Articles.Add(article);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(article);
        }

        [HttpGet]        
        public IActionResult Delete(int? id)
        {
            if (id==null)
            {
                return NotFound();
            }

            var article = _context.Articles
                .Include(a=>a.Author)
                .FirstOrDefault(a => a.Id == id);

            if (IsUserAuthorizedToEdit(article)==false)
            {
                return Forbid();
            }

            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        [HttpPost]
        [Route("/Article/Delete/{id}")]
        public IActionResult DeleteAction(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = _context.Articles
                .Include(a => a.Author)
                .FirstOrDefault(a => a.Id == id);

            if (IsUserAuthorizedToEdit(article) == false)
            {
                return Forbid();
            }

            if (article == null)
            {
                return NotFound();
            }

            _context.Articles.Remove(article);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id==null)
            {
                return NotFound();
            }

            Article article = _context.Articles
                .Include(a => a.Author)
                .FirstOrDefault(a => a.Id == id);

            if (IsUserAuthorizedToEdit(article) == false)
            {
                return Forbid();
            }

            if (article==null)
            {
                return NotFound();
            }

            var model = new ArticleViewModel();
            model.Id = article.Id;
            model.Title = article.Title;
            model.Content = article.Content;

            return View(model);
        }

        [HttpPost]
        [Route("/Article/Edit/{id}")]
        public ActionResult EditAction(ArticleViewModel model)
        {
            if (ModelState.IsValid)
            {
                Article article = _context.Articles
                    .Include(a => a.Author)
                    .FirstOrDefault(a => a.Id == model.Id);

                if (IsUserAuthorizedToEdit(article) == false)
                {
                    return Forbid();
                }

                article.Title = model.Title;
                article.Content = model.Content;

                _context.Articles.Update(article);
                _context.SaveChanges();

                return RedirectToAction("Index");

            }

            return RedirectToAction("Edit",model);
        }

        public bool IsUserAuthorizedToEdit(Article article)
        {
            return article.IsAuthor(this.User.Identity.Name);
        }
    }
}
