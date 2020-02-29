using Microsoft.VisualStudio.TestTools.UnitTesting;
using Radar.Bidding.Model.Dto;
using Radar.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Common.Tests
{
    [TestClass()]
    public class JsonsTests
    {
        [TestMethod()]
        public void ToJsonTest()
        {

            DateTime dt = DateTime.Now;
            long mills = KK.ToMills(dt);
            DateTime dt2 = KK.ToDateTime(mills);

            Console.Out.WriteLine("mills is " + mills);

            PriceActionRequest req = new PriceActionRequest();
            req.MachineCode = "123456";
            req.OccurTime = DateTime.Now;
            req.ScreenTime = DateTime.Now;
            req.UsedDelayMills = 23;

            string json = Jsons.ToJson(req);
            string reqText = req.ToLine();


            Console.Out.WriteLine("json is " + json);
        }
    }
}