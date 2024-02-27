using QuickFix;
using QuickFix.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderAccumulator
{
    public class FixOrderAccumulator : MessageCracker, IApplication
    {
        private SessionID? SessionID { get; set; }
        private const decimal limit = 1000000;
        private Dictionary<string, decimal> symbolsExposure = new Dictionary<string, decimal>();

        public void FromAdmin(Message message, SessionID sessionID) { }
        public void OnLogon(SessionID sessionID) { }
        public void OnLogout(SessionID sessionID) { }
        public void ToAdmin(Message message, SessionID sessionID) { }
        public void ToApp(Message message, SessionID sessionID) { }

        public void OnCreate(SessionID sessionID)
        {
            SessionID = sessionID;
        }

        public void FromApp(Message message, SessionID sessionID)
        {
            Crack(message, sessionID);
        }

        public void OnMessage(QuickFix.FIX50.NewOrderSingle order, SessionID sessionID)
        {
            try
            {
                QuickFix.FIX50.ExecutionReport report = new QuickFix.FIX50.ExecutionReport();
                report.Side = order.Side;
                report.Symbol = order.Symbol;
                report.OrderQty = order.OrderQty;

                if (ProcessOrder(order))
                {
                    report.OrdStatus = new OrdStatus(OrdStatus.NEW);
                    report.ExecType = new ExecType(ExecType.NEW);
                }
                else
                {
                    report.OrdStatus = new OrdStatus(OrdStatus.REJECTED);
                    report.ExecType = new ExecType(ExecType.REJECTED);
                }

                Session.SendToTarget(report, sessionID);
            }
            catch 
            {
                Console.WriteLine("Ocorreu um erro ao processar a ordem.");
            }
        }

        private bool ProcessOrder(QuickFix.FIX50.NewOrderSingle order)
        {
            if (order.OrderQty.Obj * order.Price.Obj <= limit)
            {
                if (!symbolsExposure.ContainsKey(order.Symbol.ToString()))
                    symbolsExposure.Add(order.Symbol.ToString(), 0);

                switch (order.Side.Obj)
                {
                    case Side.BUY:
                        symbolsExposure[order.Symbol.ToString()] += order.OrderQty.Obj * order.Price.Obj;
                        break;
                    case Side.SELL:
                        symbolsExposure[order.Symbol.ToString()] -= order.OrderQty.Obj * order.Price.Obj;
                        break;
                }

                Console.WriteLine("Ordem aceita.");
                Console.WriteLine("Exposição por símbolo:");
                foreach(var  symbol in symbolsExposure.Keys)
                {
                    Console.WriteLine(symbol + ": " + symbolsExposure[symbol]);
                }
                Console.WriteLine("");

                return true;
            }

            return false;
        }
    }
}
