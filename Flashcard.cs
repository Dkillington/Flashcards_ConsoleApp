using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Flashcards
{
    [Serializable]
    class Flashcard
    {
        public string question { get; set; }
        public string answer { get; set; }
        public bool currentlyMarked = false;
        public Flashcard(string _question, string _answer)
        {
            question = _question;
            answer = _answer;
        }
    }
}
