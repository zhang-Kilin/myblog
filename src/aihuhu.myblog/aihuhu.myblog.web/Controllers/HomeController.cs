using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.Unity;

namespace aihuhu.myblog.web.Controllers
{
    public class HomeController : Controller
    {
        [Dependency("DefaultLog")]
        public ILogTest Logger { get; set; }
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Licious()
        {
            System.Threading.Thread.Sleep(3000);
            return View();
        }

        public ActionResult ExceptionTest()
        {
            Logger.Log("HomeController.ExceptionTest");

            return View();
        }
        static Random ran = new Random();
        /// <summary>
        /// 返回批次加载的图片地址
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult Load()
        {

            return Json(new
            {
                baseUrl = "static/myblog/images/",
                images = new List<object>()
                {
                    new {
                        title = "",
                        url = string.Format("{0}.jpg",ran.Next(1,10)),
                        des = ""
                    },
                    new {
                        title = "",
                        url = string.Format("{0}.jpg",ran.Next(1,10)),
                        des = ""
                    },
                    new {
                        title = "",
                        url = string.Format("{0}.jpg",ran.Next(1,10)),
                        des = ""
                    },
                    new {
                        title = "",
                        url = string.Format("{0}.jpg",ran.Next(1,10)),
                        des = ""
                    },
                    new {
                        title = "",
                        url = string.Format("{0}.jpg",ran.Next(1,10)),
                        des = ""
                    },
                    new {
                        title = "",
                        url = string.Format("{0}.jpg",ran.Next(1,10)),
                        des = ""
                    },
                    new {
                        title = "",
                        url = string.Format("{0}.jpg",ran.Next(1,10)),
                        des = ""
                    },
                    new {
                        title = "",
                        url = string.Format("{0}.jpg",ran.Next(1,10)),
                        des = ""
                    }
                }
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
