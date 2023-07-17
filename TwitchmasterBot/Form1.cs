using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Chrome;
//using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System.IO;

namespace TwitchmasterBot
{
    public partial class Form1 : Form
    {
        IWebDriver web;
        string PromoName;
        string HighestName;
        int CheckPeriod = (int)300e3;
        int sleeptime = (int)1e3;
        string login;
        string passwd;
        //int excSleep = (int)300e3;
        public Form1()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            var chromeDriverService = ChromeDriverService.CreateDefaultService(Directory.GetCurrentDirectory());
            var options = new ChromeOptions();
            options.AddArgument("mute-audio");
            chromeDriverService.HideCommandPromptWindow = true;
            web = new ChromeDriver(chromeDriverService, options);
            login = LoginBox.Text;
            passwd = PasswordBox.Text;


            try
            {
                web.Navigate().GoToUrl("https://twitchmaster.ru/");
                web.FindElement(By.XPath("/html/body/div[@class='wrapper']/div[@class='container'][1]/div[@class='header']/div[@class='links f-right']/span[@class='menu-content']/a[@class='ajax-popup']")).Click();
                Thread.Sleep(sleeptime);
                web.FindElement(By.XPath("/html/body/div[@id='popup-window']/div[@class='inner']/form/input[1]")).SendKeys(login);
                web.FindElement(By.XPath("/html/body/div[@id='popup-window']/div[@class='inner']/form/input[2]")).SendKeys(passwd);
                web.FindElement(By.XPath("/html/body/div[@id='popup-window']/div[@class='inner']/form/div[@class='center'][1]/div[@class='cool-button form-submitter']")).Click();
                ((IJavaScriptExecutor)web).ExecuteScript("window.open();");
                web.SwitchTo().Window(web.WindowHandles.Last());
                web.Navigate().GoToUrl("https://www.twitch.tv/");
            }
            catch (Exception)
            {
                Application.Exit();
            }
        }

        private void TwitchButton_Click(object sender, EventArgs e)
        {

            // закрываем все вкладки; переходим на твичмастер
            while (web.WindowHandles.Count != 1)
                web.Close();
            web.SwitchTo().Window(web.WindowHandles.First());
            if (web.Url != "https://twitchmaster.ru/")
            {
                web.Navigate().GoToUrl("https://twitchmaster.ru/");
            }
            System.Threading.Thread bonus = new System.Threading.Thread(new ThreadStart(BonusClick));
            bonus.Start();

            // записываем ники стримеров в промо и поднятии; начинаем просмотр
            PromoName = "";
            HighestName = "";
            OpenStreams();

            while (true)
            {
                WaitAndCheck();
            }

        }

        private void WaitAndCheck()
        {
            Thread.Sleep(CheckPeriod);  
            web.SwitchTo().Window(web.WindowHandles.First());
            web.Navigate().GoToUrl("https://twitchmaster.ru/");

            // Проверка совпадения текущего промо канала с просматриваемым
            //if ("KhodakLive" != web.FindElement(By.XPath("/html/body/div[@class='wrapper']/div[@class='promo-stream']/div[@class='container']/div[@class='semi-transparent-layer']/div[@class='sidebar-right']/div[@class='promo-header']/div[@class='name']/a")).Text && PromoName != web.FindElement(By.XPath("/html/body/div[@class='wrapper']/div[@class='promo-stream']/div[@class='container']/div[@class='semi-transparent-layer']/div[@class='sidebar-right']/div[@class='promo-header']/div[@class='name']/a")).Text)
            //{
            //    foreach (var tab in web.WindowHandles)
            //    {
            //        web.SwitchTo().Window(tab);
            //        if (!web.Url.Contains(HighestName.ToLower()) && tab != web.WindowHandles.First())
            //            web.Close();
            //    }
            //    PromoName = "";
            //    web.SwitchTo().Window(web.WindowHandles.First());
            //}

            // Проверяем, открыты ли стримы
            CheckIfClosed(PromoName);
            CheckIfClosed(HighestName);

            //while (web.WindowHandles.Count != 1)
            //{
            //    web.Close();
            //    web.SwitchTo().Window(web.WindowHandles.First());
            //}
            //if (web.Url != "https://twitchmaster.ru/")
            //{
            //    web.Navigate().GoToUrl("https://twitchmaster.ru/");
            //}

            OpenStreams();
        }

