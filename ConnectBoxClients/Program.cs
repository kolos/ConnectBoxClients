﻿using System.Xml;

string addr = "192.168.1.1";
string password = "your password here";

string login_url = $"http://{addr}/common_page/login.html";
string getter_url = $"http://{addr}/xml/getter.xml";
string setter_url = $"http://{addr}/xml/setter.xml";


Func<HttpClientHandler, string> GetToken = handler => handler.CookieContainer.GetCookies(new Uri(login_url)).First(x => x.Name == "sessionToken").Value;
HttpClientHandler handler = new HttpClientHandler()
{
    CookieContainer = new(),
    AllowAutoRedirect = false,
};

var httpClient = new HttpClient(handler);
httpClient.DefaultRequestHeaders.Add("User-Agent", "Chrome");

await httpClient.GetAsync(login_url);
await httpClient.PostAsync(setter_url, new StringContent($"token={GetToken(handler)}&fun=15&Username=NULL&Password={password}"));

Dictionary<string, int> rates = new();

while (true)
{
    var response = await httpClient.PostAsync(getter_url, new StringContent($"token={GetToken(handler)}&fun=123"));

    string result = response.Content.ReadAsStringAsync().Result;
    if (result == "") continue;
    var xml = new XmlDocument();
    xml.LoadXml(result);

    Console.Clear();
    Console.WriteLine($"MAC address\t\tspeed\tchange\tif.id\tinterface name");
    foreach (XmlNode clientInfo in xml.SelectNodes("LanUserTable/*/clientinfo"))
    {
        var _mac = clientInfo.SelectSingleNode("MACAddr").InnerText;
        var _interface = clientInfo.SelectSingleNode("interface").InnerText;
        var _interfaceid = clientInfo.SelectSingleNode("interfaceid").InnerText;
        var _speed = Convert.ToInt32(clientInfo.SelectSingleNode("speed").InnerText);

        if(rates.ContainsKey(_mac))
        {
            Console.WriteLine($"{_mac}\t{_speed}\t{(_speed - rates[_mac]).ToString("+#;-#;0")}\t{_interfaceid}\t{_interface}");
        } else
        {
            Console.WriteLine($"{_mac}\t{_speed}\t\t{_interfaceid}\t{_interface}");
        }
        rates[_mac] = _speed;
    }
    Thread.Sleep(5 * 1000);
}