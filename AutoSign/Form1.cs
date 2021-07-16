using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;
using mshtml;
using System.Web.UI;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Interactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.InteropServices;
using System.IO;
namespace AutoSign
{
    public partial class Form1 : Form
    {
        private string account = "";
        private string passwotd = "";
        private string LoginPage = "https://ap.shu.edu.tw/SSO/login.aspx?ReturnUrl=https://ap.shu.edu.tw/sso/default.aspx";
        private static string sign = "https://ap.shu.edu.tw/HR_LEAVE/HR_LEAVE100/HR_LEAVE0112_Leave_Computer_SignIn.aspx";
        private static string FILENAME = "record.txt";
        
        private bool early = false;
        private bool noon = false;
        private delegate void Enter();
        private Enter TriggerEnter;
        private bool afternoon = false;
        private int iniearlyTimeMin = 45;
        private int ininoonTimeMin = 00;
        private int iniafternoonTimeMin = 15;

        private string  earlyTime = "";
        private string noonTime = "";

        private string afternoonTime = "";
        private Form1 form;
       
        public Form1()
        {
         
            InitializeComponent();
            
            Form1.CheckForIllegalCrossThreadCalls = false;
            initilTime();
            form = this;
            System.Timers.Timer Timer = new System.Timers.Timer();

            TriggerEnter = new Enter(Form1_DoubleClick);
            Timer.Interval = 1000 * 60;
            Timer.Elapsed += (sender, args) => OnTimedEvent(sender);
            Timer.Start();


        }
        private void initilTime()
        {
            int minute = iniearlyTimeMin + Var();
            int early = 07;
            string minuteString = "";
            string hourString = "";
            if (minute >= 60)
            {
                early += 1;
                minute -= 60;
            }
            minuteString = minute.ToString();
            if (minute < 10)
            {
                minuteString = "0" + minute.ToString();
            }
            earlyTime = early + ":" + minuteString;

            minute = ininoonTimeMin + Var();
            int noon = 12;
            if (minute >= 60)
            {
                noon += 1;
                minute -= 60;
            }

            minuteString = minute.ToString();

            if (minute < 10)
            {
                minuteString = "0" + minute.ToString();
            }
            noonTime = noon + ":" + minuteString;

            minute = iniafternoonTimeMin + Var() + iniearlyTimeMin;
            int afternoon = 17;
            if (minute > 60)
            {
                afternoon += 1;
                minute -= 60;
            }
            minuteString = minute.ToString();
            if (minute < 10)
            {
                minuteString = "0" + minute.ToString();
            }
            afternoonTime = afternoon + ":" + minuteString;
            textBox2.Text = "早卡時間\n-\n" + earlyTime;
            textBox2.Text += " \r\n午卡時間\n-\n" + noonTime;
            textBox2.Text += " \r\n晚卡時間\n-\n" + afternoonTime;
        }
        private void OnTimedEvent(Object source)
        {


            if(!File.Exists(FILENAME))
            {
                File.Create(FILENAME);
            }

            StreamReader sr = new StreamReader(FILENAME);
           
            string now = DateTime.Now.ToString("HH:mm:ss");
            string[] nows = now.Split(':');

            string nowHH = nows[0];
            string nowMM = nows[1];
            string nowSS = nows[2];

            textBox1.Text = nowHH + ":" + nowMM + ":" + nowSS;

            List<string> setHH = new List<string>();
            setHH.Add(earlyTime.Split(':')[0]);
            setHH.Add(noonTime.Split(':')[0]);
            setHH.Add(afternoonTime.Split(':')[0]);

            List<string> setmm = new List<string>();
            setmm.Add(earlyTime.Split(':')[1]);
            setmm.Add(noonTime.Split(':')[1]);
            setmm.Add(afternoonTime.Split(':')[1]);

            var records = sr.ReadLine();
            var record = new string[0];

            if (records != null)
            {
                record = records.Split(',');
            }
            if (record.Length<3)//data not correct
            { 
                record = "0,0,0".Split(',');
            }
            sr.Close();
            StreamWriter sw = new StreamWriter(FILENAME);
            for (int i = 0; i < 3; i++)//3卡時間
            {

                if (int.Parse(nowHH) == int.Parse(setHH[i]) && int.Parse(nowMM) == int.Parse(setmm[i]) - 1)
                {
                    switch (i)
                    {
                        case 0:
                            //if(record[0]=="0")
                            early = true;

                            Login();
                            break;
                        case 1:
                            noon = true;
                            Login();
                            break;
                        case 2:
                            afternoon = true;
                            Login();
                            break;
                    }



                }
                if (int.Parse(nowHH) == int.Parse(setHH[i]) && nowMM == setmm[i])
                {
                    switch (i)
                    {
                        case 0:
                            early = true;
                            Sign();

                            record[0] = "1";
                            break;
                        case 1:
                            noon = true;
                            Sign();

                            record[1] = "1";
                            break;
                        case 2:
                            afternoon = true;
                            Sign();

                            record[2] = "1";
                            break;
                    }
                 
                }
                
            }

            switch (nowHH)
            {

                case "20":
                    if (nowMM == "30")
                    {
                        early = false;
                        noon = false;
                        afternoon = false;
                        initilTime();
                        record[0] = "0";
                        record[1] = "0";
                        record[2] = "0";

                    }
                    break;
            }
            var r = record[0] + "," + record[1] + "," + record[2];
            checkBoxMoring.Checked = record[0] == "1" ? true : false;
            checkBoxNoon.Checked = record[1] == "1" ? true : false;
            checkBoxAfterNoon.Checked = record[2] == "1" ? true : false;
            sw.WriteLine(r);
            sw.Close();
        }

