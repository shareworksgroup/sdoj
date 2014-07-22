﻿using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using SdojWeb.Infrastructure.Extensions;
using SdojWeb.Infrastructure.Identity;
using SdojWeb.Manager;
using SdojWeb.Models;
using SdojWeb.Infrastructure.Alerts;
using AutoMapper.QueryableExtensions;

namespace SdojWeb.Controllers
{
    [SdojAuthorize(EmailConfirmed = true)]
    public class QuestionController : Controller
    {
        public QuestionController(ApplicationDbContext dbContext, QuestionManager manager)
        {
            _dbContext = dbContext;
            _manager = manager;
        }

        // GET: Questions
        [AllowAnonymous]
        public ActionResult Index(int? page, string orderBy, bool? asc)
        {
            var models = _dbContext.Questions.Project().To<QuestionSummaryViewModel>();
            var orderedPagedList = models.ToSortedPagedList(page, orderBy, asc);
            return View(orderedPagedList);
        }

        // GET: Questions/Details/5
        [AllowAnonymous]
        public async Task<ActionResult> Details(int id)
        {
            var question = await _dbContext.Questions
                .Project().To<QuestionDetailModel>()
                .FirstAsync(x => x.Id == id);

            if (question == null)
            {
                return RedirectToAction("Index").WithError(
                    string.Format("没找到 id 为 {0} 的题目。", id));
            }

            return View(question);
        }

        // GET: Questions/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Questions/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(QuestionCreateModel createModel)
        {
            if (ModelState.IsValid)
            {
                if (await _manager.ExistName(createModel.Name))
                {
                    ModelState.AddModelError("Name", "已有同名的题目。");
                }
                else
                {
                    await _manager.Create(createModel);
                    return RedirectToAction("Index");
                }
            }

            return View(createModel);
        }

        // GET: Questions/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var question = await _dbContext
                .Questions
                .Project().To<QuestionEditModel>()
                .FirstAsync(x => x.Id == id);

            if (!User.IsUserOrAdmin(question.CreateUserId))
            {
                return RedirectToAction("Index").WithWarning("仅题目创建者才能编辑题目。");
            }

            return View(question);
        }

        // POST: Questions/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(QuestionEditModel model)
        {
            if (ModelState.IsValid)
            {
                var secretModel = await _dbContext.Questions
                    .Where(x => x.Id == model.Id)
                    .Project().To<QuestionNotMappedEditModel>()
                    .FirstOrDefaultAsync();

                if (!User.IsUserOrAdmin(secretModel.CreateUserId))
                {
                    return RedirectToAction("Index").WithWarning("仅题目创建者才能编辑题目。");
                }

                await _manager.Update(secretModel, model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // GET: Questions/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            var question = await _dbContext.Questions.FindAsync(id);
            if (question == null)
            {
                return RedirectToAction("Index").WithError("未找到该题目。");
            }
            if (!User.IsUserOrAdmin(question.CreateUserId))
            {
                return RedirectToAction("Index").WithWarning("仅题目创建者才能删除题目。");
            }

            return View(question);
        }

        // POST: Questions/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var question = await _dbContext.Questions.FindAsync(id);
            if (question == null)
            {
                return RedirectToAction("Index").WithError("未找到该题目。");
            }
            if (!User.IsUserOrAdmin(question.CreateUserId))
            {
                return RedirectToAction("Index").WithWarning("仅题目创建者才能删除题目。");
            }

            _dbContext.Questions.Remove(question);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("Index").WithSuccess("删除成功。");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> CheckName(string name)
        {
            var exist = await _manager.ExistName(name);
            return Json(exist);
        }

        private readonly ApplicationDbContext _dbContext;

        private readonly QuestionManager _manager;
    }
}
