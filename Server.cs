using dao;

namespace ServerHelloWorld
{
    class Program
    {
        public static void Main(string[] args)
        {
            Player player = new Player();
            player.coin = 100;
            Console.WriteLine(player.text + player.coin);
            var name = Console.ReadLine();
            var currentDate = DateTime.Now;
            Console.WriteLine($"{Environment.NewLine}Hello, {name}, on {currentDate:d} at {currentDate:t}!");
            Console.Write($"{Environment.NewLine}Press any key to exit...");
            Console.ReadKey(true);
        }
    }
}