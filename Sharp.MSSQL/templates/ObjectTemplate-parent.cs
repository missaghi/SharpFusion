         
        public [%tablename%] [%ltablename%]_[%fkey%] { get { if (_[%ltablename%]_[%fkey%] == null || _[%ltablename%]_[%fkey%].id == 0) _[%ltablename%]_[%fkey%] = new [%tablename%]([%fkey%], sb); return _[%ltablename%]_[%fkey%]; } set { _[%ltablename%]_[%fkey%] = value; } }
        private [%tablename%] _[%ltablename%]_[%fkey%] { get; set; }