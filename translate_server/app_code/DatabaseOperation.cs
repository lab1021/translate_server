using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using log4net;
using System.Threading;

namespace translate_server
{
    class DatabaseOperation
    {
        /// <summary>
        /// datebase path
        /// </summary>
        private string _databasefile;
        /// <summary>
        /// 该数据文件中文本数
        /// </summary>
        private int _sum;
        /// <summary>
        /// 正在翻译的文本Max id
        /// </summary>
        private int _translating;
        /// <summary>
        /// 已翻译的文本的Max id
        /// </summary>
        private int _translated;

        private string _lastdbfile;

        private XmlDocument Xmldoc;
        private XmlElement root;

        /// <summary>
        /// secret key
        /// </summary>
        private string skey = "10211022";

        /// <summary>
        /// 独立的备份线程
        /// </summary>
        private Thread backup;

        private static Mutex Read = new Mutex();
        private static Mutex Write = new Mutex();
        private static Mutex[] M = new Mutex[]{Read, Write};

        /// <summary>
        /// 日志控制器
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(typeof(DatabaseOperation));

        public string Databasefile
        {
            get { return _databasefile; }
            set { _databasefile = value; }
        }
        public int Sum
        {
            get { return _sum; }
        }
        public int Translating
        {
            get { return _translating; }
            set { _translating = value; }
        }
        public int Translated
        {
            get { return _translated; }
            set { _translated = value; }
        }

