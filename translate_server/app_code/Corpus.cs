using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace translate_server
{
    class Corpus
    {
        #region
        /// <summary>
        /// _id前三位记录数据库id
        /// </summary>
        private string _id;
        private string _state;
        private string _checker;
        private string _translator;
        private string _submit;
        private string _title;
        private string _body;
        #endregion

        #region
        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }
        public string State
        {
            get { return _state; }
            set { _state = value; }
        }
        public string Checker
        {
            get { return _checker; }
            set { _checker = value;}
        }
        public string Translator
        {
            get { return _translator; }
            set { _translator = value; }
        }
        public string Submit
        {
            get { return _submit; }
            set { _submit = value; }
        }
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }
        public string Body
        {
            get { return _body; }
            set { _body = value; }
        }
        #endregion

        public Corpus()
        {
            _id = "";
            _checker = "";
            _translator = "";
            _submit = "";
            _title = "";
            _body = "";
        }
        public Corpus(string id, string checker, string translator, string submit, string title, string body)
        {
            _id = id;
            _checker = checker;
            _translator = translator;
            _submit =submit;
            _title = title;
            _body = body;
        }
    }
}