        private int Var()
        {
            Random rand = new Random();
            return rand.Next(0, 15);
        }
        public void reFresh()
        {
            Application.Restart();
            /*
            form.Activate();
            //webBrowser1.Refresh();
            MouseOperations m = new MouseOperations();
            MouseOperations.SetCursorPosition(form.Location.X+2, form.Location.Y+2);
            MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftDown);
            MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftUp);

            MouseOperations.SetCursorPosition(1050, 620);
            MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftDown);
            MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftUp);
            //form.Invoke(TriggerEnter);
            //Form1_DoubleClick();
            aTimer.Stop();*/
            
            
        }
       
        public void Login()
        {

          
            Uri url = new Uri(LoginPage);
            webBrowser1.Url = url;
            //driver.Navigate().GoToUrl(LoginPage);
            //driver.Manage().Timeouts().PageLoad=(TimeSpan.FromSeconds(10));

            //driver.FindElement(By.Id("txtMyId")).SendKeys(account);
            // driver.FindElement(By.Id("txtMyPd")).SendKeys(passwotd);
            //driver.FindElement(By.Id("btnLogin")).Click();
            this.TopMost = true;
           
            webBrowser1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser_DocumentCompleted2);
          
           
            


        }
       
       
        void webBrowser_DocumentCompleted2(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            HtmlDocument doc = this.webBrowser1.Document;
            doc.GetElementById("txtMyId").SetAttribute("Value", account);
            doc.GetElementById("txtMyPd").SetAttribute("Value", passwotd);
            webBrowser1.Document.GetElementById("btnLogin").InvokeMember("Click");
           
            webBrowser1.DocumentCompleted -= webBrowser_DocumentCompleted2;
        }
        bool click = false;
        private static System.Timers.Timer aTimer;
        private void OnTimedEvent2(Object source, ElapsedEventArgs e)

        {
            //Form1_DoubleClick();
            reFresh();
           // aTimer.Stop();
        }
        //IWebDriver driver = new InternetExplorerDriver(); // assume assigned elsewhere
        
        void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //InjectAlertBlocker();
           // IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            aTimer = new System.Timers.Timer();
            aTimer.Interval = 10000;
            aTimer.Elapsed += OnTimedEvent2;
            aTimer.Start();

