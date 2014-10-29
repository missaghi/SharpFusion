using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using Sharp;

namespace Sharp
{ 
    [DataContract]
    public class GenObjects
    {
        public enum AuthLevel { None, Read, Write }

        public GenObjects()
        {
            AuthorizedLevel = AuthLevel.Write;
        }

        public StateBag sb { get; set; }

        public event preUpsert preUpsertEvents;
        public event postUpsert postUpsertEvents;

        public AuthLevel AuthorizedLevel { get; set; }

        public virtual AuthLevel Authorize()
        {
            return AuthLevel.Write;
        }

        public void preUpsertEvent(StateBag sb)
        {
            if (preUpsertEvents != null)
                preUpsertEvents(sb);
        }
        public void postUpsertEvent(StateBag sb)
        {
            if (postUpsertEvents != null)
                postUpsertEvents(sb);
        } 

        public delegate void preUpsert(StateBag sb);
        public delegate void postUpsert(StateBag sb);
    }
}