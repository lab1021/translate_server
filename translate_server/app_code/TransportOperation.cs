using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using log4net;

namespace translate_server
{
    class TransportOperation
    {
        #region
        /// <summary>
        /// ip地址
        /// </summary>
        private string _ipaddress;
        /// <summary>
        /// 端口号
        /// </summary>
        private int _port;
        /// <summary>
        /// 最大挂起数
        /// </summary>
        private int _max_suspend;
        /// <summary>
        /// 最大线程数
        /// </summary>
        private int _max_thread;
        private static int _current_thread;
        private IPEndPoint _ipendp;
        private Socket server_socket;
        private static Thread listen;

        private static Hashtable LoginUser2Socket = new Hashtable();

        private DatabaseOperation DB = new DatabaseOperation();
        private UserOperation UserOP = new UserOperation();
        /// <summary>
        /// 日志控制器
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(typeof(TransportOperation));

        #endregion

        #region
        public string Ipaddress
        {
            get { return _ipaddress; }
        }
        public int Port
        {
            get { return _port; }
        }
        public int Max_suspend
        {
            get { return _max_suspend; }
            set { _max_suspend = value; }
        }
        public int Max_thread
        {
            get { return _max_thread; }
            set { _max_thread = value; }
        }
        #endregion

        #region
        public TransportOperation(string ipaddr, int port, int max_suspend,int max_thread)
        {
            _ipaddress = ipaddr;
            _port = port;
            _max_suspend = max_suspend;
            _max_thread = max_thread;
        }
        public void Run()
        {
            DB.Run();
            listen = new Thread(StartListen);
            listen.IsBackground = true; //??
            listen.Start();
        }

