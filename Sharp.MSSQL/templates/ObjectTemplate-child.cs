         
        public IList<[%tablename%]> get_children_[%ltablename%]s { get { if (_[%ltablename%]s == null || _[%ltablename%]s.Count == 0) _[%ltablename%]s = [%tablename%].Get[%tablename%]sBy[%reftable%]_[%fkey%]([%refkey%] ,sb); return _[%ltablename%]s; } set { _[%ltablename%]s = value; } }
        private IList<[%tablename%]> _[%ltablename%]s ;