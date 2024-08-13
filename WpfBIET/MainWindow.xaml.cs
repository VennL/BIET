using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace WpfBIET
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Grid and Level data
            List<GL> gls = new List<GL>
            {
                new GL { Id = 1, Direction = "X", Coordinate = "-10473.31" },
                new GL { Id = 2, Direction = "X", Coordinate = "-1473.31" },
                new GL { Id = 3, Direction = "X", Coordinate = "7526.69" },
                new GL { Id = 4, Direction = "Y", Coordinate = "-6496.22" },
                new GL { Id = 5, Direction = "Y", Coordinate = "103.78" },
                new GL { Id = 6, Direction = "Z", Coordinate = "0" },
                new GL { Id = 7, Direction = "Z", Coordinate = "3900" },
            };

            List<BC> bcs = new List<BC>
            {
                new BC { Id = 1, Type = "梁", N1 = "1004002",N2 = "1003002",Size = "300*500",Concrete = "C30",Con = "35",Bar = "HRB400",BarNo = "4",GB = "HPB300",GBN = "100" },
                new BC { Id = 2, Type = "梁", N1 = "1003002",N2 = "1002002",Size = "300*500",Concrete = "C30",Con = "35",Bar = "HRB400",BarNo = "4",GB = "HPB300",GBN = "100" },
                new BC { Id = 3, Type = "梁", N1 = "1002002",N2 = "1001002",Size = "300*500",Concrete = "C30",Con = "35",Bar = "HRB400",BarNo = "4",GB = "HPB300",GBN = "100" },
                new BC { Id = 4, Type = "梁", N1 = "1001002",N2 = "1001001",Size = "300*500",Concrete = "C30",Con = "35",Bar = "HRB400",BarNo = "4",GB = "HPB300",GBN = "100" },
                new BC { Id = 5, Type = "柱", N1 = "1004002",N2 = "1003002",Size = "500*500",Concrete = "C30",Con = "35",Bar = "HRB400",BarNo = "4",GB = "HPB300",GBN = "100" },
                new BC { Id = 6, Type = "柱", N1 = "1004002",N2 = "1003002",Size = "500*500",Concrete = "C30",Con = "35",Bar = "HRB400",BarNo = "4",GB = "HPB300",GBN = "100" },
            };

            List<WALL> walls = new List<WALL>
            {
                new WALL { Id = 1, N1 = "1004002",N2 = "1003002",BrickSize = "200*100*40",BrickType = "蒸压灰砂砖",SJ = "普通水泥砂浆" },
                new WALL { Id = 2, N1 = "1003002",N2 = "1002002",BrickSize = "200*100*40",BrickType = "蒸压灰砂砖",SJ = "普通水泥砂浆" },
                new WALL { Id = 3, N1 = "1002002",N2 = "1001002",BrickSize = "200*100*40",BrickType = "蒸压灰砂砖",SJ = "普通水泥砂浆" },
                new WALL { Id = 4, N1 = "1001002",N2 = "1004002",BrickSize = "200*100*40",BrickType = "蒸压灰砂砖",SJ = "普通水泥砂浆" },
                new WALL { Id = 5, N1 = "2004002",N2 = "2003002",BrickSize = "200*100*40",BrickType = "蒸压灰砂砖",SJ = "普通水泥砂浆" },
                new WALL { Id = 6, N1 = "2003002",N2 = "2002002",BrickSize = "200*100*40",BrickType = "蒸压灰砂砖",SJ = "普通水泥砂浆" },
            };

            List<FLAT> flats = new List<FLAT>
            {
                new FLAT { Id = 1, N1 = "1004002",N2 = "1003002",thick = "150" },
                new FLAT { Id = 2, N1 = "1003002",N2 = "1002002",thick = "150" },
                new FLAT { Id = 3, N1 = "1002002",N2 = "1001002",thick = "150" },
                new FLAT { Id = 4, N1 = "1001002",N2 = "1004002",thick = "150" },
            };

            List<MAT> material = new List<MAT>
            {
                new MAT {Id = 1, mat = "C30"},
                new MAT {Id = 2, mat = "HRB400"},
                new MAT {Id = 3, mat = "HPB300"},
            };

            // Bind data to DataGrids
            GridandLevel.ItemsSource = gls;
            BeamandColumn.ItemsSource = bcs;
            Wall.ItemsSource = walls;
            Flat.ItemsSource = flats;
            Material.ItemsSource = material;
        }

        private void studentDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void GridandLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }

    public class GL
    {
        public int Id { get; set; }
        public string Direction { get; set; }
        public string Coordinate { get; set; }
    }

    public class BC
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string N1 { get; set; }
        public string N2 { get; set; }
        public string Size { get; set; }
        public string Concrete { get; set; }
        public string Con { get; set; }
        public string Bar { get; set; }
        public string BarNo { get; set; }
        public string GB { get; set; }
        public string GBN { get; set; }
    }
    public class WALL
    {
        public int Id { get; set; }
        public string N1 { get; set; }
        public string N2 { get; set; }
        public string BrickSize { get; set; }
        public string BrickType { get; set; }
        public string SJ { get; set; }
    }
    public class FLAT
    {
        public int Id { get; set; }
        public string N1 { get; set; }
        public string N2 { get; set; }
        public string thick { get; set; }
    }
    public class MAT
    {
        public int Id { get; set; }
        public string mat { get; set; }       
    }
}
