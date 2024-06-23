/*支持的网站
 * 
 * 
 * 萌道
 * https://www.mengdaow.com/
 * 
 *影视大全
 * http://www.chunhuibaojie.com/
 * 
 * 
 * https://www.lffun.com/
 * 
 * 
 * https://www.mgtv123.com/
 * 
 * 
 * https://www.7qhb.com/
 * 
 * 
 * https://t.mindaveld.com/
 * 
 * 
 * https://www.ledlmw.com/
 * 
 * https://v1.nicotv.bet
 * 
 * https://
 * 
 * 
 * 
 * 
 * 
 */

using FFMpegCore;
using FFMpegCore.Enums;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Runtime.InteropServices;
using IniFile;
namespace 视频番剧爬取器
{


    public partial class Form1 : Form
    {
        #region 变量
        /// <summary>
        /// 整个数据包
        /// </summary>
        public List<Data> datas = new();
        private ConfigData allInfo = new();
        private readonly List<string> logList = new();
        readonly DateTime runTime = DateTime.Now;
        /// <summary>
        /// FFmpeg目录
        /// </summary>
        private string ffmpegPath = "";
        /// <summary>
        /// AES密码
        /// </summary>
        byte[] encKey = Array.Empty<byte>(); //AES-128加密密码
        //int AesType = 128;            //备用，AES加密类型
        PaddingMode padding = PaddingMode.PKCS7; //填充模式;
        byte[] iv = new byte[16];
        
        /// <summary>
        /// 当前视频名称
        /// </summary>
        string videoName = "";//名称

        /// <summary>
        /// 已完成的项目计数
        /// </summary>
        int tsCompleted = 0;
        Stopwatch timeWatch = new();
        /// <summary>
        /// 基础Url
        /// </summary>
        string baseUrl = "";
        /// <summary>
        /// URL后缀
        /// </summary>
        readonly string urlSub = "";

        bool Single = false; //是否为剧场版;
        bool IsComplete = false;
        Data? CurData = null;//正在进行的;
        /// <summary>
        /// 并发任务上限
        /// </summary>
        readonly static int semNum = 50;
        readonly SemaphoreSlim semaphore = new(semNum, semNum);


        #endregion

        #region 控件方法
        public Form1()
        {
            InitializeComponent();
        }
        private void AddButton_Click(object sender, EventArgs e)
        {
            string link = AddressText.Text;
            Data d = new(link);
            if(allP.Value == allP.Maximum)
            {
                allP.Value = 0;
                videoList.SelectedIndex = -1;
                videoList.Items.Clear();
                datas.Clear();
            }
            if (!TryAddDatas(d))
            {
                LogBox.Items.Add($"{d.Addr}:没有获取到数据");
            }
            else
            {
                Single = d.Single || cinmel.Checked;
                IsComplete = CompleteCheck.Checked;
                cinmel.Checked = CompleteCheck.Checked = false;
                AddressText.Text = "";
            }

            return;


        }
        private void Form1_Load(object sender, EventArgs e)
        {
            string systemPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) ?? "";

            // 检查 ffmpeg 是否在PATH中
            bool isFFmpegInPath = systemPath.Contains("ffmpeg");//!string.IsNullOrEmpty(systemPath) && systemPath.Contains("ffmpeg", StringComparison.OrdinalIgnoreCase);
            if (isFFmpegInPath)
            {
                var vg = systemPath.Split(";");
                foreach(string s in vg)
                {
                    if (s.Contains("ffmpeg"))
                        ffmpegPath = s;
                }

            }
            else
            {
                ffmpegPath = "./ffmpeg/bin";
            }
            if (File.Exists("下载数据.json"))
            {
                string str = File.ReadAllText("下载数据.json");
                allInfo = JsonConvert.DeserializeObject<ConfigData>(str)??new();
            }
            var iniFile = new IniFile.Ini();
            if(!File.Exists("配置文件.ini"))
            {
                iniFile.Add(new("目录", string.Empty));
                var rp = iniFile["目录"];
                rp.Add(new("根目录", Config.outPath["data"]["root"]));
                string[] des2 = {"如果有盘符，就会忽略根目录" };
                iniFile.Add(new("二级目录",des2));
                rp = iniFile["二级目录"];
                rp.Add(new("normal", Config.outPath["normal"]["path"]));
                string[] des3 = { "同样，如果有盘符，就会忽略二级目录和根目录" };
                iniFile.Add(new("三级目录", des3));
                rp = iniFile["三级目录"];
                rp.Add(new("normal_anime", Config.outPath["normal"]["anime"]));
                rp.Add(new("normal_video", Config.outPath["normal"]["video"]));
                iniFile.SaveTo("配置文件.ini");
                
            }else
            {
                iniFile = new IniFile.Ini("配置文件.ini");
                Config.outPath["data"]["root"] = iniFile["目录"]["根目录"];
                var rp = iniFile["二级目录"];
                Config.outPath["normal"]["path"] = rp["normal"];
                rp = iniFile["三级目录"];
                Config.outPath["normal"]["anmie"] = rp["normal_anime"];
                Config.outPath["normal"]["video"] = rp["normal_video"];
            }


            Test();
            //DownXdm4(null);
        }
        /// <summary>
        /// 下载按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Down_Click(object sender, EventArgs e)
        {
            await DownAsync();
        }
        //测试代码
        private void Test()
        {
            if(Single){}
            
        }

        private async void CheckVideo_Click(object sender, EventArgs e)
        {
            string[] floders =
            {
                Config.GetOutPath("normal","anime")
            };
            foreach (var floder in floders)
            {
                if (!Directory.Exists(floder)) continue;
                var epis = Directory.GetDirectories(floder);
                foreach (var epi in epis)
                {
                    string fileName = epi + "/状态.json";
                    if (!File.Exists(fileName)) continue;
                    string jsonStr = File.ReadAllText(fileName);
                    var info = JsonConvert.DeserializeObject<DownInfo>(jsonStr)??new();
                    TryAddDatas(info);
                }
            }
            await DownAsync();
            MessageBox.Show("更新完毕");

        }

        public bool TryAddDatas(Data d)
        {
            if (d.Title.Length == 0)
                return false;
            AddData(d);
            return true;
        }


        public void TryAddDatas(DownInfo info)
        {
            var urls = info.Urls;
            foreach(var url in urls)
            {
                if (url.Key.Length == 0) continue;
                Data d = new(url.Key);
                if(d.Html.Length<100)
                {
                    LogBox.Items.Add($"{info.Name}的URL:{url}可能不再可用");
                    urls[url.Key]++;
                    continue;
                }
                d.Single = false;
                AddData(d);
            }
        }

        public void AddData(Data d)
        {
            datas.Add(d);
            videoList.Items.Add(d.Title);
            allP.Value = datas.Count;
            UpdateAllPro(-1);
        }

        #endregion
        #region 对应下载方法
        /// <summary>
        /// 影视大全
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        private async Task DownChunhuibaojie(Data d)
        {
            //http://www.chunhuibaojie.com/voddetail/85355.html
            string videoId = d.Addr.Split("/")[4].Split(".")[0];
            string basePageUrl = "http://www.chunhuibaojie.com/vodplay/" + videoId + "-";
            for (int i = 1; i < 2; i++)
            {
                if (Directory.Exists("m3u8")) Directory.Delete("m3u8", true);
                string page = basePageUrl + i + "-" + 1 + ".html";
                await DownChunStack(page, d.Title);

            }
        }
        /// <summary>
        /// chun下载用的迭代
        /// </summary>
        /// <param name="url"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private async Task<bool> DownChunStack(string url, string name)
        {
            videoName = name;
            if (Directory.Exists("m3u8")) Directory.Delete("m3u8");
            using HttpClient clinet = new();
            clinet.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36 Edg/125.0.0.0");
            HttpResponseMessage response = new();
            try
            {
                response = await clinet.GetAsync(url);
            }
            catch
            {
                Debug.WriteLine("访问网页失败？");
                return false;
            }
            string content = await response.Content.ReadAsStringAsync();
            HtmlAgilityPack.HtmlDocument doc = new();
            doc.LoadHtml(content);
            HtmlNode scriptNode = doc.DocumentNode.SelectSingleNode("//div[@id='zanpiancms_player']//script");
            string scriptContent = scriptNode.InnerText;
            // 使用正则表达式提取url和next的值
            string m3Url =  Regex.Match(scriptContent, @"""url"":\s*""([^""]+)""").Groups[1].Value.Replace("\\", "");
            string next = Regex.Match(scriptContent, @"""next"":\s*""([^""]+)""").Groups[1].Value;
            var strs = url.Split("/");
            string id = strs[4].Split(".")[0].Split("-")[2];
            downP.Value = 0;
            if (
            !await Download(m3Url, id))
                return false;
            if (next == null)
            {
                return true;
            }

            else
                return await DownChunStack(next.Replace("\\", ""), name);
        }



