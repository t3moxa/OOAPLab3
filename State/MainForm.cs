using State.Properties;
using System.Windows.Forms;

namespace State
{
    public partial class MainForm : Form
    {
        List<Truck> _trucks = new List<Truck>();
        Truck _mainTruck;
        PictureBox _truckPictureBox;
        Graphics g;
        Pen _stationPen;
        Pen _pathPen;
        Pen _borderPen;
        Pen _stationInfoPen;
        int _leftIndent;
        int _topIndent;
        int _numOfStations;
        int _infoTableLeftIndent;
        int _infoTableWidth;
        int _infoTableNameHeight;
        int _infoTableHeight;
        int _infoTableTopIndent;
        int _moveDestinationId;
        Label _stationName;
        double? _timerTime;
        Dictionary<PictureBox, int> _stationDictionary = new Dictionary<PictureBox, int>();
        List<Label> _stationGoods = new List<Label>();
        List<Label> _truckGoods = new List<Label>();
        Label _idleLabel;
        TextBox _idleTextBox;
        Button _idleButton;
        Button _loadButton;
        Button _unloadButton;
        Label _timeLabel;
        Label _timeRemainingLabel;
        public MainForm()
        {
            InitializeComponent();
            _numOfStations = 5;
            _mainTruck = Truck.CreateTruck();
            InitializeDrawingTools();
            BackColor = Color.Black;
        }
        public void InitializeDrawingTools()
        {
            _leftIndent = 200;
            _topIndent = (Height - Station.FieldSize) / 2;
            g = Graphics.FromHwnd(Handle);
            _stationPen = new Pen(Brushes.Red);
            _stationPen.Width = 4;
            _borderPen = new Pen(Brushes.Green);
            _borderPen.Width = 6;
            float[] dashValues = { 4, 2 };
            _pathPen = new Pen(Brushes.Blue);
            _pathPen.Width = 2;
            _pathPen.DashPattern = dashValues;
            _stationInfoPen = new Pen(Brushes.White);
            _stationInfoPen.Width = 4;
        }
        public PictureBox DrawStation(Station s)
        {
            int x = _leftIndent + s.Location.x;
            int y = _topIndent + s.Location.y;
            PictureBox pictureBox = new PictureBox();
            pictureBox.Location = new Point(x - 16, y - 16);
            pictureBox.Image = Resources.icn_town;
            pictureBox.Width = 32;
            pictureBox.Height = 32;
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox.BackColor = Color.Transparent;
            pictureBox.MouseEnter += new EventHandler(Station_MouseEnter);
            pictureBox.MouseLeave += new EventHandler(Station_MouseLeave);
            pictureBox.Click += new EventHandler(Station_Click);
            Controls.Add(pictureBox);
            pictureBox.Show();
            Label l = new Label();
            l.Text = s.Id.ToString();
            l.ForeColor = Color.White;
            l.Location = new Point(x + 20, y - 5);
            l.Width = 10;
            l.Height = 15;
            l.BackColor = Color.Transparent;
            Controls.Add(l);
            l.Show();
            return pictureBox;
        }
        public void DrawPaths()
        {
            bool[,] wasDrawn = new bool[_numOfStations, _numOfStations];
            for (int i = 0; i < _numOfStations; i++)
            {
                for (int j = 0; j < _numOfStations; j++)
                {
                    if ((Station.TransitionMatrix[i, j] == true) && (wasDrawn[i, j] != true))
                    {
                        g.DrawLine(_pathPen, _leftIndent + Station.GetStation(i).Location.x, _topIndent + Station.GetStation(i).Location.y, _leftIndent + Station.GetStation(j).Location.x, _topIndent + Station.GetStation(j).Location.y);
                        wasDrawn[i, j] = true;
                        wasDrawn[j, i] = true;
                    }
                }
            }
        }
        public void button1_Click(object sender, EventArgs e)
        {
            Station.GenerateStations(_numOfStations);
            g.DrawRectangle(_borderPen, _leftIndent - 20, _topIndent - 20, Station.FieldSize + 40, Station.FieldSize + 40);
            DrawPaths();
            foreach (Station s in Station.Stations)
            {
                _stationDictionary.Add(DrawStation(s), s.Id);
            }
            DrawStationInfoTable();
            SpawnTruck();
            InitializeAllMenus();
            button1.Hide();
        }
        public void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            MessageBox.Show(e.KeyCode.ToString());
            if ((e.KeyCode == Keys.Escape) || (e.KeyCode == Keys.L))
            {
                Application.Exit();
            }
        }
        private void Station_MouseEnter(object sender, EventArgs e)
        {
            PictureBox station = sender as PictureBox;
            station.Location = new Point(station.Location.X - 6, station.Location.Y - 6);
            station.Width = 44;
            station.Height = 44;
        }
        private void Station_MouseLeave(object sender, EventArgs e)
        {
            PictureBox station = sender as PictureBox;
            station.Location = new Point(station.Location.X + 6, station.Location.Y + 6);
            station.Width = 32;
            station.Height = 32;
        }
        public void DrawStationInfoTable()
        {
            _infoTableLeftIndent = _leftIndent + Station.FieldSize + _leftIndent;
            _infoTableWidth = 600;
            _infoTableNameHeight = 64 + 20 * 2;
            _infoTableHeight = _infoTableNameHeight + 64 * 5 + 20 * 6;
            _infoTableTopIndent = _topIndent - 20;
            g.DrawRectangle(_stationInfoPen, _infoTableLeftIndent, _infoTableTopIndent, _infoTableWidth, _infoTableHeight);//границы области
            g.DrawRectangle(_stationInfoPen, _infoTableLeftIndent, _infoTableTopIndent, _infoTableWidth, _infoTableNameHeight);//окно заголовка
            g.DrawRectangle(_stationInfoPen, _infoTableLeftIndent + 64 + 20 * 2, _infoTableTopIndent, (_infoTableWidth - _infoTableNameHeight) / 2, _infoTableHeight);//столбцы
            var potatoes = DrawGoodsIcon(0);
            potatoes.Image = Resources.potatoe;
            var fuel = DrawGoodsIcon(1);
            fuel.Image = Resources.fuel;
            var servos = DrawGoodsIcon(2);
            servos.Image = Resources.rabochee_oborudovanie;
            var books = DrawGoodsIcon(3);
            books.Image = Resources.books;
            var electronics = DrawGoodsIcon(4);
            electronics.Image = Resources.mikro;
            DrawHandleButton();
            Label stationNameHead = DrawName(0, 0);
            stationNameHead.Text = "Station ID:";
            Label truckNameHead = DrawName(1, 0);
            truckNameHead.Text = "Truck:";
            Label stationName = DrawName(0, 1);
            stationName.Text = "-";
            _stationName = stationName;
            Label potatoesStationAmount = DrawGoodsAmount(0, 0);
            potatoesStationAmount.Text = "-";
            _stationGoods.Add(potatoesStationAmount);
            Label fuelStationAmount = DrawGoodsAmount(0, 1);
            fuelStationAmount.Text = "-";
            _stationGoods.Add(fuelStationAmount);
            Label servosStationAmount = DrawGoodsAmount(0, 2);
            servosStationAmount.Text = "-";
            _stationGoods.Add(servosStationAmount);
            Label booksStationAmount = DrawGoodsAmount(0, 3);
            booksStationAmount.Text = "-";
            _stationGoods.Add(booksStationAmount);
            Label electronicsStationAmount = DrawGoodsAmount(0, 4);
            electronicsStationAmount.Text = "-";
            _stationGoods.Add(electronicsStationAmount);

            Label potatoesTruckAmount = DrawGoodsAmount(1, 0);
            potatoesTruckAmount.Text = "-";
            _truckGoods.Add(potatoesTruckAmount);
            Label fuelTruckAmount = DrawGoodsAmount(1, 1);
            fuelTruckAmount.Text = "-";
            _truckGoods.Add(fuelTruckAmount);
            Label servosTruckAmount = DrawGoodsAmount(1, 2);
            servosTruckAmount.Text = "-";
            _truckGoods.Add(servosTruckAmount);
            Label booksTruckAmount = DrawGoodsAmount(1, 3);
            booksTruckAmount.Text = "-";
            _truckGoods.Add(booksTruckAmount);
            Label electronicsTrucknAmount = DrawGoodsAmount(1, 4);
            electronicsTrucknAmount.Text = "-";
            _truckGoods.Add(electronicsTrucknAmount);
        }
        public PictureBox DrawGoodsIcon(int i)
        {
            PictureBox icon = new PictureBox();
            icon.Location = new Point(_leftIndent + Station.FieldSize + _leftIndent + 20, _topIndent + (64 + 20 * 2) + 84 * i);
            icon.Width = 64;
            icon.Height = 64;
            icon.SizeMode = PictureBoxSizeMode.StretchImage;
            icon.BackColor = Color.Transparent;
            Controls.Add(icon);
            icon.Show();
            return icon;
        }
        public Label DrawName(int x, int y)
        {
            Label label = new Label();
            label.Font = new Font("Arial", 20);
            label.Width = 180;
            label.Height = 30;
            label.BackColor = Color.Transparent;
            label.Location = new Point(_infoTableLeftIndent + _infoTableNameHeight + ((_infoTableWidth - _infoTableNameHeight) / 2 - label.Width) / 2 + ((_infoTableWidth - _infoTableNameHeight) / 2 * x), _infoTableTopIndent + 15 + (label.Height + 15) * y);
            label.ForeColor = Color.White;
            Controls.Add(label);
            label.Show();
            return label;
        }
        public void DrawHandleButton()
        {
            int handleButtonWidth = 100;
            int handleButtonHeight = 30;
            Button handleButton = new Button();
            handleButton.Location = new Point(_infoTableLeftIndent + (_infoTableWidth - handleButtonWidth) / 2, _infoTableTopIndent + _infoTableHeight + 20);
            handleButton.Width = handleButtonWidth;
            handleButton.Height = handleButtonHeight;
            handleButton.Text = "Handle";
            handleButton.ForeColor = Color.White;
            handleButton.Click += new EventHandler(HandleButton_Click);
            Controls.Add(handleButton);
            handleButton.Show();
        }
        public Label DrawGoodsAmount(int x, int y)
        {
            Label label = new Label();
            label.Font = new Font("Arial", 20);
            label.Width = 30;
            label.Height = 30;
            label.BackColor = Color.Transparent;
            label.Location = new Point(_infoTableLeftIndent + _infoTableNameHeight + ((_infoTableWidth - _infoTableNameHeight) / 2 - label.Width) / 2 + ((_infoTableWidth - _infoTableNameHeight) / 2 * x), _infoTableTopIndent + _infoTableNameHeight + 20 + 17 + (64 + 20) * y);
            label.ForeColor = Color.White;
            Controls.Add(label);
            label.Show();
            return label;
        }
        public void Station_Click(object sender, EventArgs e)
        {
            Station curStation = Station.GetStation(_stationDictionary[(PictureBox)sender]);
            UpdateStationGoodsData(curStation);
        }
        public void SpawnTruck()
        {
            PictureBox truck = new PictureBox();
            truck.Location = new Point(Station.GetStation(0).Location.x + _leftIndent - 5, Station.GetStation(0).Location.y + _topIndent - 8);
            truck.Image = Resources.radar_machine;
            truck.Width = 10;
            truck.Height = 16;
            truck.BackColor = Color.Transparent;
            truck.ForeColor = Color.Transparent;
            truck.SizeMode = PictureBoxSizeMode.AutoSize;
            Controls.Add(truck);
            truck.BringToFront();
            truck.Show();
            _truckPictureBox = truck;
        }
        public void HandleButton_Click(object sender, EventArgs e)
        {
            TruckInterface.Handle(this);
        }
        public void InitializeAllMenus()
        {
            LoadIdleMenu();
            LoadLoadMenu();
            LoadUnloadMenu();
            LoadTimeMenu();
            HideAllMenus();
        }
        public void HideAllMenus()
        {
            _idleTextBox.Hide();
            _idleButton.Hide();
            _idleLabel.Hide();
            _loadButton.Hide();
            _unloadButton.Hide();
            _timeLabel.Hide();
            _timeRemainingLabel.Hide();
        }
        public void LoadTimeMenu()
        {
            Label label = new Label();
            label.Text = "-";
            label.Width = 100;
            label.Height = 20;
            label.Location = new Point(_infoTableLeftIndent + (_infoTableWidth - label.Width) / 2, _infoTableTopIndent + _infoTableHeight + 90);
            label.ForeColor = Color.White;
            label.BackColor = Color.Black;
            Label timeRemainingLabel = new Label();
            timeRemainingLabel.Width = 100;
            timeRemainingLabel.Height = 20;
            timeRemainingLabel.ForeColor = Color.White;
            timeRemainingLabel.BackColor = Color.Black;
            timeRemainingLabel.Location = new Point(_infoTableLeftIndent + (_infoTableWidth - label.Width) / 2, _infoTableTopIndent + _infoTableHeight + 60);
            timeRemainingLabel.Text = "Time remaining:";
            _timeRemainingLabel = timeRemainingLabel;
            Controls.Add(timeRemainingLabel);
            _timeLabel = label;
            Controls.Add(label);
        }
        public void LoadIdleMenu()
        {
            Label label = new Label();
            label.Text = "Destination ID";
            label.Width = 100;
            label.Height = 20;
            label.Location = new Point(_infoTableLeftIndent + (_infoTableWidth - label.Width) / 2, _infoTableTopIndent + _infoTableHeight + 75);
            label.ForeColor = Color.White;
            label.BackColor = Color.Black;
            _idleLabel = label;
            Controls.Add(label);
            TextBox textBox = new TextBox();
            textBox.Width = 100;
            textBox.Height = 18;
            textBox.ForeColor = Color.White;
            textBox.BackColor = Color.Black;
            textBox.BorderStyle = BorderStyle.Fixed3D;
            textBox.Location = new Point(_infoTableLeftIndent + (_infoTableWidth - textBox.Width) / 2, _infoTableTopIndent + _infoTableHeight + 100);
            _idleTextBox = textBox;
            Controls.Add(textBox);
            Button button = new Button();
            button.Text = "Move";
            button.Width = 100;
            button.Height = 30;
            button.Location = new Point(_infoTableLeftIndent + (_infoTableWidth - button.Width) / 2, _infoTableTopIndent + _infoTableHeight + 130);
            button.ForeColor = Color.White;
            button.BackColor = Color.Black;
            button.Click += new EventHandler(MoveButton_Click);
            _idleButton = button;
            Controls.Add(button);
        }
        public void LoadLoadMenu()
        {
            Button button = new Button();
            button.Text = "Load";
            button.Width = 100;
            button.Height = 30;
            button.Location = new Point(_infoTableLeftIndent + (_infoTableWidth - button.Width) / 2 - 120, _infoTableTopIndent + _infoTableHeight + 100);
            button.ForeColor = Color.White;
            button.BackColor = Color.Black;
            button.Click += new EventHandler(LoadButton_Click);
            _loadButton = button;
            Controls.Add(button);
        }
        public void LoadUnloadMenu()
        {
            Button button = new Button();
            button.Text = "Unload";
            button.Width = 100;
            button.Height = 30;
            button.Location = new Point(_infoTableLeftIndent + (_infoTableWidth - button.Width) / 2 + 120, _infoTableTopIndent + _infoTableHeight + 100);
            button.ForeColor = Color.White;
            button.BackColor = Color.Black;
            button.Click += new EventHandler(UnloadButton_Click);
            _unloadButton = button;
            Controls.Add(button);
        }
        public void ShowIdleMenu()
        {
            HideAllMenus();
            _idleButton.Show();
            _idleLabel.Show();
            _idleTextBox.Show();
            _loadButton.Show();
            _unloadButton.Show();
        }
        public void ShowTimeMenu()
        {
            HideAllMenus();
            _timeLabel.Text = _timerTime.ToString();
            _timeRemainingLabel.Show();
            _timeLabel.Show();
        }
        public void UpdateStationGoodsData(Station curStation)
        {
            _stationName.Text = curStation.Id.ToString();
            for (int i = 0; i < 5; i++)
            {
                _stationGoods[i].Text = curStation.CurrentGoods[(Goods)i].ToString();
            }
            if (curStation.Id == TruckInterface.GetPosition())
            {
                for (int i = 0; i < 5; i++)
                {
                    _truckGoods[i].Text = TruckInterface.GetCargo()[(Goods)i].ToString();
                }
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    _truckGoods[i].Text = "-";
                }
            }
        }
        public void LoadButton_Click(object sender, EventArgs e)
        {
            _timerTime = TruckInterface.StartLoading();
            timer2.Start();
        }
        public void UnloadButton_Click(object sender, EventArgs e)
        {
            _timerTime = TruckInterface.StartUnloading();
            timer2.Start();
        }
        public void MoveButton_Click(object sender, EventArgs e)
        {
            if (Station.TransitionMatrix[(int)TruckInterface.GetPosition(), Int32.Parse(_idleTextBox.Text)] == true)
            {
                _moveDestinationId = Int32.Parse(_idleTextBox.Text);
                _timerTime = _mainTruck.MoveTo(_moveDestinationId);
                timer1.Start();
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            _timerTime -= timer1.Interval / 100;
            if (_timerTime < 0)
            {
                timer1.Stop();
                MoveTruck();
            }
        }
        public void MoveTruck()
        {
            _truckPictureBox.Location = new Point(Station.GetStation(_moveDestinationId).Location.x + _leftIndent - 5, Station.GetStation(_moveDestinationId).Location.y + _topIndent - 8);
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            _timerTime -= timer2.Interval / 100;
            if (_timerTime < 0)
            {
                timer2.Stop();
                UpdateStationGoodsData(Station.GetStation((int)TruckInterface.GetPosition()));
            }
        }
    }
}