        // TODO проверить, начался ли заработок 
        private void OpenStreams()
        {
            web.SwitchTo().Window(web.WindowHandles.First());
            string[] HighestStreams = new string[3];
            HighestStreams[0] = "/html/body/div[@class='wrapper']/div[@class='container'][2]/div[@class='content-left']/div[@class='block'][1]/div[@class='streams-list']/div[@class='item vip super-vip']/div[@class='inner']/div[@class='viewers']/a";
            HighestStreams[1] = "/html/body/div[@class='wrapper']/div[@class='container'][2]/div[@class='content-left']/div[@class='block'][1]/div[@class='streams-list']/div[@class='item vip '][1]/div[@class='inner']/div[@class='viewers']/a";
            HighestStreams[2] = "/html/body/div[@class='wrapper']/div[@class='container'][2]/div[@class='content-left']/div[@class='block'][1]/div[@class='streams-list']/div[@class='item vip '][2]/div[@class='inner']/div[@class='viewers']/a[1]";

            web.Navigate().GoToUrl("https://twitchmaster.ru/");
            Actions action = new Actions(web);

            if (PromoName == "")
            {
                //IWebElement promo = web.FindElement(By.XPath("/html/body/div[@class='wrapper']/div[@class='promo-stream']/div[@class='container']/div[@class='semi-transparent-layer']/div[@class='sidebar-right']/div[@class='promo-header']/div[@class='name']/a"));
                //PromoName = promo.Text;

                //if (HighestName == PromoName)
                //{
                //    HighestName = "";
                //}
                //else
                //{
                //    if (PromoName == "KhodakLive")
                //    {
                try
                {
                    for (int i = 0; i < HighestStreams.Length; i++)
                    {
                        IWebElement tempPromo = web.FindElement(By.XPath(HighestStreams[i]));
                        if (tempPromo.Text != "KhodakLive" && tempPromo.Text != HighestName)
                        {
                            PromoName = tempPromo.Text;
                            action.KeyDown(OpenQA.Selenium.Keys.Control).MoveToElement(tempPromo).Click().Perform();

                            Thread.Sleep(sleeptime);
                            web.SwitchTo().Window(web.WindowHandles.First());
                            break;
                        }
                    }
                    //}
                    //else
                    //{
                    //    action.KeyDown(OpenQA.Selenium.Keys.Control).MoveToElement(promo).Click().Perform();

                    //    Thread.Sleep(sleeptime);
                    //    web.SwitchTo().Window(web.WindowHandles.First());
                    //}
                    //}
                    foreach (var tab in web.WindowHandles)
                    {
                        web.SwitchTo().Window(tab);
                        Thread.Sleep(sleeptime);
                        if (web.Url == "https://twitchmaster.ru/" + PromoName.ToLower() && web.Url != "https://twitchmaster.ru/")
                        {
                            if (!web.FindElement(By.XPath("/html/body/div[@class='wrapper']/div[@class='container']/div[@class='content-left']/div[@class='block'][2]/div[@class='warning']")).Text.Contains("Прямо сейчас вы зарабатываете кредиты"))
                            {
                                foreach (var tabP in web.WindowHandles)
                                {
                                    web.SwitchTo().Window(tabP);
                                    if ((!web.Url.Contains(HighestName.ToLower()) || HighestName == "") && tabP != web.WindowHandles.First())
                                        web.Close();
                                }
                                PromoName = "";
                                break;
                            }
                        }
                    }
                }
                catch (NoSuchElementException)
                {

                }
                web.SwitchTo().Window(web.WindowHandles.First());
            }

            if (HighestName == "")
            {
                try
                {
                    for (int i = 0; i < HighestStreams.Length; i++)
                    {
                        IWebElement highest = web.FindElement(By.XPath(HighestStreams[i]));
                        if (highest.Text != "KhodakLive" && highest.Text != PromoName)
                        {
                            HighestName = highest.Text;
                            action.KeyDown(OpenQA.Selenium.Keys.Control).MoveToElement(highest).Click().Perform();

                            Thread.Sleep(sleeptime);
                            web.SwitchTo().Window(web.WindowHandles.First());
                            break;
                        }
                    }

                    foreach (var tab in web.WindowHandles)
                    {
                        web.SwitchTo().Window(tab);
                        Thread.Sleep(sleeptime);
                        if (web.Url == "https://twitchmaster.ru/" + HighestName.ToLower() && web.Url != "https://twitchmaster.ru/")
                        {
                            if (!web.FindElement(By.XPath("/html/body/div[@class='wrapper']/div[@class='container']/div[@class='content-left']/div[@class='block'][2]/div[@class='warning']")).Text.Contains("Прямо сейчас вы зарабатываете кредиты"))
                            {
                                foreach (var tabP in web.WindowHandles)
                                {
                                    web.SwitchTo().Window(tabP);
                                    //Thread.Sleep(sleeptime);
                                    if ((!web.Url.Contains(PromoName.ToLower()) || PromoName == "") && tabP != web.WindowHandles.First())
                                        web.Close();
                                }
                                HighestName = "";
                                break;
                            }
                        }
                    }
                }
                catch (NoSuchElementException)
                {

                }
                web.SwitchTo().Window(web.WindowHandles.First());
            }
        }

