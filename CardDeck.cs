using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Flashcards
{
    [Serializable]
    class CardDeck
    {
        public string title { get; set; }
        public List<Flashcard> flashcards { get; set; }
        public CardDeck(string title, List<Flashcard> flashcards)
        {
            this.title = title;
            this.flashcards = flashcards;
        }
    }
}
