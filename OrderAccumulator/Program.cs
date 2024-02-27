using OrderAccumulator;
using QuickFix;

internal class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        try
        {
            if (args.Length == 0)
                args = new[] { "../../../OrderAccumulator.cfg" };

            SessionSettings settings = new(args[0]);

            ThreadedSocketAcceptor acceptor = new(
                new FixOrderAccumulator(),
                new FileStoreFactory(settings),
                settings,
                new FileLogFactory(settings));

            acceptor.Start();
            Console.WriteLine("Acceptor iniciado.");
            Console.Read();
            acceptor.Stop();
        } 
        catch (Exception ex) 
        {
            Console.WriteLine("Houve um erro ao iniciar o acumulador. - " + ex.Message);
        }
    }
}