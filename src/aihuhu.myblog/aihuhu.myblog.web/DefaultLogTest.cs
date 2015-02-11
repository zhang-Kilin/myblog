using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace aihuhu.myblog.web
{
    public class DefaultLogTest : ILogTest
    {
        public void Log(string message)
        {
            throw new Exception(string.Format("exception test. message '{0}'.当你看到这个黄页的时候，不要紧张，这说明程序运行正常。", message));
        }
    }
}