        public DatabaseOperation()
        {
            _databasefile = MainForm.ConfigOp.Db_current;
            _lastdbfile = MainForm.ConfigOp.Db_last;
            Xmldoc = new XmlDocument();
        }
        public void Run()
        {
            if (Loaddatabase(_databasefile))
            {
                backup = new Thread(Backup);
                backup.IsBackground = true;
                backup.Start();
            }
        }
        private bool Loaddatabase(string dbpath)
        {
            try
            {
                log.Info("begin to load database!");
                string input = "";
                DecryptFile(dbpath, ref input, skey);
                Xmldoc.LoadXml(input);
                root = Xmldoc.DocumentElement;
                _sum = Int32.Parse(root.GetAttribute("sum"));
                _translating = Int32.Parse(root.GetAttribute("translating"));
                _translated = Int32.Parse(root.GetAttribute("translated"));
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                return false;
            }
            log.Info("load database successfully!");
            return true;
        }
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public List<Corpus> Access_read(int num)
        {
            if (num < 0)
                return null;

            Read.WaitOne();
            List<Corpus> List_C = new List<Corpus>();
            if (num + _translating <= _sum)
            {
                for (int i = 0; i < num; i++)
                {
                    XmlElement tmp = (XmlElement)root.SelectSingleNode("/corpus/text[id=" + Convert.ToString(++_translating) + "]");
                    if (tmp != null)
                    {
                        string Checker = "";
                        string Id = tmp.GetElementsByTagName("id").Item(0).InnerText;
                        string State = tmp.GetElementsByTagName("state").Item(0).InnerText;
                        string Submit = "";
                        string Translator = "";
                        string Title = tmp.GetElementsByTagName("title").Item(0).InnerText;
                        string Body = tmp.GetElementsByTagName("body").Item(0).InnerXml;
                        List_C.Add(new Corpus(Id, Checker, Translator, Submit, Title, Body));

                        //reset state
                        tmp.GetElementsByTagName("state").Item(0).InnerText = "1"; //翻译中
                    }
                }
                root.SetAttribute("translating", _translating.ToString());
            }
            else
            {
                int last = _sum-_translating;
                if (last > 0)
                {
                    for (int i = 0; i < last; i++)
                    {
                        XmlElement tmp = (XmlElement)root.SelectSingleNode("/corpus/text[id=" + Convert.ToString(++_translating) + "]");
                        if (tmp != null)
                        {
                            string Checker = "";
                            string Id = tmp.GetElementsByTagName("id").Item(0).InnerText;
                            string State = tmp.GetElementsByTagName("state").Item(0).InnerText;
                            string Submit = "";
                            string Translator = "";
                            string Title = tmp.GetElementsByTagName("title").Item(0).InnerText;
                            string Body = tmp.GetElementsByTagName("body").Item(0).InnerXml;
                            List_C.Add(new Corpus(Id, Checker, Translator, Submit, Title, Body));

                            //reset state
                            tmp.GetElementsByTagName("state").Item(0).InnerText = "1"; //翻译中    
                        }
                    }
                    root.SetAttribute("translating", _translating.ToString());
                    MemoryStream input = new MemoryStream();
                    Xmldoc.Save(input);
                    EncryptFile(ref input, _databasefile, skey);

                    if (Reload())
                    {
                        for (int i = last; i < num; i++)
                        {
                            XmlElement tmp = (XmlElement)root.SelectSingleNode("/corpus/text[id=" + Convert.ToString(++_translating) + "]");
                            if (tmp != null)
                            {
                                string Checker = "";
                                string Id = tmp.GetElementsByTagName("id").Item(0).InnerText;
                                string State = tmp.GetElementsByTagName("state").Item(0).InnerText;
                                string Submit = "";
                                string Translator = "";
                                string Title = tmp.GetElementsByTagName("title").Item(0).InnerText;
                                string Body = tmp.GetElementsByTagName("body").Item(0).InnerXml;
                                List_C.Add(new Corpus(Id, Checker, Translator, Submit, Title, Body));

                                //reset state
                                tmp.GetElementsByTagName("state").Item(0).InnerText = "1"; //翻译中
                            }
                        }
                        root.SetAttribute("translating", _translating.ToString());
                    }
                }
                else if (Reload())
                {
                    for (int i = last; i < num; i++)
                    {
                        XmlElement tmp = (XmlElement)root.SelectSingleNode("/corpus/text[id=" + Convert.ToString(++_translating) + "]");
                        if (tmp != null)
                        {
                            string Checker = "";
                            string Id = tmp.GetElementsByTagName("id").Item(0).InnerText;
                            string State = tmp.GetElementsByTagName("state").Item(0).InnerText;
                            string Submit = "";
                            string Translator = "";
                            string Title = tmp.GetElementsByTagName("title").Item(0).InnerText;
                            string Body = tmp.GetElementsByTagName("body").Item(0).InnerXml;
                            List_C.Add(new Corpus(Id, Checker, Translator, Submit, Title, Body));

                            //reset state
                            tmp.GetElementsByTagName("state").Item(0).InnerText = "1"; //翻译中
                        }
                    }
                    root.SetAttribute("translating", _translating.ToString());
                }
                else
                    List_C = null;
            }
            Read.ReleaseMutex();
            return List_C;
        }
        public void Access_write(ref List<Corpus> List_C)
        {
            //考虑重新载入的问题，假设需要保存的数据已经不在缓冲区
            Write.WaitOne();
            foreach (Corpus item in List_C)
            {
                if (IsinBuffer(item.Id))
                {
                    XmlElement tmp = (XmlElement)root.SelectSingleNode("/corpus/text[id=" + item.Id + "]");
                    tmp.GetElementsByTagName("checker").Item(0).InnerText = item.Checker;
                    tmp.GetElementsByTagName("translator").Item(0).InnerText = item.Translator;
                    tmp.GetElementsByTagName("state").Item(0).InnerText = item.State;
                    tmp.GetElementsByTagName("submit").Item(0).InnerText = item.Submit;
                    tmp.GetElementsByTagName("body").Item(0).InnerXml = item.Body;
                    ++_translated;
                }
                else//数据不在缓冲池的特殊情况
                {
                    string dbpath = GetOldDatabasepath(item.Id);
                    XmlDocument Doctmp = new XmlDocument();
                    string input = "";
                    DecryptFile(dbpath, ref input, skey);
                    Doctmp.LoadXml(input);
                    XmlElement tmp_root = Doctmp.DocumentElement;
                    XmlElement tmp = (XmlElement)(tmp_root.SelectSingleNode("/corpus/text[id=" + item.Id + "]"));
                    tmp.GetElementsByTagName("checker").Item(0).InnerText = item.Checker;
                    tmp.GetElementsByTagName("translator").Item(0).InnerText = item.Translator;
                    tmp.GetElementsByTagName("state").Item(0).InnerText = item.State;
                    tmp.GetElementsByTagName("submit").Item(0).InnerText = item.Submit;
                    tmp.GetElementsByTagName("body").Item(0).InnerXml = item.Body;

                    int traned = Int32.Parse(tmp_root.GetAttribute("translated"))+1;
                    tmp_root.SetAttribute("translated", traned.ToString());
                    MemoryStream en_input = new MemoryStream();
                    Doctmp.Save((Stream)en_input);
                    EncryptFile(ref en_input, dbpath, skey);
                }
            }
            root.SetAttribute("translated", _translated.ToString());
            Write.ReleaseMutex();
        }
        /// <summary>
        /// 写回到硬盘，如果必要的话重新载入
        /// </summary>
        /// <returns></returns>
        public void Backup()
        {
            while (true)
            {                
                Mutex.WaitAll(M);
                MemoryStream input = new MemoryStream();
                Xmldoc.Save(input);
                EncryptFile(ref input, _databasefile, skey);
                if(_sum == _translating)
                {
                    Reload();
                }
                Read.ReleaseMutex();
                Write.ReleaseMutex();
                Thread.Sleep(300000);
                //Thread.Sleep(1000);
            }
        }
        private bool Reload()
        {
            string nextpath = GetNextDatabase(_databasefile);
            if (nextpath != null)
            {
                _databasefile = nextpath;
                Loaddatabase(nextpath);

                MainForm.ConfigOp.Reset_Db_current(_databasefile);
                return true;
            }
            return false;
        }

