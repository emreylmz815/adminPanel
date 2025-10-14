using System;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class KanbanController : Controller
    {
        private readonly KanbanBoardService _boardService;

        public KanbanController()
        {
            _boardService = new KanbanBoardService();
        }

        [HttpGet]
        public ActionResult Index()
        {
            var board = _boardService.GetOrCreateBoard(HttpContext);
            return View(board);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateTitle(string title)
        {
            var board = _boardService.GetOrCreateBoard(HttpContext);
            board.Title = string.IsNullOrWhiteSpace(title) ? board.Title : title.Trim();
            _boardService.SaveBoard(HttpContext, board);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddColumn(string name, int? workInProgressLimit)
        {
            var board = _boardService.GetOrCreateBoard(HttpContext);
            if (!string.IsNullOrWhiteSpace(name))
            {
                board.Columns.Add(new KanbanColumn
                {
                    Name = name.Trim(),
                    WorkInProgressLimit = workInProgressLimit > 0 ? workInProgressLimit : null
                });

                _boardService.SaveBoard(HttpContext, board);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RenameColumn(Guid columnId, string name)
        {
            var board = _boardService.GetOrCreateBoard(HttpContext);
            var column = board.FindColumn(columnId);
            if (column != null && !string.IsNullOrWhiteSpace(name))
            {
                column.Name = name.Trim();
                _boardService.SaveBoard(HttpContext, board);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCard(Guid columnId, string title, string description, string owner, DateTime? dueDate)
        {
            var board = _boardService.GetOrCreateBoard(HttpContext);
            var column = board.FindColumn(columnId);
            if (column == null)
            {
                return RedirectToAction("Index");
            }

            if (column.IsAtCapacity)
            {
                TempData["KanbanWarning"] = "Bu sütun WIP limitine ulaştı.";
                return RedirectToAction("Index");
            }

            if (!string.IsNullOrWhiteSpace(title))
            {
                column.Cards.Add(new KanbanCard
                {
                    Title = title.Trim(),
                    Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                    Owner = string.IsNullOrWhiteSpace(owner) ? null : owner.Trim(),
                    DueDate = dueDate
                });

                _boardService.SaveBoard(HttpContext, board);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MoveCard(Guid cardId, Guid targetColumnId)
        {
            var board = _boardService.GetOrCreateBoard(HttpContext);
            _boardService.MoveCard(board, cardId, targetColumnId);
            _boardService.SaveBoard(HttpContext, board);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCard(Guid cardId)
        {
            var board = _boardService.GetOrCreateBoard(HttpContext);
            foreach (var column in board.Columns)
            {
                var card = column.Cards.FirstOrDefault(c => c.Id == cardId);
                if (card != null)
                {
                    column.Cards.Remove(card);
                    break;
                }
            }

            _boardService.SaveBoard(HttpContext, board);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reset()
        {
            _boardService.Reset(HttpContext);
            return RedirectToAction("Index");
        }
    }
}
