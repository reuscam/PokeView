using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 



    public partial class MainWindow : Window
    {
        private const int iDelay = 1000;
        private string mFileToProcess = @"C:\Users\andrew\Source\Repos\NecroBot\PoGo.NecroBot.CLI\bin\Debug\temp\OliverBritches.inventory.json";
        private Int64 iLatestPokemonTimeStamp = 0;
        BackgroundWorker mBW = new BackgroundWorker();
        public MainWindow()
        {
            InitializeComponent();
            
            grdPokemons.Items.SortDescriptions.Add(new SortDescription("dPerfect", ListSortDirection.Descending));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtFileToProcess.Text = mFileToProcess;
            mBW.WorkerSupportsCancellation = true;
            mBW.DoWork += new DoWorkEventHandler(bw_DoWork);
            

            BeginWorker();
        }

        private void BeginWorker()
        {
            mBW.RunWorkerAsync();
        }

        class cPokemon
        {
            public string PokemonID;
            public int cp;
            public double individualAttack;
            public double individualDefense;
            public double individualStamina;
            public string firstAttack;
            public string secondAttack;
        }

        class Candy
        {
            public string Family;
            public int iCandy;
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            List<cPokemon> oPokList = new List<cPokemon>();
            List<Candy> oCandyList = new List<Candy>();
            string strFileToProcess = "";

            while (!worker.CancellationPending)
            {
                
                if ((worker.CancellationPending == true))
                {
                    e.Cancel = true;
                }
                else
                {
                    grdPokemons.Dispatcher.Invoke(
                        delegate
                        {
                            txtCandies.Document.Blocks.Clear();
                            strFileToProcess = txtFileToProcess.Text;
                            grdPokemons.Items.Clear();                                                       
                        });

                    if (!File.Exists(strFileToProcess))
                    {
                        continue;
                    }
                    string strAllText = "";
                    try
                    {
                         strAllText = File.ReadAllText(strFileToProcess);
                    }
                    catch (System.IO.IOException ex)
                    {
                        continue;
                    }
                    oPokList.Clear();
                    oCandyList.Clear();

                    

                    JArray o = JArray.Parse(strAllText);
                    IEnumerable<JToken> PokemonList = o.Select(x => x["inventoryItemData"]?["pokemonData"])
                                            .Where(x => x != null)
                                            .OrderByDescending(x => (double?)x["individualAttack"]
                                                                            + (double?)x["individualDefense"]
                                                                            + (double?)x["individualStamina"]);
                    foreach (var pokemon in PokemonList)
                    {
                        //var pokemon = InventoryItem["inventoryItemData"]["pokemonData"];
                        if (pokemon != null)
                        {
                            if (pokemon["cp"] != null)
                            {
                                cPokemon oPok = new cPokemon();
                                oPok.PokemonID = (string)pokemon["pokemonId"];
                                oPok.cp = (int)pokemon["cp"];
                                oPok.individualAttack = (pokemon["individualAttack"] == null ? 0 : (double)pokemon["individualAttack"]);
                                oPok.individualDefense = (pokemon["individualDefense"] == null ? 0 : (double)pokemon["individualDefense"]);
                                oPok.individualStamina = (pokemon["individualStamina"] == null ? 0 : (double)pokemon["individualStamina"]);
                                oPok.firstAttack = (pokemon["move1"] == null ? "unknown" : pokemon["move1"].ToString());
                                oPok.secondAttack = (pokemon["move2"] == null ? "unknown" : pokemon["move2"].ToString());
                                oPokList.Add(oPok);
                                if ((Int64)pokemon["creationTimeMs"] > iLatestPokemonTimeStamp)
                                {
                                    grdPokemons.Dispatcher.Invoke(
                                        delegate
                                        {
                                            txtLatestPokemon.Text = oPok.cp + " " + oPok.PokemonID + " " + (double)(oPok.individualAttack + oPok.individualDefense + oPok.individualStamina) / 3;
                                        });
                                    iLatestPokemonTimeStamp = (Int64)pokemon["creationTimeMs"];
                                }
                            }
                        }
                        
                    }
                    IEnumerable<JToken> CandyList = o.Select(x => x["inventoryItemData"]?["candy"])
                        .Where(x => x != null)
                        .OrderByDescending(x => x["familyId"]);

                    foreach (JObject candy in CandyList)
                    {
                        if (candy != null)
                        {
                            Candy oCandy = new Candy();
                            int.TryParse((string)candy["candy"], out oCandy.iCandy);
                            oCandy.Family = (string)candy["familyId"];
                            oCandyList.Add(oCandy);
                        }
                    }

                    grdPokemons.Dispatcher.Invoke(
                        delegate
                        {
                            Paragraph p = new Paragraph();
                            foreach (cPokemon oPok in oPokList.OrderByDescending(ooo => ooo.individualAttack + ooo.individualDefense + ooo.individualStamina).ThenByDescending(n => n.cp).ToList())
                            {
                                double dPerfect = (oPok.individualAttack + oPok.individualDefense + oPok.individualStamina) / 45 * 100.0;                                
                                
                                AddPokemonToLstBox(oPok.cp, oPok.PokemonID, dPerfect, oPok.firstAttack, oPok.secondAttack);
                            }

                            Paragraph pCandies = new Paragraph();
                            foreach (Candy oCandy in oCandyList.OrderByDescending(ooo => ooo.Family).ToList())
                            {                                
                                pCandies.Inlines.Add(new Run(oCandy.Family.PadRight(30) + "\t" + oCandy.iCandy + System.Environment.NewLine));                                
                            }
                            txtCandies.Document.Blocks.Add(pCandies);
                            
                        });
                }
                System.Threading.Thread.Sleep(iDelay);
            }


        }
        public class PokemonDisplay
        {
            public int CP { get; set; }

            public string PokemonID { get; set; }

            public double dPerfect { get; set; }

            public string FirstAttack{ get; set; }

            public string SecondAttack{ get; set; }
        }

        private void AddPokemonToLstBox(int cp, string pokemonID, double dperfect, string strFirstAttack, string strSecondAttack)
        {
            this.grdPokemons.Items.Add(new PokemonDisplay{ CP = cp, PokemonID = pokemonID, dPerfect = dperfect, FirstAttack = strFirstAttack, SecondAttack = strSecondAttack });
        }
        
        private void ParseLatLong_Click(object sender, RoutedEventArgs e)
        {
            string[] latlong = txtLatLong.Text.Trim().Split(' ');
            if (latlong.Length == 2)
            {
                string url = "https://pokevision.com/#/@" + latlong[0] + "," + latlong[1];
                System.Diagnostics.Process.Start(url);                
            }
        }

        private void btnStartStop_Click(object sender, RoutedEventArgs e)
        {
            if ((string)btnStartStop.Content == "Start")
            {
                if (!mBW.IsBusy)
                { 
                    btnStartStop.Content = "Stop";
                    BeginWorker();
                }
            }
            else
            {
                mBW.CancelAsync();
                btnStartStop.Content = "Start";

            }
        }
    }
    public class IVToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double? input = value as double?;
            if (input != null)
            {
                if (input >= 95)
                {
                    return Brushes.LightGreen;
                }
                else if (input >= 90)
                {
                    return Brushes.Gray;
                }
                else
                {
                    return Brushes.Red;
                }                
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
