using System.Net;
using System.Xml;

string addr = "192.168.1.1";
string password = "your password here";

string login_url = $"http://{addr}/common_page/login.html";
string getter_url = $"http://{addr}/xml/getter.xml";
string setter_url = $"http://{addr}/xml/setter.xml";

HttpClientHandler handler = new HttpClientHandler()
{
    CookieContainer = new(),
    AllowAutoRedirect = false,
};

var httpClient = new HttpClient(handler);
httpClient.DefaultRequestHeaders.Add("User-Agent", "Chrome");
httpClient.DefaultRequestHeaders.Add("Referer", $"http://{addr}");

await httpClient.GetAsync(login_url);
var login_response = await httpClient.PostAsync(setter_url, new StringContent($"fun=15&Username=admin&Password={password}"));
string login_result = login_response.Content.ReadAsStringAsync().Result;
if (!login_result.StartsWith("successful"))
{
    Console.WriteLine("Unsuccessful login.");
    return;
}
handler.CookieContainer.Add(new Uri($"http://{addr}"), new Cookie("SID", login_result[15..]));

Dictionary<string, int> rates = new();

while (true)
{
    var response = await httpClient.PostAsync(getter_url, new StringContent($"fun=123"));

    string result = response.Content.ReadAsStringAsync().Result;
    if (result == "") continue;
    var xml = new XmlDocument();
    xml.LoadXml(result);

    Console.Clear();
    Console.WriteLine($"MAC address\t\tspeed\tchange\tconnected to");
    foreach (XmlNode clientInfo in xml.SelectNodes("LanUserTable/*/clientinfo"))
    {
        var _mac = clientInfo.SelectSingleNode("MACAddr").InnerText;
        var _interface = clientInfo.SelectSingleNode("interface").InnerText;
        var _interfaceid = Convert.ToInt32(clientInfo.SelectSingleNode("interfaceid").InnerText);
        var _speed = Convert.ToInt32(clientInfo.SelectSingleNode("speed").InnerText);
        var _connectedto = _interfaceid switch
        {
            _ when _interfaceid >= 3 && _interfaceid <= 18 => "Wi-Fi 2.4G",
            _ when _interfaceid >= 19 && _interfaceid <= 34 => "Wi-Fi 5G",
            _ => _interface
        };

        if(rates.ContainsKey(_mac))
        {
            Console.WriteLine($"{_mac}\t{_speed}\t{(_speed - rates[_mac]).ToString("+#;-#;0")}\t{_connectedto}");
        } else
        {
            Console.WriteLine($"{_mac}\t{_speed}\t\t{_connectedto}");
        }
        rates[_mac] = _speed;
    }

    Console.WriteLine($"\nclients connected: {xml.SelectSingleNode("LanUserTable/totalClient").InnerText}");
    Thread.Sleep(5 * 1000);
}