        private async Task Down7qhb(Data d)
        {
            HtmlAgilityPack.HtmlDocument doc = new();
            doc.LoadHtml(d.Html);
            var sourceNodes = doc.DocumentNode.SelectNodes("//ul[@class='hl-from-list']/li");
            var introNode = doc.DocumentNode.SelectSingleNode("//meta [@name='description']");
            string pattern = "年份\\s?：\\s?</em>(\\d+)\\s?";
            string timeText = Regex.Match(d.Html, pattern).Groups[1].Value;
            bool isComplete = timeText.Length > 0 && int.TryParse(timeText, out int timeYear) && timeYear < DateTime.Now.Year - 1;

            List<string> sourceKeys = new();
            List<string> sourcePages = new();
            string pageBase = GetUriBase(new Uri(d.Addr));
            foreach (var sourceNode in sourceNodes)
            {
                string sourceName = sourceNode.ChildNodes[2].InnerText[..3].Replace(" ", "");
                sourceKeys.Add(sourceName);
                string sourceUrl = UrlWithBase((sourceNode.GetAttributeValue("data-href", "")), pageBase);
                sourcePages.Add(sourceUrl);
            }
            var nameNode = doc.DocumentNode.SelectSingleNode("//span[@class='hl-mob-name hl-text-site hl-lc-1']");
            d.Title = videoName = nameNode.InnerText;
            var imgNode = doc.DocumentNode.SelectSingleNode("//span[@class='hl-topbg-pic']");
            string imgUrl = imgNode.GetAttributeValue("style", "");
            imgUrl = GetBetweenString(imgUrl, "url(", ")");
            await DownloadJpeg(imgUrl, "webp");
            HtmlNode? signleNode = doc.DocumentNode.SelectSingleNode("//li/em[contains(text(),'时长：')]");
            Single = Single || (int.TryParse(signleNode?.ParentNode.InnerText.Replace("分钟", ""), out _));


            var html1 = await GetHtml(sourcePages[0]);
            doc.LoadHtml(html1);
            var epiNodes = doc.DocumentNode.SelectNodes("//ul[@class='hl-plays-list hl-sort-list hl-list-hide-xs hl-list-md clearfix']/li/a");
            List<string> keys = new();
            Dictionary<string, bool> bs = new();
            List<string> pages = new();
            foreach(var epiNode in epiNodes)
            {
                string url = epiNode.GetAttributeValue("href", "");
                string id = CopeEpi(epiNode.InnerText);
                pages.Add(UrlWithBase(url, pageBase));
                bs[id] = false;
                keys.Add(id);
            }
            DownInfo info = new(d, introNode.GetAttributeValue("content", ""), bs)
            {
                IsComplete = isComplete
            };
            if (TryLoadInfo(out DownInfo? info2))
                info.ImPortData(info2??new());
            bs = new(info.DownState);
            UpdateFileProcess(bs, info, true);
            //
            Dictionary<string, string> sources = new()
            {
                ["UK"] = "https://ukzy.ukubf4.com",
                ["WJ"] = "https://webp.ykjljdcss.com",
                ["LZ"] = "https://v.cdnlz22.com",
                ["FB"] = "https://s2.bfbfvip.com",
                ["IK"] = "https://ikcdn01.ikzybf.com",
                ["KC"] = "https://vod2.pptvoss.com",
                ["HN"] = "https://hnzy.bfvvs.com",
                ["JY"] = "https://hd.ijycnd.com",
                ["XL"] = "https://bf.xluuss.com:39443",
                ["SBO"] = "https://1080p.sbzyplay.com",
                ["GS"] = "https://v.gsuus.com",
                ["SD"] = "https://baidu.youkuoss.com",
                ["JS"] = "https://bfikuncdn.com"
            };
            //开始下载
            for (int i = 0; i < pages.Count; i++)
            {
                string id = keys[i];
                if (bs[id])
                {
                    UpdateFileProcess(bs, info);
                    continue;
                }
                string sourceName = sourceKeys[i/bs.Count];
                baseUrl = sources[sourceName];
                string page = pages[i];
                var htmlt = await GetHtml(page);
                doc.LoadHtml(htmlt);
                var dataNode = doc.DocumentNode.SelectSingleNode("//script[contains(text(), 'cmsPlayer')]");
                string url = Regex.Match(dataNode.InnerText, "url\\\":\\\"([^\"]+)\\\"").Groups[1].Value.Replace("\\", "")
                    .Replace("%3A", ":").Replace("%2F", "/").Replace("u0026","&");
                url = UrlWithBase(url, baseUrl);
                if (sourceName == "JY" || sourceName == "HN")
                    baseUrl = Regex.Match(url, "(.*/)").Groups[1].Value;
                
                bs[id] = await Download(Regex.Match(url, "url=([^\"]+)").Groups[1].Value, id);
                UpdateFileProcess(bs, info);
            }
            MakeLog(bs);
        }


        /// <summary>
        /// 下载芒果123
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>

        private async Task DownMg123(Data d)
        {

            //整理链接 顺带整理集数
            HtmlAgilityPack.HtmlDocument doc = new();
            doc.LoadHtml(d.Html);
            var epiNodes = doc.DocumentNode.SelectNodes("//ul[contains(@id,'con_playlist')]/li/a[@target='_blank']");
            var nameNode = doc.DocumentNode.SelectSingleNode("//h1[@class='text-overflow']");
            nameNode ??= doc.DocumentNode.SelectSingleNode("//a[@class='#']");
            videoName = nameNode.InnerText;
            var cimNode = doc.DocumentNode.SelectSingleNode("//li[@class='col-md-12.text']");
            var introNode = doc.DocumentNode.SelectSingleNode("//meta[@name='description']");
            Single = Single || (cimNode != null && cimNode.InnerText.Contains("电影"));

            List<string> pages = new();
            List<string> keys = new();
            Dictionary<string, bool> bs = new();
            string pageBase = GetUriBase(new Uri(d.Addr));
            foreach (var epi in epiNodes)
            {
                string url = epi.GetAttributeValue("href", "");
                pages.Add(UrlWithBase(url, pageBase));
                //处理名称
                string id = CopeEpi(epi.InnerText);
                if (!keys.Contains(id))
                    keys.Add(id);
                bs[id] = false;
            }
            videoP.Maximum = bs.Count;
            DownInfo info = new(videoName, bs);
            if (TryLoadInfo(out DownInfo? info2))
                info.ImPortData(info2 ?? new());
            bs = new(info.DownState);
            info.AddUrl(d.Addr);
            UpdateFileProcess(bs, info);
            for (int i = 0; i < pages.Count; i++)
            {
                string? page = pages[i];
                string html = await GetHtml(page);
                doc.LoadHtml(html);
                var urlNodes = doc.DocumentNode.SelectSingleNode("//div[@class='m1938']/script");
                string playUrl = GetBetweenString(urlNodes.InnerText, "url\":\"", "\"").Replace("\\", "");
                string html2 = await GetHtml(playUrl);
                string playBaseUrl = GetUriBase(new Uri(playUrl));
                string[] catchDomain = { "huyall", "ffzyread", "ffzy-online1", "ffzy-play2" };
                string[] catchFirst = { "url: '", "main=\"", "main = \"", "main = \"" };
                string[] catchSecond = { "'", "\"", "\"", "\"" };
                string first = "";
                string second = "";

                for (int i1 = 0; i1 < catchDomain.Length; i1++)
                {
                    string? str = catchDomain[i1];
                    if (playBaseUrl.Contains(str))
                    {
                        first = catchFirst[i1];
                        second = catchSecond[i1];
                    }

                }
                info.Intro = introNode.GetAttributeValue("content", "");
                info.AddUrl(d.Addr);
                string url = UrlWithBase(GetBetweenString(html2, first, second), playBaseUrl);
                string id = keys[i];
                if (!bs[id] && await Download(url, id))
                    bs[id] = true;
                UpdateFileProcess(bs, info);
            }
            MakeLog(bs);
        }
        /// <summary>
        /// 下载LEDLMW，没写完
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
       

        private async Task Downlffun(Data d)
        {
            HtmlAgilityPack.HtmlDocument doc = new();
            doc.LoadHtml(d.Html);
            //<h3 class="slide-info-title hide">回复术士的重来人生</h3>
            videoName = doc.DocumentNode.SelectSingleNode("//h3").InnerText;
            if (d.Title.Contains("日漫剧场")) Single = true;
            Dictionary<string, string> header = new()
            {
                {"Referer","https://www.lffun.com/" },
                {"authority","vod.2bdm.cc" },
                {"path","" }
            };
            var urlNodes = doc.DocumentNode.SelectNodes("//li[@class='box border']");
            Dictionary<string, bool> bs = new();
            List<string> keys = new();
            List<string> pages = new();
            foreach (var node in urlNodes)
            {
                string url = node.ChildNodes[0].GetAttributeValue("href", "");
                string id = CopeEpi(node.InnerText);
                if (!keys.Contains(id))
                    keys.Add(id);
                bs[id] = false;
                string pageBase = new Uri(d.Addr).Scheme + "://" + new Uri(d.Addr).Host;
                pages.Add(UrlWithBase(url, pageBase));
            }
            DownInfo info = new(videoName, new Dictionary<string, bool>(bs));
            if (d.Html.Contains("已完结") && keys.Count == 1) Single = true;
            if (TryLoadInfo(out DownInfo? info2))
                info.ImPortData(info2 ?? new());
            bs = new(info.DownState);
            var introNode = doc.DocumentNode.SelectSingleNode("//div[@class='text cor3']");
            info.Intro = introNode.InnerText;
            info.AddUrl(d.Addr) ;
            for (int i = 0; i < pages.Count; i++)
            {
                string? page = pages[i];
                string html2 = await GetHtml(page, header);
                doc.LoadHtml(html2);
                HtmlNode node = doc.DocumentNode.SelectSingleNode("//script[contains(text(),'player_aaaa')]");
                string url2 = GetBetweenString(node.InnerHtml, "},\"url\":\"", "\"").Replace("\\", "");
                int it = d.Addr.LastIndexOf("/");
                string videoId = d.Addr.Substring(it + 1, d.Addr.LastIndexOf('.') - it - 1);
                //string videoId = pageEles[pageEles.Length-1].Split('.')[0];
                string id = keys[i];
                string url3 = "https://www.lffun.com/addons/dp/player/dp.php?key=0&from=2bdm&id=" + videoId + "&uid=0&url=" +
                    url2 + "&jump=";
                string apiContent = await GetHtml(url3, header);
                if(apiContent.Contains("解析失败"))
                {
                    info.RemoveUrl(d.Addr);
                    return;
                }
                string url = GetBetweenString(apiContent, "url\": \"", "\"");
                header["path"] = GetBetweenString(url, "cc", "");
                if (!bs[id] && await Download(url, id, header))
                    bs[id] = true;
                UpdateFileProcess(bs, info);
            }
            MakeLog(bs);
        }

