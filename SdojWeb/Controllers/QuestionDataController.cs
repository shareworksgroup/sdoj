﻿using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using SdojWeb.Infrastructure.Alerts;
using SdojWeb.Infrastructure.Identity;
using SdojWeb.Models;

namespace SdojWeb.Controllers
{
    [SdojAuthorize(EmailConfirmed = true)]
    public class QuestionDataController : Controller
    {
        private readonly ApplicationDbContext _db;

        public QuestionDataController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /QuestionData/ListForQuestion/5
        public async Task<ActionResult> ListForQuestion(int id)
        {
            var questionDatas = await _db.QuestionDatas.Where(x => x.QuestionId == id)
                .Project().To<QuestionDataSummaryModel>()
                .ToArrayAsync();

            ViewBag.QuestionId = id;
            if (questionDatas.Length == 0)
            {
                return View(questionDatas).WithInfo("该题目目前没有任何测试数据。");
            }

            return View(questionDatas);
        }

        // GET: /QuestionData/CreateForQuestion/5
        public ActionResult CreateForQuestion(int id)
        {
            var model = new QuestionDataEditModel {QuestionId = id};
            return View(model);
        }

        // POST: /QuestionData/CreateForQuestion
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateForQuestion(QuestionDataEditModel model)
        {
            if (ModelState.IsValid)
            {
                var questionData = Mapper.Map<QuestionData>(model);

                _db.QuestionDatas.Add(questionData);
                await _db.SaveChangesAsync();
                return RedirectToAction("ListForQuestion", new {id = model.QuestionId})
                    .WithInfo("已成功创建该测试数据。");
            }
            
            return View(model);
        }

        // GET: /QuestionData/Edit/5
        public async Task<ActionResult> Edit(int id, int questionId)
        {
            var model = await _db.QuestionDatas
                .Project().To<QuestionDataEditModel>()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (User.IsUserOrAdmin(model.CreateUserId))
            {
                return View(model);
            }

            return NonOwnerReturn(model.QuestionId);
        }

        // POST: /QuestionData/Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(QuestionDataEditModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = await _db.QuestionDatas
                    .Where(x => x.Id == model.Id)
                    .Select(x => x.Question.CreateUserId)
                    .FirstAsync();

                if (User.IsUserOrAdmin(userId))
                {
                    var questionData = Mapper.Map<QuestionData>(model);
                    _db.Entry(questionData).State = EntityState.Modified;
                    await _db.SaveChangesAsync();

                    return RedirectToAction("ListForQuestion", new { id = model.QuestionId })
                        .WithInfo("测试数据保存成功。");
                }

                return NonOwnerReturn(model.QuestionId);
            }
            return View(model);
        }

        // GET: /QuestionData/Delete/5
        public async Task<ActionResult> Delete(int id, int questionId)
        {
            var model = await _db.QuestionDatas
                .Project().To<QuestionDataEditModel>()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (model == null)
            {
                return RedirectToAction("ListForQuestion", new { id = questionId }).WithError(
                    string.Format("没找到Id为{0}的测试数据。", id));
            }

            return View(model);
        }

        // POST: /QuestionData/DeleteConfirmed/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id, int questionId)
        {
            var model = await _db.QuestionDatas.FindAsync(id);
            if (model == null)
            {
                return RedirectToAction("ListForQuestion", new { id = questionId }).WithError(
                    string.Format("没找到Id为{0}的测试数据。", id));
            }

            _db.Entry(model).State = EntityState.Deleted;
            await _db.SaveChangesAsync();

            return RedirectToAction("ListForQuestion", new { id = questionId }).WithSuccess(
                string.Format("Id为{0}的测试数据删除成功。", id));
        }

        private ActionResult NonOwnerReturn(int questionId)
        {
            return RedirectToAction("ListForQuestion", new { id = questionId })
                .WithSuccess("只有创建者才能查看或操作测试数据。");
        }
    }
}
