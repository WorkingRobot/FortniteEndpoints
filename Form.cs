using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using System.Windows.Forms;
using System.Net.Http;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading;
using Parser;
using Parser2;
using Newtonsoft.Json.Linq;

namespace HACK_INTO_THE_MAINFRAME
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static string getAccessToken(string username, string password)
        {
            var getAccessToken = new RestClient("https://account-public-service-prod03.ol.epicgames.com/account/api/oauth/token");

            var gat = new RestRequest(Method.POST);

            gat.AddParameter("grant_type", "password");
            gat.AddParameter("username", username);
            gat.AddParameter("password", password);
            gat.AddParameter("includePerms", "true");

            gat.AddHeader("Authorization", "basic MzQ0NmNkNzI2OTRjNGE0NDg1ZDgxYjc3YWRiYjIxNDE6OTIwOWQ0YTVlMjVhNDU3ZmI5YjA3NDg5ZDMxM2I0MWE");
            gat.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            var repo = getAccessToken.Execute(gat).Content;

            return Parser.AccessToken.FromJson(repo).AccessTokenAccessToken;
        }

        public static string getAccessCode(string accessToken)
        {
            var client = new RestClient("https://account-public-service-prod03.ol.epicgames.com/account/api/oauth/exchange");

            var req = new RestRequest(Method.GET);

            req.AddHeader("Authorization", "bearer " + accessToken);

            var accessCode = client.Execute(req).Content;

            return Parser2.AccessCode.FromJson(accessCode).Code;
        }

        public string getExchangeToken(string accessCode)
        {
            var client = new RestClient("https://account-public-service-prod03.ol.epicgames.com/account/api/oauth/token");

            var req = new RestRequest(Method.POST);

            req.AddHeader("Authorization", "basic ZWM2ODRiOGM2ODdmNDc5ZmFkZWEzY2IyYWQ4M2Y1YzY6ZTFmMzFjMjExZjI4NDEzMTg2MjYyZDM3YTEzZmM4NGQ");
            req.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            req.AddParameter("grant_type", "exchange_code");
            req.AddParameter("exchange_code", accessCode);
            req.AddParameter("includePerms", true);
            req.AddParameter("token_type", "eg1");

            var ExchangeToken = Parser3.ExchangeToken.FromJson(client.Execute(req).Content).AccessToken;

            return ExchangeToken;
        }
        
        public static string Token = null;
        private void button1_Click(object sender, EventArgs e)
        {
            label4.Text = "Get accToken";

            var accessToken = getAccessToken(textBox1.Text, textBox2.Text);
            label4.Text = "Get accCode";
            var accessCode = getAccessCode(accessToken);
            label4.Text = "get exToken";
            var exchangeToken = getExchangeToken(accessCode);

            label4.Text = "Authed: True";
            EnableEndpoints();
            Token = exchangeToken;
        }

        public void EnableEndpoints()
        {
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = true;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = GetEndpoint("https://fortnite-public-service-prod11.ol.epicgames.com/fortnite/api/storefront/v2/catalog", true);
        }

        public string GetEndpoint(string url, bool auth)
        {
            var client = new RestClient(url);
            var req = new RestRequest(Method.GET);
            if (auth)
            {
                req.AddHeader("X-EpicGames-Language", "en");
                req.AddHeader("Authorization", "bearer " + Token);
            }            
            var response = client.Execute(req);
            var content = response.Content;

            content = JToken.Parse(content).ToString(Formatting.Indented);

            return content;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = GetEndpoint("https://fortnite-public-service-prod11.ol.epicgames.com/fortnite/api/calendar/v1/timeline", true);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var response = GetEndpoint("https://fortnite-public-service-prod11.ol.epicgames.com/fortnite/api/storefront/v2/keychain", true);

            var KEYS = Parser4.Aes.FromJson(response);

            var output = "";

            foreach (var i in KEYS)
            {
                var parts = i.Split(':');

                var guid = parts[0];
                var key = parts[1];
                var item = parts.Length > 2 ? parts[2] : "N/A";

                byte[] bytes = Convert.FromBase64String(key);

                var hex = BitConverter.ToString(bytes);

                var aeskey = hex;

                output += $"0x{aeskey.Replace("-", "")}: {item}\n";
            }

            richTextBox1.Text = output;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = GetEndpoint("https://lightswitch-public-service-prod06.ol.epicgames.com/lightswitch/api/service/bulk/status?serviceId=Fortnite", false);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = GetEndpoint("https://fortnitecontent-website-prod07.ol.epicgames.com/content/api/pages/fortnite-game", false);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
