using System;
using System.Collections.Generic;
using System.Text;

namespace BreweryLib.model
{
    public class Bottle
    {
        private static int NextId = 0;
        private int _id;
        private string _state;

        public Bottle()
        {
            _id = NextId++;
            _state = "UnCleaned";
        }

        

        public int Id
        {
            get => _id;
            set => _id = value;
        }

        public string State
        {
            get => _state;
            set => _state = value;
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(State)}: {State}";
        }
    }
}
