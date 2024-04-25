using System.Security.Policy;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace State
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
    public static class TruckInterface
    {
        public static Truck CurrentState { get; set; }
        public static List<Truck> states { get; set; } = new List<Truck>();
        public static void Handle(MainForm form)
        {
            CurrentState.Handle(form);
        }
        public static double? MoveTo(int destinationId)
        {
            return CurrentState.MoveTo(destinationId);
        }
        public static int? GetPosition()
        {
            return CurrentState.GetPosition();
        }
        public static double? StartUnloading()
        {
            return CurrentState.StartUnloading();
        }
        public static double? StartLoading()
        {
            return CurrentState.StartLoading();
        }
        public static Dictionary<Goods, int> GetCargo()
        {
            return CurrentState.GetCargo();
        }
    }
    public abstract class Truck
    {
        protected static int positionId;
        protected static Dictionary<Goods, int> Cargo  = new Dictionary<Goods, int>();

        public Dictionary<Goods, int> GetCargo()
        {
            return Cargo;
        }
        public static TruckIdle CreateTruck()
        {
            TruckIdle TI = new TruckIdle();
            positionId = 0;
            Cargo = GoodsHandler.RandomizeGoods(3);
            TruckInterface.states.Add(TI);
            TruckMoving TM = new TruckMoving();
            TruckInterface.states.Add(TM);
            TruckLoading TL = new TruckLoading();
            TruckInterface.states.Add(TL);
            TruckUnloading TU = new TruckUnloading();
            TruckInterface.states.Add(TU);
            TruckInterface.CurrentState = TI;
            return TI;
        }
        protected static void SetStateIdle()
        {
            TruckInterface.CurrentState = TruckInterface.states.ElementAt(0);
        }
        protected static void SetStateMoving()
        {
            TruckInterface.CurrentState = TruckInterface.states.ElementAt(1);

        }
        protected static void SetStateLoading()
        {
            TruckInterface.CurrentState = TruckInterface.states.ElementAt(2);
        }
        protected static void SetStateUnloading()
        {
            TruckInterface.CurrentState = TruckInterface.states.ElementAt(3);
        }
        public abstract void Handle(MainForm form);
        public abstract double? MoveTo(int destinationId);
        public abstract int? GetPosition();
        public abstract double? StartUnloading();
        public abstract double? StartLoading();
    }
    public class TruckMoving : Truck
    {
        public override void Handle(MainForm form)
        {
            form.ShowTimeMenu();
        }
        public override double? MoveTo(int destinationId)
        {
            return null;
        }
        public override int? GetPosition()
        {
            return null;
        }
        public override double? StartLoading()
        {
            return null;
        }
        public override double? StartUnloading()
        {
            return null;
        }
    }
    public class TruckLoading : Truck
    { 
        public override void Handle(MainForm form)
        {
            form.ShowTimeMenu();
        }
        public override double? MoveTo(int destinationId)
        {
            return null;
        }
        public override int? GetPosition()
        {
            return positionId;
        }
        public override double? StartLoading()
        {
            return null;
        }
        public override double? StartUnloading()
        {
            return null;
        }
    }
    public class TruckUnloading : Truck
    { 
        public override void Handle(MainForm form)
        {
            form.ShowTimeMenu();
        }
        public override double? MoveTo(int destinationId)
        {
            return null;
        }
        public override int? GetPosition()
        {
            return positionId;
        }
        public override double? StartLoading()
        {
            return null;
        }
        public override double? StartUnloading()
        {
            return null;
        }
    }
    public class TruckIdle : Truck
    { 
        public override void Handle(MainForm form)
        {
            form.ShowIdleMenu();
        }
        public override double? MoveTo(int destinationId)
        {
            SetStateMoving();
            double arrivalTime = Math.Sqrt(Math.Pow((Station.GetStation(positionId).Location.x - Station.GetStation(positionId).Location.x), 2) + Math.Pow((Station.GetStation(destinationId).Location.y - Station.Stations.ElementAt(positionId).Location.y), 2))/10;
            positionId = destinationId;
            Run(arrivalTime);
            return arrivalTime;
        }
        private async void Run(double time)
        {
            time = time * 100;
            await Task.Delay((int)time);
            SetStateIdle();
        }
        public override int? GetPosition()
        {
            return positionId;
        }
        public override double? StartLoading()
        {
            SetStateLoading();
            double loadingTime = 0;
            for (int i = 0; i < 5; i++)
            {
                loadingTime += Cargo[(Goods)i];
                Cargo[(Goods)i] += Station.GetStation(positionId).CurrentGoods[(Goods)i];
            }
            Station.GetStation(positionId).CurrentGoods = GoodsHandler.InitializeGoods();
            Run(loadingTime);
            return loadingTime;
        }
        public override double? StartUnloading()
        {
            SetStateUnloading();
            double unloadingTime = 0;
            foreach (KeyValuePair<Goods, int> entry in Cargo)
            {
                unloadingTime += entry.Value * 2;
                Station.GetStation(positionId).CurrentGoods[entry.Key] += entry.Value;
            }
            Cargo = GoodsHandler.InitializeGoods();
            Run(unloadingTime);
            return unloadingTime;
        }
    }
    public class Station
    {
        public int Id { get; set; }
        public (int x, int y) Location { get; set; }
        public static bool[,]? TransitionMatrix { get; set; }
        public static List<Station> Stations { get; set; }
        public static int StationScope { get; set; } = 70;
        public static int FieldSize { get; set; } = 800;
        public Dictionary<Goods, int> CurrentGoods { get; set; } = new Dictionary<Goods, int>();
        public Station(int id, (int, int) location)
        {
            Id = id;
            Location = location;
        }
        public static Station GetStation(int id)
        {
            return Stations[id];
        }
        public static void GenerateStations(int count)
        {
            Random rnd = new Random();
            TransitionMatrix = new bool[count, count];
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < count; j++)
                {
                    TransitionMatrix[i, j] = false;
                }
            }
            List<Station> stations = new List<Station>();
            int stationColumnCount = (int)Math.Round(Math.Sqrt(count));
            int stationRowCount = (int)count/stationColumnCount+1;
            int stationBoxWidth = FieldSize/stationColumnCount;
            int stationBoxHeight = FieldSize/stationRowCount;
            int stationRowEnumerator = 0;
            int stationColumnEnumerator = 0;
            for (int i = 0; i < count; i++)
            {
                Station s = new Station(i, (stationBoxWidth * stationColumnEnumerator + rnd.Next(stationBoxWidth), stationBoxHeight * stationRowEnumerator + rnd.Next(stationBoxHeight)));
                s.CurrentGoods = GoodsHandler.RandomizeGoods(5);
                stations.Add(s);
                if (stationColumnEnumerator == stationColumnCount-1)
                {
                    stationColumnEnumerator = 0;
                    stationRowEnumerator++;
                }
                else
                {
                    stationColumnEnumerator++;
                }
            }
            Stations = stations;
            foreach (Station s1 in stations)
            {
                foreach (Station s2 in stations)
                {
                    if (TransitionMatrix[s1.Id, s2.Id] == false)
                    {
                        if (rnd.NextDouble()<=0.3)
                        {
                            TransitionMatrix[s1.Id, s2.Id] = true;
                            TransitionMatrix[s2.Id, s1.Id] = true;
                        }
                    }
                }
            }
        }
    }
    public enum Goods
    {
        Potatoes,
        Fuel,
        Servos,
        Books,
        Electronics
    }
    public static class GoodsHandler
    {
        public static Dictionary<Goods, int> RandomizeGoods(int maxValue)
        {
            Random rnd = new Random();
            Dictionary<Goods, int> goodsList = new Dictionary<Goods, int>()
            {
                [Goods.Potatoes] = rnd.Next(maxValue),
                [Goods.Fuel] = rnd.Next(maxValue),
                [Goods.Servos] = rnd.Next(maxValue),
                [Goods.Books] = rnd.Next(maxValue),
                [Goods.Electronics] = rnd.Next(maxValue),
            };
            return goodsList;
        }
        public static Dictionary<Goods, int> InitializeGoods()
        {
            Dictionary<Goods, int> goodsList = new Dictionary<Goods, int>()
            {
                [Goods.Potatoes] = 0,
                [Goods.Fuel] = 0,
                [Goods.Servos] = 0,
                [Goods.Books] = 0,
                [Goods.Electronics] = 0,
            };
            return goodsList;
        }
    }
}