        private async Task DownMindaveld(Data d)
        {
            HtmlAgilityPack.HtmlDocument doc = new();
            var dn = doc.DocumentNode;
            doc.LoadHtml(d.Html);
            var nameNode = dn.SelectSingleNode("//div[@class='details-pic']/a");
            d.Title = videoName = nameNode.GetAttributeValue("title", "");
            var epiNodes = dn.SelectNodes("//div[@class='playlist']/ul/li/a");
            List<string> keys = new();
            Dictionary<string, bool> bs = new();
            List<string> pages = new();
            string pageBase = GetUriBase(d.Addr);
            foreach(var en in epiNodes)
            {
                string id = CopeEpi(en.InnerText);
                string pageUrl = UrlWithBase(en.GetAttributeValue("href", ""), pageBase);
                pages.Add(pageUrl);
                keys.Add(id);
                bs[id] = false;
            }
            DownInfo info = new(d, dn.SelectSingleNode("//span[@class='details-content-all collapse']").InnerText, bs);
            if(TryLoadInfo(out DownInfo? info2))
            {
                info.ImPortData(info2 ?? new());
            }
            string picUrl = Regex.Match(nameNode.InnerText, "url\\(([^()]+)\\)").Groups[1].Value;
            await DownloadJpeg(picUrl);
            //开始下载
            //制作头文件
            Dictionary<string, string> header = new()
            {
                {"Host","t.mindaveld.com"},
                {"Referer","http://t.mindaveld.com/search/"},
                {"Accept","text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7" }
            };
            //获取基本链接
            UpdateFileProcess(bs, info, true);
            //开始逐个下载
            for (int i = 0; i < pages.Count; i++)
            {
                string id = keys[i];
                if (bs[id])
                {
                    UpdateFileProcess(bs, info);
                    return;
                }
                string page = pages[i];
                header["Referer"] = page;
                string html = await GetHtml(page, header);
                doc.LoadHtml(html);
                string cmsPlay = dn.SelectSingleNode("//div[@id = 'zanpiancms_player']/div/script").InnerText;
                string url1 = Regex.Match(cmsPlay, "url\":\\s?\"([^\"]+)\"").Groups[1].Value.Replace("\\", "");
                string apiUrl = Regex.Match(cmsPlay, "apiurl\":\\s?\"([^\"]+)\"").Groups[1].Value.Replace("\\","");
                //获取第二层解析
                Dictionary<string, string> apiHeader = new()
                {
                    {"Referer","https://jiexi.ddmz6.com/" },
                    {"Origin","cdn.yddsha2.com"}
                };
                string apiHtml = await GetHtml(apiUrl + url1 + "&code=vip", apiHeader);
                string m3u8Url = Regex.Match(apiHtml, "var url\\s?=\\s?'([^']+)'").Groups[1].Value;
                
                bs[id] = await Download(m3u8Url, id, header);
                UpdateFileProcess(bs, info);
            }
            MakeLog(bs);

        }