        void CheckIfClosed(string name)
        {
            bool key = false;
            string compName;

            if (name == PromoName)
                compName = HighestName;
            else
                compName = PromoName;

            foreach (var tab in web.WindowHandles)
            {
                web.SwitchTo().Window(tab);
                Thread.Sleep(sleeptime);
                if (web.Url == "https://twitchmaster.ru/" + name.ToLower())
                {
                    key = true;
                    break;
                }
            }
            if (key == false)
            {
                foreach (var tab in web.WindowHandles)
                {
                    web.SwitchTo().Window(tab);
                    Thread.Sleep(sleeptime);
                    if ((!web.Url.Contains(compName.ToLower()) || compName == "") && tab != web.WindowHandles.First())
                    {
                        web.Close();
                    }
                    if (name == PromoName)
                        PromoName = "";
                    else
                        HighestName = "";
                }
            }
            web.SwitchTo().Window(web.WindowHandles.First());
        }

        private void BonusClick()
        {
            IWebDriver bonusweb;
            var chromeDriverService = ChromeDriverService.CreateDefaultService(Directory.GetCurrentDirectory());
            var options = new ChromeOptions();
            options.AddArgument("mute-audio");
            chromeDriverService.HideCommandPromptWindow = true;
            bonusweb = new ChromeDriver(chromeDriverService, options);

            bonusweb.Navigate().GoToUrl("https://twitchmaster.ru/");
            bonusweb.FindElement(By.XPath("/html/body/div[@class='wrapper']/div[@class='container'][1]/div[@class='header']/div[@class='links f-right']/span[@class='menu-content']/a[@class='ajax-popup']")).Click();
            Thread.Sleep(sleeptime);
            bonusweb.FindElement(By.XPath("/html/body/div[@id='popup-window']/div[@class='inner']/form/input[1]")).SendKeys(login);
            bonusweb.FindElement(By.XPath("/html/body/div[@id='popup-window']/div[@class='inner']/form/input[2]")).SendKeys(passwd);
            bonusweb.FindElement(By.XPath("/html/body/div[@id='popup-window']/div[@class='inner']/form/div[@class='center'][1]/div[@class='cool-button form-submitter']")).Click();

            int bonusTime = (int)3600e3;
            while (true)
            {
                bonusweb.Navigate().GoToUrl("https://twitchmaster.ru/static/profile");
                try
                {
                    //bonusweb.FindElement(By.XPath("/html/body/div[@class='wrapper']/div[@class='container']/div[@class='sidebar-right']/div[@id='get-daily-bonus']/div[@class='warning-2 center']/form/div[@class='cool-button alternative form-submitter']"));
                    bonusweb.FindElement(By.XPath("/html/body/div[@class='wrapper']/div[@class='container']/div[@class='sidebar-right']/div[@id='get-daily-bonus']/div[@class='warning-2 center']/form/div[@class='cool-button alternative form-submitter']")).Click();
                }
                catch (NoSuchElementException)
                {

                }
                Thread.Sleep(bonusTime);
            }
        }

    }
}