        private string GetNextDatabase(string pre)
        {
            int intpre = Int32.Parse(pre.Substring(pre.Length - 7, 3));
            int intlast = Int32.Parse(_lastdbfile.Substring(_lastdbfile.Length - 7, 3));
            if (intpre < intlast)
            {
                ++intpre;
                return pre.Substring(0, pre.Length - 7) + intpre.ToString() + ".xml";
            }
            return null;
        }

        private bool IsinBuffer(string corpus_id)
        {
            string databasename = _databasefile.Substring(_databasefile.Length - 7, 3);
            string Cname = corpus_id.Substring(0, 3);
            if (databasename.CompareTo(Cname) == 0)
            {
                return true;
            }
            return false;
        }
        private string GetOldDatabasepath(string corpus_id)
        {
            return _databasefile.Substring(0, _databasefile.Length - 7) + corpus_id.Substring(0, 3) + ".xml"; 
        }

        private void EncryptFile(ref MemoryStream fsInput, string sOutputFilename, string sKey)
        {
            FileStream fsEncrypted = new FileStream(sOutputFilename, FileMode.OpenOrCreate, FileAccess.Write);

            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            DES.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            DES.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
            ICryptoTransform desencrypt = DES.CreateEncryptor();
            CryptoStream cryptostream = new CryptoStream(fsEncrypted, desencrypt, CryptoStreamMode.Write);

            //fsInput.WriteTo(cryptostream);
            byte[] bytearrayinput = fsInput.ToArray();
            cryptostream.Write(bytearrayinput, 0, bytearrayinput.Length);
            cryptostream.Close();
            fsEncrypted.Close();
            fsInput.Close();
            //File.Copy(sOutputFilename + "b", sOutputFilename,true);
        }

        private void DecryptFile(string sInputFilename, ref string xml, string sKey)
        {
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            //A 64 bit key and IV is required for this provider.
            //Set secret key For DES algorithm.
            DES.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            //Set initialization vector.
            DES.IV = ASCIIEncoding.ASCII.GetBytes(sKey);

            //Create a file stream to read the encrypted file back.
            FileStream fsread = new FileStream(sInputFilename, FileMode.Open, FileAccess.Read);
            //Create a DES decryptor from the DES instance.
            ICryptoTransform desdecrypt = DES.CreateDecryptor();
            //Create crypto stream set to read and do a
            //DES decryption transform on incoming bytes.
            CryptoStream cryptostreamDecr = new CryptoStream(fsread, desdecrypt, CryptoStreamMode.Read);
            StreamReader reader = new StreamReader(cryptostreamDecr);
            xml = reader.ReadToEnd();
            reader.Close();
            fsread.Close();
        }

        public void Stop()
        {
            MemoryStream input = new MemoryStream();
            Xmldoc.Save(input);
            EncryptFile(ref input, _databasefile, skey);
            backup.Abort();
        }
    }
}