            try
            {
                webBrowser1.Document.GetElementById("btnExec").InvokeMember("Click");


                //this.Controls.Remove(webBrowser1);
                //webBrowser1.Stop();


                webBrowser1.DocumentCompleted -= webBrowser_DocumentCompleted;
            }
            catch(Exception ex)
            {
                //??? 尚未loading 完
            }
             
            
           


        }
      
        public void Form1_DoubleClick()
        {
            // Send the enter key to the button, which raises the click 
            // event for the button. This works because the tab stop of 
            // the button is 0.
            SendKeys.SendWait("{ENTER}");
        }
        private bool checkWeek()
        {
            
            bool[] weeks = new bool[6];
            weeks[0] = checkBox1.Checked;
            weeks[1] = checkBox2.Checked;
            weeks[2] = checkBox3.Checked;
            weeks[3] = checkBox4.Checked;
            weeks[4] = checkBox5.Checked;
            weeks[5] = checkBox6.Checked;
            DateTime dt = DateTime.Now;
            switch(dt.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    if(weeks[0])
                    {
                        return true;
                    }
                    break;
                case DayOfWeek.Tuesday:
                    if (weeks[1])
                    {
                        return true;
                    }
                    break;
                case DayOfWeek.Wednesday:
                    if (weeks[2])
                    {
                        return true;
                    }
                    break;
                case DayOfWeek.Thursday:
                    if (weeks[3])
                    {
                        return true;
                    }
                    break;
                case DayOfWeek.Friday:
                    if (weeks[4])
                    {
                        return true;
                    }
                    break;
                case DayOfWeek.Saturday:
                    if (weeks[5])
                    {
                        return true;
                    }
                    break;
            }
            return false;

        }
      
        public  void Sign()
        {
            
            this.TopMost = false;
            if (checkWeek())
            {
                Uri url = new Uri(sign);
                webBrowser1.Navigate(url);
                click = false;
                webBrowser1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser_DocumentCompleted);
            }
          
            
           


        }
        
        private void InjectAlertBlocker()
        {
            HtmlElement head = webBrowser1.Document.GetElementsByTagName("body")[0];
            HtmlElement scriptEl = webBrowser1.Document.CreateElement("script");
            IHTMLScriptElement element = (IHTMLScriptElement)scriptEl.DomElement;
            string alertBlocker = "window.alert = function () { }";
            element.text = alertBlocker;
            head.AppendChild(scriptEl);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Sign();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Login();
          
        }

        private void button3_Click(object sender, EventArgs e)
        {
            account = textBox3.Text;
            passwotd = textBox4.Text;
            textBox3.Enabled = false;
            textBox4.Enabled = false;
        }
    }
}
public class MouseOperations
{
    [Flags]
    public enum MouseEventFlags
    {
        LeftDown = 0x00000002,
        LeftUp = 0x00000004,
        MiddleDown = 0x00000020,
        MiddleUp = 0x00000040,
        Move = 0x00000001,
        Absolute = 0x00008000,
        RightDown = 0x00000008,
        RightUp = 0x00000010
    }

    [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetCursorPos(int X, int Y);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out MousePoint lpMousePoint);

    [DllImport("user32.dll")]
    private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

    public static void SetCursorPosition(int X, int Y)
    {
        SetCursorPos(X, Y);
    }

    public static void SetCursorPosition(MousePoint point)
    {
        SetCursorPos(point.X, point.Y);
    }

    public static MousePoint GetCursorPosition()
    {
        MousePoint currentMousePoint;
        var gotPoint = GetCursorPos(out currentMousePoint);
        if (!gotPoint) { currentMousePoint = new MousePoint(0, 0); }
        return currentMousePoint;
    }

    public static void MouseEvent(MouseEventFlags value)
    {
        MousePoint position = GetCursorPosition();

        mouse_event
            ((int)value,
             position.X,
             position.Y,
             0,
             0)
            ;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MousePoint
    {
        public int X;
        public int Y;

        public MousePoint(int x, int y)
        {
            X = x;
            Y = y;
        }

    }

}