        private void StartListen()
        {
            try
            {
                _ipendp = new IPEndPoint(IPAddress.Any, _port);
                server_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                server_socket.Bind(_ipendp);
                server_socket.Listen(_max_suspend);
                _current_thread = 0;
                log.Info("start translation server successfully!");

                while (true)
                {
                    Socket client = server_socket.Accept();
                    if (client.Connected && _current_thread < _max_thread)
                    {
                        Thread client_thread = new Thread(new ParameterizedThreadStart(Interaction));
                        client_thread.IsBackground = true;
                        client_thread.Start(client);
                        ++_current_thread;
                    }
                    else
                    {
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                server_socket.Close();
            }
        }
        private void Interaction(object oclient)
        {
            int recv;
            byte[] data = new byte[1024];
            Socket client = (Socket)oclient;
            IPEndPoint clientip = (IPEndPoint)client.RemoteEndPoint;

            recv = client.Receive(data);
            string enter_info = Encoding.UTF8.GetString(data, 0, recv);

            UserInfo userinfo = new UserInfo();

            try
            {
                #region login
                if (enter_info.IndexOf("&#LOG") != -1)
                {
                    if (Get_userinfo(enter_info, ref userinfo))
                    {
                        //user has logined at somewhere
                        if (LoginUser2Socket.ContainsKey(userinfo.Id))
                        {
                            string confirm_info = "2";
                            data = Encoding.UTF8.GetBytes(confirm_info);
                            client.Send(data, SocketFlags.None);
                            recv = client.Receive(data);
                            string feedback = Encoding.UTF8.GetString(data, 0, recv);
                            if (feedback == "1")
                            {
                                Socket c = (Socket)LoginUser2Socket[userinfo.Id];
                                c.Dispose();
                                c.Close();
                                LoginUser2Socket.Remove(userinfo.Id);
                            }
                            client.Dispose();
                            client.Close();
                            return;
                        }

                        if (Authenticate(userinfo))
                        {
                            log.Info("client IP: " + clientip.Address + "client port: " + clientip.Port + "userid: " + userinfo.Id + " is connected\n");
                            string confirm_info = "1";
                            data = Encoding.UTF8.GetBytes(confirm_info);
                            client.Send(data, SocketFlags.None);

                            LoginUser2Socket[userinfo.Id] = client;

                            #region translator
                            if (userinfo.Power == power.translator)
                            {
                                while (true)
                                {
                                    data = new byte[1024 * 1024];
                                    recv = client.Receive(data);
                                    if (recv == 0)
                                    {
                                        break;
                                    }
                                    string tmp = Encoding.UTF8.GetString(data, 0, recv);
                                    if (tmp.IndexOf("&#GET") != -1)
                                    {
                                        try
                                        {
                                            int get_textnum = Int32.Parse(tmp.Substring(5));
                                            List<Corpus> list_C = DB.Access_read(get_textnum);
                                            if (list_C != null)
                                            {
                                                string texts = "&#";
                                                foreach (Corpus item in list_C)
                                                {
                                                    texts = texts +
                                                        item.Id + "::" + //text id
                                                         item.Title + "::" + //text title
                                                        item.Body + "&#"; //text body
                                                }
                                                byte[] corpusdata = Encoding.UTF8.GetBytes(texts);
                                                client.Send(corpusdata, SocketFlags.None);
                                            }
                                            else
                                            {
                                                client.Send(Encoding.UTF8.GetBytes("NO data to be translated!"), SocketFlags.None);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            LoginUser2Socket.Remove(userinfo.Id);
                                            client.Dispose();
                                            client.Close();
                                            log.Error(e.ToString());
                                        }
                                    }
                                    else if (tmp.IndexOf("&#POST") != -1)
                                    {   //protocol:&#POST(NUM)&#corpus1(id::checker::translator::title::body)&#corpus2()&#...&#
                                        string[] texts_data = tmp.Split(new string[] { "&#" }, StringSplitOptions.RemoveEmptyEntries);
                                        int post_textnum = Int32.Parse(texts_data[0].Substring(4));
                                        List<Corpus> list_c = new List<Corpus>();
                                        for (int i = 1; i < post_textnum+1; i++)
                                        {
                                            Corpus C_tmp = new Corpus();
                                            string[] items = texts_data[i].Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                                            C_tmp.Id = items[0];
                                            C_tmp.Checker = items[1];
                                            C_tmp.Translator = items[2];
                                            C_tmp.Title = items[3];
                                            C_tmp.Body = items[4];
                                            C_tmp.State = "2";
                                            C_tmp.Submit = DateTime.Now.ToString();
                                            list_c.Add(C_tmp);
                                        }
                                        DB.Access_write(ref list_c);
                                        UserOP.Update_Count(ref list_c, userinfo);
                                    }
                                }
                                LoginUser2Socket.Remove(userinfo.Id);
                                client.Dispose();
                                client.Close();
                            }
                            #endregion

                            #region administrator
                            else if (userinfo.Power == power.admin)
                            {
                                while (true)
                                {
                                    data = new byte[1024 * 1024];
                                    recv = client.Receive(data);
                                    if (recv == 0)
                                    {
                                        break;
                                    }
                                    string tmp = Encoding.UTF8.GetString(data, 0, recv);
                                    if (tmp.IndexOf("&#GET") != -1)
                                    {
                                        try
                                        {
                                            List<UserInfo> list_u = UserOP.GetAllInfo();
                                            if (list_u != null)
                                            {
                                                string users = "&#";
                                                foreach (UserInfo item in list_u)
                                                {
                                                    string Online = "0";
                                                    if (LoginUser2Socket.ContainsKey(item.Id))
                                                    {
                                                        Online = "1";
                                                    }
                                                    users = users +
                                                        item.Id + "::" + //id
                                                         item.Password + "::" + //password
                                                        ((int)item.Power) + "::" + //power
                                                        item.State+ "::" + 
                                                        item.Lastlogin + "::" +
                                                        item.Count + "::" +
                                                        Online + "&#"; //
                                                }
                                                data = Encoding.UTF8.GetBytes(users);
                                                client.Send(data, SocketFlags.None);
                                            }
                                            else
                                            {
                                                client.Send(Encoding.UTF8.GetBytes("NO data to be translated!"), SocketFlags.None);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            LoginUser2Socket.Remove(userinfo.Id);
                                            client.Dispose();
                                            client.Close();
                                            log.Error(e.ToString());
                                        }
                                    }
                                    else
                                    {
                                        //protocol:&#D(C)sum&#user1(id::password::power::state)&#user2()&#...&#
                                        string[] post_data = tmp.Split(new string[] { "&#" }, StringSplitOptions.RemoveEmptyEntries);
                                        char tag = post_data[0][0];
                                        int post_usernum = Int32.Parse(post_data[0].Substring(1));
                                        List<UserInfo> list_u = new List<UserInfo>();
                                        for (int i = 1; i < post_usernum+1; i++)
                                        {
                                            UserInfo U_tmp = new UserInfo();
                                            string[] li_item = post_data[i].Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                                            U_tmp.Id = li_item[0];
                                            U_tmp.Password = li_item[1];
                                            U_tmp.Power = (power)Int32.Parse(li_item[2]);
                                            U_tmp.State = li_item[3];
                                            list_u.Add(U_tmp);
                                        }
                                        
                                        switch (tag)
                                        {
                                            case 'D'://delete user info
                                                if (UserOP.Del_User(ref list_u))
                                                    client.Send(Encoding.UTF8.GetBytes("1"), SocketFlags.None);
                                                else
                                                    client.Send(Encoding.UTF8.GetBytes("0"), SocketFlags.None);
                                                break;
                                            case 'C'://disable user
                                                if (UserOP.Update_State(ref list_u))
                                                {
                                                    client.Send(Encoding.UTF8.GetBytes("1"), SocketFlags.None);
                                                }
                                                else
                                                    client.Send(Encoding.UTF8.GetBytes("0"), SocketFlags.None);
                                                break;
                                            default:
                                                break;
                                        }                                        
                                    }
                                }
                                LoginUser2Socket.Remove(userinfo.Id);
                                client.Dispose();
                                client.Close();
                            }
                            #endregion
                        }
                        else
                        {
                            string Error = "0";
                            data = Encoding.UTF8.GetBytes(Error);
                            client.Send(data, SocketFlags.None);
                            client.Dispose();
                            client.Close();
                        }
                    }
                    else
                    {
                        string Error = "0";
                        data = Encoding.UTF8.GetBytes(Error);
                        client.Send(data, SocketFlags.None);
                        client.Dispose();
                        client.Close();
                    }
                }
                #endregion

                #region user registration
                else if (enter_info.IndexOf("&#REG") != -1)
                {
                    if (Get_userinfo(enter_info, ref userinfo))
                    {
                        if (UserOP.Registration(userinfo))
                        {
                            string Success = "1";
                            data = Encoding.UTF8.GetBytes(Success);
                            client.Send(data, SocketFlags.None);
                            client.Dispose();
                            client.Close();
                        }
                        else
                        {
                            string fail = "0";
                            data = Encoding.UTF8.GetBytes(fail);
                            client.Send(data, SocketFlags.None);
                            client.Dispose();
                            client.Close();
                        }
                    }
                    else
                    {
                        string fail = "0";
                        data = Encoding.UTF8.GetBytes(fail);
                        client.Send(data, SocketFlags.None);
                        client.Dispose();
                        client.Close();
                    }
                }
                #endregion
            }
            catch (Exception exc)
            {
                client.Dispose();
                client.Close();
                log.Debug(exc.ToString());   
            }
        }


        /// <summary>
        /// get the user name and password from the string
        /// </summary>
        /// <param name="str">example:&#REG&#IDusera&#PW1234&#PO2&#</param>
        /// <param name="user_name"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private bool Get_userinfo(string str, ref UserInfo userinfo)
        {
            if (str.IndexOf("&#") == -1)
            {
                return false;
            }
            string[] tmp = str.Split(new string[] { "&#" }, StringSplitOptions.RemoveEmptyEntries);
            userinfo.Id = tmp[1];
            userinfo.Password = tmp[2];
            userinfo.Power = (power)Int32.Parse(tmp[3]);
            return true;
        }
        private bool Authenticate(UserInfo userinfo)
        {
            if (UserOP.Check(userinfo))
            {
                return true;
            }
            return false;
        }
        public void Stop()
        {
            DB.Stop();
            StopListen();
            _current_thread = 0;
        }
        private void StopListen()
        {
            log.Info("stop translation server successfully!");
            server_socket.Close();
            listen.Abort();
        }
        #endregion
    }
}
