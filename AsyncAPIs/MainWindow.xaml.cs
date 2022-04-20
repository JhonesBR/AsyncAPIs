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
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;

namespace AsyncAPIs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {   
        public MainWindow()
        {
            InitializeComponent();
        }

        #region UpdateTextBlock
        private void UpdateTextBlock(Dictionary<string, string> values)
        {
            TextBlockResults.Text = "";
            for (int i = 0; i < values.Count; i++)
            {
                TextBlockResults.Text += values.ElementAt(i).Key + ": " + values.ElementAt(i).Value + "\n";
            }
        }
        #endregion

        #region ButtonCEPConsult_Click
        private async void ButtonCEPConsult_Click(object sender, RoutedEventArgs e)
        {
            string cep = TextBoxCEP.Text;
            if (ValidateField(@"^\d{8}$" ,cep))
            {
                var values = await Task.Run(() => ConsultaCEPAsync(cep));
                if (values != null)
                    UpdateTextBlock(values);
            } else
            {
                MessageBox.Show("CEP inválido", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region ButtonCNPJConsult_Click
        private async void ButtonCNPJConsult_Click(object sender, RoutedEventArgs e)
        {
            string cnpj = TextBoxCNPJ.Text;
            if (ValidateField(@"^\d{14}$", cnpj))
            {
                var values = await Task.Run(() => ConsultaCNPJAsync(cnpj));
                if (values != null)
                    UpdateTextBlock(values);
            }
            else
            {
                MessageBox.Show("CNPJ inválido", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
       #endregion

        #region ValidateField
        private bool ValidateField(string regex, string field)
        {
            if (field.Length == 0)
            {
                return false;
            }
            else
            {
                return System.Text.RegularExpressions.Regex.IsMatch(field, regex);
            }
        }
        #endregion

        #region ConsultaCEPAsync
        private Dictionary<string, string> ConsultaCEPAsync(string cep)
        {
            try
            {
                string url = $"https://viacep.com.br/ws/{cep}/json/";
                WebRequest? request = WebRequest.Create(url);
                var response = request.GetResponse();
                var stream = response.GetResponseStream();
                var reader = new System.IO.StreamReader(stream);
                JObject json = JObject.Parse(reader.ReadToEnd());
                if (((HttpWebResponse)response).StatusDescription.ToString() == "OK")
                {
                    var values = new Dictionary<string, string>();
                    values.Add("CEP", json["cep"].ToString());
                    values.Add("Estado", json["uf"].ToString());
                    values.Add("Cidade", json["localidade"].ToString());
                    values.Add("Bairro", json["bairro"].ToString());
                    values.Add("Rua", json["logradouro"].ToString());
                    values.Add("DDD", json["ddd"].ToString());
                    return values;
                }
                else
                {
                    MessageBox.Show($"Não foi possível se conectar ao url {url}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            } catch(Exception)
            {
                MessageBox.Show($"CEP inserido não é atribuido a nenhum municício", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            return null;
        }
        #endregion

        #region ConsultaCNPJAsync
        private Dictionary<string, string> ConsultaCNPJAsync(string cnpj)
        {
            try
            {
                string url = $"https://receitaws.com.br/v1/cnpj/{cnpj}";
                WebRequest? request = WebRequest.Create(url);
                var response = request.GetResponse();
                var stream = response.GetResponseStream();
                var reader = new System.IO.StreamReader(stream);
                JObject json = JObject.Parse(reader.ReadToEnd());
                int i = 1;
                if (((HttpWebResponse)response).StatusDescription.ToString() == "OK")
                {
                    if (json["status"].ToString() == "OK")
                    {
                        var values = new Dictionary<string, string>();
                        values.Add("CNPJ", json["cnpj"].ToString());
                        values.Add("Nome", json["nome"].ToString());
                        values.Add("Tipo", json["tipo"].ToString());
                        values.Add("Telefone", json["telefone"].ToString());
                        values.Add("Email", json["email"].ToString());
                        values.Add("Atividade principal", json["atividade_principal"][0]["text"].ToString());
                        foreach (var atv in json["atividades_secundarias"])
                        {
                            values.Add($"Atividade secundária {i}", atv["text"].ToString());
                            i++;
                        }
                        values.Add("Situação", json["situacao"].ToString());
                        values.Add("Endereço", $"{json["logradouro"]}, {json["numero"]}, {json["bairro"]}, {json["municipio"]} - {json["uf"]}");
                        return values;
                    } else
                    {
                        MessageBox.Show($"CNPJ inválido", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show($"Não foi possível se conectar ao url {url}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show($"Erro ao conectar na api {exc}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return null;
        }
        #endregion
    }
}