        private async Task DownXdm4(Data d)
        {
            /*key在
             * https://www.xdm4.com/static/js/player.js?t=a20240617
             * 获取
             */
            string _key = "57A891D97E332A9D";
            HtmlAgilityPack.HtmlDocument doc = new();
            doc.LoadHtml(d.Html);
            var dn = doc.DocumentNode;
            var NameNode = dn.SelectSingleNode("//h1");
            videoName = NameNode.InnerText;
            var PageNodes = dn.SelectNodes("//div[contains(@id,'playlist')]/ul/li/a");
            List<string> pages = new();
            Dictionary<string, bool> bs = new();
            List<string> keys = new();
            string pageBase = GetUriBase(new Uri(d.Addr));
            foreach(var pn in PageNodes)
            {
                string page = UrlWithBase(pn.GetAttributeValue("href",""), pageBase);
                string key = CopeEpi(pn.InnerText);
                pages.Add(page);
                keys.Add(key);
                bs[key] = false;
            }
            DownInfo info = new(videoName, bs)
            {
                Name = videoName
            };
            var DesNode = dn.SelectSingleNode("//meta[@name ='description']");
            info.Intro = DesNode.GetAttributeValue("content", "");
            info.AddUrl(d.Addr);
            var picNode = dn.SelectSingleNode("//div[@class='myui-panel col-pd clearfix']/div/a/img");
            string picUrl = picNode.GetAttributeValue("data-original", "");
            await DownloadJpeg(picUrl);
            if (!Single&&TryLoadInfo(out DownInfo? info2))
            {
                info.ImPortData(info2 ?? new());
            }
            bs = new(info.DownState);
            UpdateFileProcess(bs, info, true);
            for(int i = 0; i<pages.Count;i++)
            {
                if (bs[keys[i]])
                {
                    UpdateFileProcess(bs, info);
                    continue;
                }
                
                string page = pages[i];
                string pageHtml = await GetHtml(page);
                if(pageHtml.Length<100)
                {
                    Debug.WriteLine("获取xdm4Html失败");
                    continue;
                }
                doc.LoadHtml(pageHtml);
                string pattern = "player_aaaa=\\{.*?url\\\":\\\"([^\\\"]+)\\\"";
                string apiUrlArg = Regex.Match(pageHtml, pattern).Groups[1].Value;
                encKey = Encoding.UTF8.GetBytes("57A891D97E332A9D");
                string apiUrl = "https://danmu.yhdmjx.com/m3u8.php?url=" + apiUrlArg;
                string apiHtml = await GetHtml(apiUrl);
                pattern = "config\\s?=\\s?\\{([^{}]+)\\}";
                string configText = Regex.Match(apiHtml, pattern).Groups[1].Value;

                //bt_token = "53f636b9e3f613e1"
                pattern = "bt_token\\s?=\\s?\"([^\"]+)\"";
                string btToken = Regex.Match(apiHtml, pattern).Groups[1].Value;
                iv = Encoding.UTF8.GetBytes(btToken);
                pattern = "getVideoInfo\\(\\\"([^\\\"]+)\\\"";
                string encUrl = Regex.Match(configText, pattern).Groups[1].Value;
                byte[] codeBytes = Convert.FromBase64String(encUrl);
                padding = PaddingMode.Zeros;
                Aes aes = Aes.Create();
                aes.Key = Encoding.UTF8.GetBytes(_key);
                aes.IV = Encoding.UTF8.GetBytes(btToken) ;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                string url = "";
                using (MemoryStream ms = new(codeBytes))
                using (CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read))
                {
                    byte[] plainBytes = new byte[codeBytes.Length];
                    int decryptedByteCount = cs.Read(plainBytes, 0, plainBytes.Length);
                    url =  Encoding.UTF8.GetString(plainBytes, 0, decryptedByteCount);
                }
                if (await Download(url, keys[i]))
                    bs[keys[i]] = true;
                UpdateFileProcess(bs, info);
            }
            MakeLog(bs);
        }

        private async Task DownYoutube(Data d)
        {
            if (d.Html.Length < 1) return;
            Single = true;
            if (!d.Html.Contains("streamingData"))
                return;
            HtmlAgilityPack.HtmlDocument doc = new();
            doc.LoadHtml(d.Html);
            var nameNode = doc.DocumentNode.SelectSingleNode("//meta[@name='title']");
            videoName = nameNode.GetAttributeValue("content", "");
            if (File.Exists(MakePath(""))) return;
            string ytInitialPlayerResponse = GetBetweenString(d.Html, "ytInitialPlayerResponse = ", "var meta");
            ytInitialPlayerResponse = ytInitialPlayerResponse[..ytInitialPlayerResponse.LastIndexOf(";")];
            var ytInitialPlayerResponseData = ConvertJson(ytInitialPlayerResponse);
            var ytStr = ytInitialPlayerResponseData["streamingData"].ToString();
            ytStr ??= "";
            var streamData = ConvertJson(ytStr);
            ytStr = streamData["adaptiveFormats"].ToString() ?? "";
            dynamic ada = JsonConvert.DeserializeObject(ytStr)??new();
            var video = ConvertJson(ada[0].ToString());
            var audio = ConvertJson(ada.Last.ToString());
            /*
             * Cookie:
                                VISITOR_INFO1_LIVE = o4FH7QJlBjo; VISITOR_PRIVACY_METADATA = CgJISxIEGgAgRw % 3D % 3D; PREF = tz = Asia.Shanghai & f4 = 4000000; YSC = 6sRIZ8kAAbs; GPS = 1
                    */

            string videoUrl = video["url"];
            //byte[] bytes = Encoding.UTF8.GetBytes(unicodeString);
            //return Encoding.UTF8.GetString(bytes);
            byte[] bytes = Encoding.UTF8.GetBytes(videoUrl);
            videoUrl = Encoding.UTF8.GetString(bytes);
            string audioUrl = audio["url"];

            Dictionary<string, string> headers = new()
            {

            };
            ParallelDownloader dl = new(headers, true);
            if (!await dl.DownloadFileAsync(videoUrl, "File/video.mp4", downP, downPl, downSl))
            {
                return;
            }

            dl = new(headers, true);
            if (!await dl.DownloadFileAsync(audioUrl, "File/audio.mp3", downP, downPl, downSl))
            {
                return;
            }
            string ffmepgPath = ffmpegPath + "/ffmpeg.exe";
            string videoFile = "File/video.mp4";
            string audioFile = "File/audio.mp3";
            string outputFile = $"File/{videoName}-{FormatId("")}-temp.mp4";
            string arguments = $"-i \"{videoFile}\" -i \"{audioFile}\" -c:v copy -c:a aac -map 0:v:0 -map 1:a:0 \"{outputFile}\"";
            ProcessStartInfo psi = new(ffmepgPath)
            {
                Verb = "runas",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = false,

            };
            Process pro = new()
            {
                StartInfo = psi
            };
            pro.OutputDataReceived += (sender, EventArgs) =>
            {

            };
            pro.ErrorDataReceived += (sender, EventArgs) =>
            {

            };
            try
            {
                pro.Start();
            }
            catch
            {
                Debug.WriteLine("start error,854");
            }

            pro.BeginOutputReadLine();
            pro.BeginErrorReadLine();
            pro.WaitForExit();
            File.Delete("File/video.mp4");
            File.Delete("File/audio.mp4");
            MoveFile("");

        }


        private async Task DownMengdao(Data d)
        {
            bool isComplete = !d.Title.Contains("更新");
            HtmlAgilityPack.HtmlDocument doc = new();
            doc.LoadHtml(d.Html);
            var dn = doc.DocumentNode;
            string htmlUrl = d.Addr;
            if (!d.Addr.Contains("html"))
                htmlUrl += "b-0-0.html";
            var htmlHeader = new Dictionary<string, string>
            {
                {"authority","www.mengdaow.com" },
            };
            string html = await GetHtml(htmlUrl,htmlHeader);
            if (html.Length < 100)
            {
                Debug.WriteLine("Mengdao:获取加密文本失败");
                return;
            }
            
            //生成下载信息
            DownInfo info = new()
            {
                Name = videoName = dn.SelectSingleNode("//h1").InnerText,
                Intro = dn.SelectSingleNode("//meta[@name='description']").GetAttributeValue("content", "")
            };
            info.AddUrl(d.Addr);
            var cmlNode = doc.DocumentNode.SelectSingleNode("//a[@href='/comicjc/index.html']");
            //获取imgUrl
            var picNode = doc.DocumentNode.SelectSingleNode("//div[@class='pic']/img");
            string picUrl = picNode.GetAttributeValue("src", "");
            if (!Single)
                await DownloadJpeg(picUrl);
            //解密Code获取下载链接
            string code = Regex.Match(html, "base64decode\\(\\\"([^\\\"]+)\\\"", RegexOptions.Singleline).Groups[1].Value;
            //获取密钥
            string codeContentPage = await GetHtml(GetUriBase(d.Addr) + "/insssss/boocom.js");
            string pattern = "onload(.*?)\\{\\}\\)\\)";
            string codeFuntionString = Regex.Match(codeContentPage, pattern,RegexOptions.Singleline).Groups[1].Value;
            //string  = GetBetweenString(codeContentPage, "window.onload", "{}))");
            string noAsciiCodeFunction = Regex.Unescape(TransAscii(codeFuntionString));
            var stringGroup = noAsciiCodeFunction.Split("|");
            string enckeyCode = $"{stringGroup[13]}.{stringGroup[11]}";
            encKey = Encoding.UTF8.GetBytes(enckeyCode);
            iv = Encoding.UTF8.GetBytes(stringGroup[12]);
            padding = PaddingMode.Zeros;
            byte[] decryByte = Convert.FromBase64String(DecodeBase64(code));
            string urlString = Encoding.UTF8.GetString(DecryptAES(decryByte));
            var urlDatas = urlString.Split("$");
            List<string> keys = new();
            Dictionary<string, bool> bs = new();
            Dictionary<string, List<List<string>>> urls = new();
            string[] sources = {"云播放","云视频", "视频在线","高清在线" };
            string source = "";
            for (int i = 0; i + 1 < urlDatas.Length; i += 2)
            {
                string name = urlDatas[i];
                string url = urlDatas[i + 1].Replace("_xigua","");
                //检测链接是否合法,如果不合法，循环到直到合法
                if (url.Length == 0)
                {
                    //死循环
                    while (true)
                    {
                        if (sources.Contains(name))
                        {

                            source = name;
                            i += 2;
                            name = urlDatas[i];
                            url = urlDatas[i + 1].Replace("_xigua", "");
                            break;
                        }
                        i++;
                        name = urlDatas[i];
                        
                        if (i + 1 > urlDatas.Length-1)
                            break;
                        url = urlDatas[i + 1].Replace("_xigua", "");
                    }
                }
                if (url.Length == 0 || i+1>urlDatas.Length-1) 
                    continue;
                string id = CopeEpi(name);
                if (!keys.Contains(id))
                {
                    keys.Add(id);
                    bs[id] = false;
                    urls[id] = new();
                }
                var urlTemp1 = urls[id];
                switch (source)
                {
                    case ("视频在线"):
                    case ("高清在线"):
                        var urlxiHeader = new Dictionary<string, string>
                        {
                            {"authority","www.mengdaow.com" },
                            {"path","/insssss/player_html/php/urlxi.php?vid="+url },
                            {"Referer",GetUriBase(new Uri(d.Addr)) + "/insssss/player_html/mp4.html" }
                        };
                        string url1 = "https://www.mengdaow.com/insssss/player_html/php/urlxi.php?vid=" +
                                url;
                        string htmlContent = await GetHtml(url1, urlxiHeader);
                        var urlSzDatas = Regex.Matches(htmlContent, "quality:\\s?\\[([^\\[\\]]+)\\],");
                        foreach (Match sul in urlSzDatas.Cast<Match>())
                        {

                            List<string> szUrls2 = new();
                            var fsUrls = Regex.Matches(sul.Groups[1].Value, "url: '([^']+)'");
                            foreach (Match fsu in fsUrls.Cast<Match>())
                                if (fsu.Groups[1].Value.Length > 0)
                                    szUrls2.Add(fsu.Groups[1].Value);
                            urlTemp1.Add(szUrls2);
                        }
                        break;
                    default:
                        var u3 = new List<string> {url };
                        urlTemp1.Add(u3);
                        break;
                }
            }
            info.DownState = new(bs);
            //判断是否是Single
            Single = Single || (cmlNode != null && cmlNode.InnerText.Contains("剧场版"))
                || d.Single || (!d.Title.Contains("更新") && urls.Count == 1);
            if (TryLoadInfo(out DownInfo? info2))
                info.ImPortData(info2 ?? new());
            info.IsComplete = isComplete;
            bs = new(info.DownState);
            UpdateFileProcess(bs, info, true);
            foreach(var id in keys)
            {
                if (bs[id])
                {
                    UpdateFileProcess(bs, info);
                }
                var urlListLists = urls[id];
                foreach(var urlLists in urlListLists)
                {
                    if (bs[id])
                        break;
                    baseUrl = urlLists[0][..urlLists[0].LastIndexOf("/")];
                    //判断是否为上下视频
                    if (urlLists.Count==1)
                    {
                        if (await Download(urlLists[0], id))
                            bs[id] = true;
                    }
                    else
                    {

                    }
                }
                UpdateFileProcess(bs, info);
            }
            MakeLog(bs);
        }

        

        private async Task DownIyinghua(Data d)
        {
            HtmlAgilityPack.HtmlDocument doc = new();
            doc.LoadHtml(d.Html);
            var NameNode = doc.DocumentNode.SelectSingleNode("//div[@class='splay']/a");
            videoName = NameNode.GetAttributeValue("title", "");
            var DesNode = doc.DocumentNode.SelectSingleNode("//div[@class='info']");
            string description = DesNode.InnerText;
            string picUrl = GetBetweenString(d.Html, "var bdPic = \"", "\"");
            await DownloadJpeg(picUrl);
            var pageNodes = doc.DocumentNode.SelectNodes("//div[@class='movurl']/ul/li/a");
            string pageBase = GetUriBase(new Uri(d.Addr));
            var bs = new Dictionary<string, bool>();
            List<string> keys = new();
            List<string> pages = new();
            foreach(var pn in pageNodes)
            {
                string id = CopeEpi(pn.InnerText);
                bs[id] = false;
                keys.Add(id);
                pages.Add(UrlWithBase(pn.GetAttributeValue("href",""), pageBase));
            }
            DownInfo info = new(videoName, bs);
            info.AddUrl(d.Addr);
            
            if (TryLoadInfo(out DownInfo? info2))
            {
                info.ImPortData(info2??new());
            }
            bs = new(info.DownState);
            info.Intro = description;
            info.Name = videoName;
            UpdateFileProcess(bs, info);
            for (int i = 0; i < pages.Count; i++)
            {
                string id = keys[i];
                if (bs[id])
                {
                    UpdateFileProcess(bs,info);
                    continue;
                }
                string? page = pages[i];
                string html = await GetHtml(page);
                doc.LoadHtml(html);
                var urlNode = doc.DocumentNode.SelectSingleNode("//div[@class='play']/div/div/div");
                string url = urlNode.GetAttributeValue("data-vid", "");
                if(url.Contains("m3u8"))
                {
                    int idx = url.IndexOf("$");
                    url = url[..idx];
                }
                baseUrl = url[..url.LastIndexOf('/')];
                if (!bs[keys[i]] && await Download(url, keys[i]))
                    bs[keys[i]] = true;
                UpdateFileProcess(bs, info);
            }
        }

        private async Task DownNico(Data d)
        {
            HtmlAgilityPack.HtmlDocument doc = new();
            doc.LoadHtml(d.Html);
            var pageNodes = doc.DocumentNode.SelectNodes("//a[@class='btn btn-default btn-block btn-sm ff-text-hidden']");
            Dictionary<string, bool> bs = new();
            List<string> keys = new();
            List<string> pages = new();
            string pageBase = GetUriBase(new Uri(d.Addr));
            foreach (var pageNode in pageNodes)
            {
                string key = pageNode.InnerText;
                key = CopeEpi(key);
                  keys.Add(key);
                    bs[key] = false;
                string page = pageNode.GetAttributeValue("href", "");
                pages.Add(UrlWithBase(page, pageBase));
            }
            //获取名称和介绍
            videoName = doc.DocumentNode.SelectSingleNode("//a[@class='ff-text']").InnerText;
            var descriptionNodes = doc.DocumentNode.SelectSingleNode("//meta[@name = 'description']");
            DownInfo info = new(videoName, bs)
            {
                Name = videoName,
                Intro = descriptionNodes.GetAttributeValue("content", "")
            };
            info.AddUrl(d.Addr);
            if (TryLoadInfo(out DownInfo? info2))
                info.ImPortData(info2 ?? new());
            bs = new(info.DownState);
            UpdateFileProcess(bs, info, true);
            //开始下载
            for (int i = 0; i < pages.Count; i++)
            {
                string? page = pages[i];
                string id = keys[i];
                if (bs[id])
                {
                    UpdateFileProcess(bs, info);
                    continue;
                }
                string pageHtml = await GetHtml(page);
                doc.LoadHtml(pageHtml);
                var srcNode = doc.DocumentNode.SelectSingleNode("//script[contains(@src,'player.php')]");
                string playerUrl = srcNode.GetAttributeValue("src", "");
                Dictionary<string, string> headers = new()
                {
                    {"Proxy-Connection","keep-alive" },
                    {"Referer",page }
                };
                string html = await GetHtml(UrlWithBase(playerUrl, pageBase), headers);
                string wapUrl = Regex.Match(html, "url\":\"([^\"]+)\"").Groups[1].Value.Replace("\\", "");
                string wapHtml = await GetHtml(wapUrl);
                var match = Regex.Match(wapHtml,
                    "player\\(\\)\\{\\$\\.([^.\\(\\)\\[\\]\\{\\}]+)\\(\\\"([^\\\"]+)\\\",\\s(\\{[^{}]+\\})");
                //string apiFunction = match.Groups[1].Value;
                string apiPhp = match.Groups[2].Value;

                string apiUrlBase = GetUriBase(new Uri(wapUrl)) + ":8022";
                string codeText = match.Groups[3].Value;
                codeText = Regex.Replace(codeText, "(key\\\"\\s?:\\s?)([^,]+),", "$1\"$2\",");
                var JsonObj = JsonDocument.Parse(codeText);
                Dictionary<string, string> postContent = new();
                JsonElement root = JsonObj.RootElement;
                Dictionary<string, string> apiHeaders = new()
                {
                    {"Host",new Uri(apiUrlBase).Host+":8022"  },
                    {"Referer",wapUrl },
                    {"Origin",apiUrlBase },
                    {"Proxy-Connection","keep-alive" },
                    {"X-Requested-With","XMLHttpRequest" },
                };
                foreach (var key in root.EnumerateObject())
                {
                    if (key.Name != "key")
                    {
                        string vt = key.Value + "";
                        postContent[key.Name] = key.Value + "";
                    }
                    else
                    {
                        var keyCode = Regex.Match(wapHtml, "eval\\(\\\"([^\\\"]+)\\\"\\)").Value;
                        keyCode = TransAscii(keyCode);
                        keyCode = Regex.Match(keyCode, "\\.val\\(\\'([^']+)'\\)").Groups[1].Value + "stvkx2019";
                        postContent["key"] = GenerateMD5Hex(keyCode);
                    }
                }
                using var httpClient = HttpClientWithHeader(apiHeaders);
                string apiUrl = UrlWithBase(apiPhp, apiUrlBase);
                var response = await httpClient.PostAsync(apiUrl,
                    new FormUrlEncodedContent(postContent));
                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
                string VideoData = await response.Content.ReadAsStringAsync();
                string url = Regex.Match(VideoData, "url\":\"([^\"]+)\"").Groups[1].Value.Replace("\\", "");
                if (await Download(url, keys[i]))
                {
                    bs[id] = true;
                }
                UpdateFileProcess(bs, info);

            }
        }



        #endregion


        #region 函数
        /// <summary>
        /// 初始化变量
        /// </summary>
        private void RestVar()
        {
            encKey = Array.Empty<byte>(); //加密密码
            //AesType = 128;
            videoName = "";//名称
            tsCompleted = 0;
            timeWatch = new();
            baseUrl = "";
            iv = new byte[16];
            Single = false; //是否为剧场版;
            ReleaseSem();
        }

        /// <summary>
        /// 初始化设置变量
        /// </summary>
        /// <param name="data">视频数据</param>

        private void SetVar(Data data)
        {
            CurData = data;
            Single = cinmel.Checked || (data.Single);
            
        }

        /// <summary>
        /// 释放并发
        /// </summary>
        /// <param name="num">释放数量</param>
        private void ReleaseSem(int num = 999)
        {
            int canRel = semNum - semaphore.CurrentCount;
            if (canRel <= 0)
                return;
            semaphore.Release(num > canRel ? canRel : num);
        }

        


        private async Task DownAsync()
        {
            allP.Value = 0;
            Down.Enabled = false;
            allP.Maximum = datas.Count;
            timeWatch.Start();
            for (int i = 0; i < datas.Count; i++)
            {
                Data? d = datas[i];
                RestVar();
                SetVar(d);
                if (i < videoList.Items.Count)
                    videoList.SelectedIndex = i;
                videoP.Value = 0;
                allP.Maximum = videoList.Items.Count;
                allPl.Text = "总进度:" + $"{FormatNumber(0, allP.Maximum)}/{allP.Maximum}";

                
                    switch (d.Domain)
                    {
                        
                        case ("mengdaow"):
                            await DownMengdao(d);
                            break;
                        case ("mindaveld"):
                            await DownMindaveld(d);
                            break;
                        case ("mgtv123"):
                            await DownMg123(d);
                            break;
                        case ("chunhuibaojie"):
                            await DownChunhuibaojie(d);
                            break;
                        case ("lffun"):
                            await Downlffun(d);
                            break;
                        case ("7qhb"):
                            await Down7qhb(d);
                            break;
                        case ("youtube"):
                            await DownYoutube(d);
                            break;
                        case ("iyinghua"):
                            await DownIyinghua(d);
                            break;
                        case ("nicotv"):
                            await DownNico(d);
                            break;
                        case ("xdm4"):
                            await DownXdm4(d);
                            break;
                        default:
                            break;
                    }
                allInfo.AddVote(GetUriBase(d.Addr));
                allInfo.AddData(videoName, MakeInfoPath(), d.Addr);
                UpdateAllPro(i);
            }
            videoList.SelectedIndex = -1;
            datas.Clear();
            allP.Maximum = 1;
            allP.Value = 1;
            videoList.Items.Clear();
        }

       

        private void UpdateAllPro(int num)
        {
            if (num < 0) num = allP.Value;
            allPl.Text = "总进度:" + $"{FormatNumber(num, allP.Maximum)}/{allP.Maximum}";
            allP.Value = num;
        }
        

        private async Task<bool> Download(string url, string id = "", Dictionary<string, string>? header = default, bool useProxy = false)
        {
            videoName = CopeFilePath(videoName);
            header ??= new();
            if (url.Contains("m3u8"))
                return await DownM3u8(url, id, header, useProxy||Config.NeedProxy((CurData??new()).Domain));
            else
                return await DownLoadFile(url, id, header, useProxy||Config.NeedProxy((CurData??new()).Domain));
        }

        

        private bool TryLoadInfo(out DownInfo? info)
        {
            info = new();
            if (Single)
                return false;
            string outPath = Config.GetOutPath( "normal", "anime");
            string file = outPath + videoName + "/状态.json";
            if (!File.Exists(file)) 
                return false;
            string? jsonStr = File.ReadAllText(file);
            try
            {
                info = JsonConvert.DeserializeObject<DownInfo?>(jsonStr);
            }
            catch
            {
                Debug.WriteLine("方法:TryLoadInfo,转换info失败，1736");
                return false;
            }
            return true;
        }

        private async Task DownloadJpeg(string url, string suffix = "jpg", Dictionary<string ,string>? headers = null,bool useProxy = false)
        {
            string path = MakePath("封面");
            path = path.Replace("mp4", suffix);
            if (File.Exists(path)) return;
            headers ??= new();
            var dl = new ParallelDownloader(headers, useProxy);
            await dl.DownloadPngAsync(url, path);
        }

        private void MakeLog(Dictionary<string, bool> bs)
        {
            foreach (var b in bs)
            {
                if (!b.Value)
                    MakeLog($"{videoName}-{b.Key}·下载失败了");
            }
        }

        private void MakeLog(string log)
        {
            string path = $"Log/{runTime}.txt";
            if (!Directory.Exists("Log")) Directory.CreateDirectory("Log");
            logList.Add(log);
            File.WriteAllLines(path,logList);
        }


        // 提取值的辅助方法

        private bool MoveFile(string id)
        {
            string target = MakePath(id);
            string outputPath = "File/" + videoName + "-" + FormatId(id) + "-temp.mp4";
            string path = Path.GetDirectoryName(target) ?? "";
            if(path.Length == 0) { Debug.WriteLine("MoveFile:获取目录失败,1777"); return false; }
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            File.Copy(outputPath, target, false);
            File.Delete(outputPath);
            return true;
        }

        

       
        private byte[] DecryptAES(byte[] encryptedData)
        {
            //如果密钥长度为0，说明没有加密;
            if(encKey.Length == 0)
            {
                return encryptedData;
            }
            using var aesAlg = Aes.Create();
            aesAlg.Key = encKey; 
            aesAlg.Mode = CipherMode.CBC; // 根据加密方式设置模式
            aesAlg.Padding = padding; // 根据填充方式设置填充模式
            aesAlg.IV = iv;
            using MemoryStream msDecrypt = new();
            using (CryptoStream csDecrypt = new(msDecrypt, aesAlg.CreateDecryptor(), CryptoStreamMode.Write))
            {
                csDecrypt.Write(encryptedData, 0, encryptedData.Length); // 解密数据，从第16字节开始
                csDecrypt.FlushFinalBlock();
            }
            return msDecrypt.ToArray();
        }

        private string DecryptAES(string encrytedString)
        {
            byte[] encrytedData = Encoding.UTF8.GetBytes(encrytedString);
            byte[] decrytedData = DecryptAES(encrytedData);
            return Encoding.UTF8.GetString(decrytedData);
        }

        

        private static HttpClient HttpClientWithHeader(Dictionary<string, string>? headers = default, bool useProxy = false)
        {


            HttpClient http = new(new HttpClientHandler { UseProxy = useProxy });
            http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36 Edg/125.0.0.0");
            headers ??= new();
            foreach (var h in headers)
            {
                http.DefaultRequestHeaders.Add(h.Key, h.Value);
            }

            return http;
        }
        private static Dictionary<string, object> ConvertJson(string json)
        {
            Dictionary<string, object>? jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            return jsonObject??new();
        }

        static string TransAscii(string str)
        {
            string pat = "\\\\x([0-9a-fA-F]{2})";
            str = Regex.Replace(str, pat, (match) =>
            {
                try
                {
                    string str = Encoding.ASCII.GetChars(new byte[] { (byte)Convert.ToInt32(match.Groups[1].Value, 16) })[0].ToString();
                    return str;
                }
                catch
                {
                    Console.WriteLine("糟糕");
                    return match.Value;
                }
            });
            return str;
        }
        /// <summary>
        /// 处理目录中的非法字符
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        
        private void UpdateFileProcess(Dictionary<string, bool> bs, DownInfo info,bool save = false)
        {

            videoP.Maximum = bs.Count;
            int comePleteCount = bs.Count(kvp => kvp.Value);
            videoP.Value = comePleteCount;
            videoPl.Text = $"集数:{FormatNumber(videoP.Value, videoP.Maximum)}/{videoP.Maximum}";
            if (Single||(!save && info.DownState.SequenceEqual(bs))) return;
            info.DownState = bs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            string infoJson = JsonConvert.SerializeObject(info);
            string target = Config.GetOutPath( "normal", "anime") + "状态.json";
            if (File.Exists(target)) File.Delete(target);
            string path = Path.GetDirectoryName(target) ?? "";
            if(path.Length == 0) { Debug.WriteLine("UpdateFileProcess:获取目录失败,1874"); }
            if (!Directory.Exists(Path.GetDirectoryName(target))) Directory.CreateDirectory(path);
            File.WriteAllText(target, infoJson, Encoding.UTF8);

        }

        private void UpdateFileProcess(int num)
        {
            videoP.Maximum = 1;
            videoP.Value = num > 1 ? 1 : num;
            videoPl.Text = $"进度:{videoP.Value}/1";
        }
        
        
        private async Task<bool> DownLoadFile(string url, string id, Dictionary<string, string> parms, bool useProxy = false)
        {
            string target = MakePath(id);
            string? path = Path.GetDirectoryName(target);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path ?? "");
            if (File.Exists(target)) return true;
            string outputPath = "File/" + videoName + "-" + FormatId(id) + "-temp.mp4";
            ParallelDownloader downloader = new(parms, useProxy);
            if (!await downloader.DownloadFileAsync(url, outputPath, downP, downSl, downPl))
                return false;
            MoveFile(id);
            return true;

        }

        private static async Task<string> GetHtml(string url, Dictionary<string, string>? header = default, bool useProxy = false)
        {
            header ??= new();
            using HttpClient client = HttpClientWithHeader(header, useProxy);
            string content = "";
            int time = 0;
            HttpResponseMessage? response = null;
            while (content.Length < 1 && time < Config.MAX_TIME)
            {
                time++;
                try
                {
                    response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    content = await response.Content.ReadAsStringAsync();
                }
                catch
                {
                    if (response != null && ((int)response.StatusCode == 301 || (int)response.StatusCode == 302))
                    {
                        if (response.Headers != null && response.Headers.Location != null)
                            url = response.Headers.Location.ToString();
                    }
                    else
                    {
                        if (time >= Config.MAX_TIME-1)
                        {
                            await Task.Delay(1000);
                            Debug.WriteLine("获取html失败:" + url);
                        }
                    }
                }

            }
            return content;
        }


        #endregion
        #region 处理字符串方法
        private string MakeInfoPath()
        {
            return Single ? MakePath() : Regex.Match(MakePath("1"), "(.*/)").Groups[1].Value;
        }
        private static string GetUriBase(string url) => GetUriBase(new Uri(url));
        private static string GetUriBase(Uri uri) => uri.Scheme + "://" + uri.Host;

        
        /// <summary>
        /// 格式化浮点数，应该是用在downM3u8的速度中 不知道，没用过，可能会用
        /// </summary>
        /// <param name="number"></param>
        /// <param name="targetLength"></param>
        /// <returns></returns>
        public static string FormatFloat(float number, int targetLength)
        {
            string numberSTr = number.ToString("0.##");
            var group = numberSTr.Split('.');
            int lefePaddingLegnth = targetLength - group[0].Length;
            string result = new string(' ', lefePaddingLegnth) + group[0];
            if (group.Length > 1)
            {
                result = result + "." + group[1] + new string(' ', 2 - group[1].Length);
            }
            else
                result += "   ";
            return result;
        }
        /// <summary>
        /// 去除目录种的非法字符
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string CopeFilePath(string path)
        {
            string result = Regex.Replace(path, "[\\|]", "｜");
            result = result.Replace("*", "x").Replace("<", "《").Replace(">", "》").Replace("?", "？");
            result = Regex.Replace(result, "\"([^\"]+)\"", "“$1”");
            return result;
        }
        private static string CopeEpi(string epiText)
        {
            string result = CopeFilePath(epiText);
            var matches = Regex.Matches(result, "第(.*)[集话]");
            if (matches.Count > 0)
                result = matches[0].Groups[1].Value;
            if (int.TryParse(result, out int tint))
                result = tint.ToString();
            result = result.Replace(" ", "");
            return result;

        }


        /// <summary>
        /// 格式化数字，主要是前面加0
        /// </summary>
        /// <param name="number"></param>
        /// <param name="target"></param>
        /// <returns></returns>

        public static string FormatNumber(int number, int target)
        {
            string numberStr = number.ToString();
            int paddingLength = target.ToString().Length - numberStr.Length;

            if (paddingLength <= 0)
            {
                return numberStr;
            }
            else
            {
                return new string(' ', paddingLength) + numberStr;
            }
        }

        /// <summary>
        /// 格式化集数
        /// </summary>
        /// <param name="id"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private static string FormatId(string id, int length = 2)
        {
            if (!int.TryParse(id, out int uid))
                return id;
            string zeroPadding = "";
            for (int i = length - uid.ToString().Length; i > 0; i--) zeroPadding += "0";
            return $"第{zeroPadding}{uid}集";

        }
        private string MakePath(string id = "")
        {
            string key1 ="normal";
            string key2 = Single ? "video" : "anime";
            return $"{Config.GetOutPath(key1, key2)}{videoName}-{id}.mp4";
        }

        public static string GenerateMD5Hex(string inputString)
        {
            // Create an MD5 hash provider
            using MD5 md5 = MD5.Create();
            // Convert the input string to a byte array
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputString);

            // Compute the MD5 hash
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the hash bytes to a hexadecimal string
            StringBuilder sb = new();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
        /// <summary>
        /// 组装Url，避免了重复字串
        /// </summary>
        /// <param name="url"></param>
        /// <param name="baseUrlText"></param>
        /// <returns></returns>
        private static string UrlWithBase(string url, string baseUrlText)
        {
            //检测是否是已经完成的链接，如果是，就返回
            if (url.IndexOf("http") == 0)
                return url;
            //获取知道/的字符串用于检测
            string key = Regex.Match(url, "(/?[^/\\s]+/?)").Groups[1].Value;
            int idx = baseUrlText.IndexOf(key);
            if (idx >= 0)
                return string.Concat(baseUrlText.AsSpan(0, idx), url);
            else
            {
                if (url[0] == '/') //检测是否有/
                    return (baseUrlText + url).Replace("\r", "");
                else
                    return  (baseUrlText + "/" + url).Replace("\r", "");
            }

            
        }

        private static string ConvertToPattern(string str)
        {
            return Regex.Replace(str, "([\\[\\]\\\\(){}.*+?@])", "\\$1");
        }

        private static string GetBetweenString(string str, string first, string sec)
        {
            string pattern = $"{ConvertToPattern(first)}(.*?){ConvertToPattern(sec)}";
            return Regex.Match(str, pattern).Groups[1].Value;
        }

        private static string FormatTsId(int num, int max)
        {
            int length = max.ToString().Length;
            string res = "";
            for (; length > num.ToString().Length; length--)
            {
                res += "0";
            }
            return res + num + ".ts";
        }


        public static string DecodeBase64(string base64EncodedData)
        {
            byte[] data = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(data);
        }

        #endregion







        #region M3U8下载


        /// <summary>
        /// 对M3U8进行下载
        /// </summary>
        /// <param name="url">链接</param>
        /// <param name="id">集数</param>
        /// <param name="header"></param>
        /// <returns>是否下载成功</returns>
        private async Task<bool> DownM3u8(string url, string id, Dictionary<string, string> header, bool useProxy)
        {
            string target = MakePath(id);
            if (baseUrl.Length < 1)
                baseUrl = url[..url.LastIndexOf('/')];
            if (File.Exists(target))
                return true;
            var list = await NeoGetM3u8Content(url, header, useProxy);
            CurData ??= new("");
            switch (CurData.Domain)
            {
                case ("mengdaow"):
                    baseUrl = list[0][..list[0].LastIndexOf('/')];
                    break;
                case ("iyinghua"):
                    baseUrl = list[0][..list[0].LastIndexOf("/")];
                    break;
            }

            if (!await M3u8ListStack(list, id, header, useProxy))
                return false;
            return MoveFile(id);
        }
        /// <summary>
        /// 获取M3U8，并将其转换成列表
        /// </summary>
        /// <param name="url">链接</param>
        /// <param name="header">请求头</param>
        /// <returns></returns>
        private async Task<List<string>> NeoGetM3u8Content(string url, Dictionary<string, string>? header = default, bool useProxy = false)
        {
            if (baseUrl.Length < 1)
                baseUrl = url[..url.LastIndexOf('/')];

            ReleaseSem();
            using HttpClient client = HttpClientWithHeader(header, useProxy);

            client.DefaultRequestHeaders.Add("authority", new Uri(url).Host);
            List<string> list = new();
            string datas = "";
            try
            {
                datas = await client.GetStringAsync(url);
            }
            catch (Exception e)
            {
                Debug.WriteLine("1065:获取M3u8失败:" + url);
                Debug.WriteLine(e.Message);
                return new List<string>();
            }
            var lines = datas.Split("\n");
            if (url.Contains("mix"))
            {
                baseUrl = "";
                var group = url.Split("/");
                for (int i = 0; i < group.Length - 2; i++)
                {
                    baseUrl += group[i] + "/";

                }
                baseUrl += group[^2];
            }
            string tsUrlBase = Regex.Match(url, "(.*)/").Groups[1].Value;
            foreach (var line in lines)
            {
                if (line.Contains("adjump")) continue;
                if (line.IndexOf(".m3u8") > -1)
                    list.Add(UrlWithBase(line, baseUrl) + urlSub);
                //被加密的情况
                if (!line.Contains(".ts") && line.Contains("AES"))
                {
                    //#EXT-X-KEY:METHOD=AES-128,URI="enc.key",IV=0x00000000000000000000000000000000
                    string keyUrl = baseUrl + Regex.Match(line, "URI=\"([^\"]+)\"").Groups[1].Value;
                    encKey = await client.GetByteArrayAsync(keyUrl);
                    string ivHex = GetBetweenString(line, "IV=", "");
                    int findex = ivHex.IndexOf("0x");
                    if (findex > -1)
                    {
                        ivHex = ivHex.Substring(findex + 2, ivHex.Length - 2 - findex);
                        iv = Enumerable.Range(0, ivHex.Length)
                        .Where(x => x % 2 == 0)
                        .Select(x => Convert.ToByte(ivHex.Substring(x, 2), 16))
                        .ToArray();
                    }
                    else
                        iv = new byte[16];

                }
                if (line.IndexOf(".ts") > -1)
                {
                    list.Add(UrlWithBase(line, tsUrlBase));
                }
            }

            return list;
        }

        

        /// <summary>
        /// 下载TS文件
        /// </summary>
        /// <param name="url">链接</param>
        /// <param name="header">请求头</param>
        /// <param name="id">标号</param>
        /// <returns></returns>
        private async Task<bool> DownTs(string url, Dictionary<string, string> header, string id, bool useProxy = false)
        {
            int maxTime = 10;
            byte[] data = Array.Empty<byte>();

            while ((maxTime) > 0 && data.Length == 0)
            {
                try
                {
                    HttpResponseMessage response = await HttpClientWithHeader(header, useProxy).GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    data = await response.Content.ReadAsByteArrayAsync();
                    if (!Directory.Exists("m3u8")) Directory.CreateDirectory("m3u8");
                    string path = "m3u8/" + id;
                    if (encKey.Length==0)
                    {
                        File.WriteAllBytes(path, data);
                    }
                    else if(encKey.Length!=0&&encKey.Length!=16)
                    {
                        Debug.WriteLine("密码有错误");
                        return false;
                    }
                    else
                    {
                        byte[] decryData = DecryptAES(data);
                        File.WriteAllBytes(path, decryData);
                    }

                }
                catch (Exception e)
                {
                    if (maxTime-- > 1)
                        continue;
                    Debug.WriteLine(e.Message);
                    Debug.WriteLine("ts下载失败了:" + id + " url: " + url);
                    return false;
                }

            }
            

            
            ReleaseSem(1);
            try
            {
                downP.Value++;
            }
            catch (Exception e)
            {
                Debug.WriteLine("超限");
                Debug.WriteLine(e.Message);
            }
            downPl.Text = "进度:" + downP.Value + "/" + downP.Maximum;
            tsCompleted++;
            return true;
        }

        /// <summary>
        /// 对M3U8 list进行迭代，直到找到最终的M3U8。如果是最终的m3u8，就下载ts。
        /// </summary>
        /// <param name="list">列表</param>
        /// <param name="header">请求头</param>
        /// <param name="id">集数</param>
        /// <returns></returns>
        private async Task<bool> M3u8ListStack(List<string> list, string id, Dictionary<string, string> header, bool useProxy)
        {
            if (list.Count < 1) return false;
            bool tsb = list[0].Contains(".ts");
            bool s = true;
            var tasks = new List<Task<bool>>();
            timeWatch.Restart();
            int complete = 0;
            tsCompleted = 0;
            downP.Value = 0;
            ReleaseSem();
            int listCount = list.Count;
            for (int listIndex = 0; listIndex < listCount; listIndex++)
            {
                if (timeWatch.Elapsed.TotalSeconds > 0.5)
                {
                    float speed = (tsCompleted - complete) / (float)timeWatch.Elapsed.TotalSeconds;
                    string speedText = $"速度:{FormatFloat(speed,5)}/s";
                    downSl.Text = speedText;
                    complete = tsCompleted;
                    timeWatch.Restart();
                }

                string? link = list[listIndex];
                await semaphore.WaitAsync();
                foreach (Task<bool> t in tasks)
                {
                    if (t.IsCompleted && !t.Result)
                    {
                        tasks.Clear();
                        return false;
                    }
                }
                if (!(link.IndexOf(".ts") > -1))
                {
                    s = s && await M3u8ListStack(await NeoGetM3u8Content(link, header, useProxy), id, header, useProxy);
                }
                else
                {

                    downP.Maximum = list.Count;
                    //下载list;
                    string tsIdName = FormatTsId(listIndex, list.Count);

                    //添加任务
                    tasks.Add(DownTs(link, header, tsIdName));

                }



            }
            await Task.WhenAll(tasks);
            if (!s)
                return false;


            if (!Directory.Exists("File")) Directory.CreateDirectory("File");

            string path = videoName + "-" + FormatId(id);
            if (tsb && !await AssembleFile(path))
                return false;

            return true;



        }
        /// <summary>
        /// 组装文件
        /// </summary>
        /// <param name="max"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private async Task<bool> AssembleFile(string path)
        {
            string tempPath = "m3u8/" + path + ".ts";
            string outputPath = "File/" + path + "-temp.mp4";
            if (!Directory.Exists("m3u8"))
                return false;
            using (var outputStream = new FileStream(tempPath, FileMode.Create))
            {
                for (int i = 0; i < downP.Maximum; i++)
                {
                    string fileName = "m3u8/" + FormatTsId(i, downP.Maximum);
                    if (!File.Exists(fileName)) return false;
                    using var inputStream = new FileStream(fileName, FileMode.Open);
                    inputStream.CopyTo(outputStream);
                }
            }
            if (!await TranscodeVideo(tempPath, outputPath))
                return false;
            if (Directory.Exists("m3u8")) Directory.Delete("m3u8", true);
            return true;
        }

        public async Task<bool> TranscodeVideo(string inputFile, string outputFile)
        {
            await Task.Run(() =>
            {
                // 设置 FFmpeg 命令行参数
                if (File.Exists(outputFile)) File.Delete(outputFile);
                GlobalFFOptions.Configure(new FFOptions { BinaryFolder = ffmpegPath });
                FFMpegArguments
                .FromFileInput(inputFile, true)
                .OutputToFile(outputFile, true, options => options
                .WithVideoCodec(VideoCodec.LibX264)
                .WithAudioCodec(AudioCodec.Aac)
                .WithCustomArgument("-codec copy")
                .WithConstantRateFactor(28)
                .WithFastStart())
                .ProcessSynchronously();
            });
            Console.WriteLine("Transcoding completed.");
            if (File.Exists(outputFile)) return true;
            return false;
        }

        #endregion



    }

    //下载数据
    public class Data
    {

        //地址
        public string Addr { get; set; }
        //番剧名称
        public string Title { get; set; }
        //番剧源
        public string Domain { get; set; }
        public string Html { get; set; }
        public Dictionary<string,string> Headers { get; set; }
        public bool Single { get; set; }
        public string CookieD { get; set; }
        
        public Data(string address)
        {
            Title = "";
            Domain = "";
            Addr = address;
            Html = "";
            Single = false;
            CookieD = "";
            Headers = new();
            SetData();
        }

        public Data()
        {
            Title = "";
            Domain = "";
            Addr = "";
            Html = "";
            Single = false;
            CookieD = "";
            Headers = new();

        }

        private static string GetDomainFromUrl(string url)
        {
            if (url.Length < 1)
                return "";
            string domain = new Uri(url).Host;
            var group = domain.Split(".");
            return group[^2];
        }


        public void SetData()
        {
            Domain = GetDomainFromUrl(Addr);
            SetHeader();
            if (Config.Vote(Domain))
                GetTitle(Config.NeedProxy(Domain));
            else
                MessageBox.Show("不是支持的源");
        }

        private void SetHeader()
        {
            Dictionary<string, Dictionary<string, string>> headers =
            new()
            {

            };
            if (headers.ContainsKey(Domain))
                Headers = headers[Domain];



        }

        private async void GetTitle(bool useProxy)
        {

            int time = 10;
            while (time >= 10 && Title.Length < 1)
            {

                using HttpClient client = new(new HttpClientHandler { UseProxy = useProxy });
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36 Edg/125.0.0.0");
                foreach (var header in Headers)
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                try
                {

                    HttpResponseMessage response = client.GetAsync(Addr).Result;
                    if (response.Headers.TryGetValues("Set-Cookie", out var scv))
                    {
                        var cs = scv;
                    }


                    response.EnsureSuccessStatusCode();
                    Html = response.Content.ReadAsStringAsync().Result;
                    HtmlAgilityPack.HtmlDocument doc = new();
                    doc.LoadHtml(Html);
                    Title = doc.DocumentNode.SelectSingleNode("//title").InnerText;
                    return;
                }
                catch
                {
                    await Task.Delay(1000);
                    if (time == 1)
                        Debug.WriteLine("获取名称失败了");
                }
            }
        }
        
    }

    public class ParallelDownloader
    {
        private readonly HttpClient httpClient;

        public ParallelDownloader(Dictionary<string, string>? header = default, bool useProxy = false)
        {

            httpClient = new HttpClient(new HttpClientHandler { UseProxy = useProxy })
            {
                Timeout = TimeSpan.FromMinutes(20)
            };
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("User-Agent",
                   "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36 Edg/125.0.0.0");
            header ??= new();
            foreach (var v in header)
            {
                httpClient.DefaultRequestHeaders.Add(v.Key, v.Value);
            }
        }

        public static async Task DownloadFilesParallel(List<string> urls)
        {
            var tasks = new List<Task>();

            for (int i = 0; i < urls.Count; i++)
            {
                //var url = urls[i];
                //var fileName = Path.GetFileName(url);
                //var outputPath = Path.Combine(outputDirectory, fileName);

                //tasks.Add(DownloadFileAsync(url, outputPath));
            }

            await Task.WhenAll(tasks);
        }

        public async Task<bool> DownloadFileAsync(string url, string outputPath, ProgressBar pro, Label label, Label dl)
        {
            if (!Directory.Exists("File")) Directory.CreateDirectory("File");
            pro.Maximum = 10000;
            HttpResponseMessage? response = null;
            int time = 5;
            while (time-- > 0 && (response is null || !response.IsSuccessStatusCode))
            {
                try
                {
                    response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception e)
                {
                    if (time > 1) continue;
                    Debug.WriteLine(e.Message);
                    Debug.WriteLine($"DownloadFile() 文件下载失败了 {outputPath}");
                    Debug.WriteLine(url);
                }

            }
            if (response is null || !response.IsSuccessStatusCode)
            {
                Debug.WriteLine("1596:文件下载失败了：" + outputPath);
                Debug.WriteLine(url);
                return false;
            }
            var totalBytes = response.Content.Headers.ContentLength;
            var downloadedBytes = 0L;
            var stopwatch = Stopwatch.StartNew();

            using (var contentStream = await response.Content.ReadAsStreamAsync())
            {
                using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 8192, useAsync: true);
                var buffer = new byte[8192];
                var bytesRead = 0;
                var bytesPerSecond = 0L;
                var lastProgress = 0;

                while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                    downloadedBytes += bytesRead;
                    if (totalBytes.HasValue)
                    {
                        var progress = (int)Math.Round((double)downloadedBytes / totalBytes.Value * 10000);
                        if (progress != lastProgress)
                        {
                            var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                            bytesPerSecond = (long)(downloadedBytes / elapsedSeconds);
                            label.Text = FormatBytes(bytesPerSecond) + "/s";
                            lastProgress = progress;
                            dl.Text = FormatBytes(downloadedBytes) + "/" + FormatBytes((long)totalBytes);
                            pro.Value = progress;
                        }
                    }
                }
            }
            httpClient.Dispose();
            stopwatch.Stop();
            response.Dispose();
            return true;
        }
        public async Task<bool> DownloadPngAsync(string url, string outputPath)
        {
            if (!Directory.Exists("File")) Directory.CreateDirectory("File");
            HttpResponseMessage? response = null;
            int time = 5;
            while (time-- > 0 && (response is null || !response.IsSuccessStatusCode))
            {
                try
                {
                    response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                    Debug.WriteLine($"status:{response.StatusCode}");
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception e)
                {
                    if (time > 1) continue;
                    Debug.WriteLine(e.Message);
                    Debug.WriteLine($"DownloadFile() 文件下载失败了 {outputPath}");
                    Debug.WriteLine(url);
                }

            }
            if (response is null || !response.IsSuccessStatusCode)
            {
                Debug.WriteLine("1596:文件下载失败了：" + outputPath);
                Debug.WriteLine(url);
                return false;
            }
            var totalBytes = response.Content.Headers.ContentLength;
            var downloadedBytes = 0L;
            var stopwatch = Stopwatch.StartNew();

            using (var contentStream = await response.Content.ReadAsStreamAsync())
            {
                string path = Path.GetDirectoryName(outputPath) ?? "";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 8192, useAsync: true);
                var buffer = new byte[8192];
                var bytesRead = 0;
                var bytesPerSecond = 0L;
                var lastProgress = 0;

                while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                    downloadedBytes += bytesRead;
                    if (totalBytes.HasValue)
                    {
                        var progress = (int)Math.Round((double)downloadedBytes / totalBytes.Value * 10000);
                        if (progress != lastProgress)
                        {
                            var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                            bytesPerSecond = (long)(downloadedBytes / elapsedSeconds);
                            lastProgress = progress;
                        }
                    }
                }
            }
            stopwatch.Stop();
            response.Dispose();
            return true;
        }

        public static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int suffixIndex = 0;
            double bytesRemaining = bytes;

            while (bytesRemaining >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                bytesRemaining /= 1024;
                suffixIndex++;
            }
            string code = bytesRemaining.ToString("0.##");
            var group = code.Split('.');
            int paddingLength = 4 - group[0].Length;
            string result = new string(' ', paddingLength) + group[0];
            if (group.Length > 1)
            {
                result = result + "." + group[1] + new string(' ', 2 - group[1].Length);
            }
            else
                result += "   ";
            return result + suffixes[suffixIndex];


        }
    }

    public class VideoInfo
    {
        public int Qua { get; set; }
        public string Url { get; set; }
        public VideoInfo(string url, int qua)
        {
            Url = url;
            Qua = qua;
        }
    }

    public static class Config
    {
        readonly static private string[] VoteDomain =
        {
            "42mv",
            "mengdaow",
            "lffun",
            "ledlmw",
            "mindaveld",
            "chuihuibaojie",
            "mgtv123",
            "7qhb",
            "youtube",
            "iyinghua",
            "nicotv",
            "xdm4",
        };


        readonly static public string[] DomainNeedProxy =
        {
            "youtube",
        };

       
        readonly static public Dictionary<string, Dictionary<string, string>> outPath = new()
        {
            {
                "data",new Dictionary<string, string>
                {
                    {"nokey","中转站/" },
                    {"root","F://共享2/" },
                }
            },
            {
                "normal",new Dictionary<string, string>
                    {
                    {"path","" },
                    {"anime","番剧/" },
                    {"video","视频/" },
                    }
            },
            
            
        };
        public const int MAX_TIME = 5;

        public static string GetOutPath(string key1,string key2)
        {
            if(outPath.TryGetValue(key1,out Dictionary<string,string>? dic))
            {
                if(dic.TryGetValue(key2,out string? value))
                {
                    if (value.Contains(':'))
                        return value;
                    else if (dic["path"].Contains(':'))
                        return dic["path"] + value;
                    else
                        return outPath["data"]["root"]+ dic["path"] + value;
                }
            }
            return outPath["data"]["root"] + outPath["data"]["nokey"];
        }

        /// <summary>
        /// 检测是否支持域名
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool Vote(string target)
        {
            return VoteDomain.Contains(target) ;
        }
        /// <summary>
        /// 检测是否需要代理
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool NeedProxy(string target)
        {
            return DomainNeedProxy.Contains(target);
        }



    }

    public class DownInfo
    {
        public string Name { get; set; }
        public Dictionary<string, bool> DownState = new();
        public string? Intro { get; set; }
        public Dictionary<string, int> Urls = new();
        public bool IsComplete = false;

        //启用
        public string Type { get; set; }

        public DownInfo(string name, Dictionary<string, bool> downState)
        {
            Name = name;
            Type = "";
            DownState = new Dictionary<string,bool>(downState);
            CheckUrl();
        }

        public DownInfo()
        {
            Name = "";
            Type = "";
            CheckUrl();
        }
        public DownInfo(Data d,string intro,Dictionary<string,bool> bs)
        {
            Type = "";
            Name = d.Title;
            AddUrl(d.Addr);
            this.Intro = intro;
            DownState = new(bs);
            CheckUrl();
        }

        public void AddUrl(string url)
        {
            if (!Urls.ContainsKey(url))
            {
                Urls[url] = 0;
            }
        }
        


        public void RemoveUrl(string url)
        {
            Urls.Remove(url);
        }
        
        private void CheckUrl()
        {
            foreach(var kv in Urls)
            {
                if (Urls[kv.Key] >= Config.MAX_TIME)
                    RemoveUrl(kv.Key);
            }
        }


#pragma warning disable CS8714 // 类型不能用作泛型类型或方法中的类型参数。类型参数的为 Null 性与 "notnull" 约束不匹配。
        private static void CopyDictionaryValues<TKey, TValue>(Dictionary<TKey, TValue> dictionary1, Dictionary<TKey, TValue> dictionary2)
#pragma warning restore CS8714 // 类型不能用作泛型类型或方法中的类型参数。类型参数的为 Null 性与 "notnull" 约束不匹配。
        {
            // 遍历 dictionary1 中的键值对
            foreach (var kvp in dictionary1)
            {
                // 将值复制到 dictionary2 中
                dictionary2[kvp.Key] = kvp.Value;
            }
        }

        public void ImPortData(DownInfo info)
        {
            CopyDictionaryValues(info.DownState, DownState);
            foreach (var kv in info.Urls)
                Urls[kv.Key] = kv.Value;
        }

        public bool CheckComplete()
        {
            return IsComplete && !DownState.ContainsValue(false);
        }

    }

    public class ConfigData
    {
        public List<string> voteWeb = new();
        public List<VideoData> DownHistory = new();
        

        public void AddDataFun(VideoData d)
        {
            DownHistory.Add(d);
            File.WriteAllText("下载数据.json",JsonConvert.SerializeObject(this));
        }
        public void AddData(string name,string path,List<string> urls)
        {
            foreach(var s in DownHistory)
            {
                if(s.name == name || s.path == path)
                {
                    foreach(var url in urls)
                    {
                        if (!s.urls.Contains(url))
                            s.urls.Add(url);
                    }
                    return;
                }
            }
            DownHistory.Add(new(name, path, urls));
            File.WriteAllText("下载数据.json", JsonConvert.SerializeObject(this));
        }
        public void AddData(string name, string path, string url)
        {
            foreach (var s in DownHistory)
            {
                if (s.name == name || s.path == path)
                {
                    if (!s.urls.Contains(url))
                            s.urls.Add(url);
                    return;
                }
            }
            DownHistory.Add(new(name, path, url));
            File.WriteAllText("下载数据.json", JsonConvert.SerializeObject(this));
        }
        public void AddVote(string url)
        {
            if (!voteWeb.Contains(url))
                voteWeb.Add(url);
        }

        public void AddData(VideoData d)
        {
            foreach(var ds in DownHistory)
            {
                if(ds.name == d.name || ds.path == d.path)
                {
                    foreach (var url in d.urls)
                        if (!ds.urls.Contains(url))
                            ds.urls.Add(url);
                    return;
                }
            }
            AddDataFun(d);
        }

        public class VideoData
        {
            public string name = "";
            public string path = "";
            public List<string> urls = new();

            public VideoData(string name, string path, string url)
            {
                this.name = name;
                this.path = path;
                this.urls.Add(url);
            }

            public VideoData(string name ,string path,List<string> urls)
            {
                this.name = name;
                this.path = path;
                this.urls.AddRange(urls);
            }
            public VideoData()
            {
                this.name = "";
                this.path = "";
                this.urls = new();
            }
        }
    }

}
