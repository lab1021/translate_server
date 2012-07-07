using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Threading;
using log4net;

namespace translate_server
{
    enum power
    {
        admin = 1,
        checker = 2,
        translator = 3
    }
    enum state
    {
        tobeconfirm,
        disabled,
        enabled
    }
    class UserInfo
    {
        private string _id;
        private string _password;
        private power _power;
        private string _state;
        private string _lastlogin;
        private int _count;
        
        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }
        public power Power
        {
            get { return _power; }
            set { _power = value; }
        }
        public string State
        {
            get { return _state; }
            set { _state = value; }
        }
        public string Lastlogin
        {
            get { return _lastlogin; }
            set { _lastlogin = value; }
        }
        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }
        public UserInfo()
        {
            _id = "";
            _password = "";
            _power = new power();
            _state = "";
            _lastlogin = "";
            _count = 0;
        }

        public UserInfo(string id, string password, power p, string state, string lastlogin, int count)
        {
            _id = id;
            _password = password;
            _power = p;
            _state = state;
            _lastlogin = lastlogin;
            _count = count;
        }
    }
    class UserOperation
    {
        
        private string _userdb;
        private SQLiteConnection conn;

        private static Mutex Read = new Mutex();
        private static Mutex Write = new Mutex();
        private static Mutex[] M = new Mutex[] { Read, Write };

        /// <summary>
        /// 日志控制器
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(typeof(UserOperation));

        public UserOperation()
        {
            _userdb = MainForm.ConfigOp.User_db;
            conn = new System.Data.SQLite.SQLiteConnection();
            SQLiteConnectionStringBuilder connstr = new System.Data.SQLite.SQLiteConnectionStringBuilder();
            connstr.DataSource = _userdb;
            connstr.Password = "translatorpw";
            conn.ConnectionString = connstr.ToString();
            conn.Open();
        }
        public List<UserInfo> GetAllInfo()
        {
            Write.WaitOne();
            SQLiteCommand cmd = new System.Data.SQLite.SQLiteCommand();
            cmd.Connection = conn;
            string sql = "SELECT * FROM users";
            try
            {
                cmd.CommandText = sql;
                SQLiteDataReader reader = cmd.ExecuteReader();
                List<UserInfo> List_u = new List<UserInfo>();
                while (reader.Read())
                {
                    UserInfo tmp = new UserInfo(reader.GetString(0), reader.GetString(1), (power)Int32.Parse((reader.GetString(2))), reader.GetString(3), reader.GetString(4), Int32.Parse(reader.GetString(5)));
                    List_u.Add(tmp);
                }
                cmd.Dispose();
                Write.ReleaseMutex();
                return List_u;
            }
            catch (Exception e)
            {
                log.Debug(e.ToString());
                cmd.Dispose();
                Write.ReleaseMutex();
                throw;
            }
        }
        public bool Check(UserInfo userinfo)
        {
            Mutex.WaitAll(M);
            SQLiteCommand cmd = new System.Data.SQLite.SQLiteCommand();
            cmd.Connection = conn;
            string sql = "UPDATE users SET lastlogin = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' WHERE username = '" + userinfo.Id + "' AND password = '" + userinfo.Password + "'";
            try
            {
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                Read.ReleaseMutex();
                Write.ReleaseMutex();
                return true;
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                cmd.Dispose();
                Read.ReleaseMutex();
                Write.ReleaseMutex();
                return false;
            }
        }
        /// <summary>
        /// registration
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="password"></param>
        /// <param name="power">power</param>
        /// state = 0:to be confirmed 1:confirmed but disabled 2:normal
        /// <returns></returns>
        public bool Registration(UserInfo userinfo)
        {
            Mutex.WaitAll(M);
            SQLiteCommand cmd = new System.Data.SQLite.SQLiteCommand();
            cmd.Connection = conn;
            string sql = "INSERT INTO users VALUES('" + userinfo.Id + "','" + userinfo.Password + "','" + ((int)userinfo.Power).ToString() + "','" + "2','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '0')";
            cmd.CommandText = sql;
            try
            {
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                Read.ReleaseMutex();
                Write.ReleaseMutex();
                return true;
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                cmd.Dispose();
                Read.ReleaseMutex();
                Write.ReleaseMutex();
                return false;
            }
        }
        public bool Change_Pw(string userid, string oldpw, string newpw)
        {
            Mutex.WaitAll(M);
            SQLiteCommand cmd = new System.Data.SQLite.SQLiteCommand();
            cmd.Connection = conn;
            string sql = "SELECT * FROM users " + "WHERE username = '" + userid + "' AND " + "password = '" + oldpw + "'";
            try
            {
                cmd.CommandText = sql;
                SQLiteDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    sql = "UPDATE users SET password = '" + newpw + "' WHERE username = '" + userid + "'";
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    Read.ReleaseMutex();
                    Write.ReleaseMutex();
                    return true;
                }
                cmd.Dispose();
                Read.ReleaseMutex();
                Write.ReleaseMutex();
                return false;
            }
            catch (Exception e)
            {
                cmd.Dispose();
                Read.ReleaseMutex();
                Write.ReleaseMutex();
                log.Debug(e.ToString());
                throw;
            }
        }
        public bool Del_User(UserInfo userinfo)
        {
            Mutex.WaitAll(M);
            SQLiteCommand cmd = new System.Data.SQLite.SQLiteCommand();
            cmd.Connection = conn;
            string sql = "DELETE FROM users WHERE username = '" + userinfo.Id + "'";
            try
            {
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                Read.ReleaseMutex();
                Write.ReleaseMutex();
                return true;
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                cmd.Dispose();
                Read.ReleaseMutex();
                Write.ReleaseMutex();
                return false;
            }
        }
        public bool Del_User(ref List<UserInfo> list_userinfo)
        {
            string value = "";
            foreach (UserInfo item in list_userinfo)
            {
                value = value + "'" + item.Id + "',";
            }
            value = value.Substring(0, value.Length - 1);

            Mutex.WaitAll(M);
            SQLiteCommand cmd = new System.Data.SQLite.SQLiteCommand();
            cmd.Connection = conn;
            try
            {
                string sql = "DELETE FROM users WHERE username IN (" + value + ")";
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                Read.ReleaseMutex();
                Write.ReleaseMutex();
                return true;
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                cmd.Dispose();
                Read.ReleaseMutex();
                Write.ReleaseMutex();
                return false;
            }
        }

        public bool Update_State(ref List<UserInfo> list_userinfo)
        {
            Mutex.WaitAll(M);
            SQLiteCommand cmd = new System.Data.SQLite.SQLiteCommand();
            cmd.Connection = conn;

            try
            {
                string sql = "";
                foreach (UserInfo item in list_userinfo)
                {
                    sql =sql + "UPDATE users SET state = '" + item.State + "' WHERE username = '" + item.Id + "';";
                }
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
                
                cmd.Dispose();
                Read.ReleaseMutex();
                Write.ReleaseMutex();
                return true;
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                cmd.Dispose();
                Read.ReleaseMutex();
                Write.ReleaseMutex();
                return false;
            }   
        }
        //public bool ConfirmAndEnable_State(UserInfo userinfo)
        //{
        //    Mutex.WaitAll(M);
        //    SQLiteCommand cmd = new System.Data.SQLite.SQLiteCommand();
        //    cmd.Connection = conn;
        //    string sql = "UPDATE users SET state = '2' WHERE username = '" + userinfo.Id + "'";
        //    try
        //    {
        //        cmd.CommandText = sql;
        //        cmd.ExecuteNonQuery();
        //        cmd.Dispose();
        //        Read.ReleaseMutex();
        //        Write.ReleaseMutex();
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        log.Error(e.ToString());
        //        cmd.Dispose();
        //        Read.ReleaseMutex();
        //        Write.ReleaseMutex();
        //        return false;
        //    }
        //}
        //public bool ConfirmAndEnable_State(ref List<UserInfo> list_userinfo)
        //{
        //    string value = "";
        //    foreach (UserInfo item in list_userinfo)
        //    {
        //        value = value + "'" + item.Id + "',";
        //    }
        //    value = value.Substring(0, value.Length - 1);

        //    Mutex.WaitAll(M);
        //    SQLiteCommand cmd = new System.Data.SQLite.SQLiteCommand();
        //    cmd.Connection = conn;
        //    try
        //    {
        //        string sql = "UPDATE users SET state = '2' WHERE username IN (" + value + ")";
        //        cmd.CommandText = sql;
        //        cmd.ExecuteNonQuery();
        //        cmd.Dispose();
        //        Read.ReleaseMutex();
        //        Write.ReleaseMutex();
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        log.Error(e.ToString());
        //        cmd.Dispose();
        //        Read.ReleaseMutex();
        //        Write.ReleaseMutex();
        //        return false;
        //    }   
        //}

        //public bool Disable_State(UserInfo userinfo)
        //{
        //    Mutex.WaitAll(M);
        //    SQLiteCommand cmd = new System.Data.SQLite.SQLiteCommand();
        //    cmd.Connection = conn;
        //    string sql = "UPDATE users SET state = '1' WHERE username = '" + userinfo.Id + "'";
        //    try
        //    {
        //        cmd.CommandText = sql;
        //        cmd.ExecuteNonQuery();
        //        cmd.Dispose();
        //        Read.ReleaseMutex();
        //        Write.ReleaseMutex();
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        log.Error(e.ToString());
        //        cmd.Dispose();
        //        Read.ReleaseMutex();
        //        Write.ReleaseMutex();
        //        return false;
        //    }
        //}
        //public bool Disable_State(ref List<UserInfo> list_userinfo)
        //{
        //    string value = "";
        //    foreach (UserInfo item in list_userinfo)
        //    {
        //        value = value + "'" + item.Id + "',";
        //    }
        //    value = value.Substring(0, value.Length - 1);

        //    Mutex.WaitAll(M);
        //    SQLiteCommand cmd = new System.Data.SQLite.SQLiteCommand();
        //    cmd.Connection = conn;
        //    try
        //    {
        //        string sql = "UPDATE users SET state = '1' WHERE username IN (" + value + ")";
        //        cmd.CommandText = sql;
        //        cmd.ExecuteNonQuery();
        //        cmd.Dispose();
        //        Read.ReleaseMutex();
        //        Write.ReleaseMutex();
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        log.Error(e.ToString());
        //        cmd.Dispose();
        //        Read.ReleaseMutex();
        //        Write.ReleaseMutex();
        //        return false;
        //    }   
        //}
        //public bool Enable_State(UserInfo userinfo)
        //{
        //    Mutex.WaitAll(M);
        //    SQLiteCommand cmd = new System.Data.SQLite.SQLiteCommand();
        //    cmd.Connection = conn;
        //    string sql = "UPDATE users SET state = '2' WHERE username = '" + userinfo.Id + "'";
        //    try
        //    {
        //        cmd.CommandText = sql;
        //        cmd.ExecuteNonQuery();
        //        cmd.Dispose();
        //        Read.ReleaseMutex();
        //        Write.ReleaseMutex();
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        log.Error(e.ToString());
        //        cmd.Dispose();
        //        return false;
        //    }
        //}
        //public bool Enable_State(ref List<UserInfo> list_userinfo)
        //{
        //    string value = "";
        //    foreach (UserInfo item in list_userinfo)
        //    {
        //        value = value + "'" + item.Id + "',";
        //    }
        //    value = value.Substring(0, value.Length - 1);

        //    Mutex.WaitAll(M);
        //    SQLiteCommand cmd = new System.Data.SQLite.SQLiteCommand();
        //    cmd.Connection = conn;
        //    try
        //    {
        //        string sql = "UPDATE users SET state = '2' WHERE username IN (" + value + ")";
        //        cmd.CommandText = sql;
        //        cmd.ExecuteNonQuery();
        //        cmd.Dispose();
        //        Read.ReleaseMutex();
        //        Write.ReleaseMutex();
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        log.Error(e.ToString());
        //        cmd.Dispose();
        //        Read.ReleaseMutex();
        //        Write.ReleaseMutex();
        //        return false;
        //    }   
        //}

        public state Get_State(UserInfo userinfo)
        {
            Write.WaitOne();
            SQLiteCommand cmd = new System.Data.SQLite.SQLiteCommand();
            cmd.Connection = conn;
            string sql = "SELECT state FROM users" + "WHERE username = '" + userinfo.Id + "'";
            try
            {
                cmd.CommandText = sql;
                SQLiteDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    state t = (state)Int32.Parse(reader.GetString(0));
                    cmd.Dispose();
                    Write.ReleaseMutex();
                    return t;
                }
                cmd.Dispose();
                Write.ReleaseMutex();
                return state.tobeconfirm;
            }
            catch (Exception e)
            {
                cmd.Dispose();
                Write.ReleaseMutex();
                log.Debug(e.ToString());
                throw;
            }
        }

        public bool Update_Count(ref List<Corpus> list_corpus, UserInfo userinfo)
        {
            Mutex.WaitAll(M);
            SQLiteCommand cmd = new System.Data.SQLite.SQLiteCommand();
            cmd.Connection = conn;
            string sql = "SELECT count FROM users " + "WHERE username = '" + userinfo.Id + "'";
            try
            {
                cmd.CommandText = sql;
                SQLiteDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    int count = list_corpus.Count + Int32.Parse(reader.GetString(0));
                    sql = "UPDATE users SET count = '" + count.ToString() + "' WHERE username = '" + userinfo.Id + "';";
                    sql = sql + "INSERT INTO users_tid VALUES ";
                    string tmp = "";
                    foreach (Corpus item in list_corpus)
                    {
                        tmp = tmp + "('" + item.Id + "', '" + userinfo.Id + "'),";
                    }
                    sql = sql + tmp.Substring(0, tmp.Length - 1) + ";";
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    Read.ReleaseMutex();
                    Write.ReleaseMutex();
                    return true;
                }
                cmd.Dispose();
                Read.ReleaseMutex();
                Write.ReleaseMutex();
                return false;
            }
            catch (Exception e)
            {
                cmd.Dispose();
                Read.ReleaseMutex();
                Write.ReleaseMutex();
                log.Debug(e.ToString());
                throw;
            }
        }
    